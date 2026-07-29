using System.Security.Cryptography;

namespace AigioLTemplate.ServerSdk.Models.Abstractions;

/// <summary>
/// 应用程序机密项只读接口
/// </summary>
public interface IAppSecrets
{
    /// <summary>
    /// 公钥
    /// </summary>
    RSA PublicKey { get; }
}
