using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CVGeneratorAPI.Services;

public class TokenService(IOptions<JwtSettings> jwt)
{
    private readonly JwtSettings _jwt = jwt.Value;

    public string Create(UserModel user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id!), // used by CVController
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        // add one "role" claim per role (ASP.NET maps these to ClaimTypes.Role)
        foreach (var role in user.Roles ?? Array.Empty<string>())
            claims.Add(new(ClaimTypes.Role, role));
            
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
