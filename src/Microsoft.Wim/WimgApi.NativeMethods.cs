using System;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;
using LARGE_INTEGER = System.UInt64;

// ReSharper disable RedundantNameQualifier

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// The calling convention to use when calling the WIMGAPI.
        /// </summary>
        internal const CallingConvention WimgApiCallingConvention = CallingConvention.Winapi;

        /// <summary>
        /// The character set to use when calling the WIMGAPI.
        /// </summary>
        internal const CharSet WimgApiCharSet = CharSet.Unicode;

        /// <summary>
        /// The name of the assembly containing the Windows® Imaging API (WIMGAPI).
        /// </summary>
        internal const string WimgApiDllName = "WimgApi.dll";

        /// <summary>
        /// An application-defined callback function used with the CopyFileEx, MoveFileTransacted, and MoveFileWithProgress functions. It is called when a portion of a copy or move operation is completed. The LPPROGRESS_ROUTINE type defines a pointer to this callback function. CopyProgressRoutine is a placeholder for the application-defined function name.
        /// </summary>
        /// <param name="TotalFileSize">The total size of the file, in bytes.</param>
        /// <param name="TotalBytesTransferred">The total number of bytes transferred from the source file to the destination file since the copy operation began.</param>
        /// <param name="StreamSize">The total size of the current file stream, in bytes.</param>
        /// <param name="StreamBytesTransferred">The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.</param>
        /// <param name="dwStreamNumber">A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.</param>
        /// <param name="dwCallbackReason">The reason that CopyProgressRoutine was called.</param>
        /// <param name="hSourceFile">A handle to the source file.</param>
        /// <param name="hDestinationFile">A handle to the destination file.</param>
        /// <param name="lpData">Argument passed to CopyProgressRoutine by CopyFileEx, MoveFileTransacted, or MoveFileWithProgress.</param>
        /// <returns>The CopyProgressRoutine function should return one of the following values.
        /// PROGRESS_CANCEL - Cancel the copy operation and delete the destination file.
        /// PROGRESS_CONTINUE - Continue the copy operation.
        /// PROGRESS_QUIET - Continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
        /// PROGRESS_STOP - Stop the copy operation. It can be restarted at a later time.
        /// </returns>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa363854(v=vs.85).aspx
        // ReSharper disable InconsistentNaming
        public delegate CopyFileProgressAction CopyProgressRoutine(LARGE_INTEGER TotalFileSize, LARGE_INTEGER TotalBytesTransferred, LARGE_INTEGER StreamSize, LARGE_INTEGER StreamBytesTransferred, DWORD dwStreamNumber, DWORD dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

        /// <summary>
        /// Contains declarations for external native functions.
        /// </summary>
        private static partial class NativeMethods
        {
            /// <summary>
            /// Closes an open Windows® imaging (.wim) file or image handle.
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd851955.aspx">WIMCloseHandle</a>
            /// </summary>
            /// <param name="hObject">The handle to an open, image-based object.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>
            /// The WIMCloseHandle function closes handles to the following objects:
            /// A .wim file
            /// A volume image
            /// If there are any open volume image handles, closing a .wim file fails.
            /// Use the WIMCloseHandle function to close handles returned by calls to the WIMCreateFile, WIMLoadImage, and
            /// WIMCaptureImage functions.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMCloseHandle(IntPtr hObject);
            
            

            /// <summary>
            /// Mounts an image in a Windows® image (.wim) file to the specified directory.
            /// </summary>
            /// <param name="hImage">
            /// A handle to a volume image returned by the WIMLoadImage or WIMCaptureImage function. The WIM file
            /// must have been opened with WIM_GENERIC_MOUNT flag in call to WIMCreateFile.
            /// </param>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file has been mounted.
            /// This parameter is required and cannot be NULL. The specified path must not exceed MAX_PATH characters in length.
            /// </param>
            /// <param name="dwMountFlags">Specifies how the file is to be treated and what features are to be used.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMMountImageHandle function maps the contents of the given image in a .wim file to the specified mount
            /// directory. After the successful completion of this operation, users or applications can access the contents of the
            /// image mapped under the mount directory. The WIM file containing the image must be opened with WIM_GENERIC_MOUNT access.
            /// Use the WIMUnmountImageHandle function to unmount the image from the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMMountImageHandle(WimHandle hImage, string pszMountPath, DWORD dwMountFlags);

            /// <summary>
            /// Registers a log file for debugging or tracing purposes into the current WIMGAPI session.
            /// </summary>
            /// <param name="pszLogFile">
            /// A pointer to the full file path of the file to receive debug or tracing information. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMRegisterLogFile(string pszLogFile, DWORD dwFlags);

            /// <summary>
            /// Registers a function to be called with imaging-specific data.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="fpMessageProc">
            /// A pointer to an application-defined callback function. For more information, see the
            /// WIMMessageCallback function.
            /// </param>
            /// <param name="pvUserData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
            /// <returns>
            /// If the function succeeds, then the return value is the zero-based index of the callback. If the function
            /// fails, then the return value is INVALID_CALLBACK_VALUE (0xFFFFFFFF). To obtain extended error information, call the
            /// GetLastError function.
            /// </returns>
            /// <remarks>
            /// If a WIM handle is specified, the callback function receives messages for only that WIM file. If no handle is
            /// specified, then the callback function receives messages for all image handles.
            /// Call the WIMUnregisterMessageCallback function when the callback function is no longer required.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern DWORD WIMRegisterMessageCallback([Optional] WimHandle hWim, WimgApi.WIMMessageCallback fpMessageProc, IntPtr pvUserData);

            /// <summary>
            /// Reactivates a mounted image that was previously mounted to the specified directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file must be remounted.
            /// This parameter is required and cannot be NULL.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMRemountImage function maps the contents of the given image in a .wim file to the specified mount
            /// directory. After the successful completion of this operation, users or applications can access the contents of the
            /// image mapped under the mount directory. Use the WIMUnmountImage function to unmount the image from the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMRemountImage(string pszMountPath, DWORD dwFlags);

            /// <summary>
            /// Marks the image with the given image index as bootable.
            /// </summary>
            /// <param name="hWim">A handle to a Windows® image (.wim) file returned by the WIMCreateFile function.</param>
            /// <param name="dwImageIndex">The one-based index of the image to load. An image file can store multiple images.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// If the input value for the dwImageIndex is zero, then none of the images in the .wim file are marked for boot.
            /// At any time, only one image in a .wim file can be set to be bootable.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetBootImage(WimHandle hWim, DWORD dwImageIndex);

            /// <summary>
            /// Stores information about an image in the Windows® image (.wim) file.
            /// </summary>
            /// <param name="hImage">A handle returned by the WIMCreateFile, WIMLoadImage, or WIMCaptureImage functions.</param>
            /// <param name="pvImageInfo">A pointer to a buffer that contains information about the volume image.</param>
            /// <param name="cbImageInfo">Specifies the size, in bytes, of the buffer pointed to by the pvImageInfo parameter.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// The data buffer being passed into the function must be the memory representation of a Unicode XML file. Calling this
            /// function replaces any customized image data, so, to preserve existing XML information, call the WIMGetImageInformation
            /// function and append or edit the data.
            /// If the input handle is from the WIMCreateFile function, then the XML data must be enclosed by <WIM></WIM> tags. If the
            /// input handle is from the WIMLoadImage or WIMCaptureImage functions, then the XML data must be enclosed by
            /// <IMAGE></IMAGE> tags.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetImageInformation(WimHandle hImage, IntPtr pvImageInfo, DWORD cbImageInfo);

            /// <summary>
            /// Enables the WIMApplyImage and WIMCaptureImage functions to use alternate .wim files for file resources. This can enable
            /// optimization of storage when multiple images are captured with similar data.
            /// </summary>
            /// <param name="hWim">A handle to a .wim (Windows image) file returned by the WIMCreateFile function.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string containing the path of the .wim file to be added to the
            /// reference list.
            /// </param>
            /// <param name="dwFlags">
            /// Specifies how the .wim file is added to the reference list. This parameter must include one of
            /// the following two values.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// If NULL is passed in for the pszPath parameter and WIM_REFERENCE_REPLACE is passed for the dwFlags parameter,
            /// then the reference list is completely cleared, and no file resources are extracted during the WIMApplyImage function.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetReferenceFile(WimHandle hWim, string pszPath, DWORD dwFlags);

            /// <summary>
            /// Sets the location where temporary imaging files are to be stored.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by the WIMCreateFile function.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string, indicating the path where temporary image (.wim) files are
            /// to be stored during capture or application. This is the directory where the image is captured or applied.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetTemporaryPath(WimHandle hWim, string pszPath);

            /// <summary>
            /// Enables a large Windows® image (.wim) file to be split into smaller parts for replication or storage on smaller forms
            /// of media.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="pszPartPath">
            /// A pointer to a null-terminated string containing the path of the first file piece of the
            /// spanned set.
            /// </param>
            /// <param name="pliPartSize">
            /// A pointer to a LARGE_INTEGER, specifying the size of the initial piece of the spanned set.
            /// This value will also be the default size used for subsequent pieces, unless altered by a response to the WIM_MSG_SPLIT
            /// message. If the size specified is insufficient to create the first part of the spanned .wim file, the value is filled
            /// in with the minimum space required. If a single file is larger than the value specified, one of the split .swm files
            /// that results will be larger than the specified value in order to accommodate the large file. See Remarks.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// </returns>
            /// <remarks>
            /// To obtain the minimum space required for the initial .wim file, set the contents of the pliPartSize parameter to zero
            /// and call the WIMSplitFile function. The function will return FALSE and set the LastError function to ERROR_MORE_DATA,
            /// and the contents of the pliPartSize parameter will be set to the minimum space required.
            /// This function creates many parts that are required to contain the resources of the original .wim file. The calling
            /// application may alter the path and size of subsequent pieces by responding to the WIM_MSG_SPLIT message.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSplitFile(WimHandle hWim, string pszPartPath, ref long pliPartSize, DWORD dwFlags);

            /// <summary>
            /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file was mounted. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <param name="pszWimFileName">
            /// A pointer to the full file name of the .wim file that must be unmounted. This parameter is
            /// required and cannot be NULL.
            /// </param>
            /// <param name="dwImageIndex">Specifies the index of the image in the .wim file that must be unmounted.</param>
            /// <param name="bCommitChanges">
            /// A flag that indicates whether changes (if any) to the .wim file must be committed before
            /// unmounting the .wim file. This flag has no effect if the .wim file was mounted not to enable edits.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMUnmountImage function unmaps the contents of the given image in the .wim file from the specified mount
            /// directory. After the successful completion of this operation, users or applications will not be able to access the
            /// contents of the image previously mapped under the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnmountImage(string pszMountPath, string pszWimFileName, DWORD dwImageIndex, [MarshalAs(UnmanagedType.Bool)] bool bCommitChanges);

            /// <summary>
            /// Unmounts an image from a Windows® image (.wim) that was previously mounted with the WIMMountImageHandle function.
            /// </summary>
            /// <param name="hImage">A handle to an image previously mounted with WIMMountImageHandle.</param>
            /// <param name="dwUnmountFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMUnmountImageHandle function unmaps the contents of the given image in the .wim file from the specified
            /// mount directory. After the successful completion of this operation, users or applications will not be able to access
            /// the contents of the image previously mapped under the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnmountImageHandle(WimHandle hImage, DWORD dwUnmountFlags);

            /// <summary>
            /// Unregisters a log file for debugging or tracing purposes from the current WIMGAPI session.
            /// </summary>
            /// <param name="pszLogFile">
            /// The path to a log file previously specified in a call to the WIMRegisterLogFile function. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnregisterLogFile(string pszLogFile);

            /// <summary>
            /// Unregisters a function from being called with imaging-specific data.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="fpMessageProc">
            /// A pointer to the application-defined callback function to unregister. Specify NULL to
            /// unregister all callback functions.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnregisterMessageCallback([Optional] WimHandle hWim, WimgApi.WIMMessageCallback fpMessageProc);
        }

        // ReSharper restore InconsistentNaming
    }
}