namespace SrcGenTests;

public class InterfaceTests : TestsBase
{
    [Fact]
    public void TestEmptyInterface1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
            };
            """,
            "");
    }

    [Fact]
    public void TestEmptyCallbackInterface1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ManagedObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeCallback
            {
            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeCallback* PSOME_CALLBACK;

            #undef INTERFACE
            #define INTERFACE ISomeCallback
            DECLARE_INTERFACE_(ISomeCallback, IUnknown)
            {
                // ISomeCallback.
            };
            """,
            "");
    }

    [Fact]
    public void TestEmptyInterface2()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
            }

            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fe6489")]
            public partial interface ISomeInterface2 : ISomeInterface
            {
            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fe6489")
                ISomeInterface2* PSOME_INTERFACE2;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
            };

            #undef INTERFACE
            #define INTERFACE ISomeInterface2
            DECLARE_INTERFACE_(ISomeInterface2, IUnknown)
            {
                // ISomeInterface2.
            };
            """,
            "");
    }

    [Fact]
    public void TestEmptyInterface3()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5453-071f-47bd-9de6-5734c3fe6489")]
            public partial interface ISomeInterface3 : ISomeInterface2
            {
            }
            
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
            }

            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fe6489")]
            public partial interface ISomeInterface2 : ISomeInterface
            {
            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5453-071f-47bd-9de6-5734c3fe6489")
                ISomeInterface3* PSOME_INTERFACE3;
            
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;
            
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fe6489")
                ISomeInterface2* PSOME_INTERFACE2;
            
            #undef INTERFACE
            #define INTERFACE ISomeInterface3
            DECLARE_INTERFACE_(ISomeInterface3, IUnknown)
            {
                // ISomeInterface3
            };
            
            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
            };
            
            #undef INTERFACE
            #define INTERFACE ISomeInterface2
            DECLARE_INTERFACE_(ISomeInterface2, IUnknown)
            {
                // ISomeInterface2.
            };
            """,
            "");
    }

    [Fact]
    public void TestEmptyMethod1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestEmptyMethod2()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                [PreserveSig]
                ULONG Boom
                (
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD_(ULONG, Boom)(
                    THIS
                    ) PURE;
            };
            """,
            "");
    }


    [Fact]
    public void TestMethodRemarks()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                /// <remarks>
                /// blah blah
                /// wow
                /// </remarks>
                void Boom
                (
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.

                // blah blah
                // wow
                STDMETHOD(Boom)(
                    THIS
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestReservedParam1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    IntPtr Reserved = 0
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_opt_ _Reserved_ PVOID Reserved
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_opt_ PSOME_INTERFACE
                    ISomeInterface Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_opt_ PSOME_INTERFACE Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam2()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_opt_ PULONG
                    in ULONG Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_opt_ PULONG Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam3()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_opt_ LPGUID
                    in GUID Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_opt_ LPGUID Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam4()
    {
        AssertGeneratedWithMissing("""
            public partial struct DebugOffsetRegion
            {
                public ULONG64 Base;
                public ULONG64 Size;
            }

            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_opt_ PDEBUG_OFFSET_REGION
                    in DebugOffsetRegion Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            typedef struct _DEBUG_OFFSET_REGION
            {
                ULONG64 Base;
                ULONG64 Size;
            } DEBUG_OFFSET_REGION, *PDEBUG_OFFSET_REGION;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_opt_ PDEBUG_OFFSET_REGION Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam5()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _Inout_opt_ PULONG
                    ref ULONG Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _Inout_opt_ PULONG Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam6()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _Out_ PSOME_INTERFACE
                    out ISomeInterface Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _Out_ PSOME_INTERFACE *Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestPointerParam7()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _Out_ PGUID
                    out GUID Name
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _Out_ PGUID Name
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestDotDotDotParam1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // ...
                    [In, MarshalUsing(typeof(ComVariantMarshaller), ElementIndirectionDepth = 1)]
                    params object[] Args
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    ...
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestAnsiStringParam1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_ PSTR
                    [MarshalAs(UnmanagedType.LPStr)]
                    string Id,
                    // _In_opt_ PCSTR
                    [MarshalAs(UnmanagedType.LPStr)]
                    string Name,
                    // _Out_writes_opt_(2) PSTR
                    Span<byte> Name1
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_ PSTR Id,
                    _In_opt_ PCSTR Name,
                    _Out_writes_opt_(2) PSTR Name1
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestWideStringParam1()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeInterface
            {
                void Boom
                (
                    // _In_ PWSTR
                    [MarshalAs(UnmanagedType.LPWStr)]
                    string Id,
                    // _In_opt_ PCWSTR
                    [MarshalAs(UnmanagedType.LPWStr)]
                    string Name,
                    // _Out_writes_opt_(4) PWSTR
                    Span<char> Name1
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeInterface* PSOME_INTERFACE;

            #undef INTERFACE
            #define INTERFACE ISomeInterface
            DECLARE_INTERFACE_(ISomeInterface, IUnknown)
            {
                // ISomeInterface.
                STDMETHOD(Boom)(
                    THIS_
                    _In_ PWSTR Id,
                    _In_opt_ PCWSTR Name,
                    _Out_writes_opt_(4) PWSTR Name1
                    ) PURE;
            };
            """,
            "");
    }

    [Fact]
    public void TestInBufferParamForCallbacks()
    {
        AssertGeneratedWithMissing("""
            [GeneratedComInterface(Options = ComInterfaceOptions.ManagedObjectWrapper)]
            [Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
            public partial interface ISomeCallback
            {
                void Boom
                (
                    // _In_reads_bytes_(ContextSize) PVOID
                    [MarshalUsing(typeof(BufferMarshaller<,>), CountElementName = "ContextSize")]
                    ReadOnlySpan<byte> Context,
                    // _In_ ULONG
                    ULONG ContextSize
                );

            }
            """,
            hppSrc: """
            typedef interface DECLSPEC_UUID("f2df5f53-071f-47bd-9de6-5734c3fed689")
                ISomeCallback* PSOME_CALLBACK;

            #undef INTERFACE
            #define INTERFACE ISomeCallback
            DECLARE_INTERFACE_(ISomeCallback, IUnknown)
            {
                // ISomeCallback.
                STDMETHOD(Boom)(
                    THIS_
                    _In_reads_bytes_(ContextSize) PVOID Context,
                    _In_ ULONG ContextSize
                    ) PURE;
            };
            """,
            "");
    }

}
