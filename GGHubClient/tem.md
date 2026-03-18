# GGHubClient - Blazor WASM проект

## Структура проекту

```
GGHubClient/
├── Components/
│   ├── Layout/
│   │   └── Header.razor
│   └── UI/
│       ├── Button.razor
│       ├── Card.razor
│       ├── Form.razor
│       ├── Input.razor
│       ├── Tag.razor
│       └── Text.razor
├── Pages/
│   └── Index.razor
├── Properties/
│   └── launchSettings.json
├── Shared/
│   └── MainLayout.razor
├── wwwroot/
│   ├── css/
│   │   ├── animations.css
│   │   ├── app.css
│   │   ├── bento-grid.css
│   │   ├── components.css
│   │   ├── hero.css
│   │   └── variables.css
│   ├── icons/
│   │   ├── gamepad.svg
│   │   ├── lock.svg
│   │   ├── target.svg
│   │   ├── trophy.svg
│   │   └── zap.svg
│   ├── images/
│   │   ├── 5.png
│   │   └── crypto-methods.svg
│   └── index.html
├── _Imports.razor
├── App.razor
├── GGHubClient.csproj
└── Program.cs
```

---

## **GGHubClient.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.17" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.17" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

---

## **Program.cs**

```csharp
using GGHubClient;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
```

---

## **_Imports.razor**

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using GGHubClient
@using GGHubClient.Components.UI
@using GGHubClient.Components.Layout
@using GGHubClient.Shared
```

---

## **App.razor**

```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>

<style>
    body {
        font-family: var(--font-family-primary);
        background: var(--color-bg);
        background-image: radial-gradient(at 40% 20%, rgba(90, 255, 136, 0.1) 0px, transparent 50%), 
                         radial-gradient(at 80% 0%, rgba(90, 255, 136, 0.05) 0px, transparent 50%), 
                         radial-gradient(at 0% 50%, rgba(90, 255, 136, 0.08) 0px, transparent 50%), 
                         radial-gradient(at 80% 50%, rgba(90, 255, 136, 0.06) 0px, transparent 50%), 
                         radial-gradient(at 0% 100%, rgba(90, 255, 136, 0.04) 0px, transparent 50%), 
                         radial-gradient(at 80% 100%, rgba(90, 255, 136, 0.1) 0px, transparent 50%);
        color: var(--color-primary-text);
        line-height: 1.6;
        min-height: 100vh;
        overflow-x: hidden;
        margin: 0;
        padding: 0;
    }

    * {
        box-sizing: border-box;
    }

    h1, h2, h3, h4, h5, h6 {
        font-family: var(--font-family-primary);
        margin: 0;
        padding: 0;
    }

    ::-webkit-scrollbar {
        width: 8px;
    }

    ::-webkit-scrollbar-track {
        background: var(--color-bg);
    }

    ::-webkit-scrollbar-thumb {
        background: var(--color-accent);
        border-radius: 4px;
    }

    ::-webkit-scrollbar-thumb:hover {
        background: var(--color-button-hover);
    }

    .page {
        animation: fadeIn 0.5s ease-out;
    }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(20px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    *:focus {
        outline: 2px solid var(--color-accent);
        outline-offset: 2px;
    }

    ::selection {
        background: var(--color-accent);
        color: var(--color-accent-text);
    }
</style>
```

---

## **Shared/MainLayout.razor**

```razor
@inherits LayoutComponentBase

<div class="page">
    <main>
        @Body
    </main>
</div>
```

---

## **Components/Layout/Header.razor**

```razor
<header class="header">
    <div class="header-content">
        <Text Heading="1" Variant="Text.TextVariant.Accent" AdditionalClass="logo">GGHUB</Text>
        <img src="/images/crypto-methods.svg" alt="Crypto Payment Methods" class="crypto-methods" />
    </div>
</header>

<style>
.header {
    padding: 1rem 2rem;
    background: rgba(13, 15, 18, 0.1);
    backdrop-filter: blur(20px);
    border-bottom: 1px solid rgba(42, 47, 56, 0.3);
    position: sticky;
    top: 0;
    z-index: 100;
}

.header-content {
    max-width: 1400px;
    margin: 0 auto;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.logo {
    font-size: 1.8rem !important;
    font-weight: 900;
}

.crypto-methods {
    height: 36px;
    opacity: 0.9;
}
</style>
```

---

## **Components/UI/Button.razor**

```razor
<button class="btn @GetButtonClass()" @onclick="OnClick" disabled="@Disabled" type="@Type">
    @if (!string.IsNullOrEmpty(IconName))
    {
        <svg class="btn-icon">
            <use href="/icons/@(IconName).svg#@IconName"></use>
        </svg>
    }
    @if (!string.IsNullOrEmpty(Text))
    {
        <span>@Text</span>
    }
    @ChildContent
</button>

@code {
    [Parameter] public string? Text { get; set; }
    [Parameter] public string? IconName { get; set; }
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Default;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }

    private string GetButtonClass()
    {
        var classes = new List<string>();

        classes.Add(Variant switch
        {
            ButtonVariant.Primary => "btn-primary",
            ButtonVariant.Secondary => "btn-secondary",
            _ => "btn-primary"
        });

        classes.Add(Size switch
        {
            ButtonSize.Small => "btn-sm",
            ButtonSize.Large => "btn-lg",
            _ => ""
        });

        if (!string.IsNullOrEmpty(AdditionalClass))
            classes.Add(AdditionalClass);

        return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
    }

    public enum ButtonVariant { Primary, Secondary }
    public enum ButtonSize { Small, Default, Large }
}
```

---

## **Components/UI/Card.razor**

```razor
<div class="card @AdditionalClass">
    @if (HeaderContent != null)
    {
        <div class="card-header">
            @HeaderContent
        </div>
    }

    <div class="card-body">
        @BodyContent
        @ChildContent
    </div>

    @if (FooterContent != null)
    {
        <div class="card-footer">
            @FooterContent
        </div>
    }
</div>

@code {
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }
}
```

---

## **Components/UI/Text.razor**

```razor
@if (Heading > 0)
{
    @switch (Heading)
    {
        case 1:
            <h1 class="text-heading text-h1 @GetTextClass()">@Content @ChildContent</h1>
            break;
        case 2:
            <h2 class="text-heading text-h2 @GetTextClass()">@Content @ChildContent</h2>
            break;
        case 3:
            <h3 class="text-heading text-h3 @GetTextClass()">@Content @ChildContent</h3>
            break;
        case 4:
            <h4 class="text-heading text-h4 @GetTextClass()">@Content @ChildContent</h4>
            break;
    }
}
else
{
    <p class="text-body @GetTextClass()">@Content @ChildContent</p>
}

@code {
    [Parameter] public string? Content { get; set; }
    [Parameter] public int Heading { get; set; } = 0;
    [Parameter] public TextVariant Variant { get; set; } = TextVariant.Default;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }

    private string GetTextClass()
    {
        var classes = new List<string>();

        classes.Add(Variant switch
        {
            TextVariant.Muted => "text-muted",
            TextVariant.Accent => "text-accent",
            _ => ""
        });

        if (!string.IsNullOrEmpty(AdditionalClass))
            classes.Add(AdditionalClass);

        return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
    }

    public enum TextVariant { Default, Muted, Accent }
}
```

---

## **Components/UI/Tag.razor**

```razor
<span class="tag @GetTagClass()">
    @if (!string.IsNullOrEmpty(IconName))
    {
        <svg class="tag-icon">
            <use href="/icons/@(IconName).svg#@IconName"></use>
        </svg>
    }
    @Text
    @ChildContent
</span>

@code {
    [Parameter] public string? Text { get; set; }
    [Parameter] public string? IconName { get; set; }
    [Parameter] public TagVariant Variant { get; set; } = TagVariant.Primary;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }

    private string GetTagClass()
    {
        var classes = new List<string>
        {
            Variant switch
            {
                TagVariant.Primary => "tag-primary",
                TagVariant.Secondary => "tag-secondary",
                _ => "tag-primary"
            }
        };

        if (!string.IsNullOrEmpty(AdditionalClass))
            classes.Add(AdditionalClass);

        return string.Join(" ", classes);
    }

    public enum TagVariant { Primary, Secondary }
}
```

---

## **Components/UI/Input.razor**

```razor
<div class="form-group">
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="form-label">@Label</label>
    }
    <input class="form-input @AdditionalClass"
           type="@Type"
           placeholder="@Placeholder"
           @bind="@Value"
           disabled="@Disabled"
           required="@Required" />
</div>

@code {
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }
}
```

---

## **Components/UI/Form.razor**

```razor
<form @onsubmit="@OnSubmit" @onsubmit:preventDefault="true" class="@AdditionalClass">
    @ChildContent
</form>

@code {
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AdditionalClass { get; set; }
}
```

---

## **Pages/Index.razor**

```razor
@page "/"

<PageTitle>GGHUB - CS2 Duels Arena</PageTitle>

<Header />

<div class="container">
    <!-- Hero Section з дизайном Figma -->
    <section class="hero-section">
        <div class="hero-content">
            <!-- Заголовок з blur ефектом -->
            <div class="hero-title-container">
                <div class="hero-title-blur">GGHUB</div>
                <Text Heading="1" Variant="Text.TextVariant.Accent" AdditionalClass="hero-title">GGHUB</Text>
            </div>
            
            <!-- Підзаголовок турнірів -->
            <div class="hero-subtitle-container">
                <span class="tournament-type">Турніри 1v1</span>
                <span class="separator-dot"></span>
                <span class="tournament-type">2v2</span>
                <span class="separator-dot"></span>
                <span class="tournament-type">5v5</span>
            </div>
            
            <!-- Основний опис -->
            <Text AdditionalClass="hero-description">Грай, вигравай, заробляй €€€</Text>

            <!-- Steam кнопка -->
            <Button Text="LOGIN WITH STEAM"
                    IconName="gamepad"
                    Size="Button.ButtonSize.Large"
                    AdditionalClass="steam-button" />

            <!-- Безпековий текст -->
            <div class="security-text">
                <svg class="security-icon">
                    <use href="/icons/lock.svg#lock"></use>
                </svg>
                <span>Безпечно</span>
                <span class="separator"></span>
                <span>Швидко</span>
                <span class="separator"></span>
                <span>Прозорі результати</span>
            </div>
        </div>

        <div class="hero-image">
            <div class="operator-visual animate-border-glow">
                <img src="/images/5.png" alt="CS2 Operator" />
            </div>
        </div>
    </section>

    <!-- Статистики -->
    <section class="stats-section">
        <div class="stats-grid">
            <div class="stat-item">
                <Text AdditionalClass="stat-number text-accent">1,247</Text>
                <Tag Text="Active Players" Variant="Tag.TagVariant.Secondary" />
            </div>
            <div class="stat-item">
                <Text AdditionalClass="stat-number text-accent">€12,500</Text>
                <Tag Text="Prize Money Paid" Variant="Tag.TagVariant.Secondary" />
            </div>
            <div class="stat-item">
                <Text AdditionalClass="stat-number text-accent">4.8⭐</Text>
                <Tag Text="User Rating" Variant="Tag.TagVariant.Secondary" />
            </div>
        </div>
    </section>

    <!-- Bento grid з функціями -->
    <section class="bento-grid bento-grid-cols-auto animate-scale-in">
        <Card AdditionalClass="hover-lift hover-glow">
            <BodyContent>
                <div class="feature-icon">⚡</div>
                <Text Heading="3" AdditionalClass="feature-title">Як це працює</Text>
                <Text Variant="Text.TextVariant.Muted">
                    1. Увійди через Steam<br>
                    2. Створи дуель або приєднайся<br>
                    3. Заплати entry fee<br>
                    4. Грай на автоматичному сервері<br>
                    5. Отримай приз за перемогу
                </Text>
            </BodyContent>
        </Card>

        <Card AdditionalClass="hover-lift hover-glow">
            <BodyContent>
                <div class="feature-icon">🎯</div>
                <Text Heading="3" AdditionalClass="feature-title">Сервери по всьому світу</Text>
                <Text Variant="Text.TextVariant.Muted">
                    <strong>Якісні карти</strong><br>
                    128-tick rate<br>
                    гнучкі кофігурації<br>
                    <strong>Готово за 30 секунд!</strong>
                </Text>
            </BodyContent>
        </Card>
    </section>
</div>

<style>
.container {
    width: 100%;
    margin: 0 auto;
}

.stats-section {
    margin: 4rem auto;
    max-width: 1200px;
    padding: 0 2rem;
}

.stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 2rem;
}

.stat-item {
    text-align: center;
}

.stat-number {
    font-size: 2rem;
    font-weight: 900;
    margin-bottom: 0.5rem;
    display: block;
}

.bento-grid {
    max-width: 1200px;
    margin: 4rem auto;
    padding: 0 2rem;
}

.feature-icon {
    font-size: 2rem;
    margin-bottom: 1rem;
}

.feature-title {
    margin-bottom: 1rem;
}

@media (max-width: 768px) {
    .container {
        padding: 0;
    }
    
    .stats-section,
    .bento-grid {
        padding: 0 1rem;
        margin: 2rem auto;
    }
    
    .stat-number {
        font-size: 1.5rem;
    }
}
</style>
```

---

## **Properties/launchSettings.json**

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:40214",
      "sslPort": 44355
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "applicationUrl": "http://localhost:5002",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "applicationUrl": "https://localhost:7214;http://localhost:5002",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## **wwwroot/index.html**

```html
<!DOCTYPE html>
<html lang="uk">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>GGHUB - CS2 Duels Arena</title>
    <base href="/" />
    
    <!-- CSS файли в правильному порядку -->
    <link href="css/variables.css" rel="stylesheet" />
    <link href="css/components.css" rel="stylesheet" />
    <link href="css/bento-grid.css" rel="stylesheet" />
    <link href="css/hero.css" rel="stylesheet" />
    <link href="css/animations.css" rel="stylesheet" />
    
    <!-- Google Fonts - Orbitron як основний шрифт -->
    <link href="https://fonts.googleapis.com/css2?family=Orbitron:wght@400;500;700;800;900&family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    
    <!-- Meta tags для SEO -->
    <meta name="description" content="GGHUB - Турніри CS2 1v1, 2v2, 5v5. Грай, вигравай, заробляй. Безпечні та швидкі матчі з призовим фондом.">
    <meta name="keywords" content="CS2, Counter-Strike, турніри, 1v1, 2v2, 5v5, призи, eSports">
    <meta name="author" content="GGHUB">
</head>
<body>
    <div id="app">
        <!-- Стилізований loader -->
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <div class="loading-text">GGHUB</div>
        </div>
    </div>

    <script src="_framework/blazor.webassembly.js"></script>
    
    <style>
        .loading-container {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: #0D0F12;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            z-index: 9999;
        }
        
        .loading-spinner {
            width: 50px;
            height: 50px;
            border: 3px solid rgba(90, 255, 136, 0.2);
            border-top: 3px solid #5AFF88;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin-bottom: 1rem;
        }
        
        .loading-text {
            font-family: 'Orbitron', sans-serif;
            font-weight: 900;
            font-size: 24px;
            color: #5AFF88;
            letter-spacing: -0.02em;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .loaded .loading-container {
            opacity: 0;
            visibility: hidden;
            transition: opacity 0.5s ease-out, visibility 0.5s ease-out;
        }
    </style>
    
    <script>
        window.addEventListener('load', function() {
            document.body.classList.add('loaded');
            setTimeout(() => {
                const loader = document.querySelector('.loading-container');
                if (loader) {
                    loader.remove();
                }
            }, 500);
        });
    </script>
</body>
</html>
```

---

## **wwwroot/css/variables.css**

```css
:root {
    /* Оригінальні кольори фону залишаємо */
    --color-bg: #0D0F12;          
    --color-grid-bg: #0D0F12;     
    --color-card-bg: #171B21;     
    --color-border: #2A2F38;      
    
    /* Текстові кольори з Figma */
    --color-primary-text: #E2E5EA; 
    --color-secondary-text: #9AA0A9; 
    --color-accent: #5AFF88;      
    --color-accent-text: #0D0F12; 
    
    /* Кнопки */
    --color-button-bg: #5AFF88;   
    --color-button-text: #0D0F12; 
    --color-button-hover: #51E57A;
    
    /* Форми */
    --color-input-bg: #171B21;    
    --color-input-text: #E2E5EA;  
    --color-input-border: #2A2F38;
    --color-checkbox-bg: #171B21; 
    --color-checkbox-border: #2A2F38;
    --color-checkbox-check: #5AFF88;  
    --color-grid-item-bg: #171B21; 

    /* Градієнти */
    --gradient-primary: linear-gradient(135deg, #5AFF88 0%, #51E57A 100%);
    --gradient-accent: linear-gradient(135deg, #5AFF88 0%, #4AE76F 100%);
    --gradient-card: linear-gradient(135deg, rgba(23, 27, 33, 0.9) 0%, rgba(42, 47, 56, 0.2) 100%);

    /* Розміри */
    --border-radius-sm: 6px;
    --border-radius-md: 8px;
    --border-radius-lg: 12px;
    --border-radius-xl: 16px;

    --spacing-xs: 0.25rem;
    --spacing-sm: 0.5rem;
    --spacing-md: 1rem;
    --spacing-lg: 1.5rem;
    --spacing-xl: 2rem;
    --spacing-2xl: 3rem;

    /* Типографіка */
    --font-family-primary: 'Orbitron', 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
    --font-size-xs: 0.75rem;
    --font-size-sm: 0.875rem;
    --font-size-base: 1rem;
    --font-size-lg: 1.125rem;
    --font-size-xl: 1.25rem;
    --font-size-2xl: 1.5rem;
    --font-size-3xl: 1.875rem;
    --font-size-4xl: 2.25rem;
    --font-size-5xl: 3rem;
    --font-size-6xl: 4rem;

    /* Ефекти */
    --shadow-glow: 0px 4px 4px rgba(0, 0, 0, 0.25);
    --shadow-accent: 0 10px 30px rgba(90, 255, 136, 0.2);
    --filter-blur: blur(4px);
}
```

---

## **wwwroot/css/components.css**

```css
.btn {
    display: inline-flex;
    align-items: center;
    gap: var(--spacing-sm);
    padding: 12px 20px;
    border: none;
    border-radius: var(--border-radius-sm);
    font-family: var(--font-family-primary);
    font-weight: 800;
    cursor: pointer;
    transition: all 0.3s ease;
    text-decoration: none;
    text-transform: uppercase;
    letter-spacing: 0.02em;
    font-size: 16px;
    line-height: 120%;
}

.btn-primary {
    background: var(--color-button-bg);
    color: var(--color-button-text);
    box-shadow: var(--shadow-accent);
}

.btn-primary:hover {
    background: var(--color-button-hover);
    transform: translateY(-2px);
    box-shadow: 0 12px 35px rgba(90, 255, 136, 0.3);
}

.btn-secondary {
    background: transparent;
    color: var(--color-accent);
    border: 1px solid var(--color-accent);
}

.btn-secondary:hover {
    background: var(--color-accent);
    color: var(--color-accent-text);
}

.btn-sm {
    padding: var(--spacing-xs) var(--spacing-md);
    font-size: var(--font-size-sm);
}

.btn-lg {
    padding: 16px 24px;
    font-size: 16px;
    height: 64px;
}

.btn-icon {
    width: 24px;
    height: 27px;
    fill: currentColor;
}

.steam-button {
    width: 264px !important;
    justify-content: center;
}

.tag {
    display: inline-flex;
    align-items: center;
    gap: var(--spacing-xs);
    padding: var(--spacing-xs) var(--spacing-sm);
    border-radius: var(--border-radius-sm);
    font-family: var(--font-family-primary);
    font-size: var(--font-size-sm);
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.tag-primary {
    background: var(--color-accent);
    color: var(--color-accent-text);
}

.tag-secondary {
    background: var(--color-card-bg);
    color: var(--color-secondary-text);
    border: 1px solid var(--color-border);
}

.tag-icon {
    width: 14px;
    height: 14px;
    fill: currentColor;
}

.form-group {
    margin-bottom: var(--spacing-lg);
}

.form-label {
    display: block;
    margin-bottom: var(--spacing-sm);
    color: var(--color-primary-text);
    font-family: var(--font-family-primary);
    font-weight: 600;
    font-size: var(--font-size-sm);
}

.form-input {
    width: 100%;
    padding: var(--spacing-sm) var(--spacing-md);
    background: var(--color-input-bg);
    border: 1px solid var(--color-input-border);
    border-radius: var(--border-radius-md);
    color: var(--color-input-text);
    font-family: var(--font-family-primary);
    font-size: var(--font-size-base);
    transition: all 0.3s ease;
}

.form-input:focus {
    outline: none;
    border-color: var(--color-accent);
    box-shadow: 0 0 0 2px rgba(90, 255, 136, 0.2);
}

.form-input::placeholder {
    color: var(--color-secondary-text);
}

.text-heading {
    color: var(--color-primary-text);
    font-family: var(--font-family-primary);
    font-weight: 900;
    line-height: 120%;
    letter-spacing: -0.02em;
}

.text-h1 { 
    font-size: var(--font-size-6xl);
    text-shadow: var(--shadow-glow);
}
.text-h2 { font-size: var(--font-size-3xl); }
.text-h3 { font-size: var(--font-size-2xl); }
.text-h4 { font-size: var(--font-size-xl); }

.text-body {
    color: var(--color-primary-text);
    font-family: var(--font-family-primary);
    font-size: var(--font-size-base);
    line-height: 1.6;
}

.text-muted {
    color: var(--color-secondary-text);
}

.text-accent {
    color: var(--color-accent);
}

.card {
    background: var(--gradient-card);
    border: 1px solid var(--color-border);
    border-radius: var(--border-radius-lg);
    backdrop-filter: blur(10px);
    position: relative;
    overflow: hidden;
}

.card-header {
    padding: var(--spacing-lg) var(--spacing-xl) var(--spacing-lg) var(--spacing-xl);
    border-bottom: 1px solid var(--color-border);
}

.card-body {
    padding: var(--spacing-xl);
}

.card-footer {
    padding: var(--spacing-lg) var(--spacing-xl);
    border-top: 1px solid var(--color-border);
}
```

---

## **wwwroot/css/hero.css**

```css
/* Hero Section - відповідно до Figma дизайну */
.hero-section {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 3rem;
    align-items: center;
    min-height: 100vh;
    padding: 0 2rem;
    max-width: 1920px;
    margin: 0 auto;
    position: relative;
    overflow: hidden;
}

.hero-content {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 32px;
    width: 457px;
    z-index: 2;
}

/* Заголовок з blur ефектом */
.hero-title-container {
    position: relative;
    width: 262px;
    height: 77px;
}

.hero-title-blur {
    position: absolute;
    top: 0;
    left: 0;
    font-family: var(--font-family-primary);
    font-weight: 900;
    font-size: 64px;
    line-height: 120%;
    letter-spacing: -0.02em;
    color: var(--color-accent);
    filter: var(--filter-blur);
    z-index: 1;
}

.hero-title {
    position: absolute !important;
    top: 0 !important;
    left: 0 !important;
    font-size: 64px !important;
    font-weight: 900 !important;
    line-height: 120% !important;
    letter-spacing: -0.02em !important;
    text-shadow: var(--shadow-glow) !important;
    z-index: 2;
    margin: 0 !important;
}

/* Підзаголовок турнірів */
.hero-subtitle-container {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 3px 16px;
    width: 295px;
    height: 24px;
}

.tournament-type {
    font-family: var(--font-family-primary);
    font-weight: 800;
    font-size: 20px;
    line-height: 120%;
    color: var(--color-accent);
}

.separator-dot {
    width: 6px;
    height: 6px;
    background: var(--color-accent);
    border-radius: 50%;
}

/* Головний опис */
.hero-description {
    font-family: var(--font-family-primary) !important;
    font-weight: 900 !important;
    font-size: 28px !important;
    line-height: 120% !important;
    color: var(--color-primary-text) !important;
    width: 457px;
    margin: 0 !important;
}

/* Steam кнопка */
.steam-button {
    width: 264px !important;
    height: 64px !important;
    padding: 12px 20px !important;
    background: var(--color-button-bg) !important;
    border-radius: 6px !important;
    display: flex !important;
    justify-content: center !important;
    align-items: center !important;
    gap: 8px !important;
}

/* Безпековий текст */
.security-text {
    width: 279px;
    height: 14px;
    display: flex;
    align-items: center;
    gap: 8px;
    font-family: var(--font-family-primary);
    font-weight: 500;
    font-size: 12px;
    line-height: 120%;
    color: var(--color-secondary-text);
}

.security-text .separator {
    width: 2px;
    height: 2px;
    background: var(--color-secondary-text);
    border-radius: 50%;
}

.security-text .security-icon {
    width: 12px;
    height: 12px;
    fill: currentColor;
}

/* Права частина з зображенням */
.hero-image {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 720px;
}

.operator-visual {
    width: 100%;
    height: 100%;
    background: var(--gradient-card);
    border-radius: 12px;
    border: 2px solid var(--color-border);
    position: relative;
    overflow: hidden;
    box-shadow: 0 10px 30px rgba(90, 255, 136, 0.1);
    transform: scaleX(-1);
}

.operator-visual img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    object-position: center;
    display: block;
}

.operator-visual::before {
    content: '';
    position: absolute;
    top: -2px;
    left: -2px;
    right: -2px;
    bottom: -2px;
    background: var(--gradient-accent);
    animation: borderGlow 4s ease-in-out infinite alternate;
    z-index: -1;
}

/* Мобільний адаптив */
@media (max-width: 1400px) {
    .hero-content {
        width: 100%;
        max-width: 457px;
    }
    
    .hero-image {
        width: 100%;
        max-width: 600px;
        height: auto;
        aspect-ratio: 16/9;
    }
}

@media (max-width: 768px) {
    .hero-section {
        grid-template-columns: 1fr;
        gap: 2rem;
        min-height: auto;
        padding: 2rem 1rem;
    }
    
    .hero-content {
        order: 2;
        align-items: center;
        text-align: center;
        width: 100%;
    }
    
    .hero-image {
        order: 1;
        height: 400px;
        width: 100%;
    }
    
    .hero-title-container {
        width: 100%;
        height: auto;
    }
    
    .hero-title,
    .hero-title-blur {
        font-size: 48px !important;
        position: relative !important;
    }
    
    .hero-description {
        font-size: 24px !important;
        width: 100% !important;
        text-align: center !important;
    }
    
    .hero-subtitle-container {
        justify-content: center;
        width: 100%;
        height: auto;
        gap: 8px 16px;
    }
    
    .steam-button {
        align-self: center;
    }
    
    .security-text {
        justify-content: center;
        width: 100%;
        height: auto;
        flex-wrap: wrap;
    }
}

@media (max-width: 480px) {
    .hero-section {
        padding: 1rem;
    }
    
    .hero-title,
    .hero-title-blur {
        font-size: 36px !important;
    }
    
    .hero-description {
        font-size: 20px !important;
    }
    
    .tournament-type {
        font-size: 18px;
    }
    
    .steam-button {
        width: 100% !important;
        max-width: 280px;
    }
}
```

---

## **wwwroot/css/bento-grid.css**

```css
.bento-grid {
    display: grid;
    gap: var(--spacing-lg);
    width: 100%;
}

.bento-grid-cols-1 { grid-template-columns: 1fr; }
.bento-grid-cols-2 { grid-template-columns: repeat(2, 1fr); }
.bento-grid-cols-3 { grid-template-columns: repeat(3, 1fr); }
.bento-grid-cols-4 { grid-template-columns: repeat(4, 1fr); }
.bento-grid-cols-auto { grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); }

.bento-item {
    background: var(--color-card-bg);
    border: 1px solid var(--color-border);
    border-radius: var(--border-radius-lg);
    padding: var(--spacing-xl);
    transition: all 0.3s ease;
    backdrop-filter: blur(10px);
    position: relative;
    overflow: hidden;
}

.bento-item:hover {
    transform: translateY(-5px);
    border-color: var(--color-accent);
    box-shadow: 0 10px 30px rgba(90, 255, 136, 0.1);
}

.bento-item-span-2 { grid-column: span 2; }
.bento-item-span-3 { grid-column: span 3; }
.bento-item-span-4 { grid-column: span 4; }

@media (max-width: 768px) {
    .bento-grid-cols-2,
    .bento-grid-cols-3,
    .bento-grid-cols-4 {
        grid-template-columns: 1fr;
    }

    .bento-item-span-2,
    .bento-item-span-3,
    .bento-item-span-4 {
        grid-column: span 1;
    }
}
```

---

## **wwwroot/css/animations.css**

```css
@keyframes glow {
    from { filter: drop-shadow(0 0 10px rgba(90, 255, 136, 0.3)); }
    to { filter: drop-shadow(0 0 15px rgba(74, 231, 111, 0.4)); }
}

@keyframes borderGlow {
    from { opacity: 0.3; }
    to { opacity: 0.6; }
}

@keyframes slideInLeft {
    from { transform: translateX(-100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

@keyframes slideInRight {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@keyframes scaleIn {
    from { transform: scale(0.8); opacity: 0; }
    to { transform: scale(1); opacity: 1; }
}

.animate-glow {
    animation: glow 3s ease-in-out infinite alternate;
}

.animate-border-glow {
    animation: borderGlow 4s ease-in-out infinite alternate;
}

.animate-slide-in-left {
    animation: slideInLeft 0.5s ease-out;
}

.animate-slide-in-right {
    animation: slideInRight 0.5s ease-out;
}

.animate-fade-in {
    animation: fadeIn 0.3s ease-out;
}

.animate-scale-in {
    animation: scaleIn 0.3s ease-out;
}

.hover-lift {
    transition: transform 0.3s ease;
}

.hover-lift:hover {
    transform: translateY(-5px);
}

.hover-glow {
    transition: all 0.3s ease;
}

.hover-glow:hover {
    box-shadow: 0 10px 30px rgba(90, 255, 136, 0.1);
}
```

---

## **wwwroot/css/app.css**

```css
.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid red;
}

.validation-message {
    color: red;
}

#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

    #blazor-error-ui .dismiss {
        cursor: pointer;
        position: absolute;
        right: 0.75rem;
        top: 0.5rem;
    }

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTEzIDI5MSA5NC4wODE1IDI5MSA5NC42NzgyIDI5MSA5Ny4wNjUxIDI4OS4wMzggOTkgMjg2LjYxNyA5OUwyNDAuMzgzIDk5QzIzNy45NjMgOTkgMjM2IDk3LjA2NTEgMjM2IDk0LjY3ODIgMjM2IDk0LjM3OTkgMjM2LjAzMSA5NC4wODg2IDIzNi4wODkgOTMuODA3MkwyMzYuMzM4IDkzLjAxNjIgMjM2Ljg1OCA5Mi4xMzE0IDI1OS40NzMgNTMuNjI5NCAyNTkuOTYxIDUyLjc5ODUgMjYwLjQwNyA1Mi4yNjU4QzI2MS4yIDUxLjQ4MzcgMjYyLjI5NiA1MSAyNjMuNTA2IDUxWk0yNjMuNTg2IDY2LjAxODNDMjYwLjczNyA2Ni4wMTgzIDI1OS4zMTMgNjcuMTI0NSAyNTkuMzEzIDY5LjMzNyAyNTkuMzEzIDY5LjYxMDIgMjU5LjMzMiA2OS44NjA4IDI1OS4zNzEgNzAuMDg4N0wyNjEuNzk1IDg0LjAxNjEgMjY1LjM4IDg0LjAxNjEgMjY3LjgyMSA2OS43NDc1QzI2Ny44NiA2OS43MzA5IDI2Ny44NzkgNjkuNTg3NyAyNjcuODc5IDY5LjMxNzkgMjY3Ljg3OSA2Ny4xMTgyIDI2Ni40NDggNjYuMDE4MyAyNjMuNTg2IDY2LjAxODNaTTI2My41NzYgODYuMDU0N0MyNjEuMDQ5IDg2LjA1NDcgMjU5Ljc4NiA4Ny4zMDA1IDI1OS43ODYgODkuNzkyMSAyNTkuNzg2IDkyLjI4MzcgMjYxLjA0OSA5My41Mjk1IDI2My41NzYgOTMuNTI5NSAyNjYuMTE2IDkzLjUyOTUgMjY3LjM4NyA5Mi4yODM3IDI2Ny4zODcgODkuNzkyMSAyNjcuMzg3IDg3LjMwMDUgMjY2LjExNiA4Ni4wNTQ3IDI2My41NzYgODYuMDU0N1oiIGZpbGw9IiNGRkU1MDAiIGZpbGwtcnVsZT0iZXZlbm9kZCIvPjwvZz48L3N2Zz4=) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

    .blazor-error-boundary::after {
        content: "An error has occurred."
    }

.loading-progress {
    position: relative;
    display: block;
    width: 8rem;
    height: 8rem;
    margin: 20vh auto 1rem auto;
}

    .loading-progress circle {
        fill: none;
        stroke: #e0e0e0;
        stroke-width: 0.6rem;
        transform-origin: 50% 50%;
        transform: rotate(-90deg);
    }

        .loading-progress circle:last-child {
            stroke: #1b6ec2;
            stroke-dasharray: calc(3.141 * var(--blazor-load-percentage, 0%) * 0.8), 500%;
            transition: stroke-dasharray 0.05s ease-in-out;
        }

.loading-progress-text {
    position: absolute;
    text-align: center;
    font-weight: bold;
    inset: calc(20vh + 3.25rem) 0 auto 0.2rem;
}

    .loading-progress-text:after {
        content: var(--blazor-load-percentage-text, "Loading");
    }

code {
    color: #c02d76;
}
```

---

## **wwwroot/icons/gamepad.svg**

```svg
<svg xmlns="http://www.w3.org/2000/svg">
  <symbol id="gamepad" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <line x1="6" x2="10" y1="12" y2="12" />
    <line x1="8" x2="8" y1="10" y2="14" />
    <line x1="15" x2="15.01" y1="13" y2="13" />
    <line x1="18" x2="18.01" y1="11" y2="11" />
    <rect width="20" height="12" x="2" y="6" rx="2" />
  </symbol>
</svg>
```

---

## **wwwroot/icons/lock.svg**

```svg
<svg xmlns="http://www.w3.org/2000/svg">
  <symbol id="lock" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <rect width="18" height="11" x="3" y="11" rx="2" ry="2" />
    <path d="M7 11V7a5 5 0 0 1 10 0v4" />
  </symbol>
</svg>
```

---

## **wwwroot/icons/target.svg**

```svg
<svg xmlns="http://www.w3.org/2000/svg">
  <symbol id="target" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <circle cx="12" cy="12" r="10" />
    <circle cx="12" cy="12" r="6" />
    <circle cx="12" cy="12" r="2" />
  </symbol>
</svg>
```

---

## **wwwroot/icons/trophy.svg**

```svg
<svg xmlns="http://www.w3.org/2000/svg">
  <symbol id="trophy" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="M10 14.66v1.626a2 2 0 0 1-.976 1.696A5 5 0 0 0 7 21.978" />
    <path d="M14 14.66v1.626a2 2 0 0 0 .976 1.696A5 5 0 0 1 17 21.978" />
    <path d="M18 9h1.5a1 1 0 0 0 0-5H18" />
    <path d="M4 22h16" />
    <path d="M6 9a6 6 0 0 0 12 0V3a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1z" />
    <path d="M6 9H4.5a1 1 0 0 1 0-5H6" />
  </symbol>
</svg>
```

---

## **wwwroot/icons/zap.svg**

```svg
<svg xmlns="http://www.w3.org/2000/svg">
  <symbol id="zap" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="M4 14a1 1 0 0 1-.78-1.63l9.9-10.2a.5.5 0 0 1 .86.46l-1.92 6.02A1 1 0 0 0 13 10h7a1 1 0 0 1 .78 1.63l-9.9 10.2a.5.5 0 0 1-.86-.46l1.92-6.02A1 1 0 0 0 11 14z" />
  </symbol>
</svg>
```

---

## **Додаткові файли які потрібно створити:**

### **wwwroot/images/crypto-methods.svg**
Зображення логотипів криптовалютних методів оплати

### **wwwroot/images/5.png**
Зображення CS2 оператора для hero секції

### **wwwroot/favicon.ico**
Іконка сайту

---

## **Команди для запуску:**

```bash
# Встановити залежності
dotnet restore

# Запустити проект в development режимі
dotnet run

# Або запустити з watch для hot reload
dotnet watch run
```

Проект буде доступний за адресою: `https://localhost:7214` або `http://localhost:5002`