using ScreentimeTracker.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ScreentimeTracker.Data.Models;


namespace ScreentimeTracker.Core;


public class UserActivityMonitor
{
    private const uint MAX_PATH_FOR_EXE = 1024;

    public ForegroundAppInfo GetCurrentForegroundAppInfo()
    {
        IntPtr fgWindowHandle = NativeMethods.GetForegroundWindow();
        if (fgWindowHandle == IntPtr.Zero)
        {
            return new ForegroundAppInfo { IsValid = false, ErrorMessage = "No foreground window." };
        }

        NativeMethods.GetWindowThreadProcessId(fgWindowHandle, out uint processId);
        if (processId == 0)
        {
            return new ForegroundAppInfo { IsValid = false, ErrorMessage = "Could not get process ID for foreground window." };
        }

        // Get Process Executable Path
        IntPtr processHandle = IntPtr.Zero;
        string executablePath = string.Empty;
        string processName = string.Empty;
        string errorMessage = string.Empty;

        try
        {
            processHandle = NativeMethods.OpenProcess(
                NativeEnums.ProcessAccessFlags.QueryLimitedInformation,
                false, // bInheritHandle
                processId
            );

            if (processHandle != IntPtr.Zero)
            {
                Span<char> exePathBuffer = stackalloc char[(int)MAX_PATH_FOR_EXE];
                uint size = MAX_PATH_FOR_EXE;

                if (NativeMethods.QueryFullProcessImageNameW(processHandle, 0, exePathBuffer, ref size) && size > 0)
                {
                    executablePath = exePathBuffer.Slice(0, (int)size).ToString();
                    processName = Path.GetFileName(executablePath);
                }
                else
                {
                    errorMessage = $"QueryFullProcessImageNameW failed. Error: {Marshal.GetLastPInvokeError()}";
                    Debug.WriteLine(errorMessage);
                }
            }
            else
            {
                errorMessage = $"OpenProcess failed. Error: {Marshal.GetLastPInvokeError()} for PID: {processId}";
                Debug.WriteLine(errorMessage);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Exception getting process info: {ex.Message}";
            Debug.WriteLine(errorMessage);
        }
        finally
        {
            if (processHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(processHandle);
            }
        }

        return new ForegroundAppInfo
        {
            ProcessName = processName,
            ExecutablePath = executablePath,
            WindowTitle = string.Empty, // Will be empty until GetWindowText is implemented
            IsValid = !string.IsNullOrEmpty(processName),
            ErrorMessage = errorMessage
        };
    }

    public bool IsUserConsideredActive(TimeSpan idleThreshold)
    {
        if (IsSystemDisplayRequired())
        {
            return true;
        }
        if (IsDirectUserActive(idleThreshold))
        {
            return true;
        }


        return false;
    }

    private bool IsDirectUserActive(TimeSpan idleThreshold)
    {
        NativeStructs.LASTINPUTINFO lastInputInfo = new NativeStructs.LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeStructs.LASTINPUTINFO));

        if (NativeMethods.GetLastInputInfo(ref lastInputInfo))
        {
            long lastInputTicks = lastInputInfo.dwTime; // This is a tick count
            long currentTicks = Environment.TickCount; // This is also a tick count (milliseconds)

            // TickCount wraps around approx every 49.7 days.
            // For most idle detection scenarios this simple subtraction is fine.
            // More robust handling might use GetTickCount64.
            long idleTimeMs = currentTicks - lastInputTicks;

            // Handle potential TickCount wraparound (if currentTicks < lastInputTicks)
            if (idleTimeMs < 0)
            {
                // A simple way to handle this is to consider the user active,
                // or to use GetTickCount64 if very long uptimes are a concern.
                // For typical idle detection, this scenario might mean user was active very recently
                // across the wraparound boundary, or there's a very long idle period.
                // Assuming active for simplicity here if wrapped.
                return true;
            }

            return idleTimeMs < idleThreshold.TotalMilliseconds;
        }
        // If GetLastInputInfo fails, assume user is inactive or cannot determine
        Debug.WriteLine($"GetLastInputInfo failed. Error: {Marshal.GetLastPInvokeError()}");
        return false;
    }

    private bool IsSystemDisplayRequired()
    {
        try
        {
            int result = NativeMethods.CallNtPowerInformation(
                (int)NativeEnums.PowerInformationLevel.SystemExecutionState,
                IntPtr.Zero,
                0,
                out uint executionState,
                sizeof(uint));

            if (result == 0) // STATUS_SUCCESS
            {
                // Check if ES_DISPLAY_REQUIRED flag is set
                Debug.WriteLine($"IsSystemDisplayRequired: {(executionState & (uint)NativeEnums.SystemExecutionState.ES_DISPLAY_REQUIRED) != 0}");
                return (executionState & (uint)NativeEnums.SystemExecutionState.ES_DISPLAY_REQUIRED) != 0;
            }

            Debug.WriteLine($"CallNtPowerInformation failed with error code: {result}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in IsSystemDisplayRequired: {ex.Message}");
            return false;
        }
    }
}
