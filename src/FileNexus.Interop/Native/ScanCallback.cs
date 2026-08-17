using System;
using System.Runtime.InteropServices;

namespace FileNexus.Interop.Native;

/// <summary>
/// Unmanaged C callback signature invoked for each file during native scanning.
/// Returns 1 to continue scanning, 0 to abort.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int NativeScanCallbackDelegate(
    IntPtr namePtr,
    IntPtr pathPtr,
    IntPtr extPtr,
    ulong size,
    ulong createdSec,
    ulong modifiedSec,
    IntPtr userData
);
