using System.Security.Cryptography;

namespace Base.Infrastructure.Security;

public static class HashHelper
{
    public static string CreateHash(byte[] data, string hashAlgorithm, int trimByteCount = 0)
    {
        // DATA & ALGORITHM CHECK
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashAlgorithm);

        // CHOOSE HASH ALGORITHM
        HashAlgorithm algorithm = hashAlgorithm.ToUpperInvariant() switch
        {
            "SHA256" or "SHA-256" => SHA256.Create(),
            "SHA384" or "SHA-384" => SHA384.Create(),
            "SHA512" or "SHA-512" => SHA512.Create(),
            "MD5" => MD5.Create(),
            "SHA1" or "SHA-1" => SHA1.Create(),
            _ => throw new ArgumentException($"Hash algorithm '{hashAlgorithm}' is not supported.", nameof(hashAlgorithm))
        };

        // COMPUTE HASH
        using (algorithm)
        {
            var dataToHash = trimByteCount > 0 && data.Length > trimByteCount
                ? data[..trimByteCount]
                : data;

            return Convert.ToHexString(algorithm.ComputeHash(dataToHash));
        }
    }
}
