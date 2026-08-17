using System;
using System.Runtime.InteropServices;

namespace FileNexus.Interop.Native;

internal static partial class NativeMethods
{
    private const string LibraryName = "filenexus_engine";

    [LibraryImport(LibraryName, EntryPoint = "filenexus_scan_directory", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial long ScanDirectory(
        string dirPath,
        NativeScanCallbackDelegate callback,
        IntPtr userData
    );

    [LibraryImport(LibraryName, EntryPoint = "filenexus_free_string")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void FreeString(IntPtr ptr);
}
