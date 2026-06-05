using ClubeDasOfertas.Web.Domain;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ClubeDasOfertas.Web.Ui;

public static class HtmlView
{
    public static IResult Page(string title, ClaimsPrincipal user, string body, string notice = "", string antiForgeryField = "", int statusCode = StatusCodes.Status200OK)
    {
        return Results.Content(Layout(title, user, body, notice, antiForgeryField), "text/html; charset=utf-8", statusCode: statusCode);
    }

    public static string Layout(string title, ClaimsPrincipal user, string body, string notice = "", string antiForgeryField = "")
    {
        var signedIn = user.Identity?.IsAuthenticated == true;
        var displayName = signedIn ? user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name ?? "" : "";
        var role = signedIn ? user.FindFirstValue(ClaimTypes.Role) ?? "" : "";
        var nav = signedIn
            ? $$"""
              <nav class="nav">
                <a href="/campaigns">Campanhas</a>
                <a href="/catalog">Catalogo</a>
                <a href="/rules">Regras</a>
                <a href="/history">Historico</a>
                <form method="post" action="/logout">{{antiForgeryField}}<button class="ghost" type="submit">Sair</button></form>
              </nav>
              <div class="userbox">{E(displayName)} <span>{E(role)}</span></div>
              """
            : "";

        var noticeHtml = string.IsNullOrWhiteSpace(notice) ? "" : $"""<div class="notice">{E(notice)}</div>""";

        return $$"""
<!doctype html>
<html lang="pt-br">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{E(title)}} - Clube Das Ofertas</title>
  <style>
    :root {
      --bg: #f6f7f9;
      --panel: #ffffff;
      --text: #202124;
      --muted: #6b7280;
      --line: #d9dde3;
      --brand: #0f766e;
      --brand-strong: #0b5f59;
      --warn: #b45309;
      --danger: #b91c1c;
      --ok: #15803d;
      --info: #1d4ed8;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: "Segoe UI", Arial, sans-serif;
      color: var(--text);
      background: var(--bg);
      letter-spacing: 0;
    }
    header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      min-height: 58px;
      padding: 0 24px;
      border-bottom: 1px solid var(--line);
      background: var(--panel);
      position: sticky;
      top: 0;
      z-index: 10;
    }
    .brand { font-size: 18px; font-weight: 700; white-space: nowrap; }
    .nav { display: flex; gap: 4px; align-items: center; flex-wrap: wrap; }
    .nav a, .ghost {
      border: 0;
      background: transparent;
      color: var(--text);
      font: inherit;
      text-decoration: none;
      padding: 8px 10px;
      border-radius: 6px;
      cursor: pointer;
    }
    .nav a:hover, .ghost:hover { background: #eef1f4; }
    .userbox { color: var(--muted); font-size: 13px; white-space: nowrap; }
    .userbox span { margin-left: 6px; color: var(--brand); font-weight: 700; }
    main { padding: 22px 24px 36px; max-width: 1460px; margin: 0 auto; }
    h1 { font-size: 24px; margin: 0 0 16px; }
    h2 { font-size: 17px; margin: 0 0 12px; }
    .grid { display: grid; gap: 16px; }
    .grid.two { grid-template-columns: minmax(320px, 420px) minmax(0, 1fr); align-items: start; }
    .stats { display: grid; grid-template-columns: repeat(6, minmax(120px, 1fr)); gap: 10px; margin-bottom: 14px; }
    .stat, .panel {
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 14px;
    }
    .stat strong { display: block; font-size: 22px; }
    .stat span { color: var(--muted); font-size: 12px; }
    .toolbar { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; margin: 12px 0; }
    .toolbar a { text-decoration: none; }
    .notice {
      margin-bottom: 14px;
      padding: 10px 12px;
      background: #ecfdf5;
      border: 1px solid #a7f3d0;
      border-radius: 6px;
      color: #065f46;
    }
    .error { background: #fef2f2; border-color: #fecaca; color: var(--danger); }
    label { display: block; font-size: 13px; color: var(--muted); margin: 0 0 5px; }
    input, select, textarea {
      width: 100%;
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 9px 10px;
      font: inherit;
      background: #fff;
    }
    textarea { min-height: 38px; resize: vertical; }
    .field { margin-bottom: 12px; }
    button, .button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      min-height: 36px;
      border: 1px solid var(--brand);
      border-radius: 6px;
      padding: 7px 12px;
      color: #fff;
      background: var(--brand);
      text-decoration: none;
      font: inherit;
      cursor: pointer;
      white-space: nowrap;
    }
    button:hover, .button:hover { background: var(--brand-strong); }
    .button.secondary, button.secondary { background: #fff; color: var(--text); border-color: var(--line); }
    .button.secondary:hover, button.secondary:hover { background: #eef1f4; }
    .button.danger, button.danger { background: var(--danger); border-color: var(--danger); }
    .tablewrap {
      overflow: auto;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 6px;
    }
    table { width: 100%; border-collapse: collapse; min-width: 980px; }
    th, td { padding: 9px 10px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; font-size: 13px; }
    th { background: #f0f2f5; font-size: 12px; color: #374151; position: sticky; top: 58px; z-index: 5; }
    tr:last-child td { border-bottom: 0; }
    .muted { color: var(--muted); }
    .mono { font-family: Consolas, "Courier New", monospace; }
    .badges { display: flex; gap: 4px; flex-wrap: wrap; }
    .badge { display: inline-flex; border-radius: 999px; padding: 3px 7px; font-size: 12px; font-weight: 700; background: #eef1f4; color: #374151; }
    .badge.ok { background: #dcfce7; color: var(--ok); }
    .badge.warn { background: #ffedd5; color: var(--warn); }
    .badge.danger { background: #fee2e2; color: var(--danger); }
    .badge.info { background: #dbeafe; color: var(--info); }
    .actions { display: flex; gap: 6px; align-items: center; flex-wrap: wrap; min-width: 220px; }
    .login {
      max-width: 420px;
      margin: 70px auto;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 22px;
    }
    @media (max-width: 900px) {
      header { align-items: flex-start; padding: 12px 16px; flex-direction: column; }
      main { padding: 16px; }
      .grid.two { grid-template-columns: 1fr; }
      .stats { grid-template-columns: repeat(2, minmax(120px, 1fr)); }
      th { top: 0; }
    }
  </style>
</head>
<body>
  <header>
    <div class="brand">Clube Das Ofertas</div>
    {{nav}}
  </header>
  <main>
    {{noticeHtml}}
    {{body}}
  </main>
</body>
</html>
""";
    }

    public static string E(string? value) => WebUtility.HtmlEncode(value ?? "");

    public static string Badge(string text, string kind = "")
    {
        return $"""<span class="badge {E(kind)}">{E(text)}</span>""";
    }

    public static string Badges(IEnumerable<string> values)
    {
        var builder = new StringBuilder("""<div class="badges">""");
        foreach (var value in values)
        {
            var kind = value switch
            {
                "SEM_CATALOGO" or "PRECO_INVALIDO" or "QUANTIDADE_INVALIDA" => "danger",
                "PESAVEL" or "FARDO_CAIXA" => "warn",
                "DUPLICIDADE" => "info",
                _ => ""
            };
            builder.Append(Badge(value, kind));
        }

        builder.Append("</div>");
        return builder.ToString();
    }

    public static string StatusBadge(CampaignItem item)
    {
        if (item.BlockingReasons.Count > 0)
        {
            return Badge("Bloqueado", "danger");
        }

        return item.ReviewStatus switch
        {
            ReviewStatus.Approved => Badge("Aprovado", "ok"),
            ReviewStatus.Pending => Badge("Pendente", "warn"),
            ReviewStatus.Rejected => Badge("Rejeitado", "danger"),
            _ => Badge("Pronto", "ok")
        };
    }
}
