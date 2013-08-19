using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AntsCode.TableStorageBackupLib
{
    public static class StreamUtil
    {
        public static void CopyStream(Stream input, Stream output)
        {
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return;
                }
                output.Write(buffer, 0, read);
            }
        }
    }
}
