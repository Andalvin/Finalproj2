using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace SampleDeliveryService.Models
{
    /// <summary>
    /// This class is used to provide a basic JWT (Json Web Token) security token service which can issue short lived access tokens to be set on the Cookie responses to the website.
    /// This access token is verfied to be issued from the website and that noone else forged a token. Then an Azure Maps token can be issued.
    /// </summary>
    public class TokenAuthorizationProvider
    {
        readonly JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
        public const string Audience = "AzureMapsToken";
        public const string Issuer = "AnonymousAuthSample";
        private static byte[] privateKey;

        public JwtSecurityToken CreateToken()
        {
            var now = DateTime.UtcNow;
            var signingCredentials = new SigningCredentials(CreateSecurityKey(), SecurityAlgorithms.HmacSha256Signature);

            long jwtTokenId;
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetNonZeroBytes(tokenData);
                jwtTokenId = BitConverter.ToInt64(tokenData);
            }

            // add a cryptographically secured random number representing the session id.
            // this session id can be used to throttle or track much traffic is issued.
            // For additional protection, you can provide client IP address for the case of tracking source location.
            var claims = new List<Claim>() { new Claim("jti", jwtTokenId.ToString()) };
            return jwtTokenHandler.CreateJwtSecurityToken(Issuer, Audience, new ClaimsIdentity(claims), now, now.AddMinutes(10), now, signingCredentials);
        }

        public static SecurityKey CreateSecurityKey()
        {
            return new SymmetricSecurityKey(CreatePrivateKey());
        }

        /// <summary>
        /// In production code, you should consider storing the key in an Azure Key Vault. Secondly you must consider how to support private key rotation scenarios (support old key + new keys)
        /// </summary>
        private static byte[] CreatePrivateKey()
        {
            if (privateKey != null)
            {
                return privateKey;
            }
            using RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            var tokenData = new byte[32];
            rng.GetBytes(tokenData);
            privateKey = tokenData;
            return privateKey;
        }
    }
}