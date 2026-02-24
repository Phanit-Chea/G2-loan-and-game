using LoanSystem.WinForms.Domain.Games;

namespace LoanSystem.WinForms.Forms
{
    public class TicTacToeForm : Form
    {
        private readonly AppServices _services;
        private readonly Label _lblStatus = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, 11, FontStyle.Bold) };
        private readonly Label _lblModeHint = new() { AutoSize = true, ForeColor = Color.DimGray };
        private readonly Button[] _cells = new Button[9];
        private readonly TicTacToeGame _game = new();
        private readonly RadioButton _rbBot = new() { Text = "Play with bot", AutoSize = true, Checked = true };
        private readonly RadioButton _rbFriend = new() { Text = "Play with friend", AutoSize = true };

        public TicTacToeForm(AppServices services)
        {
            _services = services;
            Text = "Tic-Tac-Toe";
            Width = 560;
            Height = 740;
            MinimumSize = new Size(560, 740);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var info = new Label
            {
                Text = "Win: 10 points | Draw: 5 points | Loss: 0 points",
                AutoSize = true
            };

            var boardPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Width = 330,
                Height = 330,
                ColumnCount = 3,
                RowCount = 3,
                Margin = new Padding(0, 12, 0, 12)
            };
            for (var i = 0; i < 3; i++)
            {
                boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
                boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
            }

            for (var i = 0; i < 9; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Dock = DockStyle.Fill,
                    Font = new Font(Font.FontFamily, 22, FontStyle.Bold),
                    Tag = idx
                };
                btn.Click += (_, _) => CellClicked(idx);
                _cells[i] = btn;
                boardPanel.Controls.Add(btn, i % 3, i / 3);
            }

            var btnRestart = new Button { Text = "Restart", Width = 110 };
            var btnBack = new Button { Text = "Back", Width = 110 };
            btnRestart.Click += (_, _) => RestartGame();
            btnBack.Click += (_, _) => Close();

            _rbBot.CheckedChanged += (_, _) =>
            {
                if (_rbBot.Checked)
                {
                    _game.SetMode(TicTacToeMode.VsBot);
                    RestartGame();
                }
            };
            _rbFriend.CheckedChanged += (_, _) =>
            {
                if (_rbFriend.Checked)
                {
                    _game.SetMode(TicTacToeMode.VsFriend);
                    RestartGame();
                }
            };

            var modeGroup = new GroupBox
            {
                Text = "Select Play Mode",
                Width = 330,
                Height = 82
            };
            var modePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            modePanel.Controls.Add(_rbBot);
            modePanel.Controls.Add(_rbFriend);
            modeGroup.Controls.Add(modePanel);

            var buttons = new FlowLayoutPanel { AutoSize = true };
            buttons.Controls.Add(btnRestart);
            buttons.Controls.Add(btnBack);

            var root = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            root.Controls.Add(new Label { Text = "Tic-Tac-Toe", Font = new Font(Font.FontFamily, 14, FontStyle.Bold), AutoSize = true });
            root.Controls.Add(info);
            root.Controls.Add(modeGroup);
            root.Controls.Add(_lblModeHint);
            root.Controls.Add(_lblStatus);
            root.Controls.Add(boardPanel);
            root.Controls.Add(buttons);

            Controls.Add(root);
            _game.SetMode(TicTacToeMode.VsBot);
            RestartGame();
        }

        private void RestartGame()
        {
            _game.Reset();
            SyncBoardFromGame();
        }

        private void CellClicked(int index)
        {
            if (!_game.TryPlay(index))
            {
                return;
            }

            SyncBoardFromGame();
            if (!_game.IsRunning)
            {
                EndGame();
                return;
            }

            if (_game.IsComputerTurn)
            {
                var timer = new System.Windows.Forms.Timer { Interval = 350 };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    ComputerMove();
                };
                timer.Start();
            }
        }

        private void ComputerMove()
        {
            if (_game.PlayComputerMove() == null)
            {
                return;
            }

            SyncBoardFromGame();
            if (!_game.IsRunning)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            SyncBoardFromGame();
            var (score, result) = _game.GetHumanPlayerResult();

            var userId = _services.Session.CurrentUser?.Id;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                _services.GameService.SaveScore(userId, "TicTacToe", score, result);
            }
        }

        private void SyncBoardFromGame()
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                var cellValue = _game.Board[i];
                _cells[i].Text = cellValue;
                _cells[i].Enabled = _game.IsRunning && string.IsNullOrEmpty(cellValue);
            }

            _lblModeHint.Text = _game.Mode == TicTacToeMode.VsBot
                ? "Mode: Play with bot (you are X, bot is O)."
                : "Mode: Play with friend (X and O are both manual).";
            _lblStatus.Text = _game.GetStatusText();
        }
    }
}
