using AigioL.Common.Primitives.Columns;
using System.Diagnostics.CodeAnalysis;

namespace AigioLTemplate.ServerSdk.Services.Abstractions;

/// <summary>
/// <inheritdoc cref="IUserStoreBase"/>
public partial interface IUserStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserInfoModel> : IUserStoreBase
    where TUserInfoModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 获取当前登录用户资料
    /// <para>如果[退出登录]则为 <see langword="null"/>，对于接收到的推送消息，要求在服务端时传入接收人用户Id，客户端根据Id读取用户信息，而不使用此值</para>
    /// </summary>
    ValueTask<TUserInfoModel?> GetCurrentUserInfoAsync();

    /// <summary>
    /// 设置当前登录用户资料
    /// </summary>
    ValueTask SetCurrentUserInfoAsync(TUserInfoModel value, bool updateToDataBase);

    /// <summary>
    /// 根据用户 Id 获取用户资料
    /// </summary>
    ValueTask<TUserInfoModel?> GetUserInfoByIdAsync(Guid userId);

    /// <summary>
    /// 添加或更新用户数据到数据库中
    /// </summary>
    ValueTask InsertOrUpdateAsync(TUserInfoModel user);
}