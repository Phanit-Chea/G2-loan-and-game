using LoanSystem.WinForms.Domain.Games;

namespace LoanSystem.WinForms.Forms
{
    public class CarRacingForm : Form
    {
        private sealed class GameCanvas : Panel
        {
            public GameCanvas()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        private readonly AppServices _services;
        private readonly GameCanvas _canvas = new() { Width = 420, Height = 620, BackColor = Color.FromArgb(50, 50, 50) };
        private readonly Label _lblScore = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold) };
        private readonly Label _lblHint = new() { AutoSize = true, ForeColor = Color.DimGray };
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30 };
        private readonly CarRacingGame _game = new();
        private readonly Button _btnStart = new() { Text = "Start", Width = 120 };

        public CarRacingForm(AppServices services)
        {
            _services = services;

            Text = "Car Racing";
            Width = 520;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            KeyPreview = true;

            var lblTitle = new Label { Text = "Car Racing", AutoSize = true, Font = new Font(Font.FontFamily, 14, FontStyle.Bold) };
            var lblHelp = new Label { Text = "Use Left/Right arrow keys. Avoid red blocks and collect coins.", AutoSize = true };

            var btnBack = new Button { Text = "Back", Width = 120 };
            _btnStart.Click += (_, _) => StartGame();
            btnBack.Click += (_, _) => Close();

            var buttonPanel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            buttonPanel.Controls.Add(_btnStart);
            buttonPanel.Controls.Add(btnBack);

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblHelp);
            topPanel.Controls.Add(_lblScore);
            topPanel.Controls.Add(_lblHint);
            topPanel.Controls.Add(buttonPanel);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _canvas.Dock = DockStyle.Fill;
            root.Controls.Add(topPanel, 0, 0);
            root.Controls.Add(_canvas, 0, 1);

            Controls.Add(root);

            _canvas.Paint += CanvasOnPaint;
            _timer.Tick += TimerOnTick;
            KeyDown += CarRacingForm_KeyDown;

            _game.Initialize(_canvas.ClientSize);
            RefreshScore();
            _lblHint.Text = "Click Start or press Space to begin.";
        }

        private void StartGame()
        {
            if (_game.IsRunning)
            {
                return;
            }

            _game.Start(_canvas.ClientSize);
            RefreshScore();
            _lblHint.Text = "Game running...";
            _btnStart.Enabled = false;
            _timer.Start();
            Focus();
            _canvas.Focus();
        }

        private void CarRacingForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !_game.IsRunning)
            {
                StartGame();
                e.Handled = true;
                return;
            }

            if (!_game.IsRunning)
            {
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                _game.MoveLeft(22, _canvas.Width);
            }
            else if (e.KeyCode == Keys.Right)
            {
                _game.MoveRight(22, _canvas.Width);
            }
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (!_game.IsRunning)
            {
                return;
            }

            var gameOver = _game.Tick(_canvas.ClientSize);
            RefreshScore();
            _canvas.Invalidate();
            if (gameOver)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            _timer.Stop();
            _btnStart.Enabled = true;
            _lblHint.Text = "Game over. Click Start or press Space to play again.";

            var userId = _services.Session.CurrentUser?.Id;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                _services.GameService.SaveScore(userId, _game);
            }

            MessageBox.Show($"Game Over.\nFinal Score: {_game.Score}", "Car Racing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CanvasOnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(45, 45, 45));

            using var lanePen = new Pen(Color.White, 3) { DashPattern = new[] { 18f, 12f } };
            g.DrawLine(lanePen, _canvas.Width / 2f, 0, _canvas.Width / 2f, _canvas.Height);

            DrawPlayerCar(g, _game.Player);
            foreach (var obstacle in _game.Obstacles)
            {
                DrawEnemyCar(g, obstacle);
            }

            using var coinBrush = new SolidBrush(Color.Gold);
            foreach (var coin in _game.Coins)
            {
                g.FillEllipse(coinBrush, coin);
            }
        }

        private void RefreshScore()
        {
            _lblScore.Text = $"Score: {_game.Score}";
        }

        private static void DrawPlayerCar(Graphics g, RectangleF rect)
        {
            var body = RectangleF.Inflate(rect, -2, -2);
            using var bodyBrush = new SolidBrush(Color.Red);
            using var outlinePen = new Pen(Color.DarkRed, 2);
            g.FillRoundedRectangle(bodyBrush, body, 8);
            g.DrawRoundedRectangle(outlinePen, body, 8);

            var windshield = new RectangleF(body.X + 7, body.Y + 9, body.Width - 14, 16);
            using var glassBrush = new SolidBrush(Color.FromArgb(190, 225, 240, 255));
            g.FillRoundedRectangle(glassBrush, windshield, 5);

            DrawWheels(g, body, Color.Black);
        }

        private static void DrawEnemyCar(Graphics g, RectangleF rect)
        {
            var body = RectangleF.Inflate(rect, -2, -2);
            using var bodyBrush = new SolidBrush(Color.FromArgb(45, 45, 45));
            using var outlinePen = new Pen(Color.FromArgb(160, 160, 160), 2);
            g.FillRoundedRectangle(bodyBrush, body, 8);
            g.DrawRoundedRectangle(outlinePen, body, 8);

            var windshield = new RectangleF(body.X + 7, body.Y + 9, body.Width - 14, 16);
            using var glassBrush = new SolidBrush(Color.FromArgb(140, 180, 180, 180));
            g.FillRoundedRectangle(glassBrush, windshield, 5);

            using var accentBrush = new SolidBrush(Color.IndianRed);
            g.FillRectangle(accentBrush, body.X + 5, body.Bottom - 10, body.Width - 10, 4);

            DrawWheels(g, body, Color.Black);
        }

        private static void DrawWheels(Graphics g, RectangleF body, Color color)
        {
            using var wheelBrush = new SolidBrush(color);
            var wheelWidth = 5f;
            var wheelHeight = 12f;

            g.FillRectangle(wheelBrush, body.Left - 1, body.Y + 10, wheelWidth, wheelHeight);
            g.FillRectangle(wheelBrush, body.Right - 4, body.Y + 10, wheelWidth, wheelHeight);
            g.FillRectangle(wheelBrush, body.Left - 1, body.Bottom - 22, wheelWidth, wheelHeight);
            g.FillRectangle(wheelBrush, body.Right - 4, body.Bottom - 22, wheelWidth, wheelHeight);
        }
    }

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var diameter = radius * 2;
            var arc = new RectangleF(rect.Location, new SizeF(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var diameter = radius * 2;
            var arc = new RectangleF(rect.Location, new SizeF(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}
