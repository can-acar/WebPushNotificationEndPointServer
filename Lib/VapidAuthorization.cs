using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Lib;

public class VapidAuthorization
{
    public static string GetVapidAuthorizationHeader(string audience, string? subject, string privateKey, string publicKey)
    {
        var token = GenerateJwtToken(audience, subject, privateKey);
        var authHeader = "vapid t=" + token + ", k=" + publicKey;

        return authHeader;
    }

    public static string GenerateJwtToken(string audience, string subject, string privateKey)
    {
        var securityKey = ConvertToEcDsaSecurityKey(privateKey);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = audience,
            Issuer = subject,
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256),
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(securityToken);

        return jwtToken;
    }

    private static ECDsaSecurityKey ConvertToEcDsaSecurityKey(string privateKey)
    {
        var privateKeyBytes = Convert.FromBase64String(privateKey);

        var d = new Org.BouncyCastle.Math.BigInteger(+1, privateKeyBytes);

        using var ecdsa = ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = d.ToByteArrayUnsigned()
        });

        return new ECDsaSecurityKey(ecdsa);
    }
}


//string vapidAuthHeader = VapidAuthorization.GetVapidAuthorizationHeader(audience, VapidSubject, VapidPrivateKey, VapidPublicKey);