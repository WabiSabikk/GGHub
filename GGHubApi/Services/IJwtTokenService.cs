using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using GGHubApi.Configuration;

namespace GGHubApi.Services;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string role);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateToken(Guid userId, string role)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(ClaimTypes.Role, role)
    };

        // SOLUTION 1: Ensure minimum key size
        string secretKey = _settings.SecretKey;

        // If key is too short, extend it
        if (Encoding.UTF8.GetBytes(secretKey).Length < 32)
        {
            // Pad key to minimum 32 bytes (256 bits)
            secretKey = secretKey.PadRight(32, '0');
            Console.WriteLine($"Extended key: {secretKey}");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_settings.ExpirationDays),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
