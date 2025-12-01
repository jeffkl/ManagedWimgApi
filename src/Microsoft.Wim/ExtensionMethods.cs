// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Marshals data from an unmanaged block of memory to a newly allocated managed object of the specified type.
        /// </summary>
        /// <typeparam name="T">The System.Type of object to be created. This type object must represent a formatted class or a structure.</typeparam>
        /// <param name="ptr">A pointer to an unmanaged block of memory.</param>
        /// <returns>A managed object containing the data pointed to by the ptr parameter.</returns>
        /// <exception cref="ArgumentException">The T parameter layout is not sequential or explicit.
        /// -or-
        /// The T parameter is a generic type.</exception>
#if NET5_0_OR_GREATER
        public static T? ToStructure<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(this IntPtr ptr)
#else

        public static T? ToStructure<T>(this IntPtr ptr)
#endif
        {
#if NET5_0_OR_GREATER
            return Marshal.PtrToStructure<T>(ptr);
#else
            object? result = Marshal.PtrToStructure(ptr, typeof(T));

            if (result is null)
            {
                return default;
            }

            return (T)result;
#endif
        }
    }
}
