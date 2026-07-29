using MemoryPack;

namespace AigioLTemplate.Constants;

static partial class SerializerConstants
{
    /// <summary>
    /// 虽然 <see cref="GenerateType.VersionTolerant"/> 提供向后兼容性，但当前不支持生成 TypeScript 类型，见 https://github.com/Cysharp/MemoryPack/issues/327
    /// <para>https://github.com/Cysharp/MemoryPack?tab=readme-ov-file#version-tolerant</para>
    /// </summary>
    internal const GenerateType MP2GenerateType = GenerateType.Object;
}

