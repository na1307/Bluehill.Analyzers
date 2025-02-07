using System.ComponentModel;

#pragma warning disable IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
// ReSharper disable once UnusedType.Global
internal sealed class IsExternalInit;
