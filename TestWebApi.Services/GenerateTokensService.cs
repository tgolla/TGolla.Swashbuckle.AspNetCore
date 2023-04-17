using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TestWebApi.Services
{
    /// <summary>
    /// The generate tokens service.
    /// </summary>
    public class GenerateTokensService : IGenerateTokensService
    {
        private readonly JwtSettings jwtSettings;

        /// <summary>
        /// Initializes a new instance of TestWebApiService.
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public GenerateTokensService(IConfiguration configuration)
        {
            jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        }

        /// <summary>
        /// Generates a JWT token.
        /// </summary>
        /// <param name="privateKey">The JWT RSA private key.</param>
        /// <param name="issuer">The token issuer.</param>
        /// <param name="audience">The token audience.</param>
        /// <param name="expires">The number of seconds before the token expires.</param>
        /// <param name="email">The user email.</param>
        /// <param name="givenName">The user's given name.</param>
        /// <param name="familyName">The user's family name.</param>
        /// <param name="groups">A list of the user's group roles.</param>
        /// <returns>A JWT token.</returns>
        public string GenerateToken(string privateKey, string issuer, string audience, uint expires, 
            string email, string givenName, string familyName, IList<string> groups)
        {
            // Creating the RSA key.
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.ImportRSAPrivateKey(new ReadOnlySpan<byte>(Convert.FromBase64String(privateKey)), out _);
            RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(provider);

            // Generating the token. 
            TimeSpan timeSinceEpoch = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int secondsSinceEpoch = (int)timeSinceEpoch.TotalSeconds;

            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Aud, audience));
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Iat, secondsSinceEpoch.ToString()));
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Exp, (secondsSinceEpoch + expires).ToString()));
            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Iss, issuer));
            claimsIdentity.AddClaim(new Claim("email", email));
            claimsIdentity.AddClaim(new Claim("given_name", givenName));
            claimsIdentity.AddClaim(new Claim("family_name", familyName));
            claimsIdentity.AddClaim(new Claim("groups", JsonSerializer.Serialize(groups), JsonClaimValueTypes.JsonArray));

            var credentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            var snowflakeAccessToken = new JwtSecurityToken(
                claims: claimsIdentity.Claims,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(snowflakeAccessToken);
        }

        /// <summary>
        /// Generates an authenticated user with no group roles.
        /// </summary>
        /// <returns>An authenticated user with no group roles.</returns>
        public string GenerateUserTokenNoGroups()
        {
            IList<string> groups = new List<string>();

            return GenerateToken(jwtSettings.PrivateKey, jwtSettings.Issuer, jwtSettings.Audience, 3600,
                "someone@domain.com", "Some", "User", groups);
        }

        /// <summary>
        /// Generates a management user.
        /// </summary>
        /// <returns>A management user.</returns>
        public string GenerateManageToken()
        {
            IList<string> groups = new List<string> { "Manager" };

            return GenerateToken(jwtSettings.PrivateKey, jwtSettings.Issuer, jwtSettings.Audience, 3600,
                "management@domain.com", "Management", "User", groups);
        }

        /// <summary>
        /// Generates an administration user.        
        /// </summary>
        /// <returns>An administration user.</returns>
        public string GenerateAdministratorToken()
        {
            IList<string> groups = new List<string> { "Administrator" };

            return GenerateToken(jwtSettings.PrivateKey, jwtSettings.Issuer, jwtSettings.Audience, 3600,
                "administrator@domain.com", "Administrative", "User", groups);
        }

        /// <summary>
        /// Generates a management user with additional administrative privileges.
        /// </summary>
        /// <returns>A management user with additional administrative privileges.</returns>
        public string GenerateManagerAdministratorToken()
        {
            IList<string> groups = new List<string>{ "Manager", "Administrator" };

            return GenerateToken(jwtSettings.PrivateKey, jwtSettings.Issuer, jwtSettings.Audience, 3600,
                "management.administrator@domain.com", "Management", "User w/Administrator", groups);
        }
    }
}
