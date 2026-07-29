using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace AigioLTemplate.UnitTest;

public sealed class PrivateKeyTest : BaseUnitTest
{
    static string GetCsFilePath([CallerFilePath] string p = null!) => p;

    static (string thisCsFilePath, string privateKeyFilePath, string publicKeyFilePath) GetFilePaths()
    {
        var thisCsFilePath = GetCsFilePath();
        var privateKeyFilePath = Path.GetFullPath(Path.Combine(thisCsFilePath, "..", "..", "..", "res", "rsa", "privateKey"));
        var publicKeyFilePath = Path.GetFullPath(Path.Combine(thisCsFilePath, "..", "..", "..", "res", "rsa", "publicKey"));
        return (thisCsFilePath, privateKeyFilePath, publicKeyFilePath);
    }

    /// <summary>
    /// 生成 RSA 密钥
    /// </summary>
    [Fact]
    public void Create()
    {
        (var thisCsFilePath, var privateKeyFilePath, var publicKeyFilePath) = GetFilePaths();
        Console.WriteLine($"私钥路径: {privateKeyFilePath}");
        Console.WriteLine($"公钥路径: {publicKeyFilePath}");

        bool fileWrite = false; // 是否将生成的密钥写入文件以更新 resx 资源，通常仅在项目首次初始化时使用
        using var rsa = RSA.Create();

        // 私钥用于解密
        var privateKey = rsa.ExportParameters(true);
#pragma warning disable CS0618 // 类型或成员已过时
        RSAUtils.Parameters privateKeyO = privateKey;
        var privateKeyRowJson = JsonSerializer.Serialize(privateKeyO, RSAUtils.Parameters.GetJsonTypeInfo());
#pragma warning restore CS0618 // 类型或成员已过时
        if (fileWrite)
        {
            var filePath = privateKeyFilePath;
            using MemoryStream stream = new();
            RSAUtils.WriteParameters(stream, privateKey);
            stream.Position = 0;
            var bytes = stream.ToArray();
            File.WriteAllBytes(filePath, bytes);
            File.WriteAllText(filePath + ".row.json", privateKeyRowJson);

            RsaSecurityKey privateKeyW = new(privateKey);
            var privateKeyJ = JsonWebKeyConverter.ConvertFromRSASecurityKey(privateKeyW);
            var privateKeyJson = JsonSerializer.Serialize(privateKeyJ);
            File.WriteAllText(filePath + ".web.json", privateKeyJson);
        }

        // 公钥用于加密
        var publicKey = rsa.ExportParameters(false);
        RsaSecurityKey publicKeyW = new(publicKey);
        var publicKeyJ = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicKeyW);
        var publicKeyJson = JsonSerializer.Serialize(publicKeyJ);
        if (fileWrite)
        {
            var filePath = publicKeyFilePath;
            using MemoryStream stream = new();
            RSAUtils.WriteParameters(stream, publicKey);
            stream.Position = 0;
            var bytes = stream.ToArray();
            File.WriteAllBytes(filePath, bytes);
            File.WriteAllText(filePath + ".web.json", publicKeyJson);
        }

        Console.WriteLine("Private Key:");
        Console.WriteLine(privateKeyRowJson);
        Console.WriteLine("Public Key:");
        Console.WriteLine(publicKeyJson);
    }

    [Fact]
    public void CreateBM()
    {
        using var rsa = RSA.Create();

        // 私钥用于解密
        var privateKey = rsa.ExportParameters(true);
        using MemoryStream memoryStream = new();
        RSAUtils.WriteParameters(memoryStream, privateKey);
        var adminRSAPrivateKey = Convert.ToBase64String(memoryStream.ToArray());

        var publicKey = rsa.ExportParameters(false);
        RsaSecurityKey publicKeyW = new(publicKey);
        var publicKeyJ = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicKeyW);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        var publicKeyJson = JsonSerializer.Serialize(publicKeyJ);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        Console.WriteLine("Private Key:");
        Console.WriteLine(adminRSAPrivateKey);
        Console.WriteLine("Public Key:");
        Console.WriteLine(publicKeyJson);
    }
}