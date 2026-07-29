using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioLTemplate;

public static partial class DeviceIdExtensions
{
    public static partial void SetDeviceId(this IDeviceId deviceId)
    {
        deviceId.DeviceIdG = DeviceIdHelper.lazy.Value.g;
        deviceId.DeviceIdR = DeviceIdHelper.lazy.Value.r;
        deviceId.DeviceIdN = DeviceIdHelper.lazy.Value.n;
    }
}

file static class DeviceIdHelper
{
    internal static readonly Lazy<(Guid g, string r, string n)> lazy = new(() =>
    {
        // 在单元测试中全部使用固定值返回

        var deviceIdG = Guid.Parse(IDG_DEF); // 固定值
        var deviceIdR = "12345678"; // len 8
        var deviceIdN = "6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b"; // len 64
        return (deviceIdG, deviceIdR, deviceIdN);
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    const string IDG_DEF = "9BC636CF-C8AA-4358-8E22-172B07726A98";
}