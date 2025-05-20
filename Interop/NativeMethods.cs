using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreentimeTracker.Interop;
/// <summary>
/// Provides P/Invoke declarations for common Windows API functions
/// used for application and activity tracking.
/// All methods use LibraryImport for modern, source-generated interop.
/// </summary>
internal static partial class NativeMethods
{
    /// <summary>
    /// Retrieves a handle to the foreground window (the window with which the user is currently working).
    /// </summary>
    /// <returns>A handle to the foreground window. The return value can be IntPtr.Zero if there is no foreground window.</returns>
    [LibraryImport("user32.dll")]
    internal static partial IntPtr GetForegroundWindow();

    /// <summary>
    /// Retrieves the identifier of the thread that created the specified window and, optionally,
    /// the identifier of the process that created the window.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier.
    /// If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the
    /// process to the variable; otherwise, it does not.</param>
    /// <returns>The return value is the identifier of the thread that created the window.</returns>
    /// <remarks>
    /// If the function succeeds, the lpdwProcessId parameter receives the process ID.
    /// If the function fails, lpdwProcessId is not modified.
    /// This declaration does not set SetLastError=true by default with LibraryImport for return uint;
    /// however, the lpdwProcessId is the primary output needed here.
    /// If error checking on the thread ID is needed, SetLastError=true can be added.
    /// </remarks>
    [LibraryImport("user32.dll")] // SetLastError is not set here; usually the PID out param is what's checked.
                                  // If you want to check GetLastPInvokeError on this call, add SetLastError=true.
    internal static partial uint GetWindowThreadProcessId(
                                                IntPtr hWnd,
                                                out uint lpdwProcessId);

    /// <summary>
    /// Opens an existing local process object.
    /// </summary>
    /// <param name="processAccess">The access to the process object. This access right is
    /// checked against the security descriptor for the process.</param>
    /// <param name="bInheritHandle">If this value is TRUE, processes created by this process
    /// will inherit the handle. Otherwise, the processes do not inherit this handle.</param>
    /// <param name="processId">The identifier of the local process to be opened.</param>
    /// <returns>If the function succeeds, the return value is an open handle to the specified process.
    /// If the function fails, the return value is IntPtr.Zero. To get extended error information, call GetLastPInvokeError.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial IntPtr OpenProcess(
                                        NativeEnums.ProcessAccessFlags processAccess,
                                        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
                                        uint processId);


    /// <summary>
    /// Retrieves the full name of the executable image for the specified process.
    /// </summary>
    /// <param name="hProcess">A handle to the process.</param>
    /// <param name="dwFlags">This parameter can be 0 or PROCESS_NAME_NATIVE.
    /// If PROCESS_NAME_NATIVE, the function retrieves the name in native system format.</param>
    /// <param name="lpExeName">A Span of characters that receives the path to the executable image.
    /// The path can be in Win32 path format or native system format.</param>
    /// <param name="lpdwSize">On input, specifies the size of the lpExeName buffer, in characters.
    /// On success, receives the number of characters written to the buffer, not including the null-terminating character.
    /// If the buffer is too small, the function fails, GetLastPInvokeError returns ERROR_INSUFFICIENT_BUFFER,
    /// and this parameter receives the required buffer size, including the null-terminating character.</param>
    /// <returns>If the function succeeds, the return value is true; otherwise, it is false.
    /// To get extended error information, call GetLastPInvokeError.</returns>
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool QueryFullProcessImageNameW(
                                                IntPtr hProcess,
                                                uint dwFlags,
                                                Span<char> lpExeName,
                                                ref uint lpdwSize);

    /// <summary>
    /// Closes an open object handle.
    /// </summary>
    /// <param name="hObject">A valid handle to an open object.</param>
    /// <returns>If the function succeeds, the return value is true.
    /// If the function fails, the return value is false. To get extended error information, call GetLastPInvokeError.</returns>
    /// <remarks>
    /// This function should be used to close handles obtained from functions like OpenProcess.
    /// Failure to close handles can lead to resource leaks.
    /// </remarks>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(IntPtr hObject);


    /// <summary>
    /// Retrieves the time of the last input event.
    /// </summary>
    /// <param name="plii">A pointer to a LASTINPUTINFO structure that receives the time of the last input event.</param>
    /// <returns>If the function succeeds, the return value is true.
    /// If the function fails, the return value is false. To get extended error information, call GetLastPInvokeError.</returns>
    /// <remarks>
    /// Before calling this function, the cbSize member of the LASTINPUTINFO structure must be set to the size of the structure.
    /// </remarks>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetLastInputInfo(ref NativeStructs.LASTINPUTINFO plii);
    
    /// <summary>
    /// Retrieves power information from the system.
    /// </summary>
    /// <param name="InformationLevel">The information level (type of power information) to retrieve.</param>
    /// <param name="lpInputBuffer">A pointer to an optional input buffer.</param>
    /// <param name="nInputBufferSize">The size of the input buffer, in bytes.</param>
    /// <param name="lpOutputBuffer">A pointer to a buffer that receives the requested power information.</param>
    /// <param name="nOutputBufferSize">The size of the output buffer, in bytes.</param>
    /// <returns>If the function succeeds, the return value is zero. If the function fails, the return value is nonzero.</returns>
    [LibraryImport("powrprof.dll", SetLastError = true)]
    internal static partial int CallNtPowerInformation(
        int InformationLevel,
        IntPtr lpInputBuffer,
        uint nInputBufferSize,
        out uint lpOutputBuffer,
        uint nOutputBufferSize);
}
