namespace LoanSystem.WinForms.Forms
{
    public class RegisterForm : Form
    {
        private readonly AppServices _services;

        private readonly TextBox _txtFullName = new() { Width = 300 };
        private readonly TextBox _txtEmail = new() { Width = 300 };
        private readonly TextBox _txtPassword = new() { Width = 300, UseSystemPasswordChar = true };
        private readonly TextBox _txtConfirmPassword = new() { Width = 300, UseSystemPasswordChar = true };
        private readonly Label _lblMessage = new() { AutoSize = true };

        public string RegisteredEmail { get; private set; } = string.Empty;
        public string RegisteredPassword { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;

        public RegisterForm(AppServices services)
        {
            _services = services;

            Text = "Register New Account";
            StartPosition = FormStartPosition.CenterParent;
            Width = 520;
            Height = 360;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                ColumnCount = 2,
                RowCount = 7
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var title = new Label
            {
                Text = "Register",
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                AutoSize = true
            };

            layout.Controls.Add(title, 0, 0);
            layout.SetColumnSpan(title, 2);

            layout.Controls.Add(new Label { Text = "Full Name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_txtFullName, 1, 1);
            layout.Controls.Add(new Label { Text = "Email", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_txtEmail, 1, 2);
            layout.Controls.Add(new Label { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_txtPassword, 1, 3);
            layout.Controls.Add(new Label { Text = "Confirm Password", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            layout.Controls.Add(_txtConfirmPassword, 1, 4);
            layout.Controls.Add(_lblMessage, 0, 5);
            layout.SetColumnSpan(_lblMessage, 2);

            var btnRegister = new Button { Text = "Register", Width = 120 };
            var btnCancel = new Button { Text = "Close", Width = 120 };
            btnRegister.Click += (_, _) => OnRegister();
            btnCancel.Click += (_, _) => Close();

            var buttonPanel = new FlowLayoutPanel { AutoSize = true };
            buttonPanel.Controls.Add(btnRegister);
            buttonPanel.Controls.Add(btnCancel);
            layout.Controls.Add(buttonPanel, 1, 6);

            Controls.Add(layout);
        }

        private void OnRegister()
        {
            var (success, message) = _services.AuthService.Register(
                _txtFullName.Text,
                _txtEmail.Text,
                _txtPassword.Text,
                _txtConfirmPassword.Text);

            _lblMessage.ForeColor = success ? Color.DarkGreen : Color.Firebrick;
            _lblMessage.Text = message;

            if (success)
            {
                RegisteredEmail = _txtEmail.Text.Trim();
                RegisteredPassword = _txtPassword.Text;
                SuccessMessage = message;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
