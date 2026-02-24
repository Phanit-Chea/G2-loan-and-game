using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Forms
{
    public class UserDashboardForm : Form
    {
        private readonly AppServices _services;
        private readonly Label _lblWelcome = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, 13, FontStyle.Bold) };
        private readonly DataGridView _gridGames = new();
        private readonly DataGridView _gridLoans = new();

        public UserDashboardForm(AppServices services)
        {
            _services = services;

            Text = "User Dashboard";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1050;
            Height = 700;

            _gridGames.Dock = DockStyle.Fill;
            _gridGames.ReadOnly = true;
            _gridGames.AutoGenerateColumns = false;
            _gridGames.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Game", DataPropertyName = nameof(GameHistory.GameName), Width = 170 });
            _gridGames.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Result", DataPropertyName = nameof(GameHistory.Result), Width = 190 });
            _gridGames.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Score", DataPropertyName = nameof(GameHistory.Score), Width = 90 });
            _gridGames.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = nameof(GameHistory.PlayedAt), Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });

            _gridLoans.Dock = DockStyle.Fill;
            _gridLoans.ReadOnly = true;
            _gridLoans.AutoGenerateColumns = false;
            _gridLoans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = nameof(LoanHistory.CreatedAt), Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridLoans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Amount", DataPropertyName = nameof(LoanHistory.Amount), Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _gridLoans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rate", DataPropertyName = nameof(LoanHistory.InterestRate), Width = 90 });
            _gridLoans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Term (Y/M)", Width = 140 });
            _gridLoans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Payment", DataPropertyName = nameof(LoanHistory.TotalPayment), Width = 160, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            _gridLoans.CellFormatting += GridLoansOnCellFormatting;

            var btnLoan = new Button { Text = "Loan Management", Width = 160 };
            var btnTicTacToe = new Button { Text = "Tic-Tac-Toe", Width = 130 };
            var btnCarRacing = new Button { Text = "Car Racing", Width = 130 };
            var btnRefresh = new Button { Text = "Refresh", Width = 100 };
            var btnLogout = new Button { Text = "Logout", Width = 100 };

            btnLoan.Click += (_, _) =>
            {
                using var form = new LoanForm(_services);
                form.ShowDialog(this);
                LoadData();
            };
            btnTicTacToe.Click += (_, _) =>
            {
                using var form = new TicTacToeForm(_services);
                form.ShowDialog(this);
                LoadData();
            };
            btnCarRacing.Click += (_, _) =>
            {
                using var form = new CarRacingForm(_services);
                form.ShowDialog(this);
                LoadData();
            };
            btnRefresh.Click += (_, _) => LoadData();
            btnLogout.Click += (_, _) => Close();

            var actionPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            actionPanel.Controls.Add(btnLoan);
            actionPanel.Controls.Add(btnTicTacToe);
            actionPanel.Controls.Add(btnCarRacing);
            actionPanel.Controls.Add(btnRefresh);
            actionPanel.Controls.Add(btnLogout);

            var gamesGroup = new GroupBox { Text = "Your Game History", Dock = DockStyle.Fill };
            gamesGroup.Controls.Add(_gridGames);

            var loansGroup = new GroupBox { Text = "Your Loan History", Dock = DockStyle.Fill };
            loansGroup.Controls.Add(_gridLoans);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 260
            };
            split.Panel1.Controls.Add(gamesGroup);
            split.Panel2.Controls.Add(loansGroup);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 3
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(_lblWelcome, 0, 0);
            root.Controls.Add(actionPanel, 0, 1);
            root.Controls.Add(split, 0, 2);

            Controls.Add(root);
            LoadData();
        }

        private void LoadData()
        {
            var currentUser = _services.Session.CurrentUser;
            if (currentUser == null)
            {
                Close();
                return;
            }

            _lblWelcome.Text = $"Welcome, {currentUser.FullName}";

            var gameHistory = _services.UserDashboardService.GetGameHistory(currentUser.Id);
            var loanHistory = _services.UserDashboardService.GetLoanHistory(currentUser.Id);
            _gridGames.DataSource = gameHistory;
            _gridLoans.DataSource = loanHistory;
        }

        private static void GridLoansOnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid)
            {
                return;
            }

            if (grid.Columns[e.ColumnIndex].HeaderText != "Term (Y/M)")
            {
                return;
            }

            if (grid.Rows[e.RowIndex].DataBoundItem is LoanHistory history)
            {
                e.Value = $"{history.TermYears} Y / {history.TermMonths} M";
                e.FormattingApplied = true;
            }
        }
    }
}
