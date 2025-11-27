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

        Assert.True(documents.TryGetSummary("DEBUG_BREAKPOINT_PARAMETERS", "Offset", out summary));
        Assert.Equal("Lorem ipsum sit domit", summary);

        Assert.True(documents.TryGetSummary("DEBUG_BREAKPOINT_PARAMETERS", "Id", out summary));
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
            public partial struct DebugBreakpointParameters
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
    public void TestStructXml1()
    {
        AssertGeneratedWithDocuments("""
            /// <summary>
            /// Blah la la
            /// </summary>
            /// <remarks>
            /// Structure for querying breakpoint information
            /// all at once.
            /// </remarks>
            public partial struct DebugBreakpointParameters
            {
                /// <summary>
                /// Describes A
                /// </summary>
                public ULONG A;
                /// <summary>
                /// Tells B
                /// </summary>
                public LONG  B;
                /// <summary>
                /// Story from C
                /// </summary>
                [MarshalAs(UnmanagedType.LPWStr)]
                public string C;
            }
            """,
            """
            // Structure for querying breakpoint information
            // all at once.
            typedef struct _DEBUG_BREAKPOINT_PARAMETERS
            {
                ULONG A;
                LONG  B;
                PCWSTR C;
            } DEBUG_BREAKPOINT_PARAMETERS, *PDEBUG_BREAKPOINT_PARAMETERS;
            """,
            [
            """
            ---
            UID: NS:dbgeng._DEBUG_BREAKPOINT_PARAMETERS
            description: Blah la la
            ---
            ### -field A

            Describes A

            ### -field B
            
            Tells B

            ### -field C
            
            Story from C
            """
            ]);
    }

    [Fact]
    public void TestStructXml2()
    {
        AssertGeneratedWithDocuments("""
            /// <summary>
            /// Blah la la
            /// </summary>
            /// <remarks>
            /// Structure for querying breakpoint information
            /// all at once.
            /// </remarks>
            public partial struct DebugBreakpointParameters
            {
                /// <summary>
                /// Describes A
                /// </summary>
                public ArrayOf10<INT> A;
                /// <summary>
                /// Tells B
                /// </summary>
                public ArrayOf10<INT> B;
            }
            
            [InlineArray(10)]
            public struct ArrayOf10<T> { private T _item; }
            """,
            """
            // Structure for querying breakpoint information
            // all at once.
            typedef struct _DEBUG_BREAKPOINT_PARAMETERS
            {
                INT A[10];
                INT B[10];
            } DEBUG_BREAKPOINT_PARAMETERS, *PDEBUG_BREAKPOINT_PARAMETERS;
            """,
            [
            """
            ---
            UID: NS:dbgeng._DEBUG_BREAKPOINT_PARAMETERS
            description: Blah la la
            ---
            ### -field A

            Describes A

            ### -field B[10]
            
            Tells B
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

        Assert.True(documents.TryGetSummary("IDebugClient", "EndSession", out var summary));
        Assert.Equal("The EndSession method ends ...", summary);

        Assert.True(documents.TryGetParameters("IDebugClient", "EndSession", out var parameters));
        Assert.Equal([("Flags", "Lorem ipsum sit domit")], parameters);
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

        Assert.True(documents.TryGetSummary("IDebugClient", "EndSession", out summary));
        Assert.Equal("The EndSession method ends ...", summary);

        Assert.True(documents.TryGetParameters("IDebugClient", "EndSession", out var parameters));
        Assert.Equal([("Flags", "Lorem ipsum sit domit")], parameters);
    }

    [Fact]
    public void TestInterfaceFunctionXml()
    {
        AssertGeneratedWithDocuments("""
            /// <summary>
            /// An interface
            /// </summary>
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                /// <summary>
                /// Well ...
                /// </summary>
                /// <param name="Flags">
                /// Lorem ipsum sit domit
                /// </param>
                /// <param name="Args">
                /// Additional args
                /// </param>
                void Boom
                (
                    // _In_
                    UINT32 Flags,
                    // ...
                    [In, MarshalUsing(typeof(ComVariantMarshaller), ElementIndirectionDepth = 1)]
                    params object[] Args
                );

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
                STDMETHOD(Boom)(
                    THIS_
                    _In_ UINT32 Flags,
                    ...
                    ) PURE;
            };
            """,
            [
            """
            ---
            UID: NF:dbgeng.ISomeInterface.Boom
            description: Well ...
            ---

            ### -param Flags [in]

            Lorem ipsum sit domit

            ### -param ...

            Additional args

            """,
            """
            ---
            UID: NN:dbgeng.ISomeInterface
            description: An interface
            ---
            """
            ]);
    }

}
