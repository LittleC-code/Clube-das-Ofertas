using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
using ClubeDasOfertas.Web.Ui;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/denied";
        options.Cookie.Name = "clube_ofertas_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
});

builder.Services.AddAntiforgery(options =>
{
    options.FormFieldName = "__RequestVerificationToken";
    options.Cookie.Name = "clube_ofertas_antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = SpreadsheetImporter.MaxUploadBytes;
});

builder.Services.AddSingleton<AppDb>();
builder.Services.AddScoped<AppRepository>();
builder.Services.AddScoped<SchemaInitializer>();
builder.Services.AddScoped<SpreadsheetImporter>();
builder.Services.AddScoped<CampaignImportService>();
builder.Services.AddScoped<CampaignItemEditorService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ExportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SchemaInitializer>().InitializeAsync();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (AntiforgeryValidationException)
    {
        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "text/html; charset=utf-8";

        var antiForgeryField = context.User.Identity?.IsAuthenticated == true
            ? AntiForgeryField(context.RequestServices.GetRequiredService<IAntiforgery>(), context)
            : "";
        var body = """
<section class="panel">
  <h1>Formulário expirado</h1>
  <p>O envio não foi aceito porque o token de segurança expirou ou ficou inválido.</p>
  <p>Recarregue a página, confirme o login se necessário e envie novamente.</p>
</section>
""";

        var page = HtmlView.Layout("Formulário expirado", context.User, body, antiForgeryField: antiForgeryField);
        await context.Response.WriteAsync(page);
    }
});

app.MapGet("/", async (AppRepository repository, CancellationToken cancellationToken) =>
{
    return await repository.HasUsersAsync(cancellationToken)
        ? Results.Redirect("/campaigns")
        : Results.Redirect("/setup");
});

app.MapGet("/setup", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    if (await repository.HasUsersAsync(cancellationToken))
    {
        return context.User.Identity?.IsAuthenticated == true
            ? Results.Redirect("/campaigns")
            : Results.Redirect("/login");
    }

    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var body = $$"""
<section class="login">
  <h1>Configurar acesso inicial</h1>
  <p class="muted">Crie o primeiro administrador do sistema. Depois disso, a tela de configuração inicial será desativada.</p>
  <form method="post" action="/setup">
    {{antiForgeryField}}
    <div class="field">
      <label>Nome</label>
      <input name="display_name" required autofocus>
    </div>
    <div class="field">
      <label>Email</label>
      <input name="email" type="email" autocomplete="username" required>
    </div>
    <div class="field">
      <label>Senha</label>
      <input name="password" type="password" autocomplete="new-password" required>
    </div>
    <div class="field">
      <label>Confirmar senha</label>
      <input name="confirm_password" type="password" autocomplete="new-password" required>
    </div>
    <button type="submit">Criar administrador</button>
  </form>
</section>
""";

    return HtmlView.Page("Configuração inicial", context.User, body, Notice(context.Request));
}).AllowAnonymous();

app.MapPost("/setup", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);

    if (await repository.HasUsersAsync(cancellationToken))
    {
        return Results.Redirect("/login");
    }

    var form = await context.Request.ReadFormAsync(cancellationToken);
    var displayName = form["display_name"].ToString().Trim();
    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    var confirmPassword = form["confirm_password"].ToString();

    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return RedirectWithNotice("/setup", "Preencha nome, email e senha.");
    }

    if (!LooksLikeEmail(email))
    {
        return RedirectWithNotice("/setup", "Informe um email válido.");
    }

    if (password.Length < 10)
    {
        return RedirectWithNotice("/setup", "A senha inicial precisa ter ao menos 10 caracteres.");
    }

    if (password != confirmPassword)
    {
        return RedirectWithNotice("/setup", "A confirmação da senha não confere.");
    }

    var account = await repository.CreateUserAsync(email, displayName, Roles.Admin, password, cancellationToken);
    await repository.AddAuditAsync(null, account.Email, "Criou administrador inicial", "User", account.Id, "Configuração inicial do sistema", cancellationToken);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, account.Id.ToString()),
        new(ClaimTypes.Email, account.Email),
        new(ClaimTypes.Name, account.DisplayName),
        new(ClaimTypes.Role, account.Role)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect("/campaigns");
}).AllowAnonymous();

app.MapGet("/login", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    if (!await repository.HasUsersAsync(cancellationToken))
    {
        return Results.Redirect("/setup");
    }

    if (context.User.Identity?.IsAuthenticated == true)
    {
        return Results.Redirect("/campaigns");
    }

    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var body = $$"""
<section class="login">
  <h1>Entrar</h1>
  <form method="post" action="/login">
    {{antiForgeryField}}
    <div class="field">
      <label>Email</label>
      <input name="email" type="email" autocomplete="username" required autofocus>
    </div>
    <div class="field">
      <label>Senha</label>
      <input name="password" type="password" autocomplete="current-password" required>
    </div>
    <button type="submit">Entrar</button>
  </form>
</section>
""";
    return HtmlView.Page("Login", context.User, body, Notice(context.Request));
}).AllowAnonymous();

app.MapPost("/login", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);

    if (!await repository.HasUsersAsync(cancellationToken))
    {
        return Results.Redirect("/setup");
    }

    var form = await context.Request.ReadFormAsync(cancellationToken);
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var account = await repository.GetUserByEmailAsync(email, cancellationToken);

    if (account is null || !account.IsActive || !PasswordHasher.Verify(password, account.PasswordHash))
    {
        var antiForgeryField = AntiForgeryField(antiforgery, context);
        var body = $$"""
<section class="login">
  <h1>Entrar</h1>
  <div class="notice error">Email ou senha inválidos.</div>
  <form method="post" action="/login">
    {{antiForgeryField}}
    <div class="field">
      <label>Email</label>
      <input name="email" type="email" autocomplete="username" required autofocus>
    </div>
    <div class="field">
      <label>Senha</label>
      <input name="password" type="password" autocomplete="current-password" required>
    </div>
    <button type="submit">Entrar</button>
  </form>
</section>
""";
        return HtmlView.Page("Login", context.User, body, statusCode: StatusCodes.Status401Unauthorized);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, account.Id.ToString()),
        new(ClaimTypes.Email, account.Email),
        new(ClaimTypes.Name, account.DisplayName),
        new(ClaimTypes.Role, account.Role)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    await repository.AddAuditAsync(account.Id, account.Email, "Login", "User", account.Id, "Usuário entrou no sistema", cancellationToken);

    return Results.Redirect("/campaigns");
}).AllowAnonymous();

app.MapPost("/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).RequireAuthorization();

app.MapGet("/denied", (HttpContext context, IAntiforgery antiforgery) =>
    HtmlView.Page(
        "Acesso negado",
        context.User,
        """<h1>Acesso negado</h1><p>Seu perfil não possui permissão para esta área.</p>""",
        antiForgeryField: AntiForgeryField(antiforgery, context),
        statusCode: StatusCodes.Status403Forbidden)
).RequireAuthorization();

app.MapGet("/campaigns", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    var campaigns = await repository.ListCampaignsAsync(cancellationToken);
    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var campaignCards = new List<(Campaign Campaign, CampaignStats Stats)>(campaigns.Count);
    foreach (var campaign in campaigns)
    {
        var stats = await repository.GetCampaignStatsAsync(campaign.Id, cancellationToken);
        campaignCards.Add((campaign, stats));
    }

    return HtmlView.Page("Campanhas", context.User, RenderCampaignDashboard(campaignCards, antiForgeryField), Notice(context.Request), antiForgeryField, pageClass: "page-campaign", headerTitle: "Campanhas");
}).RequireAuthorization();

app.MapPost("/campaigns", async (HttpContext context, AppRepository repository, CampaignImportService importService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    IFormCollection form;
    try
    {
        form = await context.Request.ReadFormAsync(cancellationToken);
    }
    catch (InvalidDataException)
    {
        return RedirectWithNotice("/campaigns", $"Arquivo acima do limite de {SpreadsheetImporter.MaxUploadBytes / (1024 * 1024)} MB.");
    }

    var name = form["name"].ToString();

    if (!TryDate(form["valid_from"].ToString(), out var validFrom) || !TryDate(form["valid_to"].ToString(), out var validTo) || validTo < validFrom)
    {
        return RedirectWithNotice("/campaigns", "Datas inválidas para a campanha.");
    }

    if (string.IsNullOrWhiteSpace(name))
    {
        return RedirectWithNotice("/campaigns", "Informe um nome para a campanha.");
    }

    var campaign = await repository.CreateCampaignAsync(name, validFrom, validTo, currentUser.Id, cancellationToken);
    await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Criou campanha", "Campaign", campaign.Id, campaign.Name, cancellationToken);

    var file = form.Files.GetFile("file");
    var sheetName = CampaignSheetName(form);
    if (file is null || file.Length == 0 || string.IsNullOrWhiteSpace(file.FileName))
    {
        return Results.Redirect($"/campaigns/{campaign.Id}");
    }

    try
    {
        var batch = await importService.ImportAsync(campaign, file, currentUser, sheetName, cancellationToken);
        return RedirectWithNotice($"/campaigns/{campaign.Id}", $"Campanha criada e importada: {CountLabel(batch.RowCount, "linha de origem", "linhas de origem")} da aba {sheetName}.");
    }
    catch (ImportException ex)
    {
        return RedirectWithNotice($"/campaigns/{campaign.Id}", $"Campanha criada, mas a importação não foi concluída: {ex.Message}");
    }
}).RequireAuthorization();

app.MapPost("/campaigns/{id:guid}/delete", async (Guid id, HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var campaign = await repository.GetCampaignAsync(id, cancellationToken);
    if (campaign is null)
    {
        return RedirectWithNotice("/campaigns", "Campanha não encontrada.");
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    await repository.DeleteCampaignAsync(id, cancellationToken);
    await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Excluiu campanha", "Campaign", id, campaign.Name, cancellationToken);
    return RedirectWithNotice("/campaigns", "Campanha excluída.");
}).RequireAuthorization();

app.MapGet("/campaigns/{id:guid}", async (Guid id, string? filter, HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var campaign = await repository.GetCampaignAsync(id, cancellationToken);
    if (campaign is null)
    {
        return HtmlView.Page("Campanha não encontrada", context.User, "<h1>Campanha não encontrada</h1>", antiForgeryField: antiForgeryField, pageClass: "page-campaign", headerTitle: "Campanhas", statusCode: StatusCodes.Status404NotFound);
    }

    var stats = await repository.GetCampaignStatsAsync(id, cancellationToken);
    var items = await repository.GetCampaignItemsAsync(id, cancellationToken);
    var visibleItems = ApplyFilter(items, filter).ToList();
    var body = RenderCampaignDetails(campaign, stats, visibleItems, filter ?? "todos", antiForgeryField);
    return HtmlView.Page(campaign.Name, context.User, body, Notice(context.Request), antiForgeryField, pageClass: "page-campaign", headerTitle: "Campanhas");
}).RequireAuthorization();

app.MapPost("/campaigns/{id:guid}/import", async (Guid id, HttpContext context, AppRepository repository, CampaignImportService importService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var campaign = await repository.GetCampaignAsync(id, cancellationToken);
    if (campaign is null)
    {
        return RedirectWithNotice("/campaigns", "Campanha não encontrada.");
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    IFormCollection form;
    try
    {
        form = await context.Request.ReadFormAsync(cancellationToken);
    }
    catch (InvalidDataException)
    {
        return RedirectWithNotice($"/campaigns/{id}", $"Arquivo acima do limite de {SpreadsheetImporter.MaxUploadBytes / (1024 * 1024)} MB.");
    }

    var file = form.Files.GetFile("file");
    if (file is null)
    {
        return RedirectWithNotice($"/campaigns/{id}", "Selecione um arquivo CSV, XLSX ou XLSM.");
    }

    var sheetName = CampaignSheetName(form);

    try
    {
        var batch = await importService.ImportAsync(campaign, file, currentUser, sheetName, cancellationToken);
        return RedirectWithNotice($"/campaigns/{id}", $"Importação concluída: {CountLabel(batch.RowCount, "linha de origem", "linhas de origem")} da aba {sheetName}.");
    }
    catch (ImportException ex)
    {
        return RedirectWithNotice($"/campaigns/{id}", ex.Message);
    }
}).RequireAuthorization();

app.MapPost("/worksheets", async (HttpContext context, SpreadsheetImporter importer, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);

    IFormCollection form;
    try
    {
        form = await context.Request.ReadFormAsync(cancellationToken);
    }
    catch (InvalidDataException)
    {
        return Results.Json(new
        {
            ok = false,
            notice = $"Arquivo acima do limite de {SpreadsheetImporter.MaxUploadBytes / (1024 * 1024)} MB."
        }, statusCode: StatusCodes.Status400BadRequest);
    }

    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0 || string.IsNullOrWhiteSpace(file.FileName))
    {
        return Results.Json(new
        {
            ok = false,
            notice = "Selecione um arquivo XLSX ou XLSM para listar as abas."
        }, statusCode: StatusCodes.Status400BadRequest);
    }

    if (!IsWorkbookFile(file.FileName))
    {
        return Results.Json(new
        {
            ok = true,
            supportsSheets = false,
            defaultSheet = SpreadsheetImporter.DefaultCampaignSheetName,
            worksheets = Array.Empty<string>(),
            notice = "Arquivos CSV ou TXT não possuem abas para selecionar."
        });
    }

    try
    {
        var worksheets = await importer.ListWorksheetNamesAsync(file, cancellationToken);
        var preferred = worksheets.FirstOrDefault(x => string.Equals(x, SpreadsheetImporter.DefaultCampaignSheetName, StringComparison.OrdinalIgnoreCase))
            ?? worksheets.FirstOrDefault()
            ?? SpreadsheetImporter.DefaultCampaignSheetName;

        return Results.Json(new
        {
            ok = true,
            supportsSheets = true,
            defaultSheet = preferred,
            worksheets
        });
    }
    catch (ImportException ex)
    {
        return Results.Json(new
        {
            ok = false,
            notice = ex.Message
        }, statusCode: StatusCodes.Status400BadRequest);
    }
}).RequireAuthorization();

app.MapPost("/campaigns/{campaignId:guid}/items/{itemId:guid}/approve", async (Guid campaignId, Guid itemId, HttpContext context, AppRepository repository, ReviewService reviewService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var filter = form["filter"].ToString();
    await reviewService.ApproveAsync(itemId, currentUser, form["comment"].ToString(), cancellationToken);
    return await CampaignMutationResultAsync(campaignId, filter, "Item confirmado.", context, repository, antiforgery, cancellationToken);
}).RequireAuthorization();

app.MapPost("/campaigns/{campaignId:guid}/items/{itemId:guid}/reject", async (Guid campaignId, Guid itemId, HttpContext context, AppRepository repository, ReviewService reviewService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var filter = form["filter"].ToString();
    await reviewService.RejectAsync(itemId, currentUser, form["comment"].ToString(), cancellationToken);
    return await CampaignMutationResultAsync(campaignId, filter, "Item rejeitado e mantido bloqueado.", context, repository, antiforgery, cancellationToken);
}).RequireAuthorization();

app.MapPost("/campaigns/{campaignId:guid}/items/approve-all", async (Guid campaignId, HttpContext context, AppRepository repository, ReviewService reviewService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var campaign = await repository.GetCampaignAsync(campaignId, cancellationToken);
    if (campaign is null)
    {
        return CampaignMutationError(campaignId, "", "Campanha não encontrada.", context, 404);
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var filter = form["filter"].ToString();
    var items = await repository.GetCampaignItemsAsync(campaignId, cancellationToken);
    var itemIds = ApplyFilter(items, filter)
        .Where(IsReviewableItem)
        .Select(x => x.Id)
        .ToList();

    if (itemIds.Count == 0)
    {
        return CampaignMutationError(campaignId, filter, "Não há itens revisáveis nesse filtro.", context);
    }

    var approved = await reviewService.ApproveManyAsync(itemIds, currentUser, form["comment"].ToString(), cancellationToken);
    return await CampaignMutationResultAsync(campaignId, filter, approved == 1 ? "1 item confirmado." : $"{approved} itens confirmados.", context, repository, antiforgery, cancellationToken);
}).RequireAuthorization();

app.MapPost("/campaigns/{campaignId:guid}/items/{itemId:guid}/save", async (Guid campaignId, Guid itemId, HttpContext context, AppRepository repository, CampaignItemEditorService editorService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var campaign = await repository.GetCampaignAsync(campaignId, cancellationToken);
    if (campaign is null)
    {
        return CampaignMutationError(campaignId, "", "Campanha não encontrada.", context, 404);
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var filter = form["filter"].ToString();

    try
    {
        await editorService.SaveAsync(
            itemId,
            new CampaignItemEditInput(
                form["description_tabloid"].ToString(),
                form["description_solidus"].ToString(),
                form["barcode"].ToString(),
                form["quantity_raw"].ToString(),
                form["price_sale"].ToString(),
                form["price_club"].ToString()),
            currentUser,
            cancellationToken);

        return await CampaignMutationResultAsync(campaignId, filter, "Item atualizado.", context, repository, antiforgery, cancellationToken);
    }
    catch (InvalidOperationException ex)
    {
        return CampaignMutationError(campaignId, filter, ex.Message, context);
    }
}).RequireAuthorization();

app.MapPost("/campaigns/{id:guid}/export", async (Guid id, HttpContext context, AppRepository repository, ExportService exportService, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var campaign = await repository.GetCampaignAsync(id, cancellationToken);
    if (campaign is null)
    {
        return RedirectWithNotice("/campaigns", "Campanha não encontrada.");
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var export = await exportService.ExportAsync(campaign, currentUser, cancellationToken);
    return Results.Redirect($"/exports/{export.Id}/download");
}).RequireAuthorization();

app.MapGet("/exports/{id:guid}/download", async (Guid id, AppRepository repository, CancellationToken cancellationToken) =>
{
    var export = await repository.GetExportAsync(id, cancellationToken);
    if (export is null)
    {
        return Results.NotFound();
    }

    var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(export.Content)).ToArray();
    return Results.File(bytes, "text/csv; charset=utf-8", export.FileName);
}).RequireAuthorization();

app.MapGet("/catalog", async (string? q, string? category, HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    var entries = await repository.SearchCatalogAsync(q ?? "", category ?? "", cancellationToken);
    var categories = await repository.ListCatalogCategoriesAsync(cancellationToken);
    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var body = RenderCatalog(entries, categories, q ?? "", category ?? "", antiForgeryField);
    return HtmlView.Page("Catálogo de produtos", context.User, body, Notice(context.Request), antiForgeryField, pageClass: "page-campaign", headerTitle: "Catálogo de produtos");
}).RequireAuthorization("AdminOnly");

app.MapPost("/catalog/import", async (HttpContext context, AppRepository repository, SpreadsheetImporter importer, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    IFormCollection form;
    try
    {
        form = await context.Request.ReadFormAsync(cancellationToken);
    }
    catch (InvalidDataException)
    {
        return RedirectWithNotice("/catalog", $"Arquivo acima do limite de {SpreadsheetImporter.MaxUploadBytes / (1024 * 1024)} MB.");
    }

    var file = form.Files.GetFile("file");
    if (file is null)
    {
        return RedirectWithNotice("/catalog", "Selecione o arquivo com a base de códigos.");
    }

    try
    {
        var rows = await importer.ReadCatalogRowsAsync(file, cancellationToken);
        var count = await repository.UpsertCatalogAsync(rows, cancellationToken);
        await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Importou catálogo", "ProductCatalog", null, $"{file.FileName} ({count} registros)", cancellationToken);
        return RedirectWithNotice("/catalog", $"Catálogo importado/atualizado: {count} registros.");
    }
    catch (ImportException ex)
    {
        return RedirectWithNotice("/catalog", ex.Message);
    }
}).RequireAuthorization("AdminOnly");

app.MapGet("/rules", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    var rules = await repository.ListRulesAsync(cancellationToken);
    var antiForgeryField = AntiForgeryField(antiforgery, context);
    return HtmlView.Page("Regras", context.User, RenderRules(rules, antiForgeryField), Notice(context.Request), antiForgeryField, pageClass: "page-campaign", headerTitle: "Regras");
}).RequireAuthorization("AdminOnly");

app.MapPost("/rules", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var name = form["name"].ToString();
    var type = form["rule_type"].ToString();
    var pattern = form["pattern"].ToString();
    var multiplierText = form["multiplier"].ToString();
    var targetUnit = form["target_unit"].ToString();
    var categoryScope = form["category_scope"].ToString();
    var requiresReview = form["requires_review"] == "on";

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(pattern))
    {
        return RedirectWithNotice("/rules", "Nome, tipo e padrão são obrigatórios.");
    }

    if (!Parsing.TryMoney(multiplierText, out var multiplier) || multiplier <= 0m)
    {
        multiplier = 1m;
    }

    var now = DateTimeOffset.UtcNow;
    var rule = new ConversionRule(Guid.NewGuid(), name, type, pattern, multiplier, targetUnit, categoryScope, requiresReview, true, now, now);
    await repository.AddRuleAsync(rule, cancellationToken);
    await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Criou regra", "ConversionRule", rule.Id, rule.Name, cancellationToken);
    return RedirectWithNotice("/rules", "Regra criada.");
}).RequireAuthorization("AdminOnly");

app.MapPost("/rules/{id:guid}/save", async (Guid id, HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var existingRule = await repository.GetRuleAsync(id, cancellationToken);
    if (existingRule is null)
    {
        return RedirectWithNotice("/rules", "Regra não encontrada.");
    }

    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    var form = await context.Request.ReadFormAsync(cancellationToken);
    var name = form["name"].ToString();
    var type = form["rule_type"].ToString();
    var pattern = form["pattern"].ToString();
    var multiplierText = form["multiplier"].ToString();
    var targetUnit = form["target_unit"].ToString();
    var categoryScope = form["category_scope"].ToString();
    var requiresReview = form["requires_review"] == "on";

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(pattern))
    {
        return RedirectWithNotice("/rules", "Nome, tipo e padrão são obrigatórios.");
    }

    if (!Parsing.TryMoney(multiplierText, out var multiplier) || multiplier <= 0m)
    {
        multiplier = 1m;
    }

    var now = DateTimeOffset.UtcNow;
    var updatedRule = existingRule with
    {
        Name = name,
        RuleType = type,
        Pattern = pattern,
        Multiplier = multiplier,
        TargetUnit = targetUnit,
        CategoryScope = categoryScope,
        RequiresReview = requiresReview,
        UpdatedAt = now
    };

    await repository.UpdateRuleAsync(updatedRule, cancellationToken);
    var auditDetails = existingRule.Name == updatedRule.Name ? updatedRule.Name : $"{existingRule.Name} -> {updatedRule.Name}";
    await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Editou regra", "ConversionRule", updatedRule.Id, auditDetails, cancellationToken);
    return RedirectWithNotice("/rules", "Regra atualizada.");
}).RequireAuthorization("AdminOnly");

app.MapPost("/rules/{id:guid}/toggle", async (Guid id, HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    await antiforgery.ValidateRequestAsync(context);
    var currentUser = await CurrentUserAsync(context, repository, cancellationToken);
    await repository.ToggleRuleAsync(id, cancellationToken);
    await repository.AddAuditAsync(currentUser.Id, currentUser.Email, "Alternou regra", "ConversionRule", id, "Ativar/desativar", cancellationToken);
    return RedirectWithNotice("/rules", "Status da regra atualizado.");
}).RequireAuthorization("AdminOnly");

app.MapGet("/history", async (HttpContext context, AppRepository repository, IAntiforgery antiforgery, CancellationToken cancellationToken) =>
{
    var exports = await repository.ListExportsAsync(cancellationToken);
    var logs = await repository.ListAuditLogsAsync(cancellationToken);
    return HtmlView.Page("Histórico", context.User, RenderHistory(exports, logs), antiForgeryField: AntiForgeryField(antiforgery, context), pageClass: "page-campaign", headerTitle: "Histórico");
}).RequireAuthorization();

app.Run();

static string RenderCampaignDashboard(IReadOnlyList<(Campaign Campaign, CampaignStats Stats)> campaigns, string antiForgeryField)
{
    var body = new StringBuilder();
    body.AppendLine("""
<div class="campaign-shell">
  <section class="panel catalog-results-panel">
    <div class="panel-header">
      <div>
        <h2>Nova campanha</h2>
        <p class="panel-subtitle">Crie a campanha e, se quiser, já anexe o arquivo inicial para importar os itens na mesma etapa.</p>
      </div>
    </div>
    <form method="post" action="/campaigns" enctype="multipart/form-data" data-sheet-selector-form>
""");
    body.AppendLine(antiForgeryField);
    body.AppendLine("""
      <div class="field">
        <label>Nome</label>
        <input name="name" required placeholder="Tabloide semana 05 a 08">
      </div>
      <div class="field-grid two">
        <div class="field">
          <label>Vigência início</label>
          <input name="valid_from" type="date" data-date-picker required>
        </div>
        <div class="field">
          <label>Vigência fim</label>
          <input name="valid_to" type="date" data-date-picker required>
        </div>
      </div>
      <div class="field">
        <label>Arquivo inicial</label>
        <input type="file" name="file" accept=".csv,.txt,.xlsx,.xlsm" data-sheet-file>
        <p class="hint">Opcional. Se você anexar o arquivo agora, a campanha já será criada com a importação feita em seguida.</p>
      </div>
      <div class="field">
        <label>Aba da planilha para importar</label>
        <select name="sheet_name" data-sheet-select>
          <option value="Base Clube - CLT">Base Clube - CLT</option>
        </select>
        <p class="hint" data-sheet-hint>Selecione um arquivo XLSX ou XLSM para carregar automaticamente as abas disponíveis. Em CSV, esse campo fica apenas informativo.</p>
      </div>
      <div class="form-actions">
        <button type="submit">Criar campanha</button>
        <span class="muted">Aceita CSV, XLSX ou XLSM de até 10 MB.</span>
      </div>
    </form>
  </section>
  <section class="panel catalog-results-panel">
    <div class="panel-header">
      <div>
        <h2>Campanhas criadas</h2>
        <p class="panel-subtitle">Acompanhe a vigência, o status operacional e o volume de pendências sem precisar abrir cada campanha primeiro.</p>
      </div>
""");
    body.AppendLine($"""      {HtmlView.Badge(CountLabel(campaigns.Count, "campanha", "campanhas"), campaigns.Count == 0 ? "" : "info")}""");
    body.AppendLine("""
    </div>
""");

    if (campaigns.Count == 0)
    {
        body.AppendLine("""    <div class="empty-state">Nenhuma campanha criada ainda. Use o formulário ao lado para abrir a primeira campanha e, se quiser, já importar o arquivo inicial.</div>""");
        body.AppendLine("  </section>");
        body.AppendLine("</div>");
        return body.ToString();
    }

    body.AppendLine("""
    <div class="campaign-list">
      <div class="campaign-list-head">
        <div>Campanha</div>
        <div>Vigência</div>
        <div>Status</div>
        <div>Itens</div>
        <div>Pendências</div>
        <div></div>
      </div>
""");

    foreach (var entry in campaigns)
    {
        var pendingHtml = entry.Stats.BlockingItems == 0
            ? HtmlView.Badge("Sem pendências", "ok")
            : HtmlView.Badge(CountLabel(entry.Stats.BlockingItems, "pendência", "pendências"), entry.Stats.BlockingItems >= 10 ? "danger" : "warn");

        body.AppendLine($"""
      <article class="campaign-row">
        <div class="campaign-cell campaign-cell-main" data-label="Campanha">
          <strong>{HtmlView.E(entry.Campaign.Name)}</strong>
          <span class="muted">Criada em {entry.Campaign.CreatedAt:dd/MM/yyyy HH:mm}</span>
        </div>
        <div class="campaign-cell" data-label="Vigência">
          <strong>{entry.Campaign.ValidFrom:dd/MM/yyyy} a {entry.Campaign.ValidTo:dd/MM/yyyy}</strong>
          <span class="muted">{DaysBetween(entry.Campaign.ValidFrom, entry.Campaign.ValidTo)}</span>
        </div>
        <div class="campaign-cell" data-label="Status">{HtmlView.CampaignStatusBadge(entry.Campaign.Status)}</div>
        <div class="campaign-cell campaign-metric" data-label="Itens">
          <strong>{entry.Stats.TotalItems}</strong>
          <span class="muted">itens carregados</span>
        </div>
        <div class="campaign-cell" data-label="Pendências">{pendingHtml}</div>
        <div class="campaign-cell campaign-action" data-label="Ações">
          <div class="inline-actions">
            <a class="button secondary" href="/campaigns/{entry.Campaign.Id}">Abrir</a>
            <form method="post" action="/campaigns/{entry.Campaign.Id}/delete" onsubmit="return confirm('Excluir esta campanha? Essa ação remove itens, revisões e exportações ligadas a ela.');">
              {antiForgeryField}
              <button class="danger" type="submit">Excluir</button>
            </form>
          </div>
        </div>
      </article>
""");
    }

    body.AppendLine("""
    </div>
  </section>
</div>
""");

    return body.ToString();
}

static string RenderCampaignDetails(Campaign campaign, CampaignStats stats, IReadOnlyList<CampaignItem> items, string filter, string antiForgeryField)
{
    var normalizedFilter = NormalizeFilter(filter);
    var filterField = $"""<input type="hidden" name="filter" value="{HtmlView.E(normalizedFilter)}">""";
    var visibleReviewableItems = items.Count(IsReviewableItem);
    var exportWarning = stats.BlockingItems > 0
        ? $"""<div class="toolbar-note">{HtmlView.Badge($"{CountLabel(stats.BlockingItems, "item", "itens")} com pendências serão exportados com riscos e pendências no CSV.", "danger")}</div>"""
        : "";
    var exportConfirm = stats.BlockingItems > 0
        ? $""" onsubmit="return confirm('Esta campanha possui {CountLabel(stats.BlockingItems, "item", "itens")} com pendências. O CSV será exportado mesmo assim, incluindo riscos e pendências. Deseja continuar?');" """
        : "";
    var exportLabel = stats.BlockingItems > 0 ? "Exportar CSV com pendências" : "Exportar CSV";
    var exportButton = stats.TotalItems > 0
        ? $"""
<form method="post" action="/campaigns/{campaign.Id}/export"{exportConfirm}>
  {antiForgeryField}
  <button type="submit">{exportLabel}</button>
</form>
"""
        : """<span class="button secondary">Sem itens para exportar</span>""";
    var approveAllButton = visibleReviewableItems > 0
        ? $"""
<form method="post" action="/campaigns/{campaign.Id}/items/approve-all" class="async-campaign-form">
  {antiForgeryField}
  {filterField}
  <input type="hidden" name="comment" value="">
  <button class="secondary" type="submit" data-busy-label="Confirmando...">Confirmar todos os itens visíveis</button>
</form>
"""
        : "";

    var body = new StringBuilder();
    body.AppendLine($"""
<h1>{HtmlView.E(campaign.Name)}</h1>
<p class="panel-subtitle campaign-vigency-note">Vigência: {campaign.ValidFrom:dd/MM/yyyy} a {campaign.ValidTo:dd/MM/yyyy} ({DaysBetween(campaign.ValidFrom, campaign.ValidTo)})</p>
<div id="campaign-stats">{RenderCampaignStats(stats)}</div>
<section class="panel">
  <div class="panel-header">
    <div>
      <h2>Revisar e exportar itens</h2>
      <p class="panel-subtitle">Corrija descrição, código de barras, quantidade e preço diretamente aqui quando precisar ajustar um item antes da exportação.</p>
    </div>
  </div>
  <div class="toolbar">{exportButton}{approveAllButton}</div>
  {exportWarning}
</section>
<div class="toolbar">
  <a class="{FilterButtonClass("", normalizedFilter)}" href="/campaigns/{campaign.Id}">Todos</a>
  <a class="{FilterButtonClass("bloqueado", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=bloqueado">Bloqueados</a>
  <a class="{FilterButtonClass("pendente", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=pendente">Revisão</a>
  <a class="{FilterButtonClass("sem-codigo", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=sem-codigo">Sem código</a>
  <a class="{FilterButtonClass("pesavel", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=pesavel">Pesáveis</a>
  <a class="{FilterButtonClass("fardo", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=fardo">Fardos/caixas</a>
  <a class="{FilterButtonClass("duplicado", normalizedFilter)}" href="/campaigns/{campaign.Id}?filter=duplicado">Duplicidade</a>
  <span class="muted">Filtro atual: {HtmlView.E(DisplayFilter(normalizedFilter))}</span>
</div>
<div class="tablewrap" data-campaign-detail>
<table>
  <thead>
    <tr>
      <th>Status</th><th>Fonte</th><th>Riscos</th><th>Linha</th><th>Descrição tabloide</th><th>Descrição Solidus</th>
      <th>Código</th><th>Preço original</th><th>Preço final e quantidade</th><th>Pendências</th>
    </tr>
  </thead>
  <tbody id="campaign-items-body">
""");
    body.Append(RenderCampaignItemsTableBody(campaign, items, normalizedFilter, antiForgeryField));
    body.AppendLine("""
  </tbody>
</table>
</div>
<script>
(() => {
  const detailRoot = document.querySelector('[data-campaign-detail]');
  if (!detailRoot || detailRoot.dataset.enhanced === 'true') {
    return;
  }

  detailRoot.dataset.enhanced = 'true';

  const noticeRoot = document.getElementById('page-notice');
  const statsRoot = document.getElementById('campaign-stats');
  const tableBody = document.getElementById('campaign-items-body');

  const setNotice = (message, isError) => {
    if (!noticeRoot) {
      return;
    }

    noticeRoot.replaceChildren();
    if (!message) {
      return;
    }

    const box = document.createElement('div');
    box.className = isError ? 'notice error' : 'notice';
    box.textContent = message;
    noticeRoot.appendChild(box);
  };

  document.addEventListener('submit', async (event) => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.classList.contains('async-campaign-form')) {
      return;
    }

    event.preventDefault();
    const submitter = form.querySelector('button[type=\"submit\"]');
    const idleLabel = submitter ? submitter.textContent : '';
    const busyLabel = submitter?.dataset.busyLabel || 'Salvando...';
    const previousScroll = window.scrollY;

    if (submitter) {
      submitter.disabled = true;
      submitter.textContent = busyLabel;
    }

    try {
      const response = await fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
          'X-Requested-With': 'fetch',
          'Accept': 'application/json'
        }
      });

      const payload = await response.json();
      if (!response.ok || !payload.ok) {
        setNotice(payload.notice || 'Não foi possível concluir a ação.', true);
        return;
      }

      if (statsRoot && payload.statsHtml) {
        statsRoot.innerHTML = payload.statsHtml;
      }

      if (tableBody && payload.tableBodyHtml) {
        tableBody.innerHTML = payload.tableBodyHtml;
      }

      setNotice(payload.notice || 'Atualização concluída.', false);
      window.scrollTo({ top: previousScroll });
    }
    catch {
      setNotice('Erro ao atualizar a campanha. Tente novamente.', true);
    }
    finally {
      if (submitter) {
        submitter.disabled = false;
        submitter.textContent = idleLabel;
      }
    }
  });
})();
</script>
""");
    return body.ToString();
}

static string RenderCampaignStats(CampaignStats stats)
{
    return $"""
<div class="stats">
  <div class="stat"><strong>{stats.TotalItems}</strong><span>Itens</span></div>
  <div class="stat"><strong>{stats.BlockingItems}</strong><span>Bloqueados</span></div>
  <div class="stat"><strong>{stats.PendingReviewItems}</strong><span>Revisão</span></div>
  <div class="stat"><strong>{stats.MissingCodeItems}</strong><span>Sem código</span></div>
  <div class="stat"><strong>{stats.WeightedItems}</strong><span>Pesáveis</span></div>
  <div class="stat"><strong>{stats.PackageItems}</strong><span>Fardos/caixas</span></div>
</div>
""";
}

static string RenderCampaignItemsTableBody(Campaign campaign, IReadOnlyList<CampaignItem> items, string filter, string antiForgeryField)
{
    var body = new StringBuilder();

    if (items.Count == 0)
    {
        body.AppendLine("""<tr><td colspan="10" class="muted">Nenhum item para exibir nesse filtro.</td></tr>""");
        return body.ToString();
    }

    foreach (var item in items)
    {
        body.Append(RenderCampaignItemRows(campaign, item, filter, antiForgeryField));
    }

    return body.ToString();
}

static string RenderCampaignItemRows(Campaign campaign, CampaignItem item, string filter, string antiForgeryField)
{
    var filterField = $"""<input type="hidden" name="filter" value="{HtmlView.E(filter)}">""";
    var canReview = IsReviewableItem(item);
    var needsPackageMathHint = NeedsPackageMathHint(item);
    var packageMathHint = needsPackageMathHint
        ? "Para itens de fardos e caixas, você pode informar apenas \"Caixas\" ou \"Fardos\" na quantidade. Quando houver conta, use divisão na quantidade, como 20/6, e multiplicação nos preços, como 13,98*6."
        : "Se a descrição corrigida bater exatamente com o catálogo, o sistema tenta preencher o código automaticamente.";
    var priceSaleInputValue = string.IsNullOrWhiteSpace(item.PriceSaleRaw)
        ? Parsing.MoneyPtBr(item.FinalPriceSale)
        : item.PriceSaleRaw;
    var priceClubInputValue = string.IsNullOrWhiteSpace(item.PriceClubRaw)
        ? Parsing.MoneyPtBr(item.FinalPriceClub)
        : item.PriceClubRaw;
    var actionButtons = canReview
        ? $"""
  <form method="post" action="/campaigns/{campaign.Id}/items/{item.Id}/approve" class="async-campaign-form">
    {antiForgeryField}
    {filterField}
    <input type="hidden" name="comment" value="">
    <button type="submit" data-busy-label="Confirmando...">Confirmar</button>
  </form>
  <form method="post" action="/campaigns/{campaign.Id}/items/{item.Id}/reject" class="async-campaign-form">
    {antiForgeryField}
    {filterField}
    <input type="hidden" name="comment" value="">
    <button class="danger" type="submit" data-busy-label="Rejeitando...">Rejeitar</button>
  </form>
  <button class="ghost" type="button" data-edit-toggle="edit-{item.Id}">Editar</button>
"""
        : $"""
  <button class="ghost" type="button" data-edit-toggle="edit-{item.Id}">Editar</button>
""";
    var solidusDetails = $"""
<div class="campaign-description-stack">
  <div>{HtmlView.E(item.DescriptionSolidus)}</div>
  <div class="inline-actions campaign-item-actions">
    {actionButtons}
  </div>
</div>
""";
    var originalPriceDetails = $"""
<div class="campaign-price-stack campaign-price-stack-compact">
  <div>Venda: {Parsing.MoneyPtBr(item.OriginalPriceSale)}</div>
  <div>Clube: {Parsing.MoneyPtBr(item.OriginalPriceClub)}</div>
</div>
""";
    var finalPriceDetails = $"""
<div class="campaign-price-stack">
  <div><strong>Venda:</strong> {Parsing.MoneyPtBr(item.FinalPriceSale)}</div>
  <div><strong>Clube:</strong> {Parsing.MoneyPtBr(item.FinalPriceClub)}</div>
  <div class="muted"><strong>Qtd.:</strong> {item.Quantity:0.###} {HtmlView.E(item.Unit)}</div>
</div>
""";

    return $"""
<tr id="item-{item.Id}">
  <td>{HtmlView.StatusBadge(item)}</td>
  <td>{HtmlView.SourceBadge(item.Source)}</td>
  <td class="campaign-risks-cell">{HtmlView.Badges(item.RiskFlags)}</td>
  <td>{item.SourceRow}</td>
  <td class="campaign-description-cell">{HtmlView.E(item.DescriptionTabloid)}</td>
  <td class="campaign-description-cell">{solidusDetails}</td>
  <td class="mono">{HtmlView.E(item.Barcode)}</td>
  <td>{originalPriceDetails}</td>
  <td>{finalPriceDetails}</td>
  <td>{HtmlView.Badges(item.BlockingReasons)}</td>
</tr>
<tr class="item-edit-row" id="edit-{item.Id}" hidden>
  <td colspan="10">
    <div class="item-edit-card">
      <form method="post" action="/campaigns/{campaign.Id}/items/{item.Id}/save" class="async-campaign-form"{(needsPackageMathHint ? " data-package-math-form" : "")}>
        {antiForgeryField}
        {filterField}
        <div class="item-edit-grid">
          <div class="field span-2">
            <label>Descrição tabloide</label>
            <input name="description_tabloid" value="{HtmlView.E(item.DescriptionTabloid)}" required>
          </div>
          <div class="field">
            <label>Código de barras</label>
            <input name="barcode" value="{HtmlView.E(item.Barcode)}">
          </div>
          <div class="field span-2">
            <label>Descrição Solidus</label>
            <input name="description_solidus" value="{HtmlView.E(item.DescriptionSolidus)}">
          </div>
          <div class="field">
            <label>Quantidade</label>
            <input name="quantity_raw" value="{HtmlView.E(item.QuantityRaw)}" placeholder="{HtmlView.E(needsPackageMathHint ? "Caixas, Fardos ou 20/6" : "5 Kg ou 12 Unidades")}"{(needsPackageMathHint ? $" data-preview-unit=\"{HtmlView.E(item.Unit)}\"" : "")}>
          </div>
          <div class="field">
            <label>Preço venda</label>
            <input name="price_sale" value="{HtmlView.E(priceSaleInputValue)}" placeholder="{HtmlView.E(needsPackageMathHint ? "13,98*6" : "")}">
          </div>
          <div class="field">
            <label>Preço clube</label>
            <input name="price_club" value="{HtmlView.E(priceClubInputValue)}" placeholder="{HtmlView.E(needsPackageMathHint ? "11,98*6" : "")}">
          </div>
          {(needsPackageMathHint
              ? """
          <div class="field span-3">
            <div class="calc-preview" data-calc-preview aria-live="polite">
              <strong>Preview da conta</strong>
              <div>Digite a quantidade ou os preços para visualizar o resultado antes de salvar.</div>
            </div>
          </div>
"""
              : "")}
          <div class="field span-3">
            <div class="form-actions">
              <button type="submit" data-busy-label="Salvando...">Salvar alterações</button>
              <button class="secondary" type="button" data-edit-close="edit-{item.Id}">Fechar</button>
              <span class="muted">{HtmlView.E(packageMathHint)}</span>
            </div>
          </div>
        </div>
      </form>
    </div>
  </td>
</tr>
""";
}

static string RenderCatalog(IReadOnlyList<ProductCatalogEntry> entries, IReadOnlyList<(string Category, int Count)> categories, string query, string category, string antiForgeryField)
{
    var selectedCategory = string.IsNullOrWhiteSpace(category) ? "" : category.Trim();
    var selectedCategoryLabel = string.IsNullOrWhiteSpace(selectedCategory) ? "Todas as categorias" : selectedCategory;
    var totalCatalogItems = categories.Sum(x => x.Count);
    var body = new StringBuilder();
    body.AppendLine($$"""
<div class="catalog-layout">
  <aside class="panel catalog-sidebar">
    <div class="panel-header">
      <div>
        <h2>Navegação</h2>
        <p class="panel-subtitle">Explore todo o catálogo por busca ou categoria, sem depender da tabela limitada.</p>
      </div>
    </div>
    <div class="catalog-sidebar-section">
      <div class="catalog-summary">
        <strong>{{entries.Count}}</strong>
        <span>itens visíveis</span>
      </div>
      <div class="catalog-summary">
        <strong>{{totalCatalogItems}}</strong>
        <span>itens no catálogo</span>
      </div>
      <div class="catalog-summary">
        <strong>{{categories.Count}}</strong>
        <span>categorias</span>
      </div>
    </div>
    <div class="catalog-sidebar-section">
      <h2>Importar base de códigos</h2>
    <form method="post" action="/catalog/import" enctype="multipart/form-data">
      {{antiForgeryField}}
      <div class="field"><label>Arquivo com a aba Base - Cód. Barras ou CSV equivalente</label><input type="file" name="file" accept=".csv,.txt,.xlsx,.xlsm" required></div>
      <button type="submit">Importar catálogo</button>
    </form>
    </div>
    <div class="catalog-sidebar-section">
      <h2>Categorias</h2>
      <div class="catalog-category-list">
        <a class="{{CatalogCategoryClass("", selectedCategory)}}" href="/catalog{{CatalogQuerySuffix(query, "")}}">
          <span>Todas as categorias</span>
          <strong>{{totalCatalogItems}}</strong>
        </a>
""");

    foreach (var item in categories)
    {
        body.AppendLine($"""
        <a class="{CatalogCategoryClass(item.Category, selectedCategory)}" href="/catalog{CatalogQuerySuffix(query, item.Category)}">
          <span>{HtmlView.E(item.Category)}</span>
          <strong>{item.Count}</strong>
        </a>
""");
    }

    body.AppendLine($$"""
      </div>
    </div>
  </aside>
  <section class="panel catalog-results-panel">
    <div class="panel-header">
      <div>
        <h2>Itens do catálogo</h2>
        <p class="panel-subtitle">Categoria atual: {{HtmlView.E(selectedCategoryLabel)}}.</p>
      </div>
      {{HtmlView.Badge(CountLabel(entries.Count, "item", "itens"), entries.Count == 0 ? "" : "info")}}
    </div>
    <form method="get" action="/catalog" class="catalog-search-form">
      <div class="field"><label>Descrição, Solidus ou código</label><input name="q" value="
""");
    body.Append(HtmlView.E(query));
    body.AppendLine($$"""
"></div>
      <input type="hidden" name="category" value="{{HtmlView.E(selectedCategory)}}">
      <div class="form-actions">
        <button type="submit">Buscar</button>
        <a class="button secondary" href="/catalog">Limpar</a>
      </div>
    </form>
    <div class="catalog-list-shell">
      <div class="catalog-list">
""");

    foreach (var entry in entries)
    {
        body.AppendLine($"""
      <article class="catalog-item">
        <div class="catalog-item-main">
          <strong>{HtmlView.E(entry.DescriptionTabloid)}</strong>
          <span>{HtmlView.E(string.IsNullOrWhiteSpace(entry.DescriptionSolidus) ? "Sem descrição Solidus" : entry.DescriptionSolidus)}</span>
        </div>
        <div class="catalog-item-meta">
          <div><label>Categoria</label><span>{HtmlView.E(string.IsNullOrWhiteSpace(entry.Category) ? "Sem categoria" : entry.Category)}</span></div>
          <div><label>Código</label><span class="mono">{HtmlView.E(entry.Barcode)}</span></div>
          <div><label>Tipo</label><span>{HtmlView.E(DisplayCatalogCodeType(entry.CodeType))}</span></div>
        </div>
      </article>
""");
    }

    if (entries.Count == 0)
    {
        body.AppendLine("""<div class="empty-state">Nenhum registro encontrado para os filtros atuais.</div>""");
    }

    body.AppendLine("""
      </div>
    </div>
  </section>
</div>
""");
    return body.ToString();
}

static string RenderRules(IReadOnlyList<ConversionRule> rules, string antiForgeryField)
{
    var body = new StringBuilder();
    body.AppendLine($$"""
<div class="grid two">
  <section class="panel">
    <h2>Nova regra</h2>
    <form method="post" action="/rules">
      {{antiForgeryField}}
      <div class="field"><label>Nome</label><input name="name" required></div>
      <div class="field"><label>Tipo</label><select name="rule_type"><option value="Pesavel">Pes&aacute;vel</option><option value="Fardo">Fardo</option><option value="Caixa">Caixa</option></select></div>
      <div class="field"><label>Padr&atilde;o regex ou texto</label><input name="pattern" required></div>
      <div class="field"><label>Multiplicador</label><input name="multiplier" value="1"></div>
      <div class="field"><label>Unidade alvo</label><input name="target_unit" placeholder="Kg"></div>
      <div class="field"><label>Categorias alvo</label><input name="category_scope" placeholder="Ex.: Hortifruti, Bebidas"><p class="hint">Opcional. Informe uma ou mais categorias separadas por v&iacute;rgula para restringir a regra.</p></div>
      <div class="field"><label><input style="width:auto" type="checkbox" name="requires_review" checked> Exigir revis&atilde;o</label></div>
      <button type="submit">Criar regra</button>
    </form>
  </section>
  <section>
    <div class="tablewrap">
      <table>
        <thead><tr><th>Status</th><th>Nome</th><th>Tipo</th><th>Padr&atilde;o</th><th>Multiplicador</th><th>Unidade</th><th>Categorias</th><th>Revis&atilde;o</th><th></th></tr></thead>
        <tbody>
""");

    foreach (var rule in rules)
    {
        body.AppendLine($"""
<tr>
  <td>{HtmlView.Badge(rule.IsActive ? "Ativa" : "Inativa", rule.IsActive ? "ok" : "")}</td>
  <td>{HtmlView.E(DisplayRuleName(rule.Name))}</td>
  <td>{HtmlView.E(DisplayRuleType(rule.RuleType))}</td>
  <td class="mono">{HtmlView.E(rule.Pattern)}</td>
  <td>{rule.Multiplier:0.####}</td>
  <td>{HtmlView.E(string.IsNullOrWhiteSpace(rule.TargetUnit) ? "-" : rule.TargetUnit)}</td>
  <td>{HtmlView.E(string.IsNullOrWhiteSpace(rule.CategoryScope) ? "Todas" : rule.CategoryScope)}</td>
  <td>{(rule.RequiresReview ? "Sim" : "N\u00E3o")}</td>
  <td>
    <div class="inline-actions">
      <button class="ghost" type="button" data-edit-toggle="edit-{rule.Id}">Editar</button>
      <form method="post" action="/rules/{rule.Id}/toggle">{antiForgeryField}<button class="secondary" type="submit">Alternar</button></form>
    </div>
  </td>
</tr>
<tr class="item-edit-row" id="edit-{rule.Id}" hidden>
  <td colspan="9">
    <div class="item-edit-card">
      <form method="post" action="/rules/{rule.Id}/save">
        {antiForgeryField}
        <div class="item-edit-grid">
          <div class="field span-2">
            <label>Nome</label>
            <input name="name" value="{HtmlView.E(DisplayRuleName(rule.Name))}" required>
          </div>
          <div class="field">
            <label>Tipo</label>
            <select name="rule_type">
              {RenderRuleTypeOptions(rule.RuleType)}
            </select>
          </div>
          <div class="field span-2">
            <label>Padr&atilde;o regex ou texto</label>
            <input name="pattern" value="{HtmlView.E(rule.Pattern)}" required>
          </div>
          <div class="field">
            <label>Multiplicador</label>
            <input name="multiplier" value="{HtmlView.E(Parsing.MoneyPtBr(rule.Multiplier))}">
          </div>
          <div class="field">
            <label>Unidade alvo</label>
            <input name="target_unit" value="{HtmlView.E(rule.TargetUnit)}" placeholder="Kg">
          </div>
          <div class="field span-2">
            <label>Categorias alvo</label>
            <input name="category_scope" value="{HtmlView.E(rule.CategoryScope)}" placeholder="Ex.: Hortifruti, Bebidas">
            <p class="hint">Opcional. Informe uma ou mais categorias separadas por v&iacute;rgula para restringir a regra.</p>
          </div>
          <div class="field span-3">
            <label><input style="width:auto" type="checkbox" name="requires_review" {(rule.RequiresReview ? "checked" : "")}> Exigir revis&atilde;o</label>
          </div>
          <div class="field span-3">
            <div class="form-actions">
              <button type="submit">Salvar altera&ccedil;&otilde;es</button>
              <button class="secondary" type="button" data-edit-close="edit-{rule.Id}">Fechar</button>
              <span class="muted">O status continua sendo alterado pelo bot&atilde;o Alternar.</span>
            </div>
          </div>
        </div>
      </form>
    </div>
  </td>
</tr>
""");
    }

    body.AppendLine("</tbody></table></div></section></div>");
    return body.ToString();
}

static string RenderRuleTypeOptions(string selectedRuleType)
{
    var body = new StringBuilder();

    if (selectedRuleType != RuleTypes.Weighted && selectedRuleType != RuleTypes.PackageBale && selectedRuleType != RuleTypes.PackageBox)
    {
        body.AppendLine($"""<option value="{HtmlView.E(selectedRuleType)}" selected>{HtmlView.E(DisplayRuleType(selectedRuleType))}</option>""");
    }

    body.AppendLine(RenderRuleTypeOption(RuleTypes.Weighted, selectedRuleType));
    body.AppendLine(RenderRuleTypeOption(RuleTypes.PackageBale, selectedRuleType));
    body.AppendLine(RenderRuleTypeOption(RuleTypes.PackageBox, selectedRuleType));

    return body.ToString();
}

static string RenderRuleTypeOption(string ruleType, string selectedRuleType)
{
    var selected = ruleType == selectedRuleType ? " selected" : "";
    return $"""<option value="{ruleType}"{selected}>{HtmlView.E(DisplayRuleType(ruleType))}</option>""";
}

static string RenderHistory(IReadOnlyList<ExportBatch> exports, IReadOnlyList<AuditLog> logs)
{
    var body = new StringBuilder();
    body.AppendLine("""
<section class="panel">
  <h2>Exportações</h2>
  <div class="tablewrap"><table><thead><tr><th>Arquivo</th><th>Linhas</th><th>Data</th><th></th></tr></thead><tbody>
""");

    foreach (var export in exports)
    {
        body.AppendLine($"""<tr><td>{HtmlView.E(export.FileName)}</td><td>{export.RowCount}</td><td>{export.ExportedAt:dd/MM/yyyy HH:mm}</td><td><a class="button secondary" href="/exports/{export.Id}/download">Baixar</a></td></tr>""");
    }

    if (exports.Count == 0)
    {
        body.AppendLine("""<tr><td colspan="4" class="muted">Nenhuma exportação registrada.</td></tr>""");
    }

    body.AppendLine("</tbody></table></div></section>");
    body.AppendLine("""
<section class="panel" style="margin-top:16px">
  <h2>Auditoria</h2>
  <div class="tablewrap"><table><thead><tr><th>Data</th><th>Usuário</th><th>Ação</th><th>Entidade</th><th>Detalhes</th></tr></thead><tbody>
""");

    foreach (var log in logs)
    {
        body.AppendLine($"""<tr><td>{log.CreatedAt:dd/MM/yyyy HH:mm}</td><td>{HtmlView.E(log.ActorEmail)}</td><td>{HtmlView.E(DisplayAuditAction(log.Action))}</td><td>{HtmlView.E(DisplayAuditEntityType(log.EntityType))}</td><td>{HtmlView.E(DisplayAuditDetails(log.Details))}</td></tr>""");
    }

    if (logs.Count == 0)
    {
        body.AppendLine("""<tr><td colspan="5" class="muted">Nenhum evento registrado.</td></tr>""");
    }

    body.AppendLine("</tbody></table></div></section>");
    return body.ToString();
}

static string DisplayRuleType(string ruleType)
{
    return ruleType switch
    {
        RuleTypes.Weighted => "Pes\u00E1vel",
        RuleTypes.PackageBale => "Fardo",
        RuleTypes.PackageBox => "Caixa",
        RuleTypes.Package => "Fardo/Caixa (legado)",
        _ => ruleType
    };
}

static IEnumerable<CampaignItem> ApplyFilter(IReadOnlyList<CampaignItem> items, string? filter)
{
    return NormalizeFilter(filter) switch
    {
        "bloqueado" => items.Where(x => x.BlockingReasons.Count > 0),
        "pendente" => items.Where(x => x.ReviewStatus == ReviewStatus.Pending),
        "sem-codigo" => items.Where(x => string.IsNullOrWhiteSpace(x.Barcode)),
        "pesavel" => items.Where(x => x.RiskFlags.Contains("PESAVEL")),
        "fardo" => items.Where(x => x.RiskFlags.Contains("FARDO_CAIXA")),
        "duplicado" => items.Where(x => x.RiskFlags.Contains("DUPLICIDADE")),
        _ => items
    };
}

static async Task<IResult> CampaignMutationResultAsync(
    Guid campaignId,
    string? filter,
    string notice,
    HttpContext context,
    AppRepository repository,
    IAntiforgery antiforgery,
    CancellationToken cancellationToken)
{
    if (!WantsJson(context.Request))
    {
        return RedirectWithNotice(BuildCampaignPath(campaignId, filter), notice);
    }

    var campaign = await repository.GetCampaignAsync(campaignId, cancellationToken);
    if (campaign is null)
    {
        return Results.Json(new { ok = false, notice = "Campanha não encontrada." }, statusCode: StatusCodes.Status404NotFound);
    }

    var stats = await repository.GetCampaignStatsAsync(campaignId, cancellationToken);
    var items = await repository.GetCampaignItemsAsync(campaignId, cancellationToken);
    var antiForgeryField = AntiForgeryField(antiforgery, context);
    var normalizedFilter = NormalizeFilter(filter);
    var visibleItems = ApplyFilter(items, normalizedFilter).ToList();

    return Results.Json(new
    {
        ok = true,
        notice,
        statsHtml = RenderCampaignStats(stats),
        tableBodyHtml = RenderCampaignItemsTableBody(campaign, visibleItems, normalizedFilter, antiForgeryField)
    });
}

static IResult CampaignMutationError(Guid campaignId, string? filter, string notice, HttpContext context, int statusCode = StatusCodes.Status400BadRequest)
{
    if (WantsJson(context.Request))
    {
        return Results.Json(new { ok = false, notice }, statusCode: statusCode);
    }

    return RedirectWithNotice(BuildCampaignPath(campaignId, filter), notice);
}

static async Task<UserAccount> CurrentUserAsync(HttpContext context, AppRepository repository, CancellationToken cancellationToken)
{
    var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(idValue, out var id))
    {
        throw new InvalidOperationException("Usuário atual inválido.");
    }

    return await repository.GetUserByIdAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Usuário atual não encontrado.");
}

static bool TryDate(string value, out DateOnly date)
{
    return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

static string DaysBetween(DateOnly start, DateOnly end)
{
    var totalDays = end.DayNumber - start.DayNumber + 1;
    return totalDays <= 1
        ? "1 dia de vigência"
        : $"{totalDays} dias de vigência";
}

static bool IsReviewableItem(CampaignItem item)
{
    return item.ReviewRequired || item.ReviewStatus is ReviewStatus.Pending or ReviewStatus.Rejected;
}

static bool NeedsPackageMathHint(CampaignItem item)
{
    return item.RiskFlags.Any(flag => flag is "FARDO_CAIXA" or "FARDO" or "CAIXA")
        || string.Equals(item.Unit, "Caixas", StringComparison.OrdinalIgnoreCase)
        || string.Equals(item.Unit, "Fardos", StringComparison.OrdinalIgnoreCase);
}

static string AntiForgeryField(IAntiforgery antiforgery, HttpContext context)
{
    var tokenSet = antiforgery.GetAndStoreTokens(context);
    return $"""<input type="hidden" name="{HtmlView.E(tokenSet.FormFieldName)}" value="{HtmlView.E(tokenSet.RequestToken)}">""";
}

static bool WantsJson(HttpRequest request)
{
    if (string.Equals(request.Headers["X-Requested-With"], "fetch", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
}

static string NormalizeFilter(string? filter)
{
    if (string.IsNullOrWhiteSpace(filter))
    {
        return "";
    }

    var normalized = filter.Trim().ToLowerInvariant();
    return normalized == "todos" ? "" : normalized;
}

static string DisplayFilter(string? filter)
{
    return NormalizeFilter(filter) switch
    {
        "" => "Todos",
        "bloqueado" => "Bloqueados",
        "pendente" => "Revisão",
        "sem-codigo" => "Sem código",
        "pesavel" => "Pesáveis",
        "fardo" => "Fardos/caixas",
        "duplicado" => "Duplicidade",
        var normalized => normalized
    };
}

static string FilterButtonClass(string expectedFilter, string currentFilter)
{
    return NormalizeFilter(expectedFilter) == NormalizeFilter(currentFilter) ? "button" : "button secondary";
}

static string CampaignSheetName(IFormCollection form)
{
    var sheetName = form["sheet_name"].ToString().Trim();
    return string.IsNullOrWhiteSpace(sheetName)
        ? SpreadsheetImporter.DefaultCampaignSheetName
        : sheetName;
}

static string CountLabel(int count, string singular, string plural)
{
    return $"{count} {(count == 1 ? singular : plural)}";
}

static string DisplayCatalogCodeType(string codeType)
{
    return codeType switch
    {
        "Codigo Unificado" => "Código unificado",
        _ => codeType
    };
}

static string DisplayRuleName(string ruleName)
{
    return ruleName switch
    {
        "Pesaveis e cada 100g" => "Pesáveis e cada 100 g",
        _ => ruleName
    };
}

static string DisplayAuditAction(string action)
{
    return action switch
    {
        "Importou catalogo" => "Importou catálogo",
        "Aprovou revisao" => "Aprovou revisão",
        "Rejeitou revisao" => "Rejeitou revisão",
        _ => action
    };
}

static string DisplayAuditEntityType(string entityType)
{
    return entityType switch
    {
        "User" => "Usuário",
        "Campaign" => "Campanha",
        "CampaignItem" => "Item da campanha",
        "ProductCatalog" => "Catálogo",
        "ConversionRule" => "Regra de conversão",
        _ => entityType
    };
}

static string DisplayAuditDetails(string details)
{
    return details switch
    {
        "Bootstrap inicial do sistema" => "Configuração inicial do sistema",
        "Usuario entrou no sistema" => "Usuário entrou no sistema",
        "Ativar/desativar" => "Ativar ou desativar",
        var value => DisplayRuleName(value)
    };
}

static bool IsWorkbookFile(string fileName)
{
    var extension = Path.GetExtension(fileName).ToLowerInvariant();
    return extension is ".xlsx" or ".xlsm";
}

static string BuildCampaignPath(Guid campaignId, string? filter)
{
    var normalized = NormalizeFilter(filter);
    return string.IsNullOrWhiteSpace(normalized)
        ? $"/campaigns/{campaignId}"
        : $"/campaigns/{campaignId}?filter={UrlEncoder.Default.Encode(normalized)}";
}

static string CatalogQuerySuffix(string query, string category)
{
    var parameters = new List<string>();
    if (!string.IsNullOrWhiteSpace(query))
    {
        parameters.Add($"q={UrlEncoder.Default.Encode(query.Trim())}");
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        parameters.Add($"category={UrlEncoder.Default.Encode(category.Trim())}");
    }

    return parameters.Count == 0 ? "" : "?" + string.Join("&", parameters);
}

static string CatalogCategoryClass(string category, string currentCategory)
{
    var normalizedExpected = TextNormalizer.NormalizeKey(category);
    var normalizedCurrent = TextNormalizer.NormalizeKey(currentCategory);
    return normalizedExpected == normalizedCurrent ? "catalog-category active" : "catalog-category";
}

static bool LooksLikeEmail(string value)
{
    return value.Contains('@') && value.Contains('.');
}

static string Notice(HttpRequest request)
{
    return request.Query.TryGetValue("notice", out var value) ? value.ToString() : "";
}

static IResult RedirectWithNotice(string path, string notice)
{
    var separator = path.Contains('?') ? "&" : "?";
    return Results.Redirect($"{path}{separator}notice={UrlEncoder.Default.Encode(notice)}");
}
