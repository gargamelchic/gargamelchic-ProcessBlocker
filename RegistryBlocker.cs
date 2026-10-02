using Microsoft.Win32;

namespace ProcessBlocker
{
    public static class RegistryBlocker
    {
        private const string IFEO = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";

        /// <summary>Запретить запуск программы (подменяем её на несуществующую).</summary>
        public static void BlockProgram(string exeName)
        {
            exeName = exeName.Replace(".exe", "") + ".exe";
            using (var key = Registry.LocalMachine.CreateSubKey($@"{IFEO}\{exeName}"))
            {
                key?.SetValue("Debugger", @"C:\Windows\System32\systray.exe");
            }
        }

        /// <summary>Снять запрет запуска.</summary>
        public static void UnblockProgram(string exeName)
        {
            exeName = exeName.Replace(".exe", "") + ".exe";
            try { Registry.LocalMachine.DeleteSubKeyTree($@"{IFEO}\{exeName}", false); }
            catch { }
        }

        public static bool IsBlocked(string exeName)
        {
            exeName = exeName.Replace(".exe", "") + ".exe";
            using (var key = Registry.LocalMachine.OpenSubKey($@"{IFEO}\{exeName}"))
            {
                return key?.GetValue("Debugger") != null;
            }
        }
    }
}