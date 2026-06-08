using ClubeDasOfertas.Web.Domain;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ClubeDasOfertas.Web.Ui;

public static class HtmlView
{
    public static IResult Page(string title, ClaimsPrincipal user, string body, string notice = "", string antiForgeryField = "", string pageClass = "", string headerTitle = "", int statusCode = StatusCodes.Status200OK)
    {
        return Results.Content(Layout(title, user, body, notice, antiForgeryField, pageClass, headerTitle), "text/html; charset=utf-8", statusCode: statusCode);
    }

    public static string Layout(string title, ClaimsPrincipal user, string body, string notice = "", string antiForgeryField = "", string pageClass = "", string headerTitle = "")
    {
        var signedIn = user.Identity?.IsAuthenticated == true;
        var displayName = signedIn ? user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name ?? "" : "";
        var role = signedIn ? user.FindFirstValue(ClaimTypes.Role) ?? "" : "";
        var isCampaignTheme = pageClass.Contains("page-campaign", StringComparison.OrdinalIgnoreCase);
        var brand = isCampaignTheme
            ? $$"""
<details class="menu-shell">
  <summary class="menu-trigger">MENU</summary>
  <div class="menu-popover">
    <a href="/campaigns">Campanhas</a>
    <a href="/catalog">Catálogo</a>
    <a href="/rules">Regras</a>
    <a href="/history">Histórico</a>
    <form method="post" action="/logout">{{antiForgeryField}}<button class="menu-link" type="submit">Sair</button></form>
  </div>
</details>
"""
            : """<a class="brand" href="/campaigns">Clube Das Ofertas</a>""";
        var headerTitleHtml = isCampaignTheme
            ? $"""<div class="header-title">{E(string.IsNullOrWhiteSpace(headerTitle) ? title : headerTitle)}</div>"""
            : "";
        var cornerBrand = isCampaignTheme
            ? """<div class="header-brandmark"><img src="/clube-das-ofertas-preferencial.png" alt="Clube Das Ofertas"></div>"""
            : "";
        var bodyClass = string.IsNullOrWhiteSpace(pageClass) ? "" : $" class=\"{E(pageClass)}\"";
        var nav = signedIn
            ? (isCampaignTheme
                ? $$"""
              <div class="userbox">{{E(displayName)}} <span>{{E(role)}}</span></div>
              """
                : $$"""
              <nav class="nav">
                <a href="/campaigns">Campanhas</a>
                <a href="/catalog">Catálogo</a>
                <a href="/rules">Regras</a>
                <a href="/history">Histórico</a>
                <form method="post" action="/logout">{{antiForgeryField}}<button class="ghost" type="submit">Sair</button></form>
              </nav>
              <div class="userbox">{E(displayName)} <span>{E(role)}</span></div>
              """)
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
    @import url('https://fonts.googleapis.com/css2?family=Montserrat:wght@600;700;800&family=Source+Sans+Pro:wght@400;600;700&display=swap');
    :root {
      --bg: #f3f6fb;
      --panel: #ffffff;
      --text: #18212f;
      --muted: #637083;
      --line: #d7deea;
      --line-soft: #e8edf5;
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
      display: grid;
      grid-template-columns: auto 1fr auto;
      align-items: center;
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
    .brandmark {
      display: inline-flex;
      align-items: center;
      text-decoration: none;
    }
    .brandmark img {
      display: block;
      width: 70px;
      height: 70px;
      object-fit: contain;
    }
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
    .header-title {
      justify-self: center;
      font-size: 18px;
      font-weight: 700;
      letter-spacing: 0.02em;
    }
    .menu-shell {
      position: relative;
      justify-self: start;
    }
    .menu-shell summary {
      list-style: none;
    }
    .menu-shell summary::-webkit-details-marker {
      display: none;
    }
    .menu-trigger {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 38px;
      padding: 8px 14px;
      border: 1px solid currentColor;
      border-radius: 999px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 700;
      letter-spacing: 0.08em;
      user-select: none;
    }
    .menu-popover {
      position: absolute;
      top: calc(100% + 10px);
      left: 0;
      min-width: 180px;
      padding: 10px;
      border: 1px solid var(--line);
      border-radius: 10px;
      background: var(--panel);
      box-shadow: 0 18px 35px rgba(15, 23, 42, 0.18);
      display: grid;
      gap: 4px;
      z-index: 30;
    }
    .menu-popover a,
    .menu-link {
      width: 100%;
      border: 0;
      background: transparent;
      color: inherit;
      text-decoration: none;
      text-align: left;
      font: inherit;
      padding: 10px 12px;
      border-radius: 8px;
      cursor: pointer;
    }
    .menu-popover a:hover,
    .menu-link:hover {
      background: #eef1f4;
    }
    .userbox { color: var(--muted); font-size: 13px; white-space: nowrap; }
    .userbox span { margin-left: 6px; color: var(--brand); font-weight: 700; }
    .header-userzone {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 12px;
    }
    main { padding: 22px 24px 36px; max-width: 1460px; margin: 0 auto; }
    h1 { font-size: 24px; margin: 0 0 16px; }
    h2 { font-size: 17px; margin: 0 0 12px; }
    .grid { display: grid; gap: 16px; }
    .grid.two { grid-template-columns: minmax(320px, 420px) minmax(0, 1fr); align-items: start; }
    .field-grid {
      display: grid;
      gap: 12px;
    }
    .field-grid.two {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .stats { display: grid; grid-template-columns: repeat(6, minmax(120px, 1fr)); gap: 10px; margin-bottom: 14px; }
    .stat, .panel {
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.04);
    }
    .stat strong { display: block; font-size: 22px; }
    .stat span { color: var(--muted); font-size: 12px; }
    .toolbar { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; margin: 12px 0; }
    .toolbar a { text-decoration: none; }
    .toolbar form { margin: 0; }
    .panel-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 16px;
    }
    .panel-header h2 {
      margin-bottom: 0;
    }
    .panel-subtitle {
      margin: 6px 0 0;
      color: var(--muted);
      font-size: 13px;
      line-height: 1.45;
    }
    .hint {
      margin: 6px 0 0;
      color: var(--muted);
      font-size: 12px;
      line-height: 1.4;
    }
    .form-actions {
      display: flex;
      align-items: center;
      gap: 10px;
      flex-wrap: wrap;
      margin-top: 4px;
    }
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
      border-radius: 8px;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.04);
    }
    table { width: 100%; border-collapse: collapse; min-width: 980px; }
    th, td { padding: 9px 10px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; font-size: 13px; }
    th { background: #f4f7fb; font-size: 12px; color: #374151; position: sticky; top: 0; z-index: 2; }
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
    .actions form { margin: 0; }
    .item-edit-row td {
      padding: 0;
      background: #fbfcfe;
    }
    .item-edit-card {
      padding: 16px;
      border-top: 1px solid var(--line-soft);
      background: #fbfcfe;
    }
    .item-edit-grid {
      display: grid;
      gap: 12px;
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
    .item-edit-grid .field {
      margin-bottom: 0;
    }
    .item-edit-grid .field.span-2 {
      grid-column: span 2;
    }
    .item-edit-grid .field.span-3 {
      grid-column: 1 / -1;
    }
    .inline-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      align-items: center;
    }
    .inline-actions .ghost {
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 7px 12px;
      background: #fff;
    }
    .calc-preview {
      padding: 10px 12px;
      border: 1px dashed var(--line);
      border-radius: 8px;
      background: #f8fafc;
      color: var(--text);
      font-size: 13px;
      line-height: 1.5;
    }
    .calc-preview strong {
      display: block;
      margin-bottom: 4px;
      font-size: 13px;
    }
    .campaign-shell {
      display: grid;
      grid-template-columns: minmax(340px, 420px) minmax(0, 1fr);
      gap: 18px;
      align-items: start;
    }
    .catalog-layout {
      display: grid;
      grid-template-columns: minmax(280px, 340px) minmax(0, 1fr);
      gap: 18px;
      align-items: start;
    }
    .catalog-sidebar {
      position: sticky;
      top: 82px;
    }
    .catalog-sidebar-section + .catalog-sidebar-section {
      margin-top: 18px;
      padding-top: 18px;
      border-top: 1px solid var(--line-soft);
    }
    .catalog-summary {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: 12px;
    }
    .catalog-summary strong {
      font-size: 22px;
    }
    .catalog-summary span {
      color: var(--muted);
    }
    .catalog-summary + .catalog-summary {
      margin-top: 10px;
    }
    .catalog-category-list {
      display: grid;
      gap: 8px;
      max-height: 52vh;
      overflow: auto;
      padding-right: 4px;
    }
    .catalog-category {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 10px 12px;
      border: 1px solid var(--line);
      border-radius: 8px;
      text-decoration: none;
      color: inherit;
      background: transparent;
    }
    .catalog-category:hover {
      background: #eef1f4;
    }
    .catalog-category strong {
      font-size: 14px;
    }
    .catalog-category.active {
      border-color: var(--brand);
      background: rgba(15, 118, 110, 0.08);
    }
    .catalog-list {
      display: grid;
      gap: 12px;
    }
    .catalog-list-shell {
      max-height: calc(100vh - 220px);
      overflow-y: auto;
      overflow-x: hidden;
      padding-right: 6px;
    }
    .catalog-item {
      display: grid;
      grid-template-columns: minmax(0, 1.4fr) minmax(300px, 1fr);
      gap: 16px;
      align-items: start;
      padding: 16px;
      border: 1px solid var(--line);
      border-radius: 8px;
      background: rgba(255, 255, 255, 0.02);
    }
    .catalog-item-main strong {
      display: block;
      font-size: 17px;
      line-height: 1.35;
    }
    .catalog-item-main span {
      display: block;
      margin-top: 6px;
      color: var(--muted);
    }
    .catalog-item-meta {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 12px;
    }
    .catalog-item-meta label {
      margin-bottom: 4px;
    }
    .catalog-item-meta span {
      display: block;
      word-break: break-word;
    }
    .campaign-list {
      display: flex;
      flex-direction: column;
    }
    .campaign-list-head,
    .campaign-row {
      display: grid;
      grid-template-columns: minmax(220px, 2.2fr) minmax(160px, 1.3fr) minmax(120px, 1fr) minmax(88px, 0.7fr) minmax(120px, 0.8fr) auto;
      gap: 16px;
      align-items: center;
    }
    .campaign-list-head {
      padding: 0 4px 12px;
      color: var(--muted);
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      border-bottom: 1px solid var(--line);
    }
    .campaign-row {
      padding: 16px 4px;
      border-bottom: 1px solid var(--line-soft);
    }
    .campaign-row:last-child {
      border-bottom: 0;
      padding-bottom: 0;
    }
    .campaign-cell {
      min-width: 0;
    }
    .campaign-cell strong {
      display: block;
      font-size: 15px;
      line-height: 1.35;
    }
    .campaign-cell .muted {
      display: block;
      margin-top: 4px;
    }
    .campaign-metric strong {
      font-size: 18px;
    }
    .campaign-action {
      display: flex;
      justify-content: flex-end;
    }
    .empty-state {
      padding: 18px;
      border: 1px dashed var(--line);
      border-radius: 8px;
      color: var(--muted);
      background: #fbfcfe;
    }
    .login {
      max-width: 420px;
      margin: 70px auto;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 22px;
    }
    .header-brandmark {
      pointer-events: none;
    }
    .header-brandmark img {
      display: block;
      width: min(82px, 6vw);
      min-width: 60px;
      height: auto;
      opacity: 0.88;
    }
    body.page-campaign {
      background: #000000;
      color: #ffffff;
      font-family: "Source Sans Pro", "Segoe UI", Arial, sans-serif;
      font-size: 17px;
    }
    body.page-campaign h1 { font-size: 30px; }
    body.page-campaign h2 { font-size: 21px; }
    body.page-campaign td,
    body.page-campaign input,
    body.page-campaign select,
    body.page-campaign textarea,
    body.page-campaign button,
    body.page-campaign .button,
    body.page-campaign .panel-subtitle,
    body.page-campaign .hint,
    body.page-campaign .notice,
    body.page-campaign .muted {
      font-size: 15px;
    }
    body.page-campaign th,
    body.page-campaign .campaign-list-head,
    body.page-campaign label {
      font-size: 13px;
    }
    body.page-campaign header,
    body.page-campaign .stat,
    body.page-campaign .panel,
    body.page-campaign .tablewrap,
    body.page-campaign .empty-state {
      background: #000000;
      border-color: #2b2b2b;
      box-shadow: none;
    }
    body.page-campaign h1,
    body.page-campaign h2,
    body.page-campaign th,
    body.page-campaign .campaign-list-head,
    body.page-campaign .stat strong,
    body.page-campaign .panel-header,
    body.page-campaign .badge,
    body.page-campaign .button,
    body.page-campaign button {
      font-family: "Montserrat", "Segoe UI", Arial, sans-serif;
    }
    body.page-campaign .header-title,
    body.page-campaign .menu-trigger {
      font-family: "Montserrat", "Segoe UI", Arial, sans-serif;
    }
    body.page-campaign .userbox,
    body.page-campaign .muted,
    body.page-campaign label,
    body.page-campaign input,
    body.page-campaign textarea,
    body.page-campaign select,
    body.page-campaign td,
    body.page-campaign .notice,
    body.page-campaign .hint,
    body.page-campaign .panel-subtitle {
      font-family: "Source Sans Pro", "Segoe UI", Arial, sans-serif;
    }
    body.page-campaign header {
      border-bottom-color: #1f1f1f;
    }
    body.page-campaign .menu-trigger,
    body.page-campaign .menu-link,
    body.page-campaign .menu-popover a,
    body.page-campaign .ghost,
    body.page-campaign .userbox,
    body.page-campaign .userbox span,
    body.page-campaign .muted,
    body.page-campaign label,
    body.page-campaign h1,
    body.page-campaign h2,
    body.page-campaign .header-title,
    body.page-campaign td,
    body.page-campaign th,
    body.page-campaign .panel-subtitle,
    body.page-campaign .hint,
    body.page-campaign .empty-state,
    body.page-campaign .notice {
      color: #ffffff;
    }
    body.page-campaign .menu-popover {
      background: #050505;
      border-color: #2b2b2b;
      box-shadow: 0 18px 40px rgba(0, 0, 0, 0.5);
    }
    body.page-campaign .stat span {
      color: #b8b8b8;
    }
    body.page-campaign .menu-popover a:hover,
    body.page-campaign .menu-link:hover,
    body.page-campaign .ghost:hover,
    body.page-campaign .button.secondary:hover,
    body.page-campaign button.secondary:hover {
      background: #141414;
    }
    body.page-campaign input,
    body.page-campaign select,
    body.page-campaign textarea {
      background: #050505;
      color: #ffffff;
      border-color: #2b2b2b;
    }
    body.page-campaign input[type="date"] {
      color-scheme: dark;
      cursor: pointer;
    }
    body.page-campaign input[type="date"]::-webkit-calendar-picker-indicator {
      cursor: pointer;
    }
    body.page-campaign input::placeholder,
    body.page-campaign textarea::placeholder {
      color: #b8b8b8;
    }
    body.page-campaign th {
      background: #090909;
      color: #ffffff;
      border-bottom-color: #2b2b2b;
    }
    body.page-campaign td {
      border-bottom-color: #1f1f1f;
    }
    body.page-campaign .item-edit-row td,
    body.page-campaign .item-edit-card {
      background: #050505;
      border-color: #2b2b2b;
    }
    body.page-campaign .item-edit-card .form-actions .secondary {
      background: #141414;
      color: #ffffff;
      border-color: #ffffff;
    }
    body.page-campaign .item-edit-card .form-actions .secondary:hover {
      background: #242424;
      border-color: #ffffff;
    }
    body.page-campaign .notice {
      background: #101010;
      border-color: #2b2b2b;
    }
    body.page-campaign .notice.error {
      background: #1a0909;
      border-color: #5a1a1a;
    }
    body.page-campaign .button,
    body.page-campaign button {
      background: #d71912;
      border-color: #d71912;
      color: #ffffff;
    }
    body.page-campaign .button:hover,
    body.page-campaign button:hover {
      background: #b9130f;
      border-color: #b9130f;
    }
    body.page-campaign .button.secondary,
    body.page-campaign button.secondary,
    body.page-campaign .ghost {
      background: transparent;
      color: #ffffff;
      border-color: #ffffff;
    }
    body.page-campaign .badge {
      background: #151515;
      color: #ffffff;
    }
    body.page-campaign .badge.ok {
      background: #0e3d1f;
      color: #ffffff;
    }
    body.page-campaign .badge.warn {
      background: #553f00;
      color: #ffffff;
    }
    body.page-campaign .badge.danger {
      background: #5a1212;
      color: #ffffff;
    }
    body.page-campaign .badge.info {
      background: #102a5a;
      color: #ffffff;
    }
    body.page-campaign .catalog-sidebar-section + .catalog-sidebar-section,
    body.page-campaign .catalog-item,
    body.page-campaign .catalog-category,
    body.page-campaign .calc-preview {
      border-color: #2b2b2b;
    }
    body.page-campaign .calc-preview {
      background: #050505;
      color: #ffffff;
    }
    body.page-campaign .catalog-category:hover {
      background: #141414;
    }
    body.page-campaign .catalog-category.active {
      border-color: #d71912;
      background: rgba(215, 25, 18, 0.18);
    }
    @media (max-width: 900px) {
      header {
        grid-template-columns: 1fr;
        justify-items: start;
        padding: 12px 16px;
      }
      main { padding: 16px; }
      .grid.two { grid-template-columns: 1fr; }
      .field-grid.two { grid-template-columns: 1fr; }
      .stats { grid-template-columns: repeat(2, minmax(120px, 1fr)); }
      .header-title {
        justify-self: start;
        font-size: 16px;
      }
      .userbox {
        white-space: normal;
      }
      .header-userzone {
        justify-content: flex-start;
      }
      .header-brandmark img {
        width: min(76px, 18vw);
        min-width: 56px;
      }
      .campaign-shell { grid-template-columns: 1fr; }
      .catalog-layout { grid-template-columns: 1fr; }
      .catalog-sidebar { position: static; }
      .catalog-list-shell { max-height: none; overflow: visible; padding-right: 0; }
      .catalog-item {
        grid-template-columns: 1fr;
      }
      .catalog-item-meta {
        grid-template-columns: 1fr;
      }
      .campaign-list-head { display: none; }
      .campaign-row {
        grid-template-columns: repeat(2, minmax(0, 1fr));
        padding: 16px;
        border: 1px solid var(--line);
        border-radius: 8px;
        background: #fbfcfe;
      }
      .campaign-row + .campaign-row {
        margin-top: 10px;
      }
      .campaign-row:last-child {
        padding-bottom: 16px;
      }
      .campaign-cell::before {
        content: attr(data-label);
        display: block;
        margin-bottom: 4px;
        color: var(--muted);
        font-size: 11px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }
      .campaign-cell-main,
      .campaign-action {
        grid-column: 1 / -1;
      }
      .campaign-action {
        justify-content: flex-start;
      }
      .item-edit-grid {
        grid-template-columns: 1fr;
      }
      .item-edit-grid .field.span-2,
      .item-edit-grid .field.span-3 {
        grid-column: auto;
      }
    }
  </style>
</head>
<body{{bodyClass}}>
  <header>
    {{brand}}
    {{headerTitleHtml}}
    <div class="header-userzone">
      {{nav}}
      {{cornerBrand}}
    </div>
  </header>
  <main>
    <div id="page-notice">{{noticeHtml}}</div>
    {{body}}
  </main>
  <script>
    (() => {
      const openDatePicker = (input) => {
        if (!input || typeof input.showPicker !== 'function') {
          return;
        }

        try {
          input.showPicker();
        } catch {
        }
      };

      document.addEventListener('pointerdown', (event) => {
        const input = event.target.closest('input[data-date-picker]');
        if (!input) {
          return;
        }

        setTimeout(() => openDatePicker(input), 0);
      });

      document.addEventListener('focusin', (event) => {
        const input = event.target.closest('input[data-date-picker]');
        if (!input) {
          return;
        }

        setTimeout(() => openDatePicker(input), 0);
      });

      const updateSheetOptions = (select, options, selectedValue) => {
        if (!select) {
          return;
        }

        select.replaceChildren();
        for (const option of options) {
          const element = document.createElement('option');
          element.value = option.value;
          element.textContent = option.label;
          if (option.value === selectedValue) {
            element.selected = true;
          }
          select.appendChild(element);
        }
      };

      document.querySelectorAll('form[data-sheet-selector-form]').forEach((form) => {
        const fileInput = form.querySelector('[data-sheet-file]');
        const select = form.querySelector('[data-sheet-select]');
        const hint = form.querySelector('[data-sheet-hint]');
        const token = form.querySelector('input[name="__RequestVerificationToken"]');
        const defaultSheet = select?.value || 'Base Clube - CLT';
        const initialHint = hint?.textContent || '';

        if (!(fileInput instanceof HTMLInputElement) || !(select instanceof HTMLSelectElement)) {
          return;
        }

        const setHint = (message, isError = false) => {
          if (!hint) {
            return;
          }

          hint.textContent = message;
          hint.classList.toggle('error', isError);
        };

        const resetToDefault = () => {
          updateSheetOptions(select, [{ value: defaultSheet, label: defaultSheet }], defaultSheet);
        };

        resetToDefault();

        fileInput.addEventListener('change', async () => {
          const file = fileInput.files?.[0];
          if (!file) {
            resetToDefault();
            setHint(initialHint);
            return;
          }

          const extension = file.name.split('.').pop()?.toLowerCase() || '';
          if (extension === 'csv' || extension === 'txt') {
            updateSheetOptions(select, [{ value: defaultSheet, label: 'Arquivo sem abas (CSV/TXT)' }], defaultSheet);
            setHint('Arquivos CSV ou TXT não possuem abas. A importação segue direto pelo arquivo.');
            return;
          }

          const formData = new FormData();
          formData.append('file', file);
          if (token instanceof HTMLInputElement && token.value) {
            formData.append(token.name, token.value);
          }

          updateSheetOptions(select, [{ value: defaultSheet, label: 'Carregando abas...' }], defaultSheet);
          setHint('Lendo as abas disponíveis da planilha...');

          try {
            const response = await fetch('/worksheets', {
              method: 'POST',
              body: formData,
              headers: {
                'X-Requested-With': 'fetch',
                'Accept': 'application/json'
              }
            });

            const payload = await response.json();
            if (!response.ok || !payload.ok) {
              resetToDefault();
              setHint(payload.notice || 'Nao foi possivel ler as abas desse arquivo.', true);
              return;
            }

            if (!payload.supportsSheets) {
              updateSheetOptions(select, [{ value: defaultSheet, label: 'Arquivo sem abas (CSV/TXT)' }], defaultSheet);
              setHint(payload.notice || 'Esse arquivo não possui abas para selecionar.');
              return;
            }

            const worksheets = Array.isArray(payload.worksheets) ? payload.worksheets : [];
            if (worksheets.length === 0) {
              resetToDefault();
              setHint('Nenhuma aba disponivel foi encontrada nesse arquivo.', true);
              return;
            }

            const selected = payload.defaultSheet || worksheets[0];
            updateSheetOptions(select, worksheets.map((name) => ({ value: name, label: name })), selected);
            setHint('Selecione a aba desejada no menu abaixo.');
          } catch {
            resetToDefault();
            setHint('Erro ao carregar as abas da planilha. Tente novamente.', true);
          }
        });
      });

      const normalizeExpression = (value) =>
        value
          .replace(/[xX×]/g, '*')
          .replace(/÷/g, '/');

      const sanitizeQuantityExpression = (value) =>
        normalizeExpression(stripQuantityWords(value).replace(/[^\d\s,.\-+*/()xX×÷]/g, ''));

      const isQuantityLetter = (char) => /[A-Za-zÀ-ÿ]/.test(char);

      const stripQuantityWords = (value) => {
        const input = (value || '').trim();
        let sanitized = '';
        let currentWord = '';

        const flushWord = () => {
          if (!currentWord) {
            return;
          }

          if (currentWord.length === 1 && (currentWord === 'x' || currentWord === 'X')) {
            sanitized += currentWord;
          } else {
            sanitized += ' ';
          }

          currentWord = '';
        };

        for (const char of input) {
          if (isQuantityLetter(char)) {
            currentWord += char;
            continue;
          }

          flushWord();
          sanitized += char;
        }

        flushWord();
        return sanitized;
      };

      const normalizeNumberToken = (token) => {
        let sanitized = token.trim();
        if (sanitized.includes(',')) {
          sanitized = sanitized.replace(/\./g, '').replace(',', '.');
        }

        return sanitized;
      };

      const evaluateExpression = (value) => {
        const input = normalizeExpression((value || '').trim());
        let index = 0;

        const skipWhitespace = () => {
          while (index < input.length && /\s/.test(input[index])) {
            index += 1;
          }
        };

        const match = (expected) => {
          if (input[index] !== expected) {
            return false;
          }

          index += 1;
          return true;
        };

        const parseNumber = () => {
          skipWhitespace();
          const start = index;
          while (index < input.length && /[\d.,]/.test(input[index])) {
            index += 1;
          }

          if (start === index) {
            throw new Error('missing-number');
          }

          const parsed = Number.parseFloat(normalizeNumberToken(input.slice(start, index)));
          if (!Number.isFinite(parsed)) {
            throw new Error('invalid-number');
          }

          return parsed;
        };

        const parseFactor = () => {
          skipWhitespace();
          if (match('+')) {
            return parseFactor();
          }

          if (match('-')) {
            return -parseFactor();
          }

          if (match('(')) {
            const valueInside = parseExpression();
            skipWhitespace();
            if (!match(')')) {
              throw new Error('missing-closing-parenthesis');
            }

            return valueInside;
          }

          return parseNumber();
        };

        const parseTerm = () => {
          let valueTerm = parseFactor();
          while (true) {
            skipWhitespace();
            if (match('*')) {
              valueTerm *= parseFactor();
              continue;
            }

            if (match('/')) {
              const divisor = parseFactor();
              if (divisor === 0) {
                throw new Error('division-by-zero');
              }

              valueTerm /= divisor;
              continue;
            }

            return valueTerm;
          }
        };

        const parseExpression = () => {
          let valueExpression = parseTerm();
          while (true) {
            skipWhitespace();
            if (match('+')) {
              valueExpression += parseTerm();
              continue;
            }

            if (match('-')) {
              valueExpression -= parseTerm();
              continue;
            }

            return valueExpression;
          }
        };

        if (!input) {
          return null;
        }

        const valueResult = parseExpression();
        skipWhitespace();
        if (index !== input.length) {
          throw new Error('trailing-content');
        }

        return valueResult;
      };

      const inferQuantityUnit = (value, fallback) => {
        const normalized = (value || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toUpperCase();
        if (normalized.includes('KG')) {
          return 'Kg';
        }

        if (normalized.includes('FARDO') || normalized.includes('FARDOS') || normalized.includes('FD')) {
          return 'Fardos';
        }

        if (normalized.includes('CAIXA') || normalized.includes('CAIXAS') || normalized.includes('CX')) {
          return 'Caixas';
        }

        return fallback || 'Unidades';
      };

      const moneyFormatter = new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      });

      const quantityFormatter = new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 3
      });

      document.querySelectorAll('form[data-package-math-form]').forEach((form) => {
        const quantityInput = form.querySelector('input[name="quantity_raw"]');
        const saleInput = form.querySelector('input[name="price_sale"]');
        const clubInput = form.querySelector('input[name="price_club"]');
        const preview = form.querySelector('[data-calc-preview]');

        if (!(quantityInput instanceof HTMLInputElement) ||
            !(saleInput instanceof HTMLInputElement) ||
            !(clubInput instanceof HTMLInputElement) ||
            !(preview instanceof HTMLElement)) {
          return;
        }

        const renderPreview = () => {
          preview.hidden = false;
          const quantityValue = quantityInput.value.trim();
          const saleValue = saleInput.value.trim();
          const clubValue = clubInput.value.trim();
          const defaultPreviewHtml = `<strong>Preview da conta</strong><div>Digite a quantidade ou os preços para visualizar o resultado antes de salvar.</div>`;

          if (!quantityValue && !saleValue && !clubValue) {
            preview.innerHTML = defaultPreviewHtml;
            return;
          }

          const lines = [];

          try {
            if (quantityValue) {
              if (!/\d/.test(quantityValue)) {
                const unit = inferQuantityUnit(quantityValue, quantityInput.dataset.previewUnit || 'Unidades');
                lines.push(`Quantidade: 1 ${unit}`);
              } else {
                const quantityResult = evaluateExpression(sanitizeQuantityExpression(quantityValue));
                if (quantityResult !== null) {
                  const unit = inferQuantityUnit(quantityValue, quantityInput.dataset.previewUnit || 'Unidades');
                  lines.push(`Quantidade: ${quantityFormatter.format(quantityResult)} ${unit}`);
                }
              }
            }
          } catch {
            lines.push('Quantidade: conta invalida');
          }

          try {
            if (saleValue) {
              const saleResult = evaluateExpression(saleValue);
              if (saleResult !== null) {
                lines.push(`Preco venda: ${moneyFormatter.format(saleResult)}`);
              }
            }
          } catch {
            lines.push('Preco venda: conta invalida');
          }

          try {
            if (clubValue) {
              const clubResult = evaluateExpression(clubValue);
              if (clubResult !== null) {
                lines.push(`Preco clube: ${moneyFormatter.format(clubResult)}`);
              }
            }
          } catch {
            lines.push('Preco clube: conta invalida');
          }

          if (lines.length === 0) {
            preview.innerHTML = defaultPreviewHtml;
            return;
          }

          preview.hidden = false;
          preview.innerHTML = `<strong>Preview da conta</strong>${lines.map((line) => `<div>${line}</div>`).join('')}`;
        };

        quantityInput.addEventListener('input', renderPreview);
        saleInput.addEventListener('input', renderPreview);
        clubInput.addEventListener('input', renderPreview);
        renderPreview();
      });

      document.addEventListener('click', (event) => {
        const toggle = event.target.closest('[data-edit-toggle]');
        if (toggle) {
          const target = document.getElementById(toggle.dataset.editToggle);
          if (!target) {
            return;
          }

          target.hidden = !target.hidden;
          if (!target.hidden) {
            target.querySelector('input[name="quantity_raw"]')?.dispatchEvent(new Event('input', { bubbles: true }));
            target.querySelector('input, select, textarea, button')?.focus();
          }
          return;
        }

        const close = event.target.closest('[data-edit-close]');
        if (close) {
          const target = document.getElementById(close.dataset.editClose);
          if (target) {
            target.hidden = true;
          }
        }
      });
    })();
  </script>
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

    public static string CampaignStatusBadge(string status)
    {
        return status switch
        {
            CampaignStatus.Draft => Badge("Rascunho", "info"),
            CampaignStatus.Imported => Badge("Importado", "warn"),
            CampaignStatus.Exported => Badge("Exportado", "ok"),
            _ => Badge(status)
        };
    }
}
