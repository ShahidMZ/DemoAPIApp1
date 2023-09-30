using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey key;

    public TokenService(IConfiguration config)
    {
        this.key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public string CreateToken(AppUser user)
    {
        // Add the claim that this user is claiming to be user.UserName.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
        };

        // Specify the credentials to be used.
        var creds = new SigningCredentials(this.key, SecurityAlgorithms.HmacSha512Signature);

        // Specify the descriptor of the token to be created.
        // Add the claims, expiration date of the token and the credentials to be used to the descriptor.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
