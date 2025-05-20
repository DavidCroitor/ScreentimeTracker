using System.Runtime.InteropServices;

namespace ScreentimeTracker.Interop;

internal static class NativeStructs
{
     /// <summary>
    /// Contains information about the last input event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LASTINPUTINFO
    {
        /// <summary>
        /// The size of the structure, in bytes. This member must be set to Marshal.SizeOf(typeof(LASTINPUTINFO)).
        /// </summary>
        public uint cbSize;

        /// <summary>
        /// The tick count when the last input event was received.
        /// </summary>
        public uint dwTime;
    }


}