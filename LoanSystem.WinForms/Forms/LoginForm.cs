using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Forms
{
    public class LoginForm : Form
    {
        private readonly AppServices _services;

        private readonly TextBox _txtEmail = new() { Width = 280 };
        private readonly TextBox _txtPassword = new() { Width = 280, UseSystemPasswordChar = true };
        private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, AutoSize = true };

        public LoginForm(AppServices services)
        {
            _services = services;

            Text = "Loan & Game System - Login";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 440;
            Height = 320;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var title = new Label
            {
                Text = "Login",
                Font = new Font(Font.FontFamily, 15, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };

            var btnLogin = new Button { Text = "Login", Width = 120 };
            var btnRegister = new Button { Text = "Register", Width = 120 };

            btnLogin.Click += (_, _) => OnLogin();
            btnRegister.Click += (_, _) =>
            {
                using var registerForm = new RegisterForm(_services);
                var dialogResult = registerForm.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    _txtEmail.Text = registerForm.RegisteredEmail;
                    _txtPassword.Text = registerForm.RegisteredPassword;
                    _lblMessage.ForeColor = Color.DarkGreen;
                    _lblMessage.Text = registerForm.SuccessMessage;
                }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                ColumnCount = 2,
                RowCount = 6
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(title, 0, 0);
            layout.SetColumnSpan(title, 2);

            layout.Controls.Add(new Label { Text = "Email", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_txtEmail, 1, 1);

            layout.Controls.Add(new Label { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_txtPassword, 1, 2);

            layout.Controls.Add(_lblMessage, 0, 3);
            layout.SetColumnSpan(_lblMessage, 2);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            buttonPanel.Controls.Add(btnLogin);
            buttonPanel.Controls.Add(btnRegister);
            layout.Controls.Add(buttonPanel, 1, 4);

            Controls.Add(layout);
        }

        private void OnLogin()
        {
            _lblMessage.ForeColor = Color.Firebrick;
            _lblMessage.Text = string.Empty;
            var (user, message) = _services.AuthService.Login(_txtEmail.Text, _txtPassword.Text);
            if (user == null)
            {
                _lblMessage.Text = message;
                return;
            }

            _services.Session.CurrentUser = user;
            Hide();

            if (user.Role == UserRole.Admin)
            {
                using var adminForm = new AdminDashboardForm(_services);
                adminForm.ShowDialog(this);
            }
            else
            {
                using var userForm = new UserDashboardForm(_services);
                userForm.ShowDialog(this);
            }

            _services.Session.CurrentUser = null;
            _txtPassword.Text = string.Empty;
            Show();
        }
    }
}
