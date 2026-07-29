using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioLTemplate.ServerSdk.Models.Identity;

/// <summary>
/// 当前登录用户模型，如需增加字段，还需要在 <see cref="Clone"/> 中赋值新添加字段
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class CurrentUser : IExplicitHasValue, IPhoneNumber
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid UserId { get; set; }

    /// <summary>
    /// 登录凭证
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public JsonWebTokenValue? AuthToken { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? Email { get; set; }

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        if (!AuthToken.HasValue())
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 创建作为当前实例副本的新对象，如果当前对象值无效，则返回 <see langword="null"/>。
    /// </summary>
    /// <returns></returns>
    public CurrentUser? Clone() => this.HasValue() ?
        new()
        {
            UserId = UserId,
            AuthToken = AuthToken,
            PhoneNumber = PhoneNumber,
        } : null;
}
