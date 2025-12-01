using System.Diagnostics.CodeAnalysis;

namespace SrcGen;

internal static class TextReaderExtensions
{
    public static bool TrySeekLine(this TextReader reader, string prefix, [MaybeNullWhen(false)] out string line, bool ignoreLeadingSpaces = false, params ReadOnlySpan<string> excludePrefices)
    {
        while ((line = reader.ReadLine()) is not null)
        {
            var span = line.AsSpan();

            if (ignoreLeadingSpaces)
            {
                span = span.TrimStart();
            }

            if (span.StartsWith(prefix))
            {
                return true;
            }

            foreach (var stopper in excludePrefices)
            {
                if (span.StartsWith(stopper))
                {
                    return false;
                }
            }
        }

        return false;
    }

}
