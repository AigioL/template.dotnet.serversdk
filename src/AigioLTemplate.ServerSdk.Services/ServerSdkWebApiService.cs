using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.Devices;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioLTemplate;
using AigioLTemplate.Constants;
using AigioLTemplate.ServerSdk.Models;
using AigioLTemplate.ServerSdk.Models.Abstractions;
using AigioLTemplate.ServerSdk.Models.Identity;
using AigioLTemplate.ServerSdk.Services;
using AigioLTemplate.ServerSdk.Services.Abstractions;
using AigioLTemplate.VSAppCenter.Helpers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 <see cref="IServerSdkWebApiService"/> 服务
    /// </summary>
    public static IServiceCollection AddServerSdkWebApiService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSecrets,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserInfoModel>(this IServiceCollection services)
        where TAppSecrets : class, IAppSecrets
        where TUserInfoModel : IReadOnlyId<Guid>
    {
        services.TryAddSingleton<IUserStore<TUserInfoModel>, UserStore<TUserInfoModel>>();
        services.AddSingleton<IServerSdkWebApiService, S3dfab2fb<TAppSecrets>>();
        return services;
    }
}

file sealed partial class S3dfab2fb<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSecrets> : IServerSdkWebApiService
    where TAppSecrets : class, IAppSecrets
{
    /// <summary>
    /// 默认超时时间，20 秒
    /// </summary>
    const int DefaultTimeoutMilliseconds = 20000;

    readonly RecyclableMemoryStreamManager m = new();
    readonly IUserStore<UserInfoModel> userStore;
    readonly HttpClient client;
    readonly ILogger logger;

    public S3dfab2fb(
        ILoggerFactory loggerFactory,
        IOptions<TAppSecrets> options,
        IUserStore<UserInfoModel> userStore,
        IVersionTracking versionTracking,
        IDeviceInfo deviceInfo)
    {
        logger = loggerFactory.CreateLogger("ServerSdkWebApiService");
        this.userStore = userStore;
        var referrer = $"{UrlConstants.CUSTOM_URL_SCHEME}{deviceInfo.Platform}/{versionTracking.CurrentVersion}";
        Referrer = new(referrer, UriKind.Absolute);
        RSAInstance = options.Value.PublicKey;
        SocketsHttpHandler handler = new()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
        client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(DefaultTimeoutMilliseconds),
        };
    }

    string IServerSdkWebApiService.Referrer => Referrer.ToString();

    RSA RSAInstance { get; }

    Uri Referrer { get; }

    /// <inheritdoc/>
    public async Task SaveAuthTokenAsync(JsonWebTokenValue authToken)
    {
        var user = await userStore.GetCurrentUserAsync(false);
        if (user != null)
        {
            user.AuthToken = authToken;
            await userStore.SetCurrentUserAsync(user);
        }
    }

    /// <inheritdoc/>
    public async Task OnLoginedAsync(
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? email,
        Guid? userId,
        UserInfoModel? userInfo,
        JsonWebTokenValue? authToken)
    {
        userId ??= userInfo?.Id;
        try
        {
            if (userInfo != null)
            {
                await userStore.SetCurrentUserInfoAsync(userInfo, true);
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(email) && userId.HasValue)
            {
                CurrentUser cUser = new()
                {
                    UserId = userId.Value,
                    AuthToken = authToken,
                    PhoneNumber = phoneNumber,
                    PhoneNumberRegionCode = phoneNumberRegionCode,
                    Email = email,
                };
                await userStore.SetCurrentUserAsync(cUser);
            }
        }
        finally
        {
            VisualStudioAppCenterSDK.SetUserId(userId);
        }
    }

    public ValueTask SetCurrentUserInfoAsync(UserInfoModel value, bool updateToDataBase)
    {
        return userStore.SetCurrentUserInfoAsync(value, updateToDataBase);
    }

    /// <summary>
    /// 获取请求正文
    /// </summary>
    async Task<HttpContent?> GetRequestContentAsync<TRequestModel>(
        bool isSecurity,
        Aes? aes,
        SerializableImplType serializableImplType,
        TRequestModel? requestModel,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        if (requestModel != null)
        {
            if (requestModel is IDeviceId deviceId)
            {
                deviceId.SetDeviceId();
            }

            if (isSecurity)
            {
                ArgumentNullException.ThrowIfNull(aes);
                switch (serializableImplType)
                {
                    case SerializableImplType.SystemTextJson:
                        {
                            using var serializeStream = m.GetStream(); // 创建内存流用于 Json 序列化
                            if (requestModel is JsonElement jsonElement)
                            {
                                Utf8JsonWriter writer = new((IBufferWriter<byte>)serializeStream, SerializerConstants.DefaultJsonWriterOptions);
                                jsonElement.WriteTo(writer);
                                await writer.FlushAsync(cancellationToken);
                            }
                            else
                            {
                                ArgumentNullException.ThrowIfNull(jsonRequestTypeInfo);
                                await JsonSerializer.SerializeAsync(serializeStream, requestModel, jsonRequestTypeInfo, cancellationToken);
                            }
                            serializeStream.Position = 0;

                            var cipherStream = m.GetStream(); // 创建内存流用于存储加密后的密文数据
                            using CryptoStream cryptoStream = new(cipherStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
                            await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
                            await cryptoStream.FlushFinalBlockAsync(cancellationToken);
                            cipherStream.Position = 0;
                            var r = new StreamContent(cipherStream);
                            r.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.JSONSecurity);
                            return r;
                        }
                    case SerializableImplType.MemoryPack:
                        {
                            throw new NotImplementedException("尚未实现");
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }
            }
            else
            {
                switch (serializableImplType)
                {
                    case SerializableImplType.SystemTextJson:
                        {
                            var serializeStream = m.GetStream();
                            if (requestModel is JsonElement jsonElement)
                            {
                                Utf8JsonWriter writer = new((IBufferWriter<byte>)serializeStream, SerializerConstants.DefaultJsonWriterOptions);
                                jsonElement.WriteTo(writer);
                                await writer.FlushAsync(cancellationToken);
                            }
                            else
                            {
                                ArgumentNullException.ThrowIfNull(jsonRequestTypeInfo);
                                await JsonSerializer.SerializeAsync(serializeStream, requestModel, jsonRequestTypeInfo, cancellationToken);
                            }
                            serializeStream.Position = 0;
                            var r = new StreamContent(serializeStream);
                            r.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.JSON);
                            return r;
                        }
                    case SerializableImplType.MemoryPack:
                        {
                            throw new NotImplementedException("尚未实现");
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }
            }
        }
        return null;
    }

    const string Basic = "Bearer";

    public AuthenticationHeaderValue? GetAuthenticationHeaderValue(JsonWebTokenValue? jwt)
    {
        if (jwt.HasValue())
        {
            var authHeaderValue = new AuthenticationHeaderValue(Basic, jwt.AccessToken);
            return authHeaderValue;
        }
        return null;
    }

    /// <summary>
    /// 设置请求中的授权头
    /// </summary>
    async ValueTask<JsonWebTokenValue?> SetRequestHeaderAuthorization(HttpRequestMessage request)
    {
        var currentUser = await userStore.GetCurrentUserAsync(false);
        var authToken = currentUser?.AuthToken;
        var authHeaderValue = GetAuthenticationHeaderValue(authToken);
        if (authHeaderValue != null)
        {
            request.Headers.Authorization = authHeaderValue;
            return authToken;
        }
        return null;
    }

    async Task<string?> AccessTokenProvider()
    {
        var currentUser = await userStore.GetCurrentUserAsync(false);
        var authToken = currentUser?.AuthToken;
        if (authToken.HasValue())
        {
            return authToken.AccessToken;
        }
        return null;
    }

    const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0";

    void HandleHttpRequest(HttpRequestMessage request)
    {
        request.Version = HttpVersion.Version20;
        //request.VersionPolicy = HttpVersionPolicy.RequestVersionExact; // 强制使用 HTTP/2
        SetHeaders(request.Headers);
    }

    void SetHeaders(HttpRequestHeaders headers)
    {
        headers.AcceptLanguage.ParseAdd(CultureInfo.CurrentUICulture.Name);
        headers.Referrer = Referrer;
        headers.UserAgent.ParseAdd(UA);
    }

    void SetHeaders(IDictionary<string, string> headers)
    {
        headers.Add("Accept-Language", CultureInfo.CurrentUICulture.Name);
        headers.Add("Referer", Referrer.ToString());
        headers.Add("User-Agent", UA);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAppObsolete(HttpResponseHeaders headers)
        => headers.TryGetValues(ApiConstants.Headers_AppObsolete, out var values) &&
            values.Contains(bool.TrueString, StringComparer.OrdinalIgnoreCase);

    async Task<JsonDocument?> ReadAsJsonDocumentAsync(
        HttpContent content,
        bool isSecurity,
        Aes? aes,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value == 0)
        {
            return null;
        }

        bool isJSONSecurity = false;
        if (isSecurity)
        {
            var contentType = content.Headers.ContentType;
            if (contentType != null)
            {
                if (string.Equals(contentType.MediaType, MediaTypeNames.JSONSecurity))
                {
                    isJSONSecurity = true;
                }
            }
        }

        if (isSecurity && isJSONSecurity)
        {
            ArgumentNullException.ThrowIfNull(aes);
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            using CryptoStream cryptoStream = new(stream, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);

            using var memoryStream = m.GetStream();
            await cryptoStream.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;
            var jsonDoc = await JsonDocument.ParseAsync(memoryStream, SerializerConstants.DefaultJsonDocumentOptions, cancellationToken);
            return jsonDoc;
        }
        else
        {
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            var jsonDoc = await JsonDocument.ParseAsync(stream, SerializerConstants.DefaultJsonDocumentOptions, cancellationToken);
            return jsonDoc;
        }
    }

    async Task<ApiRsp<TResponseModel?>> ReadAsResponseModelAsync<TResponseModel>(
        HttpResponseMessage response,
        bool isSecurity,
        Aes? aes,
        JsonTypeInfo<ApiRsp<TResponseModel?>> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        ApiRsp<TResponseModel?> GetStatusCodeApiRsp()
        {
            ApiRsp<TResponseModel?> r = new()
            {
                Code = unchecked((uint)response.StatusCode),
            };
            return r;
        }

        var content = response.Content;
        if (content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value == 0)
        {
            return GetStatusCodeApiRsp();
        }

        bool isJSONSecurity = false;
        if (isSecurity)
        {
            var contentType = content.Headers.ContentType;
            if (contentType != null)
            {
                if (string.Equals(contentType.MediaType, MediaTypeNames.JSONSecurity))
                {
                    isJSONSecurity = true;
                }
            }
        }

        if (isSecurity && isJSONSecurity)
        {
            ArgumentNullException.ThrowIfNull(aes);
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            using CryptoStream cryptoStream = new(stream, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);

            using var memoryStream = m.GetStream();
            await cryptoStream.CopyToAsync(memoryStream, cancellationToken);
            //await cryptoStream.FlushFinalBlockAsync(cancellationToken); // throws NotSupportedException

            memoryStream.Position = 0;
            var r = await JsonSerializer.DeserializeAsync(memoryStream, jsonTypeInfo, cancellationToken);
            return r ?? GetStatusCodeApiRsp();
        }
        else
        {
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            var r = await JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken);
            return r ?? GetStatusCodeApiRsp();
        }
    }

    Task<bool>? taskRefreshTokenWithSaveAsync;

    async Task<bool> RefreshTokenWithSaveCoreAsync(Uri baseAddress, JsonWebTokenValue jwt)
    {
        var requestUri = new Uri("identity/v5/account/refreshtoken", UriKind.Relative);
        requestUri = new(baseAddress, requestUri);
        RefreshTokenRequest requestModel = new()
        {
            RefreshToken = jwt.RefreshToken,
        };
        var rsp = await SendAsync<RefreshTokenRequest, JsonWebTokenValue>(
            baseAddress,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                return request;
            },
            requestModel,
            isAnonymous: true, // 刷新 Token 必须匿名身份，否则将在客户端上递归导致死循环
            isSecurity: true);
        if (rsp.IsSuccess() && rsp.Content != null)
        {
            await SaveAuthTokenAsync(rsp.Content);
            return true;
        }
        else if (rsp.Code != unchecked((uint)ApiRspCode.Unauthorized))
        {
            logger.LogWarning("RefreshToken fail, Code: {0}", rsp.Code);
        }
        return false;
    }

    async Task<bool> RefreshTokenWithSaveAsync(Uri baseAddress, JsonWebTokenValue jwt)
    {
        taskRefreshTokenWithSaveAsync ??= RefreshTokenWithSaveCoreAsync(baseAddress, jwt);
        var r = await taskRefreshTokenWithSaveAsync;
        return r;
    }

    public Task<ApiRsp<TResponseModel?>> SendAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequestModel, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        var r = SendCoreAsync(
            baseAddress,
            requestFactory,
            requestModel,
            isSecurity,
            isAnonymous,
            serializableImplType,
            jsonRequestTypeInfo,
            jsonResponseModelTypeInfo,
            cancellationToken: cancellationToken);
        return r;
    }

    async Task<ApiRsp<TResponseModel?>> SendCoreAsync<TRequestModel, TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        bool isRecursiveRetry = false,
        CancellationToken cancellationToken = default)
    {
        var request = requestFactory();
        Aes? aes = null;
        HttpResponseMessage? response = null;
        bool using_response = true;
        try
        {
            if (isSecurity)
            {
                // 行业标准加密
                aes = Aes.Create();
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                //request.Options.TryAdd(nameof(Aes), aes); // 将临时变量添加进请求选项，使函数外可以访问到
            }
            const AESUtils.Flags aesFlags = AESUtils.Flags.CipherMode_CBC | AESUtils.Flags.PaddingMode_PKCS7;
            request.Content ??= await GetRequestContentAsync(
                isSecurity, aes, serializableImplType,
                requestModel, jsonRequestTypeInfo, cancellationToken);
            switch (serializableImplType)
            {
                case SerializableImplType.SystemTextJson:
                    request.Headers.Accept.ParseAdd(isSecurity ?
                        MediaTypeNames.JSONSecurity :
                        MediaTypeNames.JSON);
                    break;
                //case SerializableImplType.MessagePack:
                //    request.Headers.Accept.ParseAdd(isSecurity ?
                //        MediaTypeNames.MessagePackSecurity :
                //        MediaTypeNames.MessagePack);
                //    break;
                case SerializableImplType.MemoryPack:
                    request.Headers.Accept.ParseAdd(isSecurity ?
                        MediaTypeNames.MemoryPackSecurity :
                        MediaTypeNames.MemoryPack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializableImplType),
                        serializableImplType, null);
            }
            if (isSecurity)
            {
                ArgumentNullException.ThrowIfNull(aes);
                const int flagsLen = sizeof(ushort);
                Span<byte> skey_bytes = stackalloc byte[flagsLen + aes.IV.Length + aes.Key.Length];
                BitConverter.TryWriteBytes(skey_bytes, (ushort)aesFlags);
                aes.IV.CopyTo(skey_bytes[flagsLen..]);
                Span<byte> aesKeyReverse = stackalloc byte[aes.Key.Length];
                aes.Key.CopyTo(aesKeyReverse);
                aesKeyReverse.Reverse();
                aesKeyReverse.CopyTo(skey_bytes[(flagsLen + aes.IV.Length)..]);
                var padding = RSAUtils.GetDefaultPadding();
                var encryptData = RSAInstance.Encrypt(skey_bytes, padding);
                var skey_str = Convert.ToHexString(encryptData);
                request.Headers.Add(ApiConstants.Headers_SecurityKeyHex, skey_str);
                request.Headers.Add(ApiConstants.Headers_SecurityKeyPadding, padding.OaepHashAlgorithm.ToString() ?? string.Empty);
            }
            JsonWebTokenValue? jwt = null;
            if (!isAnonymous)
            {
                jwt = await SetRequestHeaderAuthorization(request);
            }
            HandleHttpRequest(request);
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var isAppObsolete = IsAppObsolete(response.Headers);
            if (isAppObsolete)
            {
                return ApiRspCode.AppObsolete;
            }
            var code = unchecked((ApiRspCode)response.StatusCode);
            if (!isAnonymous && code == ApiRspCode.Unauthorized && jwt != null)
            {
                if (!isRecursiveRetry) // 防止死循环递归调用
                {
                    // 401 时，调用 RefreshToken 重试
                    var isSuccessRefreshToken = await RefreshTokenWithSaveAsync(baseAddress, jwt);
                    if (isSuccessRefreshToken)
                    {
                        var r = await SendCoreAsync(
                            baseAddress,
                            requestFactory,
                            requestModel,
                            isSecurity,
                            isAnonymous,
                            serializableImplType,
                            jsonRequestTypeInfo,
                            jsonResponseModelTypeInfo,
                            isRecursiveRetry: true, // 防止死循环递归调用
                            cancellationToken: cancellationToken);
                        return r;
                    }
                }
                else
                {
                    return code;
                }
            }
            if (response.Content == null)
            {
                return code;
            }
            else if (typeof(TResponseModel) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return (TResponseModel)(object)bytes;
            }
            else if (typeof(TResponseModel) == typeof(string))
            {
                var str = await response.Content.ReadAsStringAsync(cancellationToken);
                return (TResponseModel)(object)str;
            }
            else if (typeof(TResponseModel) == typeof(Stream))
            {
                using_response = false;
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return (TResponseModel)(object)stream;
            }
            else if (typeof(TResponseModel) == typeof(JsonDocument))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                return (TResponseModel?)(object?)jsonDoc;
            }
            else if (typeof(TResponseModel) == typeof(JsonElement))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                return (TResponseModel?)(object?)jsonDoc?.RootElement;
            }
            else if (typeof(TResponseModel) == typeof(JsonElement?))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                JsonElement? temp = jsonDoc?.RootElement;
                return (TResponseModel?)(object?)temp;
            }
            else
            {
                ArgumentNullException.ThrowIfNull(jsonResponseModelTypeInfo);
                var r = await ReadAsResponseModelAsync(response, isSecurity, aes, jsonResponseModelTypeInfo, cancellationToken);
                return r;
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            aes?.Dispose();
            if (using_response)
            {
                response?.Dispose();
            }
        }
    }
}

partial class S3dfab2fb<TAppSecrets>
{
    void SetHttpConnectionOptions(HttpConnectionOptions o)
    {
        o.SkipNegotiation = true;
        o.Transports = HttpTransportType.WebSockets;
        SetHeaders(o.Headers);
        o.AccessTokenProvider = AccessTokenProvider;
        //o.WebSocketFactory = DefaultWebSocketFactory; // https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/SignalR/clients/csharp/Http.Connections.Client/src/Internal/WebSocketsTransport.cs#L92
        //o.WebSocketConfiguration = SetClientWebSocketOptions;
    }

    //void SetClientWebSocketOptions(ClientWebSocketOptions o)
    //{
    //    o.HttpVersion = HttpVersion.Version20;
    //    o.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact; // 强制使用 HTTP/2
    //      https://github.com/dotnet/aspnetcore/issues/59303 在 .NET 10.0.5 中回归
    //}

    //static async ValueTask<WebSocket> DefaultWebSocketFactory(WebSocketConnectionContext context, CancellationToken cancellationToken)
    //{
    //    var webSocket = new ClientWebSocket();
    //    var url = context.Uri;

    //    return default;

    //    //        var isBrowser = OperatingSystem.IsBrowser();
    //    //        if (!isBrowser)
    //    //        {
    //    //            // Full Framework will throw when trying to set the User-Agent header
    //    //            // So avoid setting it in netstandard2.0 and only set it in netstandard2.1 and higher
    //    //#if !NETSTANDARD2_0 && !NETFRAMEWORK
    //    //            webSocket.Options.SetRequestHeader("User-Agent", Constants.UserAgentHeader.ToString());
    //    //#else
    //    //            // Set an alternative user agent header on Full framework
    //    //            webSocket.Options.SetRequestHeader("X-SignalR-User-Agent", Constants.UserAgentHeader.ToString());
    //    //#endif

    //    //            // Set this header so the server auth middleware will set an Unauthorized instead of Redirect status code
    //    //            // See: https://github.com/aspnet/Security/blob/ff9f145a8e89c9756ea12ff10c6d47f2f7eb345f/src/Microsoft.AspNetCore.Authentication.Cookies/Events/CookieAuthenticationEvents.cs#L42
    //    //            webSocket.Options.SetRequestHeader("X-Requested-With", "XMLHttpRequest");
    //    //        }

    //    //        if (context.Options != null)
    //    //        {
    //    //            if (context.Options.Headers.Count > 0)
    //    //            {
    //    //                if (isBrowser)
    //    //                {
    //    //                    Log.HeadersNotSupported(_logger);
    //    //                }
    //    //                else
    //    //                {
    //    //                    foreach (var header in context.Options.Headers)
    //    //                    {
    //    //                        webSocket.Options.SetRequestHeader(header.Key, header.Value);
    //    //                    }
    //    //                }
    //    //            }

    //    //#if NET7_0_OR_GREATER
    //    //            var allowHttp2 = true;
    //    //#endif

    //    //            if (!isBrowser)
    //    //            {
    //    //                if (context.Options.Cookies != null)
    //    //                {
    //    //                    webSocket.Options.Cookies = context.Options.Cookies;
    //    //                }

    //    //                if (context.Options.ClientCertificates is { Count: > 0 })
    //    //                {
    //    //                    webSocket.Options.ClientCertificates.AddRange(context.Options.ClientCertificates);
    //    //                }

    //    //                if (context.Options.Credentials != null)
    //    //                {
    //    //                    webSocket.Options.Credentials = context.Options.Credentials;
    //    //                    // Negotiate Auth isn't supported over HTTP/2 and HttpClient does not gracefully fallback to HTTP/1.1 in that case
    //    //                    // https://github.com/dotnet/runtime/issues/1582
    //    //#if NET7_0_OR_GREATER
    //    //                    allowHttp2 = false;
    //    //#endif
    //    //                }

    //    //                var originalProxy = webSocket.Options.Proxy;
    //    //                if (context.Options.Proxy != null)
    //    //                {
    //    //                    webSocket.Options.Proxy = context.Options.Proxy;
    //    //                }

    //    //                if (context.Options.UseDefaultCredentials != null)
    //    //                {
    //    //                    webSocket.Options.UseDefaultCredentials = context.Options.UseDefaultCredentials.Value;
    //    //                    if (context.Options.UseDefaultCredentials.Value)
    //    //                    {
    //    //                        // Negotiate Auth isn't supported over HTTP/2 and HttpClient does not gracefully fallback to HTTP/1.1 in that case
    //    //                        // https://github.com/dotnet/runtime/issues/1582
    //    //#if NET7_0_OR_GREATER
    //    //                        allowHttp2 = false;
    //    //#endif
    //    //                    }
    //    //                }

    //    //                context.Options.WebSocketConfiguration?.Invoke(webSocket.Options);

    //    //#if NET7_0_OR_GREATER
    //    //                if (webSocket.Options.HttpVersion >= HttpVersion.Version20 && allowHttp2)
    //    //                {
    //    //                    // Reset options we set on the users' behalf since they are already on the HttpClient that we're passing to ConnectAsync
    //    //                    // And ConnectAsync will throw if these options are set on the ClientWebSocketOptions
    //    //                    if (ReferenceEquals(webSocket.Options.Cookies, context.Options.Cookies))
    //    //                    {
    //    //                        webSocket.Options.Cookies = null;
    //    //                    }
    //    //                    if (IsX509CertificateCollectionEqual(webSocket.Options.ClientCertificates, context.Options.ClientCertificates))
    //    //                    {
    //    //                        webSocket.Options.ClientCertificates.Clear();
    //    //                    }
    //    //                    if (ReferenceEquals(webSocket.Options.Credentials, context.Options.Credentials))
    //    //                    {
    //    //                        webSocket.Options.Credentials = null;
    //    //                    }
    //    //                    if (webSocket.Options.UseDefaultCredentials == (context.Options.UseDefaultCredentials ?? false))
    //    //                    {
    //    //                        webSocket.Options.UseDefaultCredentials = false;
    //    //                    }
    //    //                    if (ReferenceEquals(webSocket.Options.Proxy, context.Options.Proxy))
    //    //                    {
    //    //                        webSocket.Options.Proxy = originalProxy;
    //    //                    }
    //    //                }

    //    //                if (!allowHttp2 && webSocket.Options.HttpVersion >= HttpVersion.Version20)
    //    //                {
    //    //                    // We shouldn't fallback to HTTP/1.1 if the user explicitly states
    //    //                    if (webSocket.Options.HttpVersionPolicy == HttpVersionPolicy.RequestVersionOrLower)
    //    //                    {
    //    //                        webSocket.Options.HttpVersion = HttpVersion.Version11;
    //    //                    }
    //    //                    else
    //    //                    {
    //    //                        throw new InvalidOperationException("Negotiate Authentication doesn't work with HTTP/2 or higher.");
    //    //                    }
    //    //                }

    //    //                static bool IsX509CertificateCollectionEqual(X509CertificateCollection? left, X509CertificateCollection? right)
    //    //                {
    //    //                    var leftCount = left?.Count ?? 0;
    //    //                    var rightCount = right?.Count ?? 0;
    //    //                    if (leftCount == rightCount)
    //    //                    {
    //    //                        for (var i = 0; i < rightCount; ++i)
    //    //                        {
    //    //                            if (!ReferenceEquals(left![i], right![i]))
    //    //                            {
    //    //                                return false;
    //    //                            }
    //    //                        }
    //    //                        return true;
    //    //                    }

    //    //                    return false;
    //    //                }
    //    //#endif
    //    //            }
    //    //        }

    //    //        if (_httpConnectionOptions.AccessTokenProvider != null
    //    //#if NET7_0_OR_GREATER
    //    //            && webSocket.Options.HttpVersion < HttpVersion.Version20
    //    //#endif
    //    //            )
    //    //        {
    //    //            // Apply access token logic when using HTTP/1.1 because we don't use the AccessTokenHttpMessageHandler via HttpClient unless the user specifies HTTP/2.0 or higher
    //    //            var accessToken = await _httpConnectionOptions.AccessTokenProvider().ConfigureAwait(false);
    //    //            if (!string.IsNullOrWhiteSpace(accessToken))
    //    //            {
    //    //                // We can't use request headers in the browser, so instead append the token as a query string in that case
    //    //                if (OperatingSystem.IsBrowser())
    //    //                {
    //    //                    var accessTokenEncoded = UrlEncoder.Default.Encode(accessToken);
    //    //                    accessTokenEncoded = "access_token=" + accessTokenEncoded;
    //    //                    url = Utils.AppendQueryString(url, accessTokenEncoded);
    //    //                }
    //    //                else
    //    //                {
    //    //                    webSocket.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");
    //    //                }
    //    //            }
    //    //        }

    //    //        try
    //    //        {
    //    //#if NET7_0_OR_GREATER
    //    //            // Only share the HttpClient if the user opts-in to HTTP/2 (or higher)
    //    //            // This is because there is some non-obvious behavior changes when passing in an invoker to ConnectAsync
    //    //            // and there isn't really any benefit to sharing the HttpClient in HTTP/1.1
    //    //            if (webSocket.Options.HttpVersion > HttpVersion.Version11)
    //    //            {
    //    //                await webSocket.ConnectAsync(url, invoker: _httpClient, cancellationToken).ConfigureAwait(false);
    //    //            }
    //    //            else
    //    //#endif
    //    //            {
    //    //                await webSocket.ConnectAsync(url, cancellationToken).ConfigureAwait(false);
    //    //            }
    //    //        }
    //    //        catch
    //    //        {
    //    //            webSocket.Dispose();
    //    //            throw;
    //    //        }

    //    //        return webSocket;
    //}

    public HubConnection CreateHubConnection(string url/*, params IReadOnlyList<IJsonTypeInfoResolver> resolvers*/)
    {
        var b = new HubConnectionBuilder()
            .WithUrl(url, SetHttpConnectionOptions)
            .WithAutomaticReconnect()
            .AddJsonProtocol(o =>
            {
                o.PayloadSerializerOptions.TypeInfoResolverChain.Add(ServerSdkJsonSerializerContext.Default);
            });
        b.Services.AddLogging(Log.Factory);
        var conn = b.Build();
        return conn;
    }
}

file static partial class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// Adds logging services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="loggerFactory"></param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddLogging(this IServiceCollection services, ILoggerFactory loggerFactory)
    {
        services.TryAdd(ServiceDescriptor.Singleton(loggerFactory));
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        return services;
    }
}