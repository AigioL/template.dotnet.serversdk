using System.Security.Cryptography;

namespace AigioLTemplate.Constants.Properties;

static partial class Resources
{
    internal static RSAParameters GetPublicKey()
    {
        using var stream = Bf1175ab.PublicKey;
        var r = RSAUtils.ReadParameters(stream);
        return r;
    }
}

/// <summary>
/// {ROOT_ProjPath}\ref\serversdk\res\rsa\bf1175ab.resx
/// </summary>
file static class Bf1175ab
{
    /// <summary>
    ///   返回此类使用的缓存的 ResourceManager 实例。
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Resources.ResourceManager ResourceManager
    {
        get
        {
            if (object.ReferenceEquals(field, null))
            {
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("bf1175ab", typeof(Bf1175ab).Assembly);
                field = temp;
            }
            return field;
        }
    }

    /// <summary>
    ///   查找类似于 System.IO.MemoryStream 的 System.IO.UnmanagedMemoryStream 类型的本地化资源。
    /// </summary>
    internal static System.IO.UnmanagedMemoryStream PublicKey
    {
        get
        {
            return ResourceManager.GetStream("_6128a3e8", null)!;
        }
    }
}