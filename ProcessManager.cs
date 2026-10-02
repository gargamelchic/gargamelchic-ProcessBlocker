using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ProcessBlocker
{
    public static class ProcessManager
    {
        // ---- Завершение по имени ----
        public static int KillByName(string name)
        {
            name = name.Replace(".exe", "").Trim();
            int count = 0;
            foreach (var p in Process.GetProcessesByName(name))
            {
                try { p.Kill(); p.WaitForExit(2000); count++; }
                catch { /* игнорируем системные */ }
                finally { p.Dispose(); }
            }
            return count;
        }

        // ---- Завершение по PID ----
        public static bool KillByPid(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit(2000);
                p.Dispose();
                return true;
            }
            catch { return false; }
        }

        // ---- Завершение по порту ----
        public static int KillByPort(int port)
        {
            var pids = GetPidsByPort(port);
            int killed = 0;
            foreach (var pid in pids)
                if (KillByPid(pid)) killed++;
            return killed;
        }

        // ---- Получить PID'ы, слушающие порт ----
        public static List<int> GetPidsByPort(int port)
        {
            var result = new List<int>();
            var props = IPGlobalProperties.GetIPGlobalProperties();
            var listeners = props.GetActiveTcpListeners()
                .Concat(props.GetActiveTcpConnections().Select(c => c.LocalEndPoint));

            // Netstat через WMI надёжнее, но здесь используем P/Invoke
            var table = GetExtendedTcpTable();
            foreach (var row in table)
            {
                if (row.LocalPort == port)
                    result.Add(row.OwningPid);
            }
            return result.Distinct().ToList();
        }

        // ---- Список всех процессов ----
        public static List<ProcessInfo> GetAllProcesses()
        {
            var list = new List<ProcessInfo>();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    list.Add(new ProcessInfo
                    {
                        Pid = p.Id,
                        Name = p.ProcessName,
                        Memory = p.WorkingSet64,
                        StartTime = TryGetStartTime(p)
                    });
                }
                catch { }
                finally { p.Dispose(); }
            }
            return list.OrderBy(x => x.Name).ToList();
        }

        private static DateTime? TryGetStartTime(Process p)
        {
            try { return p.StartTime; } catch { return null; }
        }

        // ================= P/Invoke для GetExtendedTcpTable =================
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen,
            bool sort, int ipVersion, int tblClass, int reserved);

        private struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public uint localPort;
            public uint remoteAddr;
            public uint remotePort;
            public uint owningPid;
        }

        private class TcpRow
        {
            public int LocalPort;
            public int OwningPid;
        }

        private static List<TcpRow> GetExtendedTcpTable()
        {
            var result = new List<TcpRow>();
            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, 2, 5, 0); // AF_INET=2, TCP_TABLE_OWNER_PID_ALL=5

            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                uint ret = GetExtendedTcpTable(buffer, ref bufferSize, true, 2, 5, 0);
                if (ret != 0) return result;

                int rowCount = Marshal.ReadInt32(buffer);
                IntPtr rowPtr = (IntPtr)((long)buffer + 4);
                int rowSize = Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));

                for (int i = 0; i < rowCount; i++)
                {
                    var row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(
                        (IntPtr)((long)rowPtr + i * rowSize), typeof(MIB_TCPROW_OWNER_PID));

                    result.Add(new TcpRow
                    {
                        LocalPort = (int)(((row.localPort & 0xFF) << 8) | ((row.localPort >> 8) & 0xFF)),
                        OwningPid = (int)row.owningPid
                    });
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
            return result;
        }
    }

    public class ProcessInfo
    {
        public int Pid { get; set; }
        public string Name { get; set; }
        public long Memory { get; set; }
        public DateTime? StartTime { get; set; }
    }
}