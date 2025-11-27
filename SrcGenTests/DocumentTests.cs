using SrcGen;

namespace SrcGenTests;

public class DocumentTests : TestsBase
{
    [Fact]
    public void TestInterface()
    {
        var documents = new Documents();
        documents.Parse([
            new StringReader("""
                ---
                UID: NN:dbgeng.IDebugClient
                description: IDebugClient interface
                ---
                """)
        ]);

        Assert.True(documents.TryGetSummary("IDebugClient", out var summary));
        Assert.Equal("IDebugClient interface", summary);
    }

    [Fact]
    public void TestInterfaceXml()
    {
        AssertGeneratedWithDocuments("""
            /// <summary>
            /// An interface
            /// </summary>
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
            }
            """,
            """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;
            
            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
            };
            """,
            [
            """
            ---
            UID: NN:dbgeng.ISomeInterface
            description: An interface
            ---
            """
            ]);
    }

    [Fact]
    public void TestStruct()
    {
        var documents = new Documents();
        documents.Parse([
            new StringReader("""
                ---
                UID: NS:dbgeng._DEBUG_BREAKPOINT_PARAMETERS
                description: Blah la la
                ---

                ### -field Offset

                Lorem ipsum sit domit

                ### -field Id

                Je <a href="wewe">ha jit mi</a>.
                
                """)
        ]);

        Assert.True(documents.TryGetSummary("DEBUG_BREAKPOINT_PARAMETERS", out var summary));
        Assert.Equal("Blah la la", summary);

        Assert.True(documents.TryGetMembers("DEBUG_BREAKPOINT_PARAMETERS", out var members));
        Assert.Equal(["DEBUG_BREAKPOINT_PARAMETERS.Offset", "DEBUG_BREAKPOINT_PARAMETERS.Id"], members);

        Assert.True(documents.TryGetSummary(members[0], out summary));
        Assert.Equal("Lorem ipsum sit domit", summary);

        Assert.True(documents.TryGetSummary(members[1], out summary));
        Assert.Equal("Je <a href=\"wewe\">ha jit mi</a>.", summary);
    }

    [Fact]
    public void TestStructXml()
    {
        AssertGeneratedWithDocuments("""
            /// <summary>
            /// Blah la la
            /// </summary>
            /// <remarks>
            /// Structure for querying breakpoint information
            /// all at once.
            /// </remarks>
            public struct DebugBreakpointParameters
            {
            }
            """,
            """
            // Structure for querying breakpoint information
            // all at once.
            typedef struct _DEBUG_BREAKPOINT_PARAMETERS
            {
            } DEBUG_BREAKPOINT_PARAMETERS, *PDEBUG_BREAKPOINT_PARAMETERS;
            """,
            [
            """
            ---
            UID: NS:dbgeng._DEBUG_BREAKPOINT_PARAMETERS
            description: Blah la la
            ---
            """
            ]);
    }

    [Fact]
    public void TestIgnoreDllExport()
    {
        var documents = new Documents();
        documents.Parse([
            new StringReader("""
                ---
                UID: NF:dbgeng.DebugCreate
                description: The DebugCreate function ...
                api_type:
                 - DllExport
                ---

                ### -param InterfaceId [in]

                hahaha

                ### -param Interface [out]

                wawaaw
                
                """)
        ]);

        Assert.False(documents.TryGetSummary("DebugCreate", out _));
        Assert.False(documents.TryGetMembers("DebugCreate", out _));
        Assert.False(documents.TryGetSummary("DebugCreate.InterfaceId", out _));
        Assert.False(documents.TryGetSummary("DebugCreate.Interface", out _));
    }

    [Fact]
    public void TestFunction()
    {
        var documents = new Documents();
        documents.Parse([
            new StringReader("""
                ---
                UID: NF:dbgeng.IDebugClient.EndSession
                title: IDebugClient::EndSession (dbgeng.h)
                description: The EndSession method ends ...
                ---

                ### -param Flags [in]

                Lorem ipsum sit domit                
                """)
        ]);

        Assert.True(documents.TryGetSummary("IDebugClient.EndSession", out var summary));
        Assert.Equal("The EndSession method ends ...", summary);

        Assert.True(documents.TryGetMembers("IDebugClient.EndSession", out var members));
        Assert.Equal(["IDebugClient.EndSession.Flags"], members);

        Assert.True(documents.TryGetSummary(members[0], out summary));
        Assert.Equal("Lorem ipsum sit domit", summary);
    }


    [Fact]
    public void TestInterfaceFunction()
    {
        var documents = new Documents();
        documents.Parse([
            new StringReader("""
                ---
                UID: NF:dbgeng.IDebugClient.EndSession
                title: IDebugClient::EndSession (dbgeng.h)
                description: The EndSession method ends ...
                ---

                ### -param Flags [in]

                Lorem ipsum sit domit                
                """),
            new StringReader("""
                ---
                UID: NN:dbgeng.IDebugClient
                description: IDebugClient interface
                ---
                """)
        ]);

        Assert.True(documents.TryGetSummary("IDebugClient", out var summary));
        Assert.Equal("IDebugClient interface", summary);

        Assert.True(documents.TryGetMembers("IDebugClient", out var members));
        Assert.Equal(["IDebugClient.EndSession"], members);

        Assert.True(documents.TryGetSummary("IDebugClient.EndSession", out summary));
        Assert.Equal("The EndSession method ends ...", summary);

        Assert.True(documents.TryGetMembers("IDebugClient.EndSession", out members));
        Assert.Equal(["IDebugClient.EndSession.Flags"], members);

        Assert.True(documents.TryGetSummary("IDebugClient.EndSession.Flags", out summary));
        Assert.Equal("Lorem ipsum sit domit", summary);
    }
}
