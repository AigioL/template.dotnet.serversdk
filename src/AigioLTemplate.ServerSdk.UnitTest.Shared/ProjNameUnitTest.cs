using AigioLTemplate.Constants;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using RsaRes = AigioLTemplate.Constants.Properties.Resources;

namespace AigioLTemplate.UnitTest;

public sealed class ProjNameUnitTest
{
    [Fact]
    public void ProjNameCheck()
    {
        RSAParameters? rsaParams = null;
        try
        {
            rsaParams = RsaRes.GetPublicKey();
        }
        catch
        {
        }

        var asmName = typeof(ProjNameUnitTest).Assembly.FullName;
        bool isMoBan = asmName != null && asmName.StartsWith(Encoding.UTF8.GetString(Convert.FromBase64String("QWlnaW9MVGVtcGxhdGUu")));
        if (isMoBan)
        {
            return;
        }

        if (rsaParams == null)
        {
            throw new InvalidOperationException(@"res\rsa\publicKey 文件未正确写入数据，调用 PrivateKeyTest.Create 创建密钥数据");
        }
        if (string.Equals("todo.com", UrlConstants.Production.OfficialHostName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals("todo.todo", UrlConstants.Production.OfficialHostName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("常量 PrimaryDomain 或 PrimaryDomainPostfix 未正确设置");
        }
        if (string.IsNullOrWhiteSpace(UrlConstants.MicrosoftStoreId))
        {
            throw new InvalidOperationException("常量 MicrosoftStoreId 未正确设置");
        }
        if (string.Equals("todo", UrlConstants.CUSTOM_URL_SCHEME_NAME, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("常量 CUSTOM_URL_SCHEME_NAME(自定义 URL Scheme 名称) 未正确设置");
        }
    }
}
