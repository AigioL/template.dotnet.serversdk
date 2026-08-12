using System.Reflection;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioLTemplate.VSAppCenter.Helpers;

/// <summary>
/// Visual Studio App Center
/// <list type="bullet">
/// <item>将移动开发人员常用的多种服务整合到一个集成的产品中。</item>
/// <item>您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。</item>
/// <item>https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin</item>
/// <item>https://visualstudio.microsoft.com/zh-hans/app-center</item>
/// </list>
/// </summary>
static partial class VisualStudioAppCenterSDK
{
    /// <summary>
    /// 初始化 Visual Studio App Center
    /// </summary>
    internal static partial void Init();

    /// <summary>
    /// 设置用户 Id，App Center SDK 将使用该 Id 来标识用户，以便在分析和崩溃报告中提供更有意义的数据
    /// </summary>
    /// <param name="userId"></param>
    internal static partial void SetUserId(Guid? userId);
}
