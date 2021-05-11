// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Shell32
    {
        [DllImport(Libraries.Shell32, ExactSpelling = true, EntryPoint = "DragQueryFileW", CharSet = CharSet.Unicode)]
        private static extern unsafe uint DragQueryFileWInternal(IntPtr hDrop, uint iFile, char* lpszFile, uint cch);

        public static unsafe uint DragQueryFileW(IntPtr hDrop, uint iFile, int capacity, out string lpszFile)
        {
            if (capacity == 0 || iFile == 0xFFFFFFFF)
            {
                lpszFile = string.Empty;
                return DragQueryFileWInternal(hDrop, iFile, null, 0);
            }

            uint resultValue;
            char[] buf = ArrayPool<char>.Shared.Rent(capacity);
            fixed (char* valueChars = &buf[0])
            {
                // iterating by allocating chunk of memory each time we find the length is not sufficient.
                // Performance should not be an issue for current MAX_PATH length due to this
                if ((resultValue = DragQueryFileWInternal(hDrop, iFile, valueChars, (uint)capacity)) == capacity)
                {
                    // passing null for buffer will return actual number of characters in the file name.
                    // So, one extra call would be suffice to avoid while loop in case of long path.
                    uint internalCapacity = DragQueryFileWInternal(hDrop, iFile, null, 0);
                    if (internalCapacity < Kernel32.MAX_UNICODESTRING_LEN)
                    {
                        ArrayPool<char>.Shared.Return(buf, true);
                        buf = ArrayPool<char>.Shared.Rent((int)internalCapacity);
                        resultValue = DragQueryFileWInternal(hDrop, iFile, valueChars, internalCapacity);
                    }
                    else
                    {
                        resultValue = 0;
                    }
                }
            }

            // Set lpszFile to the buffer's data.
            lpszFile = buf.AsSpan().Slice(0, (int)resultValue).ToString();
            ArrayPool<char>.Shared.Return(buf, true);
            return resultValue;
        }
    }
}
