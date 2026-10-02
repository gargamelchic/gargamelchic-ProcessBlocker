using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ProcessBlocker
{
    public partial class MainForm : Form
    {
        private HashSet<string> blockedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private const string ConfigFile = "blocked.txt";
        private Thread watchThread;
        private volatile bool running = true;

        public MainForm()
        {
            InitializeComponent();
            LoadConfig();
            RefreshProcessList();
            RefreshBlockedList();

            // Авто-обновление списка процессов
            var t = new System.Windows.Forms.Timer { Interval = 3000 };
            t.Tick += (s, e) => { if (tabControl.SelectedTab == tabKill) RefreshProcessList(); };
            t.Start();

            // Поток авто-блокировки
            watchThread = new Thread(WatchLoop) { IsBackground = true };
            watchThread.Start();

            FormClosing += (s, e) => { running = false; SaveConfig(); };
        }

        // ================= Ручное завершение =================
        private void KillByName()
        {
            int n = ProcessManager.KillByName(txtName.Text);
            Log($"Убито по имени '{txtName.Text}': {n}");
        }

        private void KillByPid()
        {
            if (!int.TryParse(txtPid.Text, out int pid)) { Log("Некорректный PID"); return; }
            bool ok = ProcessManager.KillByPid(pid);
            Log(ok ? $"PID {pid} завершён" : $"Не удалось завершить PID {pid}");
        }

        private void KillByPort()
        {
            if (!int.TryParse(txtPort.Text, out int port)) { Log("Некорректный порт"); return; }
            int n = ProcessManager.KillByPort(port);
            Log($"Убито процессов на порту {port}: {n}");
        }

        private void KillSelected()
        {
            foreach (ListViewItem item in lvProcesses.SelectedItems)
            {
                if (int.TryParse(item.Text, out int pid))
                {
                    bool ok = ProcessManager.KillByPid(pid);
                    Log(ok ? $"Убит PID {pid}" : $"Ошибка PID {pid}");
                }
            }
            RefreshProcessList();
        }

        private void RefreshProcessList()
        {
            lvProcesses.BeginUpdate();
            lvProcesses.Items.Clear();
            foreach (var p in ProcessManager.GetAllProcesses())
            {
                var item = new ListViewItem(p.Pid.ToString());
                item.SubItems.Add(p.Name);
                item.SubItems.Add((p.Memory / 1024 / 1024).ToString());
                item.SubItems.Add(p.StartTime?.ToString("HH:mm:ss dd.MM") ?? "—");
                lvProcesses.Items.Add(item);
            }
            lvProcesses.EndUpdate();
        }

        // ================= Авто-блокировка =================
        private void AddBlocked()
        {
            string name = txtAddBlock.Text.Replace(".exe", "").Trim();
            if (string.IsNullOrWhiteSpace(name)) return;
            blockedProcesses.Add(name);
            lbBlocked.Items.Add(name);
            Log($"Добавлено в авто-блок: {name}");
        }

        private void RemoveBlocked()
        {
            if (lbBlocked.SelectedItem == null) return;
            string name = lbBlocked.SelectedItem.ToString();
            blockedProcesses.Remove(name);
            lbBlocked.Items.Remove(name);
            Log($"Удалено из авто-блока: {name}");
        }

        private void WatchLoop()
        {
            while (running)
            {
                if (chkAutoStart.InvokeRequired)
                {
                    chkAutoStart.Invoke(new Action(() => {
                        if (chkAutoStart.Checked) DoAutoBlock();
                    }));
                }
                else if (chkAutoStart.Checked) DoAutoBlock();

                Thread.Sleep(1000);
            }
        }

        private void DoAutoBlock()
        {
            foreach (var name in blockedProcesses.ToList())
            {
                var procs = Process.GetProcessesByName(name);
                foreach (var p in procs)
                {
                    try
                    {
                        int pid = p.Id;
                        p.Kill();
                        Log($"[AUTO] Убит {name} (PID {pid})");
                    }
                    catch { }
                    finally { p.Dispose(); }
                }
            }
        }

        // ================= Запрет запуска =================
        private void BlockExe()
        {
            string exe = txtBlockExe.Text.Trim();
            if (string.IsNullOrWhiteSpace(exe)) return;
            RegistryBlocker.BlockProgram(exe);
            Log($"Запрещён запуск: {exe}");
            RefreshBlockedList();
        }

        private void UnblockExe()
        {
            string exe = txtBlockExe.Text.Trim();
            if (string.IsNullOrWhiteSpace(exe)) return;
            RegistryBlocker.UnblockProgram(exe);
            Log($"Разрешён запуск: {exe}");
            RefreshBlockedList();
        }

        private void RefreshBlockedList()
        {
            lbBlockedExe.Items.Clear();
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"))
                {
                    if (key == null) return;
                    foreach (var sub in key.GetSubKeyNames())
                    {
                        using (var k = key.OpenSubKey(sub))
                        {
                            if (k?.GetValue("Debugger") != null)
                                lbBlockedExe.Items.Add(sub);
                        }
                    }
                }
            }
            catch (Exception ex) { Log("Ошибка чтения реестра: " + ex.Message); }
        }

        // ================= Конфиг и лог =================
        private void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                foreach (var line in File.ReadAllLines(ConfigFile))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    blockedProcesses.Add(line);
                    lbBlocked.Items.Add(line);
                }
            }
        }

        private void SaveConfig()
        {
            try { File.WriteAllLines(ConfigFile, blockedProcesses); } catch { }
        }

        private void Log(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(msg)));
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }
    }
}