using System.Text;

namespace SrcGen;

public class Documents
{
    const string UidPrefix = "UID:";
    const string UidDbgEngPrefix = "Nx:dbgeng.";
    const string DescriptionPrefix = "description:";

    readonly Dictionary<string, string> Summaries = [];
    readonly Dictionary<string, List<string>> Members = [];

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
                ParseStruct(uid[UidDbgEngPrefix.Length..], reader);
                return;

            default:
                throw new NotImplementedException($"UID of N{uid[1]} is not seen yet.");
        }
    }

    private void ParseInterface(ReadOnlySpan<char> name, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        Summaries.Add(name.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());
    }

    private void ParseFunction(ReadOnlySpan<char> functionName, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        Summaries.Add(functionName.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());

        reader.SeekLineWithPrefix("api_type:");
        fullLine = reader.ReadLine();

        if (fullLine is null || fullLine.Contains("DllExport"))
        {
            // we hand write those, skip
            return;
        }

        AddMember(functionName[..functionName.IndexOf('.')], functionName.ToString());

        const string memberHeader = "### -param ";
        var descriptionBuilder = new StringBuilder();

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            while (true)
            {
                var parameterName = memberLine.AsSpan(memberHeader.Length).Trim();

                var space = parameterName.IndexOf(' ');
                if (space > 0)
                {
                    parameterName = parameterName[..space];
                }

                parameterName = ParseMemberDescription(functionName, parameterName, reader, descriptionBuilder, memberHeader);

                if (parameterName.IsEmpty)
                {
                    break;
                }
            }
        }
    }

    private void ParseStruct(ReadOnlySpan<char> structName, TextReader reader)
    {
        var fullLine = reader.SeekLineWithPrefix(DescriptionPrefix);
        Summaries.Add(structName.ToString(), fullLine.AsSpan(DescriptionPrefix.Length).Trim().ToString());

        const string memberHeader = "### -field ";
        var descriptionBuilder = new StringBuilder();

        if (reader.SeekLineWithPrefix(memberHeader) is string memberLine)
        {
            while (true)
            {
                var fieldName = memberLine.AsSpan(memberHeader.Length).Trim();

                fieldName = ParseMemberDescription(structName, fieldName, reader, descriptionBuilder, memberHeader);

                if (fieldName.IsEmpty)
                {
                    break;
                }
            }
        }
    }

    private ReadOnlySpan<char> ParseMemberDescription(ReadOnlySpan<char> parentName, ReadOnlySpan<char> memberName, TextReader reader, StringBuilder builder, string memberHeader)
    {
        var lookupName = $"{parentName}.{memberName}";
        AddMember(parentName, lookupName);

        builder.Clear();

        while (reader.ReadLine() is string fullLine)
        {
            if (fullLine.StartsWith(memberHeader))
            {
                var description = builder.ToString();

                Summaries.Add(lookupName, description);

                return fullLine.AsSpan()[memberHeader.Length..].Trim();
            }

            builder.AppendLine(fullLine);
        }

        var description1 = builder.ToString();

        Summaries.Add(lookupName, description1);

        return [];
    }

    private void AddMember(ReadOnlySpan<char> parent, string child)
    {
        var lookup = Members.GetAlternateLookup<ReadOnlySpan<char>>();

        if (lookup.TryGetValue(parent, out var list))
        {
            list.Add(child);
        }
        else
        {
            lookup[parent] = [child];
        }
    }
}