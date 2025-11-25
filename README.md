# Interop.DbgEng.dll

A .Net interop library to use the [Microsoft Debugger Engine](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/debugger-engine-overview).

This library is created by translating the `dbgeng.h` to C# code that utilizes [Source Generated COM interop](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/comwrappers-source-generation).

## Get started

1. Add a package reference to [DumbPrograms.DbgEng](https://www.nuget.org/packages/DumbPrograms.DbgEng) in your project.
1. Get an `IDebugClient` from the static `Interop.DbgEng.IDebugClient.Create()` method.
1. Voila!

## Example

See https://github.com/DumbPrograms/PrintDmpStack

## Build

After you cloned this repository, run the following commands within it to get the docs from Microsoft:

```cmd
git clone --filter=tree:0 --sparse https://github.com/MicrosoftDocs/windows-driver-docs-ddi msdocs
cd msdocs
git sparse-checkout set wdk-ddi-src/content/dbgeng
```

Open the `DbgEngIdl.slnx` file in Visual Studio.
