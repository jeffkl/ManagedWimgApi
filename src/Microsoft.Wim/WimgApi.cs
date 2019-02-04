using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DWORD = System.UInt32;

// ReSharper disable ArrangeStaticMemberQualifier
// ReSharper disable RedundantNameQualifier
namespace Microsoft.Wim
{
    /// <summary>
    /// Represents the Windows® Imaging API (WIMGAPI) for capturing and applying Windows® images (WIMs).
    /// </summary>
    public static partial class WimgApi
    {
        /// <summary>
        /// Used as an object for locking.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// An instance of the <see cref="WimRegisteredCallbacks"/> class for keeping track of registered callbacks.
        /// </summary>
        private static readonly WimRegisteredCallbacks RegisteredCallbacks = new WimRegisteredCallbacks();

        
        /// <summary>
        /// Registers a log file for debugging or tracing purposes from the current WIMGAPI session.
        /// </summary>
        /// <param name="logFile">The full file path of the file to receive debug or tracing information.</param>
        /// <exception cref="ArgumentNullException">logFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void RegisterLogFile(string logFile)
        {
            // See if logFile is null
            //
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMRegisterLogFile(logFile, 0))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback function.</param>
        /// <returns>The zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimMessageCallback messageCallback)
        {
            // Call an overload
            //
            return RegisterMessageCallback(messageCallback, null);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <param name="userData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
        /// <returns>-1 if the callback is already registered, otherwise the zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimMessageCallback messageCallback, object userData)
        {
            // Call an overload
            //
            return RegisterMessageCallback(WimHandle.Null, messageCallback, userData);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">An optional <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback function.</param>
        /// <returns>The zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // Call an overload
            //
            return WimgApi.RegisterMessageCallback(wimHandle, messageCallback, null);
        }

        /// <summary>
        /// Registers a function to be called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">An optional <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <param name="userData">A pointer that specifies an application-defined value to be passed to the callback function.</param>
        /// <returns>-1 if the callback is already registered, otherwise the zero-based index of the callback.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static int RegisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback, object userData)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if messageCallback is null
            //
            if (messageCallback == null)
            {
                // Throw an ArgumentNullException
                //
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // Establish a lock
            //
            lock (WimgApi.LockObject)
            {
                // See if the user wants to register the handler in the global space for all WIMs
                //
                if (wimHandle == WimHandle.Null)
                {
                    // See if the callback is already registered
                    //
                    if (WimgApi.RegisteredCallbacks.IsCallbackRegistered(messageCallback))
                    {
                        // Just exit, the callback is already registered
                        //
                        return -1;
                    }

                    // Add the callback to the globally registered callbacks
                    //
                    if (!WimgApi.RegisteredCallbacks.RegisterCallback(messageCallback, userData))
                    {
                        return -1;
                    }
                }
                else
                {
                    // See if the message callback is already registered
                    //
                    if (WimgApi.RegisteredCallbacks.IsCallbackRegistered(wimHandle, messageCallback))
                    {
                        // Just exit, the callback is already registered
                        //
                        return -1;
                    }

                    // Add the callback to the registered callbacks by handle
                    //
                    WimgApi.RegisteredCallbacks.RegisterCallback(wimHandle, messageCallback, userData);
                }

                // Call the native function
                //
                DWORD hr = WimgApi.NativeMethods.WIMRegisterMessageCallback(wimHandle, wimHandle == WimHandle.Null ? WimgApi.RegisteredCallbacks.GetNativeCallback(messageCallback) : WimgApi.RegisteredCallbacks.GetNativeCallback(wimHandle, messageCallback), IntPtr.Zero);

                // See if the function returned INVALID_CALLBACK_VALUE
                //
                if (hr == WimgApi.INVALID_CALLBACK_VALUE)
                {
                    // Throw a Win32Exception based on the last error code
                    //
                    throw new Win32Exception();
                }

                // Return the zero-based index of the callback
                //
                return (int)hr;
            }
        }

        /// <summary>
        /// Reactivates a mounted image that was previously mounted to the specified directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file must be remounted.</param>
        /// <exception cref="ArgumentNullException">mountPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void RemountImage(string mountPath)
        {
            // See if mountPath is null
            //
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if mount path does not exist
            //
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMRemountImage(mountPath, 0))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Marks the image with the given image index as bootable.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a Windows® image (.wim) file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="imageIndex">The one-based index of the image to load. An image file can store multiple images.</param>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1 or greater than the number of images in the Windows® image file.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If imageIndex is zero, then none of the images in the .wim file are marked for boot. At any time, only one image in a .wim file can be set to be bootable.</remarks>
        public static void SetBootImage(WimHandle wimHandle, int imageIndex)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if the specified index is valid
            //
            if (imageIndex < 1 || imageIndex > WimgApi.GetImageCount(wimHandle))
            {
                throw new IndexOutOfRangeException($"There is no image at index {imageIndex}.");
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMSetBootImage(wimHandle, (DWORD)imageIndex))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Stores information about an image in the Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of an image returned by the <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/> methods.</param>
        /// <param name="imageInfoXml">An <see cref="IXPathNavigable"/> object that contains information about the volume image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="wimHandle"/> or <paramref name="imageInfoXml"/> is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If the wimHandle parameter is from the <see cref="CreateFile"/> method, then the XML data must be enclosed by &lt;WIM&gt;&lt;/WIM&gt; tags. If the input handle is from the <see cref="LoadImage"/> or <see cref="CaptureImage"/> methods, then the XML data must be enclosed by &lt;IMAGE&gt;&lt;/IMAGE&gt; tags.</remarks>
        public static void SetImageInformation(WimHandle wimHandle, IXPathNavigable imageInfoXml)
        {
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            if (imageInfoXml == null)
            {
                throw new ArgumentNullException(nameof(imageInfoXml));
            }

            SetImageInformation(wimHandle, imageInfoXml.CreateNavigator()?.OuterXml);
        }

        /// <summary>
        /// Stores information about an image in the Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of an image returned by the <see cref="CreateFile"/>, <see cref="LoadImage"/>, or <see cref="CaptureImage"/> methods.</param>
        /// <param name="imageInfoXml">A <see cref="String"/> object that contains information about the volume image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="wimHandle"/> or <paramref name="imageInfoXml"/> is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If the wimHandle parameter is from the <see cref="CreateFile"/> method, then the XML data must be enclosed by &lt;WIM&gt;&lt;/WIM&gt; tags. If the input handle is from the <see cref="LoadImage"/> or <see cref="CaptureImage"/> methods, then the XML data must be enclosed by &lt;IMAGE&gt;&lt;/IMAGE&gt; tags.</remarks>
        public static void SetImageInformation(WimHandle wimHandle, string imageInfoXml)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if imageInfoXml is null
            //
            if (imageInfoXml == null)
            {
                throw new ArgumentNullException(nameof(imageInfoXml));
            }

            // Append a unicode file marker to the xml as a string
            //
            string imageInfo = $"\uFEFF{imageInfoXml}";

            // Allocate enough memory for the info
            //
            IntPtr imageInfoPtr = Marshal.StringToHGlobalUni(imageInfo);

            try
            {
                // Call the native function
                //
                if (!WimgApi.NativeMethods.WIMSetImageInformation(wimHandle, imageInfoPtr, (DWORD)(imageInfo.Length + 1) * 2))
                {
                    // Throw a Win32Exception based on the last error code
                    //
                    throw new Win32Exception();
                }
            }
            finally
            {
                // Free the string buffer
                //
                Marshal.FreeHGlobal(imageInfoPtr);
            }
        }

        /// <summary>
        /// Enables the <see cref="ApplyImage"/> and <see cref="CaptureImage"/> methods to use alternate .wim files for file resources. This can enable optimization of storage when multiple images are captured with similar data.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim (Windows image) file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="path">The path of the .wim file to be added to the reference list.</param>
        /// <param name="mode">Specifies whether the .wim file is added to the reference list or replaces other entries.</param>
        /// <param name="options">Specifies options when adding the .wim file to the reference list.</param>
        /// <exception cref="ArgumentNullException">wimHandle is null
        /// -or-
        /// mode is not WimSetReferenceMode.Replace and path is null.</exception>
        /// <exception cref="FileNotFoundException">path does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If <c>null</c> is passed in for the path parameter and <see cref="WimSetReferenceMode.Replace"/> is passed for the mode parameter, then the reference list is completely cleared, and no file resources are extracted during the <see cref="ApplyImage"/> method.</remarks>
        public static void SetReferenceFile(WimHandle wimHandle, string path, WimSetReferenceMode mode, WimSetReferenceOptions options)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if not replacing and path is null
            //
            if (mode != WimSetReferenceMode.Replace && path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // See if path does not exist
            //
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find part of the path '{path}'");
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMSetReferenceFile(wimHandle, path, (DWORD)mode | (DWORD)options))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Sets the location where temporary imaging files are to be stored.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="path">The path where temporary image (.wim) files are to be stored during capture or application. This is the directory where the image is captured or applied.</param>
        /// <exception cref="ArgumentNullException">wimHandle or path is null.</exception>
        /// <exception cref="DirectoryNotFoundException">path does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void SetTemporaryPath(WimHandle wimHandle, string path)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if path is null
            //
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // See if path does not exist
            //
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Could not find part of the path '{path}'");
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMSetTemporaryPath(wimHandle, path))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Enables a large Windows® image (.wim) file to be split into smaller parts for replication or storage on smaller forms of media.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="partPath">The path of the first file piece of the spanned set.</param>
        /// <param name="partSize">The size of the initial piece of the spanned set. This value will also be the default size used for subsequent pieces.</param>
        /// <exception cref="ArgumentNullException">wimHandle or partPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Directory of partPath does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void SplitFile(WimHandle wimHandle, string partPath, long partSize)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if partPath is null
            //
            if (partPath == null)
            {
                throw new ArgumentNullException(nameof(partPath));
            }

            // See if the directory of partPath does not exist
            //
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(partPath)))
            {
                throw new DirectoryNotFoundException($"Could not find part of the path '{Path.GetDirectoryName(partPath)}'");
            }

            // Create a copy of part size so it can be safely passed by reference
            //
            long partSizeCopy = partSize;

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMSplitFile(wimHandle, partPath, ref partSizeCopy, 0))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Gets the minimum size needed to to create a split WIM.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="partPath">The path of the first file piece of the spanned set.</param>
        /// <returns>The minimum space required to split the WIM.</returns>
        /// <exception cref="ArgumentNullException">wimHandle or partPath is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static long SplitFile(WimHandle wimHandle, string partPath)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if partPath is null
            //
            if (partPath == null)
            {
                throw new ArgumentNullException(nameof(partPath));
            }

            // Declare a part size as zero
            //
            long partSize = 0;

            // Call the WIMSplitFile function which should return false and set partSize to the minimum size needed
            //
            if (!WimgApi.NativeMethods.WIMSplitFile(wimHandle, partPath, ref partSize, 0))
            {
                // See if the return code was not ERROR_MORE_DATA
                //
                if (Marshal.GetLastWin32Error() != WimgApi.ERROR_MORE_DATA)
                {
                    // Throw a Win32Exception based on the last error code
                    //
                    throw new Win32Exception();
                }
            }

            return partSize;
        }

        /// <summary>
        /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file was mounted.</param>
        /// <param name="imagePath">The full file name of the .wim file that must be unmounted.</param>
        /// <param name="imageIndex">Specifies the index of the image in the .wim file that must be unmounted.</param>
        /// <param name="commitChanges"><c>true</c> to commit changes made to the .wim file if any, otherwise <c>false</c> to discard changes.  This parameter has no effect if the .wim file was mounted not to enable edits.</param>
        /// <exception cref="ArgumentNullException">mountPath or imagePath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="FileNotFoundException">imagePath does not exist.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>This method unmaps the contents of the given image in the .wim file from the specified mount directory. After the successful completion of this operation, users or applications will not be able to access the contents of the image previously mapped under the mount directory.</remarks>
        public static void UnmountImage(string mountPath, string imagePath, int imageIndex, bool commitChanges)
        {
            // See if mountPath is null
            //
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if imagePath is null
            //
            if (imagePath == null)
            {
                throw new ArgumentNullException(nameof(imagePath));
            }

            // See if the specified index is valid
            //
            if (imageIndex < 1)
            {
                throw new IndexOutOfRangeException($"There is no image at index {imageIndex}.");
            }

            // See if mount path does not exist
            //
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // See if the image does not exist
            //
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Could not find a part of the path '{imagePath}'");
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMUnmountImage(mountPath, imagePath, (DWORD)imageIndex, commitChanges))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> of an image previously mounted with <see cref="MountImage(WimHandle, string, WimMountImageOptions)"/>.</param>
        /// <exception cref="ArgumentNullException">imageHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unmount")]
        public static void UnmountImage(WimHandle imageHandle)
        {
            // See if imageHandle is null
            //
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMUnmountImageHandle(imageHandle, 0))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }

            // Close the image handle
            //
            imageHandle.Close();
        }

        /// <summary>
        /// Unregisters a log file for debugging or tracing purposes from the current WIMGAPI session.
        /// </summary>
        /// <param name="logFile">The path to a log file previously specified in a call to the <see cref="RegisterLogFile"/> method.</param>
        /// <exception cref="ArgumentNullException">logFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterLogFile(string logFile)
        {
            // See if logFile is null
            //
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMUnregisterLogFile(logFile))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Unregisters a method from being called with imaging-specific data for all image handles.
        /// </summary>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <exception cref="ArgumentOutOfRangeException">messageCallback is not registered.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterMessageCallback(WimMessageCallback messageCallback)
        {
            UnregisterMessageCallback(WimHandle.Null, messageCallback);
        }

        /// <summary>
        /// Unregisters a method from being called with imaging-specific data for only the specified WIM file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="messageCallback">An application-defined callback method.</param>
        /// <exception cref="ArgumentOutOfRangeException">messageCallback is not registered.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnregisterMessageCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // See if wimHandle is null
            //
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Establish a lock
            //
            lock (WimgApi.LockObject)
            {
                // See if wimHandle was not specified but the message callback was
                //
                if (wimHandle == WimHandle.Null && messageCallback != null)
                {
                    // See if the message callback is registered
                    //
                    if (!WimgApi.RegisteredCallbacks.IsCallbackRegistered(messageCallback))
                    {
                        // Throw an ArgumentOutOfRangeException
                        //
                        throw new ArgumentOutOfRangeException(nameof(messageCallback), "Message callback is not registered.");
                    }
                }

                // See if the wimHandle and callback were specified
                //
                if (wimHandle != WimHandle.Null && messageCallback != null)
                {
                    // See if the callback is registered
                    //
                    if (!WimgApi.RegisteredCallbacks.IsCallbackRegistered(wimHandle, messageCallback))
                    {
                        // Throw an ArgumentOutOfRangeException
                        //
                        throw new ArgumentOutOfRangeException(nameof(messageCallback), "Message callback is not registered under this handle.");
                    }
                }

                // See if the message callback is null, meaning the user wants to unregister all callbacks
                //
                bool success = messageCallback == null ?
                    // Call the native function and pass null for the callback
                    //
                    WimgApi.NativeMethods.WIMUnregisterMessageCallback(wimHandle, fpMessageProc: null) : WimgApi.NativeMethods.WIMUnregisterMessageCallback(wimHandle, wimHandle == WimHandle.Null ? WimgApi.RegisteredCallbacks.GetNativeCallback(messageCallback) :
                        // Call the native function and pass the appropriate callback
                        //
                        WimgApi.RegisteredCallbacks.GetNativeCallback(wimHandle, messageCallback));

                // See if the native call succeeded
                //
                if (!success)
                {
                    // Throw a Win32Exception based on the last error code
                    //
                    throw new Win32Exception();
                }

                // See if a single globally registered callback should be removed
                //
                if (wimHandle == WimHandle.Null && messageCallback != null)
                {
                    // Unregister the globally registered callback
                    //
                    WimgApi.RegisteredCallbacks.UnregisterCallback(messageCallback);
                }

                // See if a single registered callback by handle should be removed
                //
                if (wimHandle != WimHandle.Null && messageCallback != null)
                {
                    // Unregister the callback for the handle
                    //
                    WimgApi.RegisteredCallbacks.UnregisterCallback(wimHandle, messageCallback);
                }

                // See if all registered callbacks for this handle should be removed
                //
                if (wimHandle != WimHandle.Null && messageCallback == null)
                {
                    // Unregister all callbacks for this handle
                    //
                    WimgApi.RegisteredCallbacks.UnregisterCallbacks(wimHandle);
                }

                // See if all registered callbacks by handle and all globally registered callbacks should be removed
                //
                if (wimHandle == WimHandle.Null && messageCallback == null)
                {
                    // Unregister all callbacks
                    //
                    WimgApi.RegisteredCallbacks.UnregisterCallbacks();
                }
            } // Release lock
        }

        /// <summary>
        /// Closes an open Windows® imaging (.wim) file or image handle.
        /// </summary>
        /// <param name="handle">A <see cref="WimHandle"/> to an open, image-based object.</param>
        /// <returns><c>true</c> if the handle was successfully closed, otherwise <c>false</c>.</returns>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        internal static bool CloseHandle(IntPtr handle)
        {
            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMCloseHandle(handle))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }

            return true;
        }
    }
}