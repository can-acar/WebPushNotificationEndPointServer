using System.Security.Cryptography;

namespace Lib;

public static class VapidKeyGenerator
{
    public static VapidKeys GenerateVapidKeys()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var publicKey = key.ExportSubjectPublicKeyInfo();
        var privateKey = key.ExportECPrivateKey();

        return new VapidKeys
        {
            PublicKey = Convert.ToBase64String(publicKey),
            PrivateKey = Convert.ToBase64String(privateKey)
        };
    }
}

// public class VapidKeyGenerator
// {
//     public static (string PublicKey, string PrivateKey) GenerateVapidKeys()
//     {
//         X9ECParameters curve = SecNamedCurves.GetByName("secp256r1");
//         ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
//
//         var generator = new ECKeyPairGenerator();
//         var keyGenParam = new ECKeyGenerationParameters(domain, new SecureRandom());
//         generator.Init(keyGenParam);
//
//         AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
//
//         var publicKeyParams = (ECPublicKeyParameters) keyPair.Public;
//         var privateKeyParams = (ECPrivateKeyParameters) keyPair.Private;
//
//         byte[] publicKeyBytes = publicKeyParams.Q.GetEncoded(false);
//         byte[] privateKeyBytes = privateKeyParams.D.ToByteArrayUnsigned();
//
//         string publicKey = Convert.ToBase64String(publicKeyBytes);
//         string privateKey = Convert.ToBase64String(privateKeyBytes);
//
//         return (publicKey, privateKey);
//     }
// }