namespace ProcessBlocker
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabKill;
        private System.Windows.Forms.TabPage tabAuto;
        private System.Windows.Forms.TabPage tabBlock;
        private System.Windows.Forms.TabPage tabLog;

        // Вкладка "Ручное завершение"
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnKillName;
        private System.Windows.Forms.TextBox txtPid;
        private System.Windows.Forms.Button btnKillPid;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnKillPort;
        private System.Windows.Forms.ListView lvProcesses;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnKillSelected;

        // Вкладка "Авто-блокировка"
        private System.Windows.Forms.ListBox lbBlocked;
        private System.Windows.Forms.TextBox txtAddBlock;
        private System.Windows.Forms.Button btnAddBlock;
        private System.Windows.Forms.Button btnRemoveBlock;
        private System.Windows.Forms.CheckBox chkAutoStart;

        // Вкладка "Запрет запуска"
        private System.Windows.Forms.TextBox txtBlockExe;
        private System.Windows.Forms.Button btnBlockExe;
        private System.Windows.Forms.Button btnUnblockExe;
        private System.Windows.Forms.ListBox lbBlockedExe;
        private System.Windows.Forms.Button btnRefreshBlocked;

        // Вкладка "Лог"
        private System.Windows.Forms.TextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabKill = new System.Windows.Forms.TabPage();
            this.tabAuto = new System.Windows.Forms.TabPage();
            this.tabBlock = new System.Windows.Forms.TabPage();
            this.tabLog = new System.Windows.Forms.TabPage();

            // ===== Общие размеры =====
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Controls.Add(this.tabKill);
            this.tabControl.Controls.Add(this.tabAuto);
            this.tabControl.Controls.Add(this.tabBlock);
            this.tabControl.Controls.Add(this.tabLog);

            // ===== Вкладка "Ручное завершение" =====
            this.tabKill.Text = "Ручное завершение";

            this.txtName = new System.Windows.Forms.TextBox { Left = 10, Top = 15, Width = 180, Text = "notepad" };
            this.btnKillName = new System.Windows.Forms.Button { Left = 200, Top = 13, Width = 130, Text = "Убить по имени" };
            this.btnKillName.Click += (s, e) => KillByName();

            this.txtPid = new System.Windows.Forms.TextBox { Left = 10, Top = 50, Width = 180, Text = "1234" };
            this.btnKillPid = new System.Windows.Forms.Button { Left = 200, Top = 48, Width = 130, Text = "Убить по PID" };
            this.btnKillPid.Click += (s, e) => KillByPid();

            this.txtPort = new System.Windows.Forms.TextBox { Left = 10, Top = 85, Width = 180, Text = "8080" };
            this.btnKillPort = new System.Windows.Forms.Button { Left = 200, Top = 83, Width = 130, Text = "Убить по порту" };
            this.btnKillPort.Click += (s, e) => KillByPort();

            this.lvProcesses = new System.Windows.Forms.ListView
            {
                Left = 350,
                Top = 15,
                Width = 520,
                Height = 400,
                View = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                MultiSelect = true
            };
            this.lvProcesses.Columns.Add("PID", 70);
            this.lvProcesses.Columns.Add("Имя", 200);
            this.lvProcesses.Columns.Add("Память (MB)", 100);
            this.lvProcesses.Columns.Add("Запущен", 140);

            this.btnRefresh = new System.Windows.Forms.Button { Left = 350, Top = 425, Width = 120, Text = "Обновить" };
            this.btnRefresh.Click += (s, e) => RefreshProcessList();

            this.btnKillSelected = new System.Windows.Forms.Button { Left = 480, Top = 425, Width = 180, Text = "Убить выбранные" };
            this.btnKillSelected.Click += (s, e) => KillSelected();

            this.tabKill.Controls.AddRange(new System.Windows.Forms.Control[] {
                txtName, btnKillName, txtPid, btnKillPid, txtPort, btnKillPort,
                lvProcesses, btnRefresh, btnKillSelected
            });

            // ===== Вкладка "Авто-блокировка" =====
            this.tabAuto.Text = "Авто-блокировка";

            this.lbBlocked = new System.Windows.Forms.ListBox { Left = 10, Top = 40, Width = 250, Height = 300 };
            this.txtAddBlock = new System.Windows.Forms.TextBox { Left = 10, Top = 10, Width = 180, Text = "notepad" };
            this.btnAddBlock = new System.Windows.Forms.Button { Left = 200, Top = 8, Width = 60, Text = "Добавить" };
            this.btnAddBlock.Click += (s, e) => AddBlocked();

            this.btnRemoveBlock = new System.Windows.Forms.Button { Left = 10, Top = 350, Width = 120, Text = "Удалить" };
            this.btnRemoveBlock.Click += (s, e) => RemoveBlocked();

            this.chkAutoStart = new System.Windows.Forms.CheckBox
            {
                Left = 150,
                Top = 352,
                Width = 200,
                Text = "Авто-блокировка вкл",
                Checked = true
            };

            this.tabAuto.Controls.AddRange(new System.Windows.Forms.Control[] {
                lbBlocked, txtAddBlock, btnAddBlock, btnRemoveBlock, chkAutoStart
            });

            // ===== Вкладка "Запрет запуска" =====
            this.tabBlock.Text = "Запрет запуска (реестр)";

            this.txtBlockExe = new System.Windows.Forms.TextBox { Left = 10, Top = 15, Width = 200, Text = "notepad.exe" };
            this.btnBlockExe = new System.Windows.Forms.Button { Left = 220, Top = 13, Width = 130, Text = "Запретить запуск" };
            this.btnBlockExe.Click += (s, e) => BlockExe();

            this.btnUnblockExe = new System.Windows.Forms.Button { Left = 360, Top = 13, Width = 130, Text = "Разрешить запуск" };
            this.btnUnblockExe.Click += (s, e) => UnblockExe();

            this.lbBlockedExe = new System.Windows.Forms.ListBox { Left = 10, Top = 60, Width = 300, Height = 300 };
            this.btnRefreshBlocked = new System.Windows.Forms.Button { Left = 320, Top = 58, Width = 150, Text = "Обновить список" };
            this.btnRefreshBlocked.Click += (s, e) => RefreshBlockedList();

            this.tabBlock.Controls.AddRange(new System.Windows.Forms.Control[] {
                txtBlockExe, btnBlockExe, btnUnblockExe, lbBlockedExe, btnRefreshBlocked
            });

            // ===== Вкладка "Лог" =====
            this.tabLog.Text = "Лог";
            this.txtLog = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                Dock = System.Windows.Forms.DockStyle.Fill,
                ReadOnly = true
            };
            this.tabLog.Controls.Add(this.txtLog);

            // ===== Форма =====
            this.Text = "Process Blocker — Администратор";
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this.tabControl);
        }
    }
}