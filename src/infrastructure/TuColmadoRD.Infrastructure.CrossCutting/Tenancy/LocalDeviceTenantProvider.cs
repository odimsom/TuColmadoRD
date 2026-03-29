using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TuColmadoRD.Core.Application.DTOs.Tenancy;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Infrastructure.CrossCutting.Tenancy;

public sealed class LocalDeviceTenantProvider : ITenantProvider, IDeviceIdentityStore
{
    private readonly LocalDeviceOptions _options;
    private DeviceIdentity? _cached;

    public LocalDeviceTenantProvider(IOptions<LocalDeviceOptions> options)
    {
        _options = options.Value;
        Load();
    }

    public TenantIdentifier TenantId =>
        _cached is null
            ? TenantIdentifier.Validate(Guid.Empty).TryGetResult(out var e) ? e! : default!
            : TenantIdentifier.Validate(_cached.TenantId).TryGetResult(out var t) ? t! : default!;

    public Guid TerminalId => _cached?.TerminalId ?? Guid.Empty;

    public bool IsPaired => _cached is not null;

    public OperationResult<DeviceIdentity, DevicePairingError> Read() =>
        _cached is null
            ? OperationResult<DeviceIdentity, DevicePairingError>.Bad(DevicePairingError.IoError)
            : OperationResult<DeviceIdentity, DevicePairingError>.Good(_cached);

    public OperationResult<Unit, DevicePairingError> Persist(DeviceIdentity identity)
    {
        try
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(identity);
            var encrypted = Encrypt(json);
            File.WriteAllBytes(_options.IdentityFilePath, encrypted);
            _cached = identity;
            return OperationResult<Unit, DevicePairingError>.Good(Unit.Value);
        }
        catch
        {
            return OperationResult<Unit, DevicePairingError>.Bad(DevicePairingError.IoError);
        }
    }

    private void Load()
    {
        if (!File.Exists(_options.IdentityFilePath))
            return;

        try
        {
            var encrypted = File.ReadAllBytes(_options.IdentityFilePath);
            var json = Decrypt(encrypted);
            _cached = JsonSerializer.Deserialize<DeviceIdentity>(json);
        }
        catch
        {
            _cached = null;
        }
    }

    private static byte[] DeriveKey()
    {
        var raw = $"{Environment.MachineName}{Environment.OSVersion}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(raw));
    }

    private static byte[] Encrypt(byte[] plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(plaintext, 0, plaintext.Length);

        return ms.ToArray();
    }

    private static byte[] Decrypt(byte[] ciphertext)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey();

        var iv = ciphertext[..16];
        var data = ciphertext[16..];
        aes.IV = iv;

        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }
}
