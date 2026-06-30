# MO — MARKtime Mobile

ASP.NET Core 8 MVC projekt — mobilní webové rozhraní MARKtime.

## Závislosti
- BL, BO, DL (project references)
- DaisyUI 5 + Tailwind 4 (standalone CLI, mimo solution)
- Material Icons (lokální `.woff2`, sdílené s UI)

## Build CSS (Tailwind + DaisyUI)

### Jednorázová příprava
1. Stáhni `tailwindcss-windows-x64.exe` z https://github.com/tailwindlabs/tailwindcss/releases/latest a polož do `c:\DEV\_tailwind\`
2. Stáhni `daisyui.mjs` a `daisyui-theme.mjs` z https://github.com/saadeghi/daisyui/releases/latest a polož vedle exe

### Generování `wwwroot/css/app.css`
Z adresáře `MO/Styles/`:
```
c:\DEV\_tailwind\tailwindcss-windows-x64.exe -i app.css -o ../wwwroot/css/app.css --minify
```

Pro vývoj s automatickým rebuild:
```
c:\DEV\_tailwind\tailwindcss-windows-x64.exe -i app.css -o ../wwwroot/css/app.css --watch
```

**Bez vygenerovaného `wwwroot/css/app.css` aplikace nemá styly DaisyUI ani Tailwind.**

> **Poznámka k cestě k plugin**: V `Styles/app.css` je `@plugin "c:/DEV/_tailwind/daisyui.mjs"` jako absolutní cesta. Pokud Tailwind hlásí, že plugin nenajde, zkopíruj `daisyui.mjs` do `MO/Styles/` a změň direktivu na `@plugin "./daisyui.mjs"`.

## SSO s UI
Sdílená auth cookie funguje, pokud:
- `Authentication:KeyPath` je stejný v obou projektech (sdílený filesystem)
- `Authentication:AppName` je stejný (`MARKTIME`)
- `Authentication:CookieName` je stejný (`Marktime7Auth`)
- `Authentication:Domain` je nastaven v appsettings obou projektů (např. `.marktime.zakaznik.cz`) a oba projekty běží na subdoménách této domény

V dev nastavení (různé porty na localhost) SSO **nefunguje** — cookie scope je per-host, ne per-port.

## Spuštění
```
dotnet run --project MO
```
Default profile: https://localhost:7202 / http://localhost:5202

## Co je v úvodní verzi
- Login (jen user/password, bez Google OAuth a SMS 2FA)
- Logout
- MyProfile (read-only)
- Index (dashboard s odkazy)
- Placeholdery: Kalendář, Úkony, Úkoly, Sestavy, Více
- Top navbar (logo + Odhlásit + theme switcher)
- Spodní dock menu (6 položek)
- Pravý drawer pro „Více"
- Telerik Reporting (balíčky + registrace v Startup) — připraveno, view zatím placeholder
