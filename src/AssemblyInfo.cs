using System.Diagnostics;
using System.Reflection;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioLTemplate;

/// <summary>
/// 程序集信息
/// </summary>
static partial class AssemblyInfo
{
    /// <summary>
    /// 提供程序集的说明。
    /// <para><see cref="AssemblyTitleAttribute"/></para>
    /// </summary>
    public const string Title =
#if DEBUG
        $"[Debug] {Trademark}"
#else
        Trademark
#endif
#if PROJ_WEBHOST
        + " (WebHost)"
#elif PROJ_SETUP
        + " (Setup)"
#elif PROJ_TOOLS_PUBLISH
        + " (Publish Tools)"
#elif PROJ_DLLEXPORT
        + " (Dll Export)"
#elif PROJ_DOTNETFX
        + " (.NET FX)"
#else
#endif
#if TARGET_X86
        + " x86"
#elif TARGET_X64
        + " x64"
#elif TARGET_ARM64
        + " arm64"
#endif
        + "";

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// <para><see cref="AssemblyTrademarkAttribute"/></para>
    /// </summary>
    public const string Trademark = "AigioLTemplate"; // 占位符，待定

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// <para><see cref="AssemblyProductAttribute"/></para>
    /// </summary>
    public const string Product = Trademark;

    /// <summary>
    /// 提供程序集的文本说明。
    /// <para><see cref="AssemblyDescriptionAttribute"/></para>
    /// </summary>
    public const string Description = "TODO"; // 对齐 Package.appxmanifest uap:VisualElements

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// <para><see cref="AssemblyCompanyAttribute"/></para>
    /// </summary>
    public const string Company = "TODO TODO Technology Co., Ltd"; // 不可更改

    /// <summary>
    /// 与应用程序关联的版权声明。
    /// <para><see cref="AssemblyCopyrightAttribute"/></para>
    /// </summary>
    public const string Copyright = $"©️ {Company}. All rights reserved."; // 不可更改

    /// <summary>
    /// 是否为调试版本
    /// </summary>
    public const bool Debuggable =
#if DEBUG
true
#else
false
#endif
    ;

    /// <summary>
    /// 验证程序集是否为合法程序集
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <returns></returns>
    public static bool ValidateAssembly(string assemblyPath)
    {
        var fvi = FileVersionInfo.GetVersionInfo(assemblyPath);
        if (fvi.Comments != Description)
        {
            return false;
        }
        else if (fvi.CompanyName != Company)
        {
            return false;
        }
        else if (fvi.LegalCopyright != Copyright)
        {
            return false;
        }
        else if (fvi.LegalTrademarks != Trademark)
        {
            return false;
        }
        else if (fvi.ProductName != Product)
        {
            return false;
        }

        // TODO: Digital signature verification
        return true;
    }

    static bool EqualsCopyright(ReadOnlySpan<char> l, ReadOnlySpan<char> r)
    {
        // 比较字符串相等，忽略中间出现的 char 65039 值
#pragma warning disable IDE0057 // 使用范围运算符
        Span<char> ls = stackalloc char[l.Length];
        int i = 0, index = 0;
        for (; i < l.Length; i++)
        {
            var it = l[i];
            if (it == 65039)
            {
                continue;
            }
            ls[index] = it;
            index++;
        }
        ls = index == ls.Length ? ls : ls.Slice(0, index);

        Span<char> rs = stackalloc char[r.Length];
        i = 0;
        index = 0;
        for (; i < r.Length; i++)
        {
            var it = r[i];
            if (it == 65039)
            {
                continue;
            }
            rs[index] = it;
            index++;
        }
        if (ls.Length != index)
        {
            return false;
        }
        rs = index == rs.Length ? rs : rs.Slice(0, index);
        return ls.SequenceEqual(rs);
#pragma warning restore IDE0057 // 使用范围运算符
    }

    public static bool ValidateRustApp(string assemblyPath)
    {
        var fvi = FileVersionInfo.GetVersionInfo(assemblyPath);
        if (fvi.Comments != Description && !string.IsNullOrEmpty(fvi.Comments))
        {
            return false;
        }
        else if (fvi.CompanyName != Company)
        {
            return false;
        }
        else if (!EqualsCopyright(fvi.LegalCopyright, Copyright))
        {
            return false;
        }
        else if (fvi.LegalTrademarks != Trademark && !string.IsNullOrEmpty(fvi.LegalTrademarks))
        {
            return false;
        }
        else if (fvi.ProductName != Product)
        {
            return false;
        }

        // TODO: Digital signature verification
        return true;
    }

#if ANDROID || IOS || MACCATALYST
    public const string ApplicationId = "com.github.aigiol.template.mobile";
#else
    public const string ApplicationId = "com.github.aigiol.template";
#endif
}
