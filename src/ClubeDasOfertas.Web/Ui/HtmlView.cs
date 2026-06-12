using ClubeDasOfertas.Web.Domain;
using ClubeDasOfertas.Web.Services;
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
        var displayRole = signedIn ? DisplayRole(role) : "";
        var isCampaignTheme = pageClass.Contains("page-campaign", StringComparison.OrdinalIgnoreCase);
        var brand = isCampaignTheme
            ? $$"""
<details class="menu-shell">
  <summary class="menu-trigger">MENU</summary>
  <div class="menu-popover">
    <a href="/campaigns">Campanhas</a>
    <a href="/catalog">Catálogo de produtos</a>
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
        var footerBrand = """<div class="footer-brandmark"><img src="/clube-das-ofertas-secundaria.png" alt="Clube Das Ofertas"></div>""";
        var bodyClass = string.IsNullOrWhiteSpace(pageClass) ? "" : $" class=\"{E(pageClass)}\"";
        var nav = signedIn
            ? (isCampaignTheme
                ? $$"""
              <div class="userbox">{{E(displayName)}} <span>{{E(displayRole)}}</span></div>
              """
                : $$"""
              <nav class="nav">
                <a href="/campaigns">Campanhas</a>
                <a href="/catalog">Catálogo de produtos</a>
                <a href="/rules">Regras</a>
                <a href="/history">Histórico</a>
                <form method="post" action="/logout">{{antiForgeryField}}<button class="ghost" type="submit">Sair</button></form>
              </nav>
              <div class="userbox">{E(displayName)} <span>{E(displayRole)}</span></div>
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
      --bg: #f5f1e8;
      --panel: #ffffff;
      --text: #161616;
      --muted: #5f5a4f;
      --line: #e8c93a;
      --line-soft: #f4e7a0;
      --brand: #a21815;
      --brand-strong: #950000;
      --brand-soft: #fff6a8;
      --brand-soft-strong: #ffed00;
      --brand-black: #000000;
      --brand-black-soft: #121212;
      --brand-white: #ffffff;
      --warn: #8a5d00;
      --danger: #a21815;
      --ok: #5b3d00;
      --info: #000000;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: "Segoe UI", Arial, sans-serif;
      color: var(--text);
      background: linear-gradient(180deg, #fffdf6 0%, var(--bg) 100%);
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
      background: linear-gradient(180deg, var(--brand-black-soft) 0%, var(--brand-black) 100%);
      position: sticky;
      top: 0;
      z-index: 10;
    }
    .brand {
      font-size: 18px;
      font-weight: 700;
      white-space: nowrap;
      color: var(--brand-soft-strong);
      text-decoration: none;
    }
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
      color: var(--brand-white);
      font: inherit;
      text-decoration: none;
      padding: 8px 10px;
      border-radius: 6px;
      cursor: pointer;
    }
    .nav a:hover, .ghost:hover {
      background: rgba(255, 237, 0, 0.18);
      color: var(--brand-soft-strong);
    }
    .header-title {
      justify-self: center;
      font-size: 18px;
      font-weight: 700;
      letter-spacing: 0.02em;
      color: var(--brand-white);
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
      color: var(--brand-soft-strong);
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
      box-shadow: 0 18px 35px rgba(0, 0, 0, 0.14);
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
      background: var(--brand-soft);
    }
    .userbox { color: rgba(255, 255, 255, 0.82); font-size: 13px; white-space: nowrap; }
    .userbox span { margin-left: 6px; color: var(--brand-soft-strong); font-weight: 700; }
    .header-userzone {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 12px;
    }
    main { padding: 22px 24px 96px; max-width: 1460px; margin: 0 auto; }
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
      box-shadow: 0 12px 28px rgba(0, 0, 0, 0.08);
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
      background: #fff8c5;
      border: 1px solid var(--line);
      border-radius: 6px;
      color: #5b3d00;
    }
    .error { background: #ffe5e2; border-color: #d78680; color: var(--danger); }
    label { display: block; font-size: 13px; color: var(--muted); margin: 0 0 5px; }
    input, select, textarea {
      width: 100%;
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 9px 10px;
      font: inherit;
      background: #fffef8;
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
    .button.secondary, button.secondary {
      background: var(--brand-soft-strong);
      color: var(--brand-black);
      border-color: var(--brand-soft-strong);
    }
    .button.secondary:hover, button.secondary:hover { background: var(--brand-soft); border-color: var(--line); }
    .button.danger, button.danger { background: var(--danger); border-color: var(--danger); }
    .tablewrap {
      overflow: auto;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 8px;
      box-shadow: 0 12px 28px rgba(0, 0, 0, 0.08);
    }
    table { width: 100%; border-collapse: collapse; min-width: 980px; }
    th, td { padding: 9px 10px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; font-size: 13px; }
    th { background: var(--brand-soft-strong); font-size: 12px; color: var(--brand-black); position: sticky; top: 0; z-index: 2; }
    tr:last-child td { border-bottom: 0; }
    .muted { color: var(--muted); }
    .mono { font-family: Consolas, "Courier New", monospace; }
    .badges { display: flex; gap: 4px; flex-wrap: wrap; }
    .badge { display: inline-flex; border-radius: 999px; padding: 3px 7px; font-size: 12px; font-weight: 700; background: #fff8c5; color: var(--text); }
    .badge.ok { background: var(--brand-soft-strong); color: var(--brand-black); }
    .badge.warn { background: #ffe38d; color: var(--warn); }
    .badge.danger { background: var(--danger); color: var(--brand-white); }
    .badge.info { background: var(--brand-black); color: var(--brand-white); }
    .actions { display: flex; gap: 6px; align-items: center; flex-wrap: wrap; min-width: 220px; }
    .actions form { margin: 0; }
    .item-edit-row td {
      padding: 0;
      background: #fffdf8;
    }
    .item-edit-card {
      padding: 16px;
      border-top: 1px solid var(--line-soft);
      background: #fffdf3;
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
    .campaign-vigency-note {
      margin-top: -6px;
      margin-bottom: 16px;
    }
    .campaign-price-stack {
      display: grid;
      gap: 4px;
    }
    .campaign-price-stack-compact {
      white-space: nowrap;
    }
    .campaign-description-stack {
      display: grid;
      gap: 10px;
    }
    .campaign-item-actions {
      margin-top: 0;
      padding-top: 0;
      margin-left: 0;
      gap: 6px;
      flex-wrap: nowrap;
      align-items: stretch;
      justify-content: flex-start;
    }
    .campaign-item-actions form {
      margin: 0;
    }
    .campaign-item-actions button,
    .campaign-item-actions .button {
      padding: 6px 10px;
    }
    .inline-actions .ghost {
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 7px 12px;
      background: #fffef8;
    }
    .calc-preview {
      padding: 10px 12px;
      border: 1px dashed var(--line);
      border-radius: 8px;
      background: #fff9de;
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
    .catalog-main-column {
      display: grid;
      gap: 24px;
      align-content: start;
      min-width: 0;
      margin-top: 0;
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
      border-left: 4px solid var(--category-accent, var(--line));
      border-radius: 8px;
      text-decoration: none;
      color: inherit;
      background: transparent;
    }
    .catalog-category-all {
      --category-accent: #5f6368;
      margin-bottom: 6px;
      background: #fffef8;
    }
    .catalog-category-name {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      min-width: 0;
      font-weight: 600;
    }
    .catalog-category-dot {
      width: 10px;
      height: 10px;
      border-radius: 999px;
      flex: 0 0 auto;
      background: var(--category-accent, var(--brand));
      box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.08);
    }
    .catalog-category:hover {
      background: var(--brand-soft);
    }
    .catalog-category strong {
      font-size: 14px;
    }
    .catalog-category.active {
      border-color: var(--brand);
      background: rgba(255, 237, 0, 0.28);
    }
    .catalog-category-all.active {
      border-color: #5f6368;
      background: rgba(95, 99, 104, 0.12);
    }
    .catalog-list {
      display: grid;
      gap: 12px;
    }
    .catalog-search-form {
      margin-bottom: 18px;
    }
    .catalog-results-panel {
      display: flex;
      flex-direction: column;
      height: auto;
      min-height: 0;
      overflow: visible;
      margin-top: -3px;
    }
    .catalog-list-shell {
      max-height: 52vh;
      overflow-y: scroll;
      overflow-x: hidden;
      overscroll-behavior: contain;
      scrollbar-gutter: stable;
      padding-right: 8px;
    }
    .catalog-chart-layout {
      display: grid;
      grid-template-columns: minmax(180px, 220px) minmax(0, 1fr);
      gap: 22px;
      align-items: center;
    }
    .catalog-chart-visual-wrap {
      position: relative;
      width: min(220px, 100%);
      aspect-ratio: 1;
      margin: 0 auto;
    }
    .catalog-chart-visual {
      width: 100%;
      height: 100%;
      display: block;
      overflow: visible;
      filter: drop-shadow(0 14px 28px rgba(0, 0, 0, 0.12));
    }
    .catalog-chart-segment {
      fill: none;
      stroke-linecap: butt;
      transform: rotate(-90deg);
      transform-origin: 100px 100px;
      transition: opacity 0.18s ease, filter 0.18s ease;
      cursor: pointer;
    }
    .catalog-chart-segment:hover,
    .catalog-chart-segment:focus-visible {
      opacity: 0.92;
      filter: brightness(1.03);
      outline: none;
    }
    .catalog-chart-total {
      position: absolute;
      inset: 0;
      display: grid;
      place-content: center;
      text-align: center;
      z-index: 1;
      pointer-events: none;
    }
    .catalog-chart-total strong {
      display: block;
      font-size: 30px;
      line-height: 1;
    }
    .catalog-chart-total span {
      display: block;
      margin-top: 6px;
      color: var(--muted);
      font-size: 13px;
    }
    .catalog-chart-legend {
      display: grid;
      gap: 10px;
      max-height: 220px;
      overflow: auto;
      padding-right: 4px;
    }
    .catalog-chart-row {
      display: grid;
      grid-template-columns: 14px minmax(0, 1fr) auto auto;
      gap: 10px;
      align-items: center;
      padding: 10px 12px;
      border: 1px solid var(--line-soft);
      border-radius: 10px;
      background: #fffef8;
      cursor: pointer;
    }
    .catalog-chart-swatch {
      width: 14px;
      height: 14px;
      border-radius: 999px;
      box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.08);
    }
    .catalog-chart-label {
      min-width: 0;
      font-weight: 700;
      word-break: break-word;
    }
    .catalog-chart-share {
      color: var(--muted);
      white-space: nowrap;
    }
    .catalog-chart-tooltip {
      display: none;
      position: fixed;
      left: 0;
      top: 0;
      transform: translate(-50%, calc(-100% - 12px));
      min-width: 180px;
      max-width: min(280px, calc(100vw - 24px));
      padding: 10px 12px;
      border: 1px solid var(--line);
      border-radius: 10px;
      background: var(--panel);
      color: var(--text);
      box-shadow: 0 14px 30px rgba(0, 0, 0, 0.16);
      z-index: 125;
      pointer-events: none;
    }
    .catalog-chart-tooltip strong,
    .catalog-chart-tooltip span {
      display: block;
    }
    .catalog-chart-tooltip span {
      margin-top: 5px;
      color: var(--muted);
      font-size: 12px;
    }
    .catalog-chart-tooltip[data-open="true"] {
      display: block;
    }
    .catalog-item {
      display: grid;
      grid-template-columns: minmax(0, 1.4fr) minmax(300px, 1fr);
      gap: 16px;
      align-items: start;
      padding: 16px;
      border: 1px solid var(--line);
      border-left: 4px solid var(--category-accent, var(--line));
      border-radius: 8px;
      background: #fffef8;
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
    .catalog-category-pill {
      display: inline-flex;
      align-items: center;
      width: fit-content;
      padding: 4px 10px;
      border-radius: 999px;
      background: color-mix(in srgb, var(--category-accent, var(--brand)) 16%, #fffef8);
      color: color-mix(in srgb, var(--category-accent, var(--text)) 72%, #2b2418);
      font-weight: 700;
      box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--category-accent, var(--line)) 32%, transparent);
    }
    .campaign-list {
      display: flex;
      flex-direction: column;
    }
    .campaign-toolbar {
      justify-content: space-between;
      align-items: center;
    }
    .campaign-search-form {
      display: flex;
      gap: 8px;
      align-items: center;
      flex-wrap: wrap;
      margin-right: auto;
    }
    .campaign-search-form input {
      width: min(320px, 100%);
      min-width: 220px;
    }
    .campaign-list-head,
    .campaign-row {
      display: grid;
      grid-template-columns: minmax(164px, 1.95fr) minmax(118px, 1fr) minmax(82px, 0.64fr) minmax(54px, 0.38fr) minmax(88px, 0.58fr) minmax(102px, 0.76fr);
      gap: 8px;
      align-items: center;
    }
    .campaign-list-head {
      padding: 0 2px 10px;
      color: var(--muted);
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      border-bottom: 1px solid var(--line);
    }
    .campaign-row {
      padding: 14px 2px;
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
      font-size: 14px;
      line-height: 1.3;
    }
    .campaign-cell .muted {
      display: block;
      margin-top: 3px;
      font-size: 12px;
      line-height: 1.3;
    }
    .campaign-metric strong {
      font-size: 16px;
    }
    .campaign-list .badge {
      padding: 2px 6px;
      font-size: 12px;
    }
    .campaign-action {
      display: flex;
      justify-content: flex-end;
    }
    .campaign-action .inline-actions {
      justify-content: flex-end;
      gap: 6px;
    }
    .campaign-action .button,
    .campaign-action button {
      min-height: 32px;
      padding: 5px 8px;
      font-size: 13px;
    }
    .rules-tablewrap table {
      min-width: 0;
      table-layout: fixed;
    }
    .rules-tablewrap th:nth-child(1) { width: 88px; }
    .rules-tablewrap th:nth-child(2) { width: 154px; }
    .rules-tablewrap th:nth-child(4) { width: 118px; }
    .rules-tablewrap th:nth-child(5) { width: 188px; }
    .rule-pattern-cell {
      position: relative;
      overflow: visible;
      white-space: nowrap;
      line-height: 1.45;
      cursor: help;
    }
    .rule-pattern-preview {
      display: block;
      max-width: 100%;
    }
    .rule-pattern-text {
      display: block;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .rule-pattern-overlay {
      display: none;
      position: fixed;
      left: 0;
      top: 0;
      width: max-content;
      transform: translate(-50%, -50%);
      gap: 6px;
      min-width: min(360px, calc(100vw - 24px));
      max-width: min(560px, calc(100vw - 24px));
      padding: 10px 12px;
      border: 1px solid var(--line);
      border-radius: 10px;
      background: var(--panel);
      color: var(--text);
      box-shadow: 0 14px 30px rgba(0, 0, 0, 0.16);
      white-space: normal;
      word-break: break-word;
      z-index: 120;
      pointer-events: none;
    }
    .rule-pattern-overlay strong,
    .rule-pattern-overlay span {
      display: block;
    }
    .rule-pattern-overlay span {
      margin-top: 6px;
      font-size: 11px;
      opacity: 0.82;
    }
    .rule-pattern-overlay[data-open="true"] {
      display: grid;
      transform: translate(-50%, -50%);
    }
    .empty-state {
      padding: 18px;
      border: 1px dashed var(--line);
      border-radius: 8px;
      color: var(--muted);
      background: #fffdf3;
    }
    .login {
      max-width: 420px;
      margin: 70px auto;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 6px;
      padding: 22px;
    }
    .footer-brandmark {
      position: fixed;
      right: 4px;
      bottom: 6px;
      z-index: 25;
      pointer-events: none;
    }
    .footer-brandmark img {
      display: block;
      width: min(82px, 6vw);
      min-width: 60px;
      height: auto;
      opacity: 0.88;
    }
    body.page-campaign {
      background: linear-gradient(180deg, #fffdf6 0%, var(--bg) 100%);
      color: var(--text);
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
      font-size: 12px;
    }
    body.page-campaign .campaign-list-head {
      font-size: 12px;
    }
    body.page-campaign .campaign-cell .muted {
      font-size: 12px;
    }
    body.page-campaign .campaign-list .badge {
      font-size: 12px;
    }
    body.page-campaign .campaign-action .button,
    body.page-campaign .campaign-action button {
      font-size: 13px;
    }
    body.page-campaign .stat,
    body.page-campaign .panel,
    body.page-campaign .tablewrap,
    body.page-campaign .empty-state {
      background: rgba(255, 255, 255, 0.94);
      border-color: var(--line);
      box-shadow: 0 16px 30px rgba(0, 0, 0, 0.10);
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
      background: linear-gradient(180deg, var(--brand-black-soft) 0%, var(--brand-black) 100%);
      border-bottom-color: var(--line);
    }
    body.page-campaign .menu-trigger,
    body.page-campaign .menu-link,
    body.page-campaign .menu-popover a,
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
      color: var(--text);
    }
    body.page-campaign .nav a,
    body.page-campaign .ghost,
    body.page-campaign .userbox {
      color: rgba(255, 255, 255, 0.82);
    }
    body.page-campaign .brand,
    body.page-campaign .header-title,
    body.page-campaign .menu-trigger,
    body.page-campaign .userbox span {
      color: var(--brand-soft-strong);
    }
    body.page-campaign .menu-popover {
      background: #fffef8;
      border-color: var(--line);
      box-shadow: 0 18px 40px rgba(0, 0, 0, 0.14);
    }
    body.page-campaign .stat span {
      color: var(--muted);
    }
    body.page-campaign .menu-popover a:hover,
    body.page-campaign .menu-link:hover,
    body.page-campaign .ghost:hover,
    body.page-campaign .button.secondary:hover,
    body.page-campaign button.secondary:hover,
    body.page-campaign .nav a:hover {
      background: rgba(255, 237, 0, 0.18);
    }
    body.page-campaign input,
    body.page-campaign select,
    body.page-campaign textarea {
      background: #fffdf7;
      color: var(--text);
      border-color: var(--line);
    }
    body.page-campaign input[type="date"] {
      color-scheme: light;
      cursor: pointer;
    }
    body.page-campaign input[type="date"]::-webkit-calendar-picker-indicator {
      cursor: pointer;
    }
    body.page-campaign input::placeholder,
    body.page-campaign textarea::placeholder {
      color: #8f8165;
    }
    body.page-campaign th {
      background: var(--brand-soft-strong);
      color: var(--brand-black);
      border-bottom-color: var(--line);
    }
    body.page-campaign td {
      border-bottom-color: var(--line-soft);
    }
    body.page-campaign .item-edit-row td,
    body.page-campaign .item-edit-card {
      background: #fffdf3;
      border-color: var(--line);
    }
    body.page-campaign .item-edit-card .form-actions .secondary {
      background: #fffef8;
      color: var(--brand);
      border-color: var(--brand);
    }
    body.page-campaign .item-edit-card .form-actions .secondary:hover {
      background: var(--brand-soft);
      border-color: var(--brand);
    }
    body.page-campaign .notice {
      background: #fff8c5;
      border-color: var(--line);
    }
    body.page-campaign .notice.error {
      background: #ffe5e2;
      border-color: #d78680;
    }
    body.page-campaign .button,
    body.page-campaign button {
      background: var(--brand);
      border-color: var(--brand);
      color: #ffffff;
    }
    body.page-campaign .button:hover,
    body.page-campaign button:hover {
      background: var(--brand-strong);
      border-color: var(--brand-strong);
    }
    body.page-campaign .button.secondary,
    body.page-campaign button.secondary,
    body.page-campaign .ghost {
      background: var(--brand-soft-strong);
      color: var(--brand-black);
      border-color: var(--brand-soft-strong);
    }
    body.page-campaign .badge {
      background: #fff8c5;
      color: var(--text);
    }
    body.page-campaign .badge.ok {
      background: var(--brand-soft-strong);
      color: var(--brand-black);
    }
    body.page-campaign .badge.warn {
      background: #ffe38d;
      color: var(--warn);
    }
    body.page-campaign .badge.danger {
      background: var(--danger);
      color: var(--brand-white);
    }
    body.page-campaign .badge.info {
      background: var(--brand-black);
      color: var(--brand-white);
    }
    body.page-campaign .catalog-sidebar-section + .catalog-sidebar-section,
    body.page-campaign .catalog-item,
    body.page-campaign .catalog-category,
    body.page-campaign .calc-preview {
      border-color: var(--line);
    }
    body.page-campaign .calc-preview {
      background: #fff9de;
      color: var(--text);
    }
    body.page-campaign .catalog-category:hover {
      background: var(--brand-soft);
    }
    body.page-campaign .catalog-category.active {
      border-color: var(--brand);
      background: rgba(255, 237, 0, 0.32);
    }
    body.page-campaign [data-campaign-detail] th,
    body.page-campaign [data-campaign-detail] td {
      white-space: nowrap;
    }
    body.page-campaign [data-campaign-detail] .campaign-description-cell {
      white-space: normal;
      min-width: 240px;
      line-height: 1.45;
      word-break: break-word;
    }
    body.page-campaign [data-campaign-detail] .campaign-risks-cell {
      white-space: normal;
      min-width: 180px;
    }
    body.page-campaign [data-campaign-detail] .campaign-risks-cell .badges {
      flex-wrap: wrap;
    }
    body.page-campaign [data-campaign-detail] .badges {
      flex-wrap: nowrap;
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
      .footer-brandmark img {
        width: min(76px, 18vw);
        min-width: 56px;
      }
      .campaign-shell { grid-template-columns: 1fr; }
      .catalog-layout { grid-template-columns: 1fr; }
      .catalog-main-column { gap: 18px; margin-top: 0; }
      .catalog-results-panel { margin-top: 0; }
      .catalog-sidebar { position: static; }
      .catalog-results-panel { height: auto; min-height: 0; overflow: visible; }
      .catalog-list-shell { max-height: none; overflow: visible; padding-right: 0; }
      .catalog-chart-layout { grid-template-columns: 1fr; }
      .catalog-chart-legend { max-height: none; overflow: visible; padding-right: 0; }
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
      background: #fffdf3;
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
    </div>
  </header>
  <main>
    <div id="page-notice">{{noticeHtml}}</div>
    {{body}}
  </main>
  {{footerBrand}}
  <div id="rule-pattern-tooltip" class="rule-pattern-overlay" aria-hidden="true" data-open="false"></div>
  <div id="catalog-chart-tooltip" class="catalog-chart-tooltip" aria-hidden="true" data-open="false"></div>
  <script>
    (() => {
      const rulePatternTooltip = document.getElementById('rule-pattern-tooltip');
      const catalogChartTooltip = document.getElementById('catalog-chart-tooltip');
      let activeRulePatternCell = null;
      let activeCatalogTooltipTarget = null;

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

      const hideRulePatternTooltip = () => {
        if (!rulePatternTooltip) {
          return;
        }

        activeRulePatternCell = null;
        rulePatternTooltip.dataset.open = 'false';
        rulePatternTooltip.style.visibility = '';
        rulePatternTooltip.replaceChildren();
      };

      const positionRulePatternTooltip = (cell) => {
        if (!rulePatternTooltip) {
          return;
        }

        const preview = cell.querySelector('.rule-pattern-preview');
        const source = cell.querySelector('.rule-pattern-tooltip-source');
        if (!(preview instanceof HTMLElement) || !(source instanceof HTMLTemplateElement)) {
          hideRulePatternTooltip();
          return;
        }

        rulePatternTooltip.replaceChildren(source.content.cloneNode(true));
        rulePatternTooltip.dataset.open = 'true';
        rulePatternTooltip.style.visibility = 'hidden';

        const previewRect = preview.getBoundingClientRect();
        const centerX = previewRect.left + previewRect.width / 2;
        const centerY = previewRect.top + previewRect.height / 2;
        rulePatternTooltip.style.left = `${centerX}px`;
        rulePatternTooltip.style.top = `${centerY}px`;

        const tooltipRect = rulePatternTooltip.getBoundingClientRect();
        const margin = 12;
        let left = centerX;
        let top = centerY;

        if (tooltipRect.left < margin) {
          left += margin - tooltipRect.left;
        } else if (tooltipRect.right > window.innerWidth - margin) {
          left -= tooltipRect.right - (window.innerWidth - margin);
        }

        if (tooltipRect.top < margin) {
          top += margin - tooltipRect.top;
        } else if (tooltipRect.bottom > window.innerHeight - margin) {
          top -= tooltipRect.bottom - (window.innerHeight - margin);
        }

        rulePatternTooltip.style.left = `${left}px`;
        rulePatternTooltip.style.top = `${top}px`;
        rulePatternTooltip.style.visibility = 'visible';
        activeRulePatternCell = cell;
      };

      const refreshRulePatternTooltip = () => {
        if (activeRulePatternCell) {
          positionRulePatternTooltip(activeRulePatternCell);
        }
      };

      const hideCatalogChartTooltip = () => {
        if (!catalogChartTooltip) {
          return;
        }

        activeCatalogTooltipTarget = null;
        catalogChartTooltip.dataset.open = 'false';
        catalogChartTooltip.style.visibility = '';
        catalogChartTooltip.replaceChildren();
      };

      const positionCatalogChartTooltip = (target) => {
        if (!catalogChartTooltip) {
          return;
        }

        const title = target.getAttribute('data-chart-tooltip-title');
        const meta = target.getAttribute('data-chart-tooltip-meta');
        if (!title || !meta) {
          hideCatalogChartTooltip();
          return;
        }

        const strong = document.createElement('strong');
        strong.textContent = title;
        const span = document.createElement('span');
        span.textContent = meta;
        catalogChartTooltip.replaceChildren(strong, span);
        catalogChartTooltip.dataset.open = 'true';
        catalogChartTooltip.style.visibility = 'hidden';

        const rect = target.getBoundingClientRect();
        let left = rect.left + rect.width / 2;
        let top = rect.top;
        catalogChartTooltip.style.left = `${left}px`;
        catalogChartTooltip.style.top = `${top}px`;

        const tooltipRect = catalogChartTooltip.getBoundingClientRect();
        const margin = 12;
        if (tooltipRect.left < margin) {
          left += margin - tooltipRect.left;
        } else if (tooltipRect.right > window.innerWidth - margin) {
          left -= tooltipRect.right - (window.innerWidth - margin);
        }

        if (tooltipRect.top < margin) {
          top = rect.bottom + 12;
          catalogChartTooltip.style.transform = 'translate(-50%, 0)';
        } else {
          catalogChartTooltip.style.transform = 'translate(-50%, calc(-100% - 12px))';
        }

        catalogChartTooltip.style.left = `${left}px`;
        catalogChartTooltip.style.top = `${top}px`;
        catalogChartTooltip.style.visibility = 'visible';
        activeCatalogTooltipTarget = target;
      };

      const refreshCatalogChartTooltip = () => {
        if (activeCatalogTooltipTarget) {
          positionCatalogChartTooltip(activeCatalogTooltipTarget);
        }
      };

      document.querySelectorAll('.rule-pattern-cell').forEach((cell) => {
        cell.addEventListener('pointerenter', () => positionRulePatternTooltip(cell));
        cell.addEventListener('pointerleave', hideRulePatternTooltip);
        cell.addEventListener('focusin', () => positionRulePatternTooltip(cell));
        cell.addEventListener('focusout', (event) => {
          if (!cell.contains(event.relatedTarget)) {
            hideRulePatternTooltip();
          }
        });
      });

      document.querySelectorAll('[data-chart-tooltip-title]').forEach((target) => {
        target.addEventListener('pointerenter', () => positionCatalogChartTooltip(target));
        target.addEventListener('pointerleave', hideCatalogChartTooltip);
        target.addEventListener('focusin', () => positionCatalogChartTooltip(target));
        target.addEventListener('focusout', (event) => {
          if (!target.contains?.(event.relatedTarget)) {
            hideCatalogChartTooltip();
          }
        });
      });

      document.addEventListener('scroll', refreshRulePatternTooltip, true);
      document.addEventListener('scroll', refreshCatalogChartTooltip, true);
      window.addEventListener('resize', refreshRulePatternTooltip);
      window.addEventListener('resize', refreshCatalogChartTooltip);
      document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
          hideRulePatternTooltip();
          hideCatalogChartTooltip();
        }
      });

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
              setHint(payload.notice || 'Não foi possível ler as abas desse arquivo.', true);
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
              setHint('Nenhuma aba disponível foi encontrada nesse arquivo.', true);
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
            lines.push('Quantidade: conta inválida');
          }

          try {
            if (saleValue) {
              const saleResult = evaluateExpression(saleValue);
              if (saleResult !== null) {
                lines.push(`Preço venda: ${moneyFormatter.format(saleResult)}`);
              }
            }
          } catch {
            lines.push('Preço venda: conta inválida');
          }

          try {
            if (clubValue) {
              const clubResult = evaluateExpression(clubValue);
              if (clubResult !== null) {
                lines.push(`Preço clube: ${moneyFormatter.format(clubResult)}`);
              }
            }
          } catch {
            lines.push('Preço clube: conta inválida');
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
            builder.Append(Badge(DisplayBadgeText(value), BadgeKind(value)));
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

    public static string SourceBadge(string source)
    {
        var normalized = source?.Trim() ?? "";
        var kind = normalized switch
        {
            "App" or "Aplicativo" => "info",
            "Tabloide" => "warn",
            "" => "",
            _ => "ok"
        };

        var displaySource = normalized switch
        {
            "App" => "Aplicativo",
            _ => string.IsNullOrWhiteSpace(normalized) ? "Sem fonte" : normalized
        };

        return Badge(displaySource, kind);
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

    private static string BadgeKind(string value)
    {
        return TextNormalizer.NormalizeKey(value) switch
        {
            "SEM_CATALOGO" or "PRECO_INVALIDO" or "QUANTIDADE_INVALIDA" => "danger",
            "PRODUTO SEM CATALOGO/CODIGO" or "PRECO INVALIDO OU ZERADO" or "QUANTIDADE INVALIDA" => "danger",
            "PESAVEL" or "FARDO_CAIXA" or "FARDO" or "CAIXA" => "warn",
            "CONVERSAO DE PESAVEL PENDENTE" or "FARDO PENDENTE" or "CAIXA PENDENTE" or "FARDO/CAIXA PENDENTE" => "warn",
            "REVISAO REJEITADA" => "danger",
            "DUPLICIDADE" => "info",
            _ => ""
        };
    }

    private static string DisplayBadgeText(string value)
    {
        return TextNormalizer.NormalizeKey(value) switch
        {
            "SEM_CATALOGO" => "Sem catálogo",
            "PRECO_INVALIDO" => "Preço inválido",
            "QUANTIDADE_INVALIDA" => "Quantidade inválida",
            "PESAVEL" => "Pesável",
            "FARDO_CAIXA" => "Fardo/Caixa",
            "FARDO" => "Fardo",
            "CAIXA" => "Caixa",
            "DUPLICIDADE" => "Duplicidade",
            "PRODUTO SEM CATALOGO/CODIGO" => "Produto sem catálogo/código",
            "PRECO INVALIDO OU ZERADO" => "Preço inválido ou zerado",
            "QUANTIDADE INVALIDA" => "Quantidade inválida",
            "CONVERSAO DE PESAVEL PENDENTE" => "Conversão de pesável pendente",
            "FARDO PENDENTE" => "Fardo pendente",
            "CAIXA PENDENTE" => "Caixa pendente",
            "FARDO/CAIXA PENDENTE" => "Fardo/caixa pendente",
            "REVISAO REJEITADA" => "Revisão rejeitada",
            _ => value
        };
    }

    private static string DisplayRole(string role)
    {
        return role switch
        {
            Roles.Admin => "Administrador",
            _ => role
        };
    }
}
