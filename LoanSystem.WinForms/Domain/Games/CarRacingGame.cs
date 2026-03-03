namespace LoanSystem.WinForms.Domain.Games
{
    public class CarRacingGame : GameSessionBase
    {
        private readonly List<RectangleF> _obstacles = new();
        private readonly List<RectangleF> _coins = new();

        public override string GameName => "CarRacing";
        public RectangleF Player { get; private set; }
        public IReadOnlyList<RectangleF> Obstacles => _obstacles;
        public IReadOnlyList<RectangleF> Coins => _coins;

        private float _speed;

        public void Initialize(Size canvasSize)
        {
            ResetState(canvasSize);
            IsRunning = false;
        }

        public void Start(Size canvasSize)
        {
            ResetState(canvasSize);
            IsRunning = true;
        }

        public void MoveLeft(float step, float canvasWidth)
        {
            if (!IsRunning)
            {
                return;
            }

            Player = new RectangleF(
                Math.Max(0, Player.X - step),
                Player.Y,
                Player.Width,
                Player.Height);
        }

        public void MoveRight(float step, float canvasWidth)
        {
            if (!IsRunning)
            {
                return;
            }

            Player = new RectangleF(
                Math.Min(canvasWidth - Player.Width, Player.X + step),
                Player.Y,
                Player.Width,
                Player.Height);
        }

        public bool Tick(Size canvasSize)
        {
            if (!IsRunning)
            {
                return false;
            }

            SpawnObjects(canvasSize.Width);
            MoveObjects(canvasSize.Height);
            CheckCoinCollisions();

            if (_obstacles.Any(x => Player.IntersectsWith(x)))
            {
                IsRunning = false;
                return true;
            }

            return false;
        }

        private void ResetState(Size canvasSize)
        {
            Score = 0;
            _speed = 6f;
            _obstacles.Clear();
            _coins.Clear();
            Player = new RectangleF((canvasSize.Width / 2f) - 20, canvasSize.Height - 90, 40, 70);
        }

        private void SpawnObjects(int canvasWidth)
        {
            if (Random.Shared.NextDouble() < 0.045)
            {
                var x = (float)Random.Shared.NextDouble() * (canvasWidth - 38);
                _obstacles.Add(new RectangleF(x, -80, 38, 68));
            }

            if (Random.Shared.NextDouble() < 0.055)
            {
                var x = (float)Random.Shared.NextDouble() * (canvasWidth - 30);
                _coins.Add(new RectangleF(x, -80, 30, 30));
            }
        }

        private void MoveObjects(int canvasHeight)
        {
            for (var i = _obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = _obstacles[i];
                obstacle.Y += _speed;
                if (obstacle.Y > canvasHeight + 40)
                {
                    _obstacles.RemoveAt(i);
                    continue;
                }

                _obstacles[i] = obstacle;
            }

            for (var i = _coins.Count - 1; i >= 0; i--)
            {
                var coin = _coins[i];
                coin.Y += _speed;
                if (coin.Y > canvasHeight + 40)
                {
                    _coins.RemoveAt(i);
                    continue;
                }

                _coins[i] = coin;
            }
        }

        private void CheckCoinCollisions()
        {
            for (var i = _coins.Count - 1; i >= 0; i--)
            {
                if (!Player.IntersectsWith(_coins[i]))
                {
                    continue;
                }

                _coins.RemoveAt(i);
                Score += 50;
            }
        }

        public override GameOutcome BuildOutcome()
        {
            return new GameOutcome(GameName, Score, $"Score: {Score}");
        }
    }
}
