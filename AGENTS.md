# AGENTS.md - GGHub Platform

Цей файл надає інструкції для AI-агентів, які працюють з кодовою базою платформи CS2 Duels.

## Project Overview

GGHub - це платформа для проведення дуелів та турнірів (1×1/2×2/3x3/4x4/5×5 Duel) в Counter-Strike 2 з інтеграцією Steam авторизації, системою оплати, автоматичним створенням серверів та Telegram-ботом.
Фішка в тому що користувачі самі формують депозитний фонд своїми внесками за участь

## Project Structure

```
/
├── GGHubApi/              # Main Web API project (.NET 9.0)
│   ├── Configuration/     # DI, JWT, Steam auth, logging setup
│   ├── Controllers/       # API endpoints (Auth, Tournament, Users, Webhook)
│   ├── Extensions/        # DatHost services, Steam auth extensions
│   ├── Hubs/             # SignalR hubs for real-time updates
│   ├── MapProfiles/      # AutoMapper configuration
│   └── Services/         # Business services (JWT, Cryptomus, DatHost, Steam)
├── GGHubBot/             # Telegram Bot (.NET 8.0)
│   ├── Handlers/         # Message, callback, duel, tournament handlers
│   ├── Models/           # Bot-specific models and user states
│   └── Services/         # Bot services (API, localization, media, admin)
├── GGHubDb/              # Data Access Layer (.NET 8.0)
│   ├── Models/           # Entity models (User, Duel, Tournament, Transaction)
│   ├── Repos/            # Repository pattern implementations
│   └── Services/         # Business logic services
├── GGHubShared/          # Shared models and utilities (.NET 8.0)
│   ├── Enums/            # Shared enumerations
│   ├── Models/           # DTOs, requests, responses
│   └── Helpers/          # Constants and utilities
└── DatHostApi/           # External DatHost API integration
```

## Architecture & Design Patterns

### Core Patterns
- **Repository Pattern**: All database operations go through repository interfaces
- **Service Layer**: Business logic separated into dedicated services
- **Dependency Injection**: Configured in `DependencyInjection.cs`
- **CQRS-like**: Clear separation between read/write operations
- **API Response Pattern**: All endpoints return `ApiResponse<T>` wrapper

### Key Technologies
- **ASP.NET Core 9.0** - Main API framework
- **Entity Framework Core** - ORM with SQL Server
- **AutoMapper** - Object-to-object mapping
- **SignalR** - Real-time communication
- **Serilog** - Structured logging
- **Telegram.Bot** - Telegram integration

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (`TournamentService`, `UserController`)
- **Methods**: PascalCase (`GetByIdAsync`, `CreateTournamentAsync`)
- **Variables**: camelCase (`userId`, `tournamentDto`)
- **Constants**: UPPER_SNAKE_CASE (`DEFAULT_AVATAR`, `API_BASE_URL`)
- **Private fields**: `_camelCase` (`_logger`, `_userRepository`)

### Code Organization
```csharp
// Controller method structure
[HttpGet("{id:guid}")]
public async Task<ActionResult<ApiResponse<UserDto>>> GetById(
    Guid id, 
    CancellationToken cancellationToken = default)
{
    var result = await _userService.GetByIdAsync(id, cancellationToken);
    
    if (!result.Success)
        return NotFound(result);
        
    return Ok(result);
}

// Service method structure
public async Task<ApiResponse<UserDto>> GetByIdAsync(
    Guid id, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "User not found"
            };
        }

        return new ApiResponse<UserDto>
        {
            Success = true,
            Data = _mapper.Map<UserDto>(user)
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
        return new ApiResponse<UserDto>
        {
            Success = false,
            Message = "An error occurred while retrieving user",
            Errors = { ex.Message }
        };
    }
}
```

### Error Handling
- **Always use try-catch** in service methods
- **Log errors** with structured logging using Serilog
- **Return ApiResponse<T>** with Success flag and error messages
- **Use appropriate HTTP status codes** in controllers
- **Include correlation IDs** for tracking

### Async/Await Patterns
- **All database operations** must be async
- **Use CancellationToken** for all async methods
- **Configure await** appropriately (`ConfigureAwait(false)` in services)
- **Async method naming**: End with `Async` suffix

## Database & Entity Framework

### Entity Conventions
- **All entities** inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted)
- **Soft delete** pattern - set `IsDeleted = true`, never actually delete
- **UTC timestamps** for all DateTime fields
- **Navigation properties** properly configured with virtual keyword

### Repository Pattern
```csharp
// Repository interface example
public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
}

// Always include proper logging and error handling
public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
{
    _logger.LogDebug("Getting user by email: {Email}", email);
    return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
}
```

## Authentication & Authorization

### Steam OAuth Integration
- **Steam OpenID** authentication flow in `SteamAuthenticationHandler`
- **JWT tokens** for API authentication
- **Prime subscription** required for creating duels/tournaments
- **Role-based authorization** (User, Moderator, Admin)

### Security Practices
- **Never store passwords** in plain text
- **Use secure JWT secrets** configured via appsettings
- **Validate all inputs** with data annotations
- **Sanitize external API responses**

## External Service Integrations

### DatHost (Game Server Management)
```csharp
// Always handle external API failures gracefully
public async Task<DathostServerResult> CreateTournamentServerAsync(
    Guid tournamentId, 
    Guid matchId, 
    string mapName, 
    List<string> playerSteamIds)
{
    try
    {
        // Create server configuration
        // Start server
        // Return connection details
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating server for match {MatchId}", matchId);
        return new DathostServerResult { Success = false, ErrorMessage = ex.Message };
    }
}
```

### Cryptomus (Payment Processing)
- **Webhook validation** required for all payment confirmations
- **Signature verification** using MD5 hash
- **Idempotent payment processing** to prevent double-processing

### Steam API
- **Rate limiting** considerations for Steam API calls
- **Cache user data** when appropriate
- **Handle Steam API downtime** gracefully

## SignalR Real-time Communication

### Hub Implementation
```csharp
// Tournament updates hub
public class TournamentHub : Hub
{
    public async Task JoinTournamentGroup(string tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tournament_{tournamentId}");
    }
}

// Service notifications
public async Task NotifyTournamentUpdated(Guid tournamentId, TournamentDto tournament)
{
    await _hubContext.Clients.Group($"Tournament_{tournamentId}")
        .SendAsync("TournamentUpdated", tournament);
}
```

## Telegram Bot Integration

### Bot Architecture
- **State-based** conversation flow using `UserStateService`
- **Multilingual** support (Ukrainian, English, Russian)
- **Inline keyboards** for user interactions
- **Admin commands** for moderation

### Message Handling
```csharp
// Always validate user authentication for sensitive operations
public async Task HandleCallbackAsync(CallbackQuery callbackQuery, UserState userState, string data, CancellationToken cancellationToken)
{
    if (!userState.IsAuthenticated && RequiresAuth(data))
    {
        await ShowAuthRequiredAsync(chatId, userState.Language, cancellationToken);
        return;
    }
    
    // Process callback
}
```


## Performance Considerations

### Database Optimization
- **Use Include()** judiciously for navigation properties
- **Implement pagination** for large datasets
- **Add database indexes** on frequently queried fields
- **Use compiled queries** for hot paths

### Caching Strategy
- **Cache Steam user data** for short periods
- **Cache tournament brackets** during active tournaments
- **Use Redis** for distributed caching in production

## Configuration Management

### Environment Variables
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=GGHubDB;..."
  },
  "Steam": {
    "ApiKey": "steam-api-key",
    "CallbackPath": "/auth/steam/callback"
  },
  "Cryptomus": {
    "MerchantId": "merchant-id",
    "PaymentKey": "payment-key"
  },
  "DatHost": {
    "Email": "email",
    "Password": "password",
    "WebhookUrl": "webhook-url"
  }
}
```

*Note:* During `DEBUG` builds `CryptomusService` automatically targets the provider's test API endpoints.

### Security Practices
- **Never commit secrets** to repository
- **Use user secrets** in development
- **Environment-specific** configurations
- **Validate required settings** on startup

## Deployment & DevOps

### Monitoring & Logging
- **Structured logging** with Serilog
- **Performance counters** for API endpoints
- **Error tracking** and alerting
- **Database query monitoring**

## Troubleshooting Common Issues

### Steam Authentication
- **Check API key validity** and rate limits
- **Verify callback URLs** match configuration
- **Handle Steam API timeouts** gracefully

### Payment Processing
- **Verify webhook signatures** before processing
- **Check payment provider status** for failures
- **Implement retry logic** for failed operations

### Tournament Management
- **Validate team sizes** before starting tournaments
- **Handle server creation failures** with proper rollback
- **Ensure proper match result processing**

## Pull Request Guidelines

### Code Review Checklist
- [ ] All new code follows established patterns
- [ ] Proper error handling and logging implemented
- [ ] Database migrations included if schema changes
- [ ] Configuration updated for new features
- [ ] Documentation updated for API changes
- [ ] Tests added for new functionality

### Commit Message Format
```
feat(tournaments): add automated bracket generation

- Implement tournament bracket creation algorithm
- Add SignalR notifications for bracket updates
- Include proper error handling for invalid team counts

Closes #123
```

## Resources & Documentation

### API Documentation
- Swagger UI available at `/swagger` in development
- All endpoints documented with XML comments
- Request/response models clearly defined

### External APIs
- [DatHost API Documentation](https://dathost.net/api)
- [Steam Web API Documentation](https://steamcommunity.com/dev)
- [Cryptomus API Documentation](https://doc.cryptomus.com/)

---