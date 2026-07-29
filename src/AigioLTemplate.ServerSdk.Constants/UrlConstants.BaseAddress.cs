using static AigioLTemplate.Constants._45ad8d8a;
using static AigioLTemplate.Constants.UrlConstants;
using static AigioLTemplate.Constants.UrlConstants.Development;

namespace AigioLTemplate.Constants;

static partial class UrlConstants
{
    /// <summary>
    /// 正式环境
    /// </summary>
    public static partial class Production
    {
        /// <summary>
        /// 官网域名
        /// </summary>
        internal const string OfficialHostName = $"{PrimaryDomain}.{PrimaryDomainPostfix}";

        /// <summary>
        /// WebApi 域名
        /// </summary>
        internal const string OfficialApiHostName = $"api.speedtest.{PrimaryDomain}.{PrimaryDomainPostfix}";
    }

    /// <summary>
    /// 开发环境
    /// </summary>
    public static partial class Development
    {
        /// <summary>
        /// 官网域名
        /// </summary>
        internal const string OfficialHostName = $"dev.{PrimaryDomain}.{PrimaryDomainPostfix}";

        /// <summary>
        /// WebApi 域名
        /// </summary>
        internal const string OfficialApiHostName = $"dev.api.speedtest.{PrimaryDomain}.{PrimaryDomainPostfix}";

        /// <summary>
        /// 仅 IPv6 开发环境
        /// </summary>
        public static partial class Ipv6Only
        {
            /// <summary>
            /// 官网域名
            /// </summary>
            internal const string OfficialHostName = $"ipv6.dev.{PrimaryDomain}.{PrimaryDomainPostfix}";

            /// <summary>
            /// WebApi 域名
            /// </summary>
            internal const string OfficialApiHostName = $"ipv6.dev.api.speedtest.{PrimaryDomain}.{PrimaryDomainPostfix}";
        }

        /// <summary>
        /// 本地调试环境
        /// </summary>
        public static partial class Localhost
        {
            /// <summary>
            /// 官网域名
            /// </summary>
            internal const string OfficialHostName = "localhost:5001";

            /// <summary>
            /// WebApi 域名
            /// </summary>
            internal const string OfficialApiHostName = "localhost:5002";
        }
    }

    /// <summary>
    /// WebApi 基地址
    /// </summary>
    public static string ApiBaseUrl => _45ad8d8a.ApiBaseUrl;

    /// <summary>
    /// 官网网址
    /// </summary>
    public static string OfficialWebsite => _45ad8d8a.OfficialWebsite;
}

static partial class UrlConstants_
{
    /// <inheritdoc cref="UrlConstants.ApiBaseUrl"/>
    internal static string ApiBaseUrl
    {
        set => _45ad8d8a.ApiBaseUrl = value;
    }

    /// <inheritdoc cref="UrlConstants.OfficialWebsite"/>
    internal static string OfficialWebsite
    {
        set => _45ad8d8a.OfficialWebsite = value;
    }
}

file static class _45ad8d8a
{
    /// <summary>
    /// 主域名后缀，例如 com,net,cn
    /// </summary>
    internal const string PrimaryDomainPostfix = "com";

    /// <summary>
    /// 主域名
    /// </summary>
    internal const string PrimaryDomain = "todo"; // 待定

    internal static string ApiBaseUrl
    {
        get
        {
            if (field == null)
            {
#if DEBUG
                field = $"https://{OfficialApiHostName}";
#else
                field = $"https://{Production.OfficialApiHostName}";
#endif
            }
            return field;
        }
        set
        {
            switch (value)
            {
                case nameof(Production):
                    field = $"https://{Production.OfficialApiHostName}";
                    break;
                case nameof(Development):
                    field = $"https://{OfficialApiHostName}";
                    break;
                case nameof(Localhost):
                    field = $"https://{Localhost.OfficialApiHostName}";
                    break;
                case nameof(Ipv6Only):
                    field = $"https://{Ipv6Only.OfficialApiHostName}";
                    break;
            }
        }
    }

    internal static string OfficialWebsite
    {
        get
        {
            if (field == null)
            {
#if DEBUG
                field = $"https://{OfficialHostName}";
#else
                field = $"https://{Production.OfficialHostName}";
#endif
            }
            return field;
        }
        set
        {
            switch (value)
            {
                case nameof(Production):
                    field = $"https://{Production.OfficialHostName}";
                    break;
                case nameof(Development):
                    field = $"https://{OfficialHostName}";
                    break;
                case nameof(Localhost):
                    field = $"https://{Localhost.OfficialHostName}";
                    break;
                case nameof(Ipv6Only):
                    field = $"https://{Ipv6Only.OfficialHostName}";
                    break;
            }
        }
    }

    // TODO: 将常量字符串通过加密藏匿，因生成的二进制中可以轻易逆向出包含的常量值
}
