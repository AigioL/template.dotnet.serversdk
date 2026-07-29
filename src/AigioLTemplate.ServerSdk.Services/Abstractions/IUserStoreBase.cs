using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Columns;
using AigioLTemplate.ServerSdk.Models.Identity;

namespace AigioLTemplate.ServerSdk.Services.Abstractions;

/// <summary>
/// 为管理用户帐户的存储提供抽象
/// </summary>
public interface IUserStoreBase
{
    /// <summary>
    /// 获取当前登录用户
    /// <para>如果[退出登录]则为 <see langword="null"/>，对于接收到的推送消息，要求在服务端时传入接收人用户 Id，客户端根据 Id 读取用户信息，而不使用此值</para>
    /// </summary>
    ValueTask<CurrentUser?> GetCurrentUserAsync(bool clone = true);

    /// <summary>
    /// 设置当前登录用户，当[退出登录]时可传入<see langword="null"/>
    /// </summary>
    ValueTask SetCurrentUserAsync(CurrentUser? value);

    /// <summary>
    /// 获取当前登录用户的手机号码
    /// </summary>
    async ValueTask<string?> GetCurrentUserPhoneNumberAsync(bool notHideMiddleFour = false)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(currentUser.PhoneNumber))
        {
            return string.Empty;
        }
        return $"{(string.IsNullOrWhiteSpace(currentUser.PhoneNumberRegionCode) ? IPhoneNumber.DefaultPhoneNumberRegionCode : currentUser.PhoneNumberRegionCode)}{(notHideMiddleFour ? currentUser.PhoneNumber : IPhoneNumber.ToStringHideMiddleFour(currentUser.PhoneNumber))}";
    }

    /// <summary>
    /// 当登出时
    /// </summary>
    event Action? OnSignOut;

    /// <summary>
    /// 获取登录的 JWT 数据
    /// </summary>
    ValueTask<JsonWebTokenValue?> GetAuthTokenAsync();

    /// <summary>
    /// 登出
    /// </summary>
    ValueTask SignOutAsync();
}
