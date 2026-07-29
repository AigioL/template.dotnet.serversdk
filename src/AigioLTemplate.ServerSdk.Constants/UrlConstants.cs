namespace AigioLTemplate.Constants;

static partial class UrlConstants
{
    /// <summary>
    /// 微软应用商店包 Id
    /// </summary>
    public const string MicrosoftStoreId = "";

    /// <summary>
    /// 自定义 URL Scheme 名称
    /// </summary>
    public const string CUSTOM_URL_SCHEME_NAME = "todo";

    /// <summary>
    /// {[SUMMARY]CUSTOM_URL_SCHEME_NAME}://
    /// </summary>
    public const string CUSTOM_URL_SCHEME = $"{CUSTOM_URL_SCHEME_NAME}://";

    /// <summary>
    /// WebView2 运行时常青引导程序下载地址
    /// <para>引导程序是一个小型安装程序，用于下载常青运行时匹配设备体系结构并将其安装在本地</para>
    /// <para>https://developer.microsoft.com/zh-cn/microsoft-edge/webview2</para>
    /// </summary>
    public const string WebView2RuntimeEvergreenBootstrapperDownloadUrl =
        "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

    /// <summary>
    /// <see cref="WebView2RuntimeEvergreenBootstrapperDownloadUrl"/> 下载的文件名，如果响应头中没有获取到值则使用此默认值
    /// </summary>
    public const string WebView2RuntimeEvergreenBootstrapperFileName = "MicrosoftEdgeWebview2Setup.exe";
}
