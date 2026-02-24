using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Forms
{
    public class LoanForm : Form
    {
        private readonly AppServices _services;

        private readonly NumericUpDown _numAmount = new() { Minimum = 1, Maximum = 100000000, DecimalPlaces = 2, Increment = 1000, Value = 10000, Width = 170 };
        private readonly NumericUpDown _numRate = new() { Minimum = 0, Maximum = 100, DecimalPlaces = 2, Increment = 0.25M, Value = 6, Width = 170 };
        private readonly NumericUpDown _numTermYears = new() { Minimum = 0, Maximum = 50, Value = 5, Width = 170 };
        private readonly NumericUpDown _numTermMonths = new() { Minimum = 0, Maximum = 11, Value = 0, Width = 170 };
        private readonly DateTimePicker _dtStart = new() { Width = 170 };

        private readonly CheckBox _chkExtras = new() { Text = "Include extra payments", AutoSize = true };
        private readonly NumericUpDown _numExtraMonthly = new() { Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Width = 170 };
        private readonly NumericUpDown _numExtraYearly = new() { Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Width = 170 };
        private readonly NumericUpDown _numExtraOneTime = new() { Minimum = 0, Maximum = 1000000, DecimalPlaces = 2, Width = 170 };
        private readonly DateTimePicker _dtExtraOneTime = new() { Width = 170, ShowCheckBox = true };

        private readonly Label _lblSummary = new() { AutoSize = true, ForeColor = Color.DarkGreen };
        private readonly DataGridView _gridYearly = new();
        private readonly DataGridView _gridMonthly = new();

        private List<MonthlyAmortizationSchedule> _lastMonthlySchedule = new();

        public LoanForm(AppServices services)
        {
            _services = services;

            Text = "Loan Management";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 760;
            MinimumSize = new Size(1100, 700);

            _dtStart.Value = DateTime.Today;
            _dtExtraOneTime.Value = DateTime.Today;

            var left = BuildInputPanel();
            var right = BuildResultsPanel();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 420,
                FixedPanel = FixedPanel.Panel1,
                Panel1MinSize = 360
            };
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(right);

            Controls.Add(split);
        }

        private Control BuildInputPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var title = new Label
            {
                Text = "Amortization Calculator",
                Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
                AutoSize = true
            };
            layout.Controls.Add(title, 0, 0);
            layout.SetColumnSpan(title, 2);

            AddField(layout, 1, "Loan Amount", _numAmount);
            AddField(layout, 2, "Interest Rate (%)", _numRate);
            AddField(layout, 3, "Term Years", _numTermYears);
            AddField(layout, 4, "Term Months", _numTermMonths);
            AddField(layout, 5, "Start Date", _dtStart);

            layout.Controls.Add(_chkExtras, 0, 6);
            layout.SetColumnSpan(_chkExtras, 2);

            AddField(layout, 7, "Extra Monthly", _numExtraMonthly);
            AddField(layout, 8, "Extra Yearly", _numExtraYearly);
            AddField(layout, 9, "Extra One-Time", _numExtraOneTime);
            AddField(layout, 10, "One-Time Date", _dtExtraOneTime);

            ToggleExtraControls(false);
            _chkExtras.CheckedChanged += (_, _) => ToggleExtraControls(_chkExtras.Checked);

            var btnCalculate = new Button { Text = "Calculate", Width = 130 };
            var btnExport = new Button { Text = "Export CSV", Width = 130 };
            var btnClose = new Button { Text = "Back", Width = 130 };

            btnCalculate.Click += (_, _) => CalculateLoan();
            btnExport.Click += (_, _) => ExportCsv();
            btnClose.Click += (_, _) => Close();

            var buttonPanel = new FlowLayoutPanel { AutoSize = true };
            buttonPanel.Controls.Add(btnCalculate);
            buttonPanel.Controls.Add(btnExport);
            buttonPanel.Controls.Add(btnClose);
            layout.Controls.Add(buttonPanel, 0, 11);
            layout.SetColumnSpan(buttonPanel, 2);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildResultsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 220
            };

            _gridYearly.Dock = DockStyle.Fill;
            _gridYearly.ReadOnly = true;
            _gridYearly.AutoGenerateColumns = true;

            _gridMonthly.Dock = DockStyle.Fill;
            _gridMonthly.ReadOnly = true;
            _gridMonthly.AutoGenerateColumns = true;

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            top.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            top.Controls.Add(_lblSummary, 0, 0);

            var yearlyBox = new GroupBox { Text = "Yearly Schedule", Dock = DockStyle.Fill };
            yearlyBox.Controls.Add(_gridYearly);
            top.Controls.Add(yearlyBox, 0, 1);

            var monthlyBox = new GroupBox { Text = "Monthly Schedule", Dock = DockStyle.Fill };
            monthlyBox.Controls.Add(_gridMonthly);

            split.Panel1.Controls.Add(top);
            split.Panel2.Controls.Add(monthlyBox);

            panel.Controls.Add(split);
            return panel;
        }

        private void CalculateLoan()
        {
            if (_numTermYears.Value == 0 && _numTermMonths.Value == 0)
            {
                MessageBox.Show("Term should be greater than 0 month.", "Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var loan = new Loan
            {
                LoanAmount = _numAmount.Value,
                InterestRate = (double)_numRate.Value,
                TermYears = (int)_numTermYears.Value,
                TermMonths = (int)_numTermMonths.Value,
                StartDate = _dtStart.Value.Date,
                ExtraMonthlyPayment = _chkExtras.Checked ? _numExtraMonthly.Value : 0,
                ExtraYearlyPayment = _chkExtras.Checked ? _numExtraYearly.Value : 0,
                ExtraOneTimePayment = _chkExtras.Checked ? _numExtraOneTime.Value : 0,
                ExtraOneTimePaymentDate = _chkExtras.Checked && _dtExtraOneTime.Checked ? _dtExtraOneTime.Value.Date : null
            };

            var result = _services.LoanService.Calculate(loan);
            _lastMonthlySchedule = result.MonthlySchedule;

            _lblSummary.Text =
                $"Monthly Payment: {result.MonthlyPayment:C2} | Total Paid: {result.TotalPayment:C2} | Total Interest: {result.TotalInterest:C2} | Payoff Date: {result.PayoffDate:MMM yyyy}";

            _gridYearly.DataSource = result.YearlySchedule;
            _gridMonthly.DataSource = result.MonthlySchedule;

            var user = _services.Session.CurrentUser;
            if (user != null && result.MonthlySchedule.Any())
            {
                _services.LoanService.SaveHistory(user.Id, loan, result.TotalPayment);
            }
        }

        private void ExportCsv()
        {
            if (_lastMonthlySchedule.Count == 0)
            {
                MessageBox.Show("Calculate first before exporting.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "AmortizationSchedule.csv"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var csv = _services.LoanService.BuildCsv(_lastMonthlySchedule);
            File.WriteAllText(dialog.FileName, csv);
            MessageBox.Show("CSV exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToggleExtraControls(bool show)
        {
            _numExtraMonthly.Enabled = show;
            _numExtraYearly.Enabled = show;
            _numExtraOneTime.Enabled = show;
            _dtExtraOneTime.Enabled = show;
        }

        private static void AddField(TableLayoutPanel layout, int row, string label, Control control)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            layout.Controls.Add(control, 1, row);
        }
    }
}
