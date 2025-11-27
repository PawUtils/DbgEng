using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace SrcGen;

public class Documents
{
    const string UidPrefix = "UID:";
    const string UidDbgEngPrefix = "Nx:dbgeng.";
    const string DescriptionPrefix = "description:";

    readonly Dictionary<string, string> TypeSummaries = [];
    readonly Dictionary<string, Dictionary<string, string>> MemberSummaries = [];
    readonly Dictionary<string, List<(string name, string summary)>> Parameters = [];

    public static Documents Empty { get; } = new();

    internal static Documents From(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return Empty;
        }

        var documents = new Documents();

        documents.Parse(Directory.EnumerateFiles(dir).Select(File.OpenText));

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

    public bool TryGetParameters(ReadOnlySpan<char> type, ReadOnlySpan<char> method, [MaybeNullWhen(false)] out IReadOnlyList<(string name, string summary)> parameters)
    {
        if (Parameters.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue([.. type, '.', .. method], out var list))
        {
            parameters = list;
            return true;
        }

        parameters = null;
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
        if (reader.SeekLineWithPrefix(UidPrefix) is not string fullLine)
        {
            return;
        }

        var uid = fullLine.AsSpan(UidPrefix.Length).Trim();

        switch (uid[1])
        {
            case 'A':
            // index page, no use, skip
            case 'C':
            // callback functions, skip for now
            case 'L':
                // IXyzCallbacks base implementations, skip
                return;

            case 'N':
                ParseInterface(uid[UidDbgEngPrefix.Length..], reader);
                return;
            case 'F':
                ParseFunction(uid[UidDbgEngPrefix.Length..], reader);
                return;
            case 'S':
                ParseStruct(uid[(UidDbgEngPrefix.Length + 1)..], reader);
                return;

            default:
                throw new NotImplementedException($"UID of N{uid[1]} is not seen yet.");
        }
    }

    private void ParseInterface(ReadOnlySpan<char> name, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
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

        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        var summary = fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString();

        AddMemberSummary(functionName[..dot], functionName[(dot + 1)..].ToString(), summary);

        const string memberHeader = "### -param ";

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            var parameterName = getParameterName(memberLine);
            var lookup = Parameters.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(functionName, out var parameters))
            {
                lookup[functionName] = parameters = [];
            }

            do
            {
                var parameterNameString = parameterName.ToString();
                var description = ParseMemberDescription(reader, memberHeader, getParameterName, out parameterName);

                parameters.Add((parameterNameString, description));
            }
            while (!parameterName.IsEmpty);
        }

        static ReadOnlySpan<char> getParameterName(string memberLine)
        {
            var parameterName = memberLine.AsSpan(memberHeader.Length).Trim();

            var space = parameterName.IndexOf(' ');
            if (space > 0)
            {
                parameterName = parameterName[..space];
            }

            return parameterName;
        }
    }

    private void ParseStruct(ReadOnlySpan<char> structName, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        TypeSummaries.Add(structName.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());

        const string memberHeader = "### -field ";

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            var fieldName = getFieldName(memberLine);
            var lookup = MemberSummaries.GetAlternateLookup<ReadOnlySpan<char>>();

            if (!lookup.TryGetValue(structName, out var fields))
            {
                lookup[structName] = fields = [];
            }

            do
            {
                var fieldNameString = fieldName.ToString();
                var description = ParseMemberDescription(reader, memberHeader, getFieldName, out fieldName);

                fields.Add(fieldNameString, description);
            }
            while (!fieldName.IsEmpty);
        }

        static ReadOnlySpan<char> getFieldName(string memberLine)
        {
            var fieldName = memberLine.AsSpan(memberHeader.Length).Trim();

            return fieldName;
        }
    }

    private static string ParseMemberDescription(TextReader reader, string memberHeader, Func<string, ReadOnlySpan<char>> getMemberName, out ReadOnlySpan<char> memberName)
    {
        var builder = new DefaultInterpolatedStringHandler(512, 0);

        while (reader.ReadLine() is string fullLine)
        {
            if (fullLine.StartsWith(memberHeader))
            {
                memberName = getMemberName(fullLine);
                goto exit;
            }
            else if (fullLine.StartsWith("## ") || fullLine.StartsWith("# "))
            {
                break;
            }

            builder.AppendLiteral(fullLine);
            builder.AppendLiteral(Environment.NewLine);
        }

        memberName = [];

    exit:
        var result = builder.Text.Trim().ToString();

        builder.Clear();
        return result;
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
}