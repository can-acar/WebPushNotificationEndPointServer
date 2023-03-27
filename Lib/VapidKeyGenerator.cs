using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Lib;

public class VapidKeyGenerator
{
    public static (string PublicKey, string PrivateKey) GenerateVapidKeys()
    {
        X9ECParameters curve = SecNamedCurves.GetByName("secp256r1");
        ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        var generator = new ECKeyPairGenerator();
        var keyGenParam = new ECKeyGenerationParameters(domain, new SecureRandom());
        generator.Init(keyGenParam);

        AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        var publicKeyParams = (ECPublicKeyParameters) keyPair.Public;
        var privateKeyParams = (ECPrivateKeyParameters) keyPair.Private;

        byte[] publicKeyBytes = publicKeyParams.Q.GetEncoded(false);
        byte[] privateKeyBytes = privateKeyParams.D.ToByteArrayUnsigned();

        string publicKey = Convert.ToBase64String(publicKeyBytes);
        string privateKey = Convert.ToBase64String(privateKeyBytes);

        return (publicKey, privateKey);
    }
}