namespace Frends.MQTT.Receive.Helpers;

using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Shared certificate loading utility for MQTT client authentication.
/// </summary>
internal static class CertificateLoader
{
    /// <summary>
    /// Returns appropriate X509KeyStorageFlags based on whether the process
    /// is running as a Windows service (machine key store) or interactively (user key store).
    /// </summary>
    /// <returns>X509KeyStorageFlags</returns>
    internal static X509KeyStorageFlags GetKeyStorageFlags()
    {
        bool isService = !Environment.UserInteractive;
        var locationFlag = isService
            ? X509KeyStorageFlags.MachineKeySet
            : X509KeyStorageFlags.UserKeySet;

        return locationFlag | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
    }

    /// <summary>
    /// Loads a certificate from the Windows certificate store by thumbprint.
    /// </summary>
    /// <returns>X509Certificate2</returns>
    internal static X509Certificate2 LoadFromStore(string thumbprint, StoreName storeName, StoreLocation storeLocation)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);

        thumbprint = thumbprint.Replace(" ", string.Empty).ToUpperInvariant();

        var certs = store.Certificates
            .Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

        if (certs.Count == 0)
            throw new ArgumentException($"Certificate with thumbprint '{thumbprint}' not found.");

        var cert = certs[0];

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("Certificate does not contain a private key.");

        return cert;
    }

    /// <summary>
    /// Loads a certificate from a PFX/P12 or PEM/CRT file.
    /// For PEM, a separate private key file path is required.
    /// </summary>
    internal static X509Certificate2 LoadFromFile(string certificateFilePath, string? keyFilePath = null, string? password = null)
    {
        if (!File.Exists(certificateFilePath))
            throw new FileNotFoundException("Certificate file not found.", certificateFilePath);

        var extension = Path.GetExtension(certificateFilePath).ToLowerInvariant();

        if (extension is ".pfx" or ".p12")
        {
            return LoadFromPfx(certificateFilePath, password);
        }
        else if (extension is ".crt" or ".pem")
        {
            return LoadFromPem(certificateFilePath, keyFilePath);
        }
        else
        {
            throw new NotSupportedException($"Unsupported certificate file extension: {extension}");
        }
    }

    /// <summary>
    /// Loads a certificate from a base64-encoded PFX string.
    /// </summary>
    internal static X509Certificate2 LoadFromBase64(string base64, string password = null)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new ArgumentException("Base64 certificate string cannot be empty.");

        var raw = Convert.FromBase64String(base64);

        var cert = new X509Certificate2(raw, password, GetKeyStorageFlags());

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("Certificate does not contain a private key.");

        return cert;
    }

    private static X509Certificate2 LoadFromPfx(string path, string password)
    {
        var cert = new X509Certificate2(
            path,
            password ?? string.Empty,
            GetKeyStorageFlags());

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("The PFX certificate does not contain a private key.");

        return cert;
    }

    private static X509Certificate2 LoadFromPem(string certPath, string keyPath)
    {
        if (string.IsNullOrEmpty(keyPath))
            throw new ArgumentException("Private key file path is required for PEM certificates.");

        if (!File.Exists(keyPath))
            throw new FileNotFoundException("Private key file not found.", keyPath);

        // CreateFromPemFile produces an ephemeral key — PFX round-trip forces
        // the private key into the Windows key store so SChannel can use it
        using var ephemeral = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        var pfxBytes = ephemeral.Export(X509ContentType.Pfx);

        var cert = new X509Certificate2(pfxBytes, (string)null, GetKeyStorageFlags());

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("The PEM certificate or key is invalid or missing the private key.");

        return cert;
    }
}