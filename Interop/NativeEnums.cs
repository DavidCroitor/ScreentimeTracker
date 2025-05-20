
namespace ScreentimeTracker.Interop;

internal static class NativeEnums
{
    /// <summary>
    /// Defines the access rights for opening a process.
    /// </summary>
    [Flags]
    internal enum ProcessAccessFlags : uint
    {
        /// <summary>
        /// Required to query limited information about a process, such as its name.
        /// </summary>
        QueryLimitedInformation = 0x00001000
    }
    /// <summary>
    /// Execution states that can be set with the SetThreadExecutionState function
    /// and retrieved with CallNtPowerInformation.
    /// </summary>
    [Flags]
    internal enum SystemExecutionState : uint
    {
        /// <summary>
        /// Forces the system to be in the working state by resetting the system idle timer.
        /// </summary>
        ES_SYSTEM_REQUIRED = 0x00000001,
        
        /// <summary>
        /// Forces the display to be on by resetting the display idle timer.
        /// </summary>
        ES_DISPLAY_REQUIRED = 0x00000002,
        
        /// <summary>
        /// This value is not supported. If ES_USER_PRESENT is combined with other values,
        /// the call will fail and none of the specified states will be set.
        /// </summary>
        ES_USER_PRESENT = 0x00000004,
        
        /// <summary>
        /// Enables away mode. This value must be specified with ES_CONTINUOUS.
        /// </summary>
        ES_AWAYMODE_REQUIRED = 0x00000040,
        
        /// <summary>
        /// Informs the system that the state being set should remain in effect until
        /// the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
        /// </summary>
        ES_CONTINUOUS = 0x80000000
    }

    /// <summary>
    /// Information levels for power information.
    /// </summary>
    internal enum PowerInformationLevel
    {
        /// <summary>
        /// The system execution state.
        /// </summary>
        SystemExecutionState = 16
    }
}
