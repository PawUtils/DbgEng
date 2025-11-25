using System.Runtime.InteropServices;
using System.Text;

namespace Interop.DbgEng;

/// <summary>
/// Helper methods to convert <c>Span</c> buffers with null-terminated strings into managed <see cref="string"/>s.
/// </summary>
public static class SpanExtensions
{
    /// <summary>
    /// Gets a copy of <see cref="string"/> from the ANSI null-terminated string <paramref name="buffer"/> with <paramref name="filledSize"/> of bytes.
    /// </summary>
    public static string GetString(this Span<byte> buffer, uint filledSize)
    {
        return Encoding.ASCII.GetString(buffer[..(int)(filledSize - 1)]);
    }

    /// <summary>
    /// Gets a copy of <see cref="string"/> from the ANSI null-terminated string <paramref name="buffer"/> with <paramref name="filledSize"/> of bytes.
    /// </summary>
    public static string GetString(this ReadOnlySpan<byte> buffer, uint filledSize)
    {
        return Encoding.ASCII.GetString(buffer[..(int)(filledSize - 1)]);
    }

    /// <summary>
    /// Gets a copy of <see cref="string"/> from the UTF-16 null-terminated string <paramref name="buffer"/> with <paramref name="filledSize"/> of chars.
    /// </summary>
    public static string GetString(this Span<char> buffer, uint filledSize)
    {
        return Encoding.Unicode.GetString(MemoryMarshal.AsBytes(buffer[..(int)(filledSize - 1)]));
    }

    /// <summary>
    /// Gets a copy of <see cref="string"/> from the UTF-16 null-terminated string <paramref name="buffer"/> with <paramref name="filledSize"/> of chars.
    /// </summary>
    public static string GetString(this ReadOnlySpan<char> buffer, uint filledSize)
    {
        return Encoding.Unicode.GetString(MemoryMarshal.AsBytes(buffer[..(int)(filledSize - 1)]));
    }
}
