using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Wim
{
    /// <summary>
    /// Represents a handle to a Windows® image (.wim) file or an image inside of a .wim file.
    /// </summary>
    public sealed class WimHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Represents a <c>null</c> handle.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly WimHandle Null = new WimHandle();

        /// <summary>
        /// Initializes a new instance of the WimHandle class.
        /// </summary>
        internal WimHandle()
            : base(true)
        {
            // Default to a null handle
            //
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// Frees the handle.
        /// </summary>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return !IsInvalid && WimgApi.CloseHandle(handle);
        }
    }
}