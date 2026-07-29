using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;

namespace AigioLTemplate.ServerSdk.Services.Abstractions;

/// <summary>
/// WebApi 服务（客户端侧调用 SDK Client）
/// </summary>
public partial interface IServerSdkWebApiService
{
    /// <summary>
    /// 获取用于 HTTP 请求头的 Referrer 值
    /// </summary>
    string Referrer { get; }

    AuthenticationHeaderValue? GetAuthenticationHeaderValue(JsonWebTokenValue? jwt);

    /// <summary>
    /// 保存用户登录凭证
    /// </summary>
    Task SaveAuthTokenAsync(JsonWebTokenValue authToken);

    /// <summary>
    /// 当登录完成时
    /// </summary>
    Task OnLoginedAsync(
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? email,
        Guid? userId,
        UserInfoModel? userInfo,
        JsonWebTokenValue? authToken);

    ValueTask SetCurrentUserInfoAsync(UserInfoModel value, bool updateToDataBase);

    Task<ApiRsp<TResponseModel?>> SendAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequestModel,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        CancellationToken cancellationToken = default);

    HubConnection CreateHubConnection(string url);
}
