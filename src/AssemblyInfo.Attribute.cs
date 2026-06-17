using System.Reflection;
using AssemblyInfo = global::AigioLTemplate.Server.AssemblyInfo;

#if (WINDOWS7_0_OR_GREATER || WINDOWS) && NET5_0_OR_GREATER
[assembly: global::System.Runtime.Versioning.SupportedOSPlatform("Windows10.0.17763")]
#endif
[assembly: AssemblyTitle(AssemblyInfo.Title)]
[assembly: AssemblyTrademark(AssemblyInfo.Trademark)]
[assembly: AssemblyDescription(
#if PROJ_TOOLS_PUBLISH_REFERENCE
    $"{A.Trademark} Publish Tools 3rd Party Application Component"
#elif PROJ_TOOLS_PUBLISH
    $"{A.Trademark} Publish Tools"
#else
    AssemblyInfo.Description
#endif
    )]
[assembly: AssemblyProduct(AssemblyInfo.Product)]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyCompany(AssemblyInfo.Company)]
//[assembly: AssemblyVersion(AssemblyInfo.Version)]
//[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]
//[assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersion)]