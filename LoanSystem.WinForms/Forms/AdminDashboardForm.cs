using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Forms
{
    public class AdminDashboardForm : Form
    {
        private const string ActionColumnName = "ActionColumn";
        private const string ApplyColumnName = "ApplyColumn";

        private readonly AppServices _services;
        private readonly DataGridView _gridUsers = new();
        private readonly Label _lblInfo = new() { AutoSize = true, ForeColor = Color.DimGray };

        public AdminDashboardForm(AppServices services)
        {
            _services = services;

            Text = "Admin Dashboard";
            StartPosition = FormStartPosition.CenterParent;
            Width = 950;
            Height = 620;

            var header = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                AutoSize = true
            };

            _gridUsers.Dock = DockStyle.Fill;
            _gridUsers.AutoGenerateColumns = false;
            _gridUsers.ReadOnly = false;
            _gridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridUsers.MultiSelect = false;
            _gridUsers.AllowUserToAddRows = false;
            _gridUsers.AllowUserToDeleteRows = false;

            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = nameof(AppUser.Email), Width = 220, ReadOnly = true });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = nameof(AppUser.FullName), Width = 210, ReadOnly = true });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", DataPropertyName = nameof(AppUser.Role), Width = 90, ReadOnly = true });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(AppUser.Status), Width = 90, ReadOnly = true });
            _gridUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(AppUser.CreatedAt), Width = 160, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }, ReadOnly = true });

            var actionColumn = new DataGridViewComboBoxColumn
            {
                Name = ActionColumnName,
                HeaderText = "Action",
                Width = 110,
                FlatStyle = FlatStyle.Flat
            };
            actionColumn.Items.AddRange("Approve", "Reject");
            _gridUsers.Columns.Add(actionColumn);

            var applyColumn = new DataGridViewButtonColumn
            {
                Name = ApplyColumnName,
                HeaderText = string.Empty,
                Text = "Apply",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            _gridUsers.Columns.Add(applyColumn);

            _gridUsers.CellContentClick += GridUsersOnCellContentClick;
            _gridUsers.CurrentCellDirtyStateChanged += GridUsersOnCurrentCellDirtyStateChanged;
            _gridUsers.DataBindingComplete += GridUsersOnDataBindingComplete;
            _gridUsers.DataError += (_, _) => { };

            var btnRefresh = new Button { Text = "Refresh", Width = 100 };
            var btnLogout = new Button { Text = "Logout", Width = 100 };

            btnRefresh.Click += (_, _) => LoadUsers();
            btnLogout.Click += (_, _) => Close();

            var modules = new ListBox
            {
                Dock = DockStyle.Fill
            };
            modules.Items.AddRange(new object[] { "Tic-Tac-Toe", "Car Racing", "Loan Management System" });

            var rightPanel = new GroupBox { Text = "Games & Systems", Dock = DockStyle.Fill };
            rightPanel.Controls.Add(modules);

            var leftPanel = new Panel { Dock = DockStyle.Fill };
            leftPanel.Controls.Add(_gridUsers);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 680
            };
            split.Panel1.Controls.Add(leftPanel);
            split.Panel2.Controls.Add(rightPanel);

            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnLogout);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(_lblInfo, 0, 1);
            root.Controls.Add(topPanel, 0, 2);
            root.Controls.Add(split, 0, 3);

            Controls.Add(root);
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = _services.AdminService.GetUsers()
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            _gridUsers.DataSource = users;
            _lblInfo.Text = $"Total users: {users.Count}";
        }

        private void GridUsersOnCurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_gridUsers.IsCurrentCellDirty)
            {
                _gridUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void GridUsersOnDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in _gridUsers.Rows)
            {
                if (row.DataBoundItem is not AppUser user)
                {
                    continue;
                }

                var isLocked = user.IsAdmin;
                var actionCell = new DataGridViewComboBoxCell
                {
                    FlatStyle = FlatStyle.Flat,
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
                };
                var actions = GetActionsForStatus(user.Status);
                foreach (var action in actions)
                {
                    actionCell.Items.Add(action);
                }

                if (actions.Count > 0)
                {
                    actionCell.Value = actions[0];
                }

                row.Cells[ActionColumnName] = actionCell;
                row.Cells[ActionColumnName].ReadOnly = isLocked;
                row.Cells[ApplyColumnName].ReadOnly = isLocked;
                row.Cells[ActionColumnName].Style.BackColor = isLocked ? Color.Gainsboro : Color.White;

                if (row.Cells[ApplyColumnName] is DataGridViewButtonCell buttonCell)
                {
                    buttonCell.Value = isLocked ? "-" : "Apply";
                    buttonCell.Style.ForeColor = isLocked ? Color.DimGray : SystemColors.ControlText;
                }
            }
        }

        private void GridUsersOnCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (_gridUsers.Columns[e.ColumnIndex].Name != ApplyColumnName)
            {
                return;
            }

            if (_gridUsers.Rows[e.RowIndex].DataBoundItem is not AppUser selected)
            {
                return;
            }

            if (selected.IsAdmin)
            {
                MessageBox.Show("Admin account status cannot be changed from this screen.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var action = _gridUsers.Rows[e.RowIndex].Cells[ActionColumnName].Value?.ToString();
            if (string.IsNullOrWhiteSpace(action))
            {
                MessageBox.Show("Please choose an action in the row first.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (action.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            {
                _services.AdminService.ApproveUser(selected.Id);
            }
            else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                _services.AdminService.RejectUser(selected.Id);
            }
            else if (action.Equals("Terminate", StringComparison.OrdinalIgnoreCase))
            {
                _services.AdminService.TerminateUser(selected.Id);
            }
            else if (action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                var confirm = MessageBox.Show(
                    "Set this user status to Deleted?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                _services.AdminService.DeleteUser(selected.Id);
            }

            LoadUsers();
        }

        private static List<string> GetActionsForStatus(AccountStatus status)
        {
            return status switch
            {
                AccountStatus.Pending => new List<string> { "Approve", "Reject" },
                AccountStatus.Active => new List<string> { "Terminate", "Delete" },
                AccountStatus.Terminated => new List<string> { "Approve" },
                AccountStatus.Rejected => new List<string> { "Approve", "Delete" },
                AccountStatus.Deleted => new List<string> { "Approve" },
                _ => new List<string> { "Approve" }
            };
        }
    }
}
