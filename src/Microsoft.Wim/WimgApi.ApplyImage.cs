using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Applies an image to a directory path from a Windows® image (.wim) file.
        /// </summary>
        /// <param name="imageHandle">A handle to a volume image returned by the <see cref="LoadImage" /> or <see cref="CaptureImage" /> methods.</param>
        /// <param name="path">The root drive or the directory path where the image data will be applied.</param>
        /// <param name="options">Specifies how the file is to be treated and what features are to be used.</param>
        /// <returns><c>true</c> if the image was successfully applied, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">imageHandle is null.</exception>
        /// <exception cref="OperationCanceledException">The operation of applying the image was aborted by a callback returning <see cref="WimMessageResult.Abort" />.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void ApplyImage(WimHandle imageHandle, string path, WimApplyImageOptions options)
        {
            // See if the handle is null
            //
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMApplyImage(imageHandle, path, (UInt32)options))
            {
                // Get the last error
                //
                Win32Exception win32Exception = new Win32Exception();

                switch (win32Exception.NativeErrorCode)
                {
                    case WimgApi.ERROR_REQUEST_ABORTED:
                        // If the operation was aborted, throw an OperationCanceledException exception
                        //
                        throw new OperationCanceledException(win32Exception.Message, win32Exception);
                    default:
                        throw win32Exception;
                }
            }
        }

        private static partial class NativeMethods
        {
            /// <summary>
            /// Applies an image to a directory path from a Windows® image (.wim) file.
            /// </summary>
            /// <param name="hImage">A handle to a volume image returned by the WIMLoadImage or WIMCaptureImage functions.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string containing the root drive or the directory path where the
            /// image data will be applied.
            /// </param>
            /// <param name="dwApplyFlags">Specifies how the file is to be treated and what features are to be used.</param>
            /// <returns>
            /// If the function succeeds, the return value is an open handle to the specified image file.
            /// If the function fails, the return value is NULL. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>
            /// To obtain more information during an image apply, see the WIMRegisterMessageCallback function.
            /// To obtain the list of files in an image without actually applying the image, specify the WIM_FLAG_NO_APPLY flag and
            /// register a callback that handles the WIM_MSG_PROCESS message. To obtain additional file information from the
            /// WIM_MSG_FILEINFO message, specify the WIM_FLAG_FILEINFO.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMApplyImage(WimHandle hImage, string pszPath, DWORD dwApplyFlags);
        }
    }
}