using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioLTemplate.ServerSdk.Models;

[JsonSerializable(typeof(ApiRsp))]
[JsonSerializable(typeof(ApiRsp<bool>))]
[JsonSerializable(typeof(ApiRsp<bool[]>))]
[JsonSerializable(typeof(ApiRsp<byte>))]
[JsonSerializable(typeof(ApiRsp<sbyte>))]
[JsonSerializable(typeof(ApiRsp<ushort>))]
[JsonSerializable(typeof(ApiRsp<short>))]
[JsonSerializable(typeof(ApiRsp<uint>))]
[JsonSerializable(typeof(ApiRsp<int>))]
[JsonSerializable(typeof(ApiRsp<int[]>))]
[JsonSerializable(typeof(ApiRsp<ulong>))]
[JsonSerializable(typeof(ApiRsp<long>))]
[JsonSerializable(typeof(ApiRsp<Guid>))]
[JsonSerializable(typeof(ApiRsp<Guid[]>))]
[JsonSerializable(typeof(ApiRsp<float>))]
[JsonSerializable(typeof(ApiRsp<double>))]
[JsonSerializable(typeof(ApiRsp<decimal>))]
[JsonSerializable(typeof(ApiRsp<DateOnly>))]
[JsonSerializable(typeof(ApiRsp<DateTime>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset>))]
[JsonSerializable(typeof(ApiRsp<bool?>))]
[JsonSerializable(typeof(ApiRsp<byte?>))]
[JsonSerializable(typeof(ApiRsp<sbyte?>))]
[JsonSerializable(typeof(ApiRsp<ushort?>))]
[JsonSerializable(typeof(ApiRsp<short?>))]
[JsonSerializable(typeof(ApiRsp<uint?>))]
[JsonSerializable(typeof(ApiRsp<int?>))]
[JsonSerializable(typeof(ApiRsp<ulong?>))]
[JsonSerializable(typeof(ApiRsp<long?>))]
[JsonSerializable(typeof(ApiRsp<Guid?>))]
[JsonSerializable(typeof(ApiRsp<float?>))]
[JsonSerializable(typeof(ApiRsp<double?>))]
[JsonSerializable(typeof(ApiRsp<decimal?>))]
[JsonSerializable(typeof(ApiRsp<DateOnly?>))]
[JsonSerializable(typeof(ApiRsp<DateTime?>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset?>))]
[JsonSerializable(typeof(ApiRsp<string>))]
[JsonSerializable(typeof(ApiRsp<string[]>))]
[JsonSerializable(typeof(ApiRsp<nil>))]
[JsonSerializable(typeof(ApiRsp<nil?>))]
[JsonSerializable(typeof(ApiRsp<JsonWebTokenValue?>))]
[JsonSerializable(typeof(ApiRsp<MembershipInfo?>))]
[JsonSourceGenerationOptions]
public sealed partial class ServerSdkJsonSerializerContext : JsonSerializerContext
{
    static ServerSdkJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new ServerSdkJsonSerializerContext(o);
    }
}
