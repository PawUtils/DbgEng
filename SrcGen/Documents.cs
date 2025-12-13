using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SrcGen;

public partial class Documents
{
    const string UidPrefix = "UID:";
    const string UidDbgEngPrefix = "Nx:dbgeng.";
    const string UidWinNTPrefix = "Nx:winnt.";
    const string DescriptionPrefix = "description:";
    const string DescriptionHeader = "## -description";

    readonly Dictionary<string, string> TypeSummaries = [];
    readonly Dictionary<string, Dictionary<string, string>> MemberSummaries = [];
    readonly Dictionary<string, List<(bool isOut, string name, string summary)>> Parameters = [];
    readonly Dictionary<string, HashSet<string>> ReturnCodes = [];

    public static Documents Empty { get; } = new();

    internal static Documents From(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return Empty;
        }

        var documents = new Documents();

        documents.Parse(Directory.EnumerateFiles(dir, "*.md", SearchOption.AllDirectories).Select(File.OpenText));

        return documents;
    }

    public bool TryGetSummary(ReadOnlySpan<char> type, [MaybeNullWhen(false)] out string summary)
        => TypeSummaries.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(type, out summary);

    public bool TryGetSummary(ReadOnlySpan<char> type, ReadOnlySpan<char> member, [MaybeNullWhen(false)] out string summary)
    {
        if (MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(type, out var members)
            && members.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(member, out summary))
        {
            return true;
        }

        summary = null;
        return false;
    }

    public bool TryGetParameters(ReadOnlySpan<char> type, ReadOnlySpan<char> method, [MaybeNullWhen(false)] out IReadOnlyList<(bool isOut, string name, string summary)> parameters)
    {
        if (Parameters.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue([.. type, '.', .. method], out var list))
        {
            parameters = list;
            return true;
        }

        parameters = null;
        return false;
    }

    public bool TryGetReturnCodes(ReadOnlySpan<char> type, ReadOnlySpan<char> method, [MaybeNullWhen(false)] out IReadOnlySet<string> codes)
    {
        if (ReturnCodes.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue([.. type, '.', .. method], out var list))
        {
            codes = list;
            return true;
        }

        codes = null;
        return false;
    }

    public void Parse(IEnumerable<TextReader> readers)
    {
        foreach (var reader in readers)
        {
            try
            {
                Parse(reader);
            }
            finally
            {
                reader.Dispose();
            }
        }
    }

    private void Parse(TextReader reader)
    {
        if (!reader.TrySeekLine(UidPrefix, out var fullLine))
        {
            return;
        }

        var uid = fullLine.AsSpan(UidPrefix.Length).Trim();
        var isDbgEng = uid.Contains(":dbgeng.", StringComparison.Ordinal);

        switch (uid[1])
        {
            case 'A': // index page, no use, skip
            case 'C': // callback functions, skip for now
            case 'E': // enums, not seen yet, skip
            case 'L': // IXyzCallbacks base implementations, skip
                return;

            case 'N':
                if (isDbgEng)
                {
                    ParseInterface(uid[UidDbgEngPrefix.Length..], reader);
                }
                return;
            case 'F':
                if (isDbgEng)
                {
                    ParseFunction(uid[UidDbgEngPrefix.Length..], reader);
                }
                return;
            case 'S':
                if (isDbgEng)
                {
                    ParseStruct(uid[(UidDbgEngPrefix.Length + 1)..], reader);
                }
                else if (uid.Contains(":winnt.", StringComparison.Ordinal))
                {
                    ParseStruct(uid[(UidWinNTPrefix.Length + 1)..], reader);
                }
                return;

            default:
                throw new NotImplementedException($"UID of N{uid[1]} is not seen yet.");
        }
    }

    private void ParseInterface(ReadOnlySpan<char> name, TextReader reader)
    {
        var hasDescription = reader.TrySeekLine(DescriptionPrefix, out var fullLine);
        Debug.Assert(hasDescription);

        TypeSummaries.Add(name.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());
    }

    private void ParseFunction(ReadOnlySpan<char> functionName, TextReader reader)
    {
        var dot = functionName.IndexOf('.');
        if (dot < 0)
        {
            // DllExports, we hand write those, skip
            return;
        }

        var hasDescription = reader.TrySeekLine(DescriptionPrefix, out var fullLine);
        Debug.Assert(hasDescription);

        var summary = fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString();

        AddMemberSummary(functionName[..dot], functionName[(dot + 1)..].ToString(), summary);

        const string memberHeader = "### -param ";
        const string returnsHeader = "## -returns";

        if (reader.TrySeekLine(memberHeader, out var nextLine, ignoreLeadingSpaces: true, returnsHeader))
        {
            var lookup = Parameters.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(functionName, out var parameters))
            {
                lookup[functionName] = parameters = [];
            }

            bool more;
            do
            {
                var parameterName = getParameterName(nextLine!, out var isOut);
                var description = ParseDescription(reader, memberHeader, out nextLine, out more);

                parameters.Add((isOut, parameterName, description));
            }
            while (more);
        }

        if (nextLine?.StartsWith(returnsHeader) == true || reader.TrySeekLine(returnsHeader, out _))
        {
            var lookup1 = ReturnCodes.GetAlternateLookup<ReadOnlySpan<char>>();
            if (!lookup1.TryGetValue(functionName, out var codesSet))
            {
                lookup1[functionName] = codesSet = [];
            }

            var codes = codesSet.GetAlternateLookup<ReadOnlySpan<char>>();

            while ((nextLine = reader.ReadLine()) is not null && !nextLine.StartsWith('#'))
            {
                foreach (var match in HResultRegex.EnumerateMatches(nextLine))
                {
                    codes.Add(nextLine.AsSpan(match.Index, match.Length));
                }
            }
        }

        static string getParameterName(string memberLine, out bool isOut)
        {
            isOut = false;

            var parameterName = memberLine.AsSpan(memberHeader.Length).Trim();

            var space = parameterName.IndexOf(' ');
            if (space > 0)
            {
                isOut = parameterName[space..].Contains("out", StringComparison.Ordinal);
                parameterName = parameterName[..space];
            }

            if (parameterName.SequenceEqual("..."))
            {
                return "Args";
            }

            return parameterName.ToString();
        }
    }

    private void ParseStruct(ReadOnlySpan<char> structName, TextReader reader)
    {
        var hasDescription = reader.TrySeekLine(DescriptionPrefix, out var fullLine);
        Debug.Assert(hasDescription);

        TypeSummaries.Add(structName.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());

        const string memberHeader = "### -field ";

        if (reader.TrySeekLine(memberHeader, out var memberLine))
        {
            var lookup = MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(structName, out var fields))
            {
                lookup[structName] = fields = [];
            }

            bool more;
            do
            {
                var fieldName = getFieldName(memberLine!);
                var description = ParseDescription(reader, memberHeader, out memberLine, out more);

                fields.Add(fieldName, description);
            }
            while (more);
        }

        static string getFieldName(string memberLine)
        {
            var fieldName = memberLine.AsSpan(memberHeader.Length).Trim();

            var square = fieldName.IndexOf('[');
            if (square > 0)
            {
                fieldName = fieldName[..square];
            }

            return fieldName.ToString();
        }
    }

    private static string ParseDescription(TextReader reader, string header, out string? nextLine, out bool more)
    {
        var builder = new DefaultInterpolatedStringHandler(512, 0);
        more = false;

        while ((nextLine = reader.ReadLine()) is not null)
        {
            if (nextLine.StartsWith(header))
            {
                more = true;
                break;
            }
            else if (nextLine.StartsWith("## ") || nextLine.StartsWith("# "))
            {
                break;
            }

            builder.AppendLiteral(nextLine);
            builder.AppendLiteral(Environment.NewLine);
        }

        var description = builder.Text.Trim().ToString();

        builder.Clear();

        return description;
    }

    private void AddMemberSummary(ReadOnlySpan<char> parent, string child, string summary)
    {
        var lookup = MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>();

        if (!lookup.TryGetValue(parent, out var members))
        {
            lookup[parent] = members = [];
        }

        members.Add(child, summary);
    }

    [GeneratedRegex(@"\b[SE]_[A-Z]+")]
    private static partial Regex HResultRegex { get; }
}