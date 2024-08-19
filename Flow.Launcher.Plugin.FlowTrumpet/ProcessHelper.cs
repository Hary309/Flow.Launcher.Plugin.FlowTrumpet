using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Flow.Launcher.Plugin.FlowTrumpet
{
    // source code from: https://github.com/Flow-Launcher/Flow.Launcher/blob/dev/Plugins/Flow.Launcher.Plugin.ProcessKiller/ProcessHelper.cs
    internal static class ProcessHelper
    {
        public static string TryGetProcessIconFilename(Process p)
        {
            try
            {
                int capacity = 2000;
                StringBuilder builder = new StringBuilder(capacity);
                IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
                if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
                {
                    return string.Empty;
                }

                return builder.ToString();
            }
            catch
            {
                return "";
            }
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            [In] IntPtr hProcess,
            [In] int dwFlags,
            [Out] StringBuilder lpExeName,
            ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);
    }
}
