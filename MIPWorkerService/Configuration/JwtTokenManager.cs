using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.Configuration
{
    public class JwtTokenManager
    {
        private readonly RSA _rsa;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiresInMinutes;
        private string? _token; //cached token
        private DateTime _tokenExpiration;

        public JwtTokenManager(string privateKey, IConfiguration configuration)
        {
            _rsa = RSA.Create();
            _rsa.ImportFromPem(privateKey);

            _issuer = configuration["JwtSettings:Issuer"]!;
            _audience = configuration["JwtSettings:Audience"]!;
            _expiresInMinutes = int.Parse(configuration["JwtSettings:ExpiresInMinutes"]!);
            _tokenExpiration = DateTime.MinValue;
        }

        public string GetToken()
        {
            if (!string.IsNullOrEmpty(_token) && _tokenExpiration > DateTime.UtcNow)
            {
                return _token; // Return cached token if still valid
            }

            // Generate a new token
            var token = GenerateToken();
            _token = token;

            _tokenExpiration = DateTime.UtcNow.AddMinutes(_expiresInMinutes);

            return token;
        }

        public string GenerateToken()
        {
            var credentials = new SigningCredentials(new RsaSecurityKey(_rsa), SecurityAlgorithms.RsaSha512);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, _issuer), //ASKUG.UNG
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
            new(JwtRegisteredClaimNames.Exp, DateTime.Now.AddMinutes(_expiresInMinutes).ToString()),
            new(ClaimTypes.Name, _issuer)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
