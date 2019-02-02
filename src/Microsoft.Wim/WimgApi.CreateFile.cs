using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Makes a new image file or opens an existing image file.
        /// </summary>
        /// <param name="path">The name of the file to create or to open.</param>
        /// <param name="desiredAccess">The type of <see cref="WimFileAccess"/> to the object. An application can obtain read access, write access, read/write access, or device query access.</param>
        /// <param name="creationDisposition">The <see cref="WimCreationDisposition"/> to take on files that exist, and which action to take when files do not exist.</param>
        /// <param name="options"><see cref="WimCreateFileOptions"/> to be used for the specified file.</param>
        /// <param name="compressionType">The <see cref="WimCompressionType"/> to be used for a newly created image file.  If the file already exists, then this value is ignored.</param>
        /// <exception cref="ArgumentNullException">path is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimHandle CreateFile(string path, WimFileAccess desiredAccess, WimCreationDisposition creationDisposition, WimCreateFileOptions options, WimCompressionType compressionType)
        {
            // See if destinationFile is null
            //
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Call the native function
            //
            WimHandle wimHandle = WimgApi.NativeMethods.WIMCreateFile(path, (DWORD)desiredAccess, (DWORD)creationDisposition, (DWORD)options, (DWORD)compressionType, out _);

            // See if the handle returned is valid
            //
            if (wimHandle == null || wimHandle.IsInvalid)
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }

            // Return the handle to the wim
            //
            return wimHandle;
        }

        private static partial class NativeMethods
        {
            /// <summary>
            /// Makes a new image file or opens an existing image file.
            /// </summary>
            /// <param name="pszWimPath">
            /// A pointer to a null-terminated string that specifies the name of the file to create or to
            /// open.
            /// </param>
            /// <param name="dwDesiredAccess">
            /// Specifies the type of access to the object. An application can obtain read access, write
            /// access, read/write access, or device query access.
            /// </param>
            /// <param name="dwCreationDisposition">
            /// Specifies which action to take on files that exist, and which action to take when
            /// files do not exist.
            /// </param>
            /// <param name="dwFlagsAndAttributes">Specifies special actions to be taken for the specified file.</param>
            /// <param name="dwCompressionType">
            /// Specifies the compression mode to be used for a newly created image file. If the file
            /// already exists, then this value is ignored.
            /// </param>
            /// <param name="pdwCreationResult">
            /// A pointer to a variable that receives one of the following creation-result values. If
            /// this information is not required, specify NULL.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is an open handle to the specified image file.
            /// If the function fails, the return value is NULL. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>Use the WIMCloseHandle function to close the handle returned by the WIMCreateFile function.</remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern WimHandle WIMCreateFile(string pszWimPath, DWORD dwDesiredAccess, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, DWORD dwCompressionType, out WimCreationResult pdwCreationResult);
        }
    }
}