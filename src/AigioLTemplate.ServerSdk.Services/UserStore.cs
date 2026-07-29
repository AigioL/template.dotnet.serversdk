using AigioL.Common.Essentials.Storage;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Columns;
using AigioLTemplate.ServerSdk.Models.Identity;
using AigioLTemplate.ServerSdk.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AigioLTemplate.ServerSdk.Services;

partial class UserStore : IUserStoreBase
{
    /// <summary>
    /// 键值对存储键（当前登录用户）
    /// </summary>
    protected const string KEY_CURRENT_LOGIN_USER = "KEY_CURRENT_LOGIN_USER";

    protected readonly ILogger logger;
    protected readonly ISecureStorage secureStorage;

    /// <summary>
    /// 是否未登录
    /// </summary>
    protected bool isAnonymous;

#pragma warning disable IDE0290 // 使用主构造函数
    public UserStore(
#pragma warning restore IDE0290 // 使用主构造函数
        ILogger<UserStore> logger,
        ISecureStorage secureStorage)
    {
        this.logger = logger;
        this.secureStorage = secureStorage;
    }

    /// <summary>
    /// 当前登录用户
    /// </summary>
    protected CurrentUser? CurrentUser
    {
        set
        {
            field = value;
            isAnonymous = value == null;
        }
        get;
    }

    protected virtual object? CurrentUserInfoObject
    {
        set
        {
        }

        get => null;
    }

    [Conditional("DEBUG")]
    protected void PrintCurrentUser(string name)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        logger.LogDebug("name: {name}, PhoneNumber: {phoneNumber}",
            name,
            IPhoneNumber.ToStringHideMiddleFour(CurrentUser?.PhoneNumber));
#pragma warning restore CA1873 // Avoid potentially expensive logging
    }

    protected async ValueTask<CurrentUser?> GetCurrentUserCoreAsync()
    {
        var result = await secureStorage.GetAsync<CurrentUser>(KEY_CURRENT_LOGIN_USER);
        return result;
    }

    public async ValueTask<CurrentUser?> GetCurrentUserAsync(bool clone)
    {
        if (CurrentUser == null && !isAnonymous)
        {
            try
            {
                CurrentUser = await GetCurrentUserCoreAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, nameof(GetCurrentUserAsync));
            }
            PrintCurrentUser(nameof(GetCurrentUserAsync));
        }
        return clone ? CurrentUser?.Clone() : CurrentUser;
    }

    protected async ValueTask SetCurrentUserCoreAsync(CurrentUser? value)
    {
        await secureStorage.SetAsync(KEY_CURRENT_LOGIN_USER, value);
    }

    public async ValueTask SetCurrentUserAsync(CurrentUser? value)
    {
        await SetCurrentUserCoreAsync(value);

        CurrentUser = value;
        PrintCurrentUser("SetCurrentUser");
    }

    public event Action? OnSignOut;

    public async ValueTask<JsonWebTokenValue?> GetAuthTokenAsync()
    {
        var value = await GetCurrentUserAsync(false);
        var result = value?.AuthToken;
        return result;
    }

    protected async ValueTask SignOutCoreAsync(bool callSetCurrentUserAsync)
    {
        PrintCurrentUser("SignOut");
        CurrentUserInfoObject = null;
        isAnonymous = true;
        if (callSetCurrentUserAsync)
        {
            await SetCurrentUserAsync(null);
        }
        OnSignOut?.Invoke();
    }

    public ValueTask SignOutAsync()
    {
        return SignOutCoreAsync(callSetCurrentUserAsync: true);
    }
}

sealed partial class UserStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserInfoModel> : UserStore, IUserStore<TUserInfoModel> where TUserInfoModel : IReadOnlyId<Guid>
{
    const string sharedName = "9bc3c902";
    readonly IPreferences preferences;

#pragma warning disable IDE0290 // 使用主构造函数
    public UserStore(
#pragma warning restore IDE0290 // 使用主构造函数
        ILogger<UserStore> logger,
        ISecureStorage secureStorage,
        IPreferences preferences) : base(logger, secureStorage)
    {
        this.preferences = preferences;
    }

    protected override object? CurrentUserInfoObject
    {
        get => CurrentUserInfo;
        set
        {
            if (value == null)
            {
                CurrentUserInfo = default;
            }
        }
    }

    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    TUserInfoModel? CurrentUserInfo { get; set; }

    public async ValueTask<TUserInfoModel?> GetCurrentUserInfoAsync()
    {
        if (CurrentUserInfo == null && !isAnonymous)
        {
            var cUser = await GetCurrentUserAsync(false);
            if (cUser != null)
            {
                var result = await GetUserInfoByIdAsync(cUser.UserId);
                return CurrentUserInfo = result;
            }
        }
        return CurrentUserInfo;
    }

    public async ValueTask SetCurrentUserInfoAsync(TUserInfoModel value, bool updateToDataBase)
    {
        if (updateToDataBase)
        {
            await InsertOrUpdateAsync(value);
        }
        CurrentUserInfo = value;
    }

    public ValueTask<TUserInfoModel?> GetUserInfoByIdAsync(Guid userId)
    {
        var key = ShortGuid.Encode(userId);
        var result = preferences.Get<TUserInfoModel>(key, sharedName: sharedName);
        return ValueTask.FromResult(result);
    }

    public ValueTask InsertOrUpdateAsync(TUserInfoModel user)
    {
        var key = ShortGuid.Encode(user.Id);
        preferences.Set(key, user, sharedName: sharedName);
        return ValueTask.CompletedTask;
    }
}
