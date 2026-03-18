# GGHub - CS2 Tournament Platform

A full-stack competitive gaming platform for Counter-Strike 2, supporting **1v1, 2v2, and 5v5 duels and tournaments** with prize pools, automatic game server provisioning, cryptocurrency payments, and real-time updates.

## Features

- **Duels & Tournaments** -- Create and join 1v1, 2v2, 3v3, 4v4, and 5v5 matches with configurable entry fees, map pools, and round formats (BO1/BO3/BO5)
- **Automatic Server Provisioning** -- Game servers are created, configured, and started automatically via the [DatHost API](https://dathost.net). Players receive connection details (IP, port, password, Steam URL) instantly
- **Cryptocurrency Payments** -- Entry fee deposits and prize payouts via [Cryptomus](https://cryptomus.com) (crypto payments). 10% platform commission on all prize pools
- **Steam Authentication** -- Players authenticate via Steam OpenID. Steam profiles are linked for matchmaking and anti-cheat purposes
- **Telegram Bot** -- Full-featured bot for creating duels, joining matches, checking status, managing tournaments, and receiving real-time notifications
- **Real-time Updates** -- SignalR hubs push live match events (player connected, round ended, match completed) to all participants
- **Blazor Web App** -- Web interface for browsing tournaments, managing profile, viewing match history, and spectating
- **Forfeit & Complaint System** -- Automated forfeit detection (ping/packet loss thresholds) with admin review. Players can file complaints with evidence
- **Multi-language Support** -- Ukrainian, English, and Russian localization in the Telegram bot

## Architecture

The solution consists of 9 projects:

```
GGHub.sln
├── GGHubApi/           # ASP.NET Core Web API (.NET 9.0) - Main backend
│   ├── Controllers/    # Auth, Duel, Tournament, Payment, Webhook, Users, Steam, Metrics
│   ├── Services/       # DatHost, Cryptomus, JWT, Steam, Duel logic, Ping analysis
│   └── Hubs/           # SignalR hubs (DuelHub, TournamentHub)
├── GGHubBot/           # Telegram Bot (.NET 8.0) - Bot client
│   ├── Handlers/       # Message, Callback, Tournament handlers
│   └── Services/       # API client, Localization, Media, Admin
├── GGHubDb/            # Data Access Layer (.NET 8.0)
│   ├── Models/         # User, Duel, Tournament, Transaction, GameServer, Complaint
│   ├── Repos/          # Repository pattern (EF Core + SQL Server)
│   └── Services/       # Business logic services
├── GGHubShared/        # Shared library (.NET 8.0) - DTOs, Enums, Helpers
├── GGHubWeb/           # Blazor WebAssembly frontend (.NET 8.0)
├── GGHubClient/        # Blazor WebAssembly client (.NET 8.0)
├── DatHostApi/         # DatHost API client library (.NET 8.0)
├── DatHostApi.Tests/   # Integration tests for DatHost API
└── NgrokSetup/         # Development tunnel setup utility
```

## Tech Stack

| Layer | Technology |
|---|---|
| **API** | ASP.NET Core 9.0, Entity Framework Core 9.0, SignalR |
| **Bot** | Telegram.Bot 22.x, SignalR Client |
| **Frontend** | Blazor WebAssembly (.NET 8.0) |
| **Database** | SQL Server (API), SQLite (Bot local cache) |
| **Auth** | Steam OpenID, JWT Bearer tokens |
| **Payments** | Cryptomus API (crypto) |
| **Game Servers** | DatHost REST API (CS2 dedicated servers) |
| **Mapping** | AutoMapper |
| **Logging** | Serilog (structured logging) |
| **API Docs** | Swagger / Swashbuckle |

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for GGHubApi)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for Bot, DB, Web projects)
- SQL Server (LocalDB, Express, or full)
- [ngrok](https://ngrok.com/) (for development webhooks)

### Configuration

Copy the template `appsettings.json` files and fill in your credentials:

**GGHubApi/appsettings.json:**
- `ConnectionStrings:DefaultConnection` -- SQL Server connection string
- `DatHost:Email`, `DatHost:Password` -- [DatHost](https://dathost.net) account credentials
- `DatHost:WebhookUrl`, `DatHost:WebhookSecret` -- Webhook endpoint for match events
- `Cryptomus:MerchantId`, `Cryptomus:PaymentKey` -- [Cryptomus](https://doc.cryptomus.com/) merchant credentials
- `Steam:ApiKey` -- [Steam Web API key](https://steamcommunity.com/dev/apikey)
- `Jwt:SecretKey` -- Secret key for JWT token signing (min 32 characters)
- `Cors:AllowedOrigins` -- Allowed CORS origins for your domain
- `App:PublicBaseUrl` -- Your public-facing URL

**GGHubBot/appsettings.json:**
- `TelegramBot:Token` -- Telegram Bot token from [@BotFather](https://t.me/BotFather)
- `TelegramBot:WebhookUrl` -- Webhook URL for receiving Telegram updates
- `TelegramBot:AdminIds` -- Array of Telegram user IDs with admin access
- `Steam:ApiKey` -- Steam Web API key
- `GGHubApi:BaseUrl` -- URL of the running GGHubApi instance

### Running

```bash
# 1. Restore dependencies
dotnet restore

# 2. Apply database migrations (or create DB manually)
# The API uses SQL Server, the Bot uses SQLite

# 3. Start the API
cd GGHubApi
dotnet run

# 4. Start the Bot (in a separate terminal)
cd GGHubBot
dotnet run

# 5. (Optional) Start the Web frontend
cd GGHubWeb
dotnet run
```

For local development with webhooks, use the `NgrokSetup` project to automatically configure ngrok tunnels.

## How It Works

1. **Player registers** via Steam OpenID (through Bot or Web)
2. **Player creates a duel** -- selects format (1v1/2v2/5v5), entry fee, maps, rounds
3. **Opponent joins** via invite link or matchmaking
4. **Both players pay** entry fee through Cryptomus (crypto)
5. **Server auto-provisions** -- DatHost API creates and starts a CS2 server
6. **Players connect** using provided IP:Port or Steam URL
7. **Match plays out** -- DatHost sends webhook events (round_end, match_end)
8. **Results processed** -- winner receives prize pool minus 10% commission
9. **Notifications sent** -- via Telegram Bot and SignalR real-time events

## API Documentation

When running in development mode, Swagger UI is available at `/swagger`.

Key API endpoints:
- `POST /api/duel` -- Create a new duel
- `POST /api/tournament` -- Create a tournament
- `POST /auth/steam/callback` -- Steam OAuth callback
- `POST /api/webhook/dathost` -- DatHost match event webhook
- `POST /api/webhook/cryptomus` -- Cryptomus payment webhook
- `GET /api/tournament/{id}/bracket` -- Get tournament bracket

## License

This project is provided as-is for educational and portfolio purposes.
