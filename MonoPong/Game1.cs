using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Threading;

namespace MonoPong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _doubleBuffer;
        private Rectangle _renderRectangle;
        private Texture2D _texture;

        private bool _lastPointSide = true;
        private readonly Random _rand;

        private Ball _ball;
        private Paddle[] _paddles;

        private int[] _scores;
        private SpriteFont _font;

        private SoundEffect _bounceSound;
        private SoundEffect _hitSound;
        private SoundEffect _scoreSound;

        public enum GameState { Idle, Choose, Start, Play, CheckEnd }
        private GameState _gameState;

        private readonly PlayerPad[] _pads;

        private Texture2D _keyboard;
        private Texture2D _mouse;
        private Texture2D _gamepad;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            this.TargetElapsedTime = new TimeSpan(333333);
            Window.AllowUserResizing = true;

            _gameState = GameState.Idle;

            _rand = new Random();

            _paddles = new Paddle[2];

            _pads = new PlayerPad[2];
            _pads[0] = new PlayerPad(PadType.AI);
            _pads[1] = new PlayerPad(PadType.AI);
        }

        protected override void Initialize()
        {
            _doubleBuffer = new RenderTarget2D(GraphicsDevice, 640, 480);

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            _graphics.IsFullScreen = false;

            _graphics.ApplyChanges();

            Window.ClientSizeChanged += OnWindowSizeChange;
            OnWindowSizeChange(null, null);

            _ball = new Ball(_rand, _lastPointSide);

            base.Initialize();
        }

        private void OnWindowSizeChange(object sender, EventArgs e)
        {
            var width = Window.ClientBounds.Width;
            var height = Window.ClientBounds.Height;

            if (height < width / (float)_doubleBuffer.Width * _doubleBuffer.Height)
            {
                width = (int)(height / (float)_doubleBuffer.Height * _doubleBuffer.Width);
            }
            else
            {
                height = (int)(width / (float)_doubleBuffer.Width * _doubleBuffer.Height);
            }

            var x = (Window.ClientBounds.Width - width) / 2;
            var y = (Window.ClientBounds.Height - height) / 2;
            _renderRectangle = new Rectangle(x, y, width, height);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _texture = new Texture2D(GraphicsDevice, 1, 1);
            Color[] data = new Color[1];
            data[0] = Color.White;
            _texture.SetData(data);

            _font = Content.Load<SpriteFont>("font");
            _bounceSound = Content.Load<SoundEffect>("Click3");
            _hitSound = Content.Load<SoundEffect>("Click7");
            _scoreSound = Content.Load<SoundEffect>("Warning");

            _keyboard = Content.Load<Texture2D>("Sprites/Keyboard");
            _mouse = Content.Load<Texture2D>("Sprites/Mouse");
            _gamepad = Content.Load<Texture2D>("Sprites/Pad");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            switch (_gameState)
            {
                case GameState.Idle:
                    (_, bool bounce) = _ball.Move(true);
                    if (bounce) _bounceSound.Play();

                    var (_, butt) = PlayerPad.SelectionPoll();
                    if (butt == Button.Click) _gameState = GameState.Choose;
                    break;
                case GameState.Choose:
                    var (con, but) = PlayerPad.SelectionPoll();
                    if (but == Button.Left && _pads[1].Type == PadType.AI) _pads[1] = new PlayerPad(PadType.AI);
                    else if (but == Button.Left && _pads[0].Type == PadType.AI) _pads[0] = new PlayerPad(con);

                    if (but == Button.Right && _pads[0].Type == con) _pads[0] = new PlayerPad(PadType.AI);
                    else if (but == Button.Right && _pads[1].Type == PadType.AI) _pads[1] = new PlayerPad(con);

                    if (but == Button.Click) _gameState = GameState.Start;
                    break;
                case GameState.Start:
                    _ball = new Ball(_rand, _lastPointSide);
                    _paddles[0] = new Paddle(false);
                    _paddles[1] = new Paddle(true);
                    _scores = new int[2];
                    _pads[0] = new PlayerPad(PadType.Keyboard);
                    _gameState = GameState.Play;
                    break;
                case GameState.Play:
                    (int scored, bool bounced) = _ball.Move(false);
                    if (bounced) _bounceSound.Play();

                    for (int i = 0; i < 2; i++)
                    {
                        if (_pads[i].Type == PadType.AI)
                        {
                            _paddles[i].AIMove(_ball);
                        }
                        else
                        {
                            _pads[i].Poll();
                            _paddles[i].PlayerMove(_pads[i].Y);
                        }
                    }

                    var hit = _paddles[0].CollisionCheck(_ball);
                    hit |= _paddles[1].CollisionCheck(_ball);

                    if (hit)
                    {
                        _hitSound.Play();
                        return;
                    }

                    if (scored == 0) return;

                    _gameState = GameState.CheckEnd;
                    
                    _lastPointSide = scored == 1;
                    int index = _lastPointSide ? 0 : 1;
                    _scores[index]++;
                    _scoreSound.Play();

                    break;
                case GameState.CheckEnd:
                    _ball = new Ball(_rand, _lastPointSide);
                    if (_scores[0] > 9 || _scores[1] > 9)
                    {
                        _gameState = GameState.Idle;
                        return;
                    }
                    _gameState = GameState.Play;
                    break;
                default:
                    _gameState = GameState.Idle;
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_doubleBuffer);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            for (int i = 0; i < 31; ++i)
            {
                _spriteBatch.Draw(
                    _texture,
                    new Rectangle(_doubleBuffer.Width / 2, i * _doubleBuffer.Height / 31,
                    2,
                    _doubleBuffer.Height / 62), Color.White
                    );
            }

            switch (_gameState)
            {
                case GameState.Idle:
                    _spriteBatch.Draw(_texture, _ball.Box, Color.White);
                    _spriteBatch.DrawString(_font, "Click", new Vector2(_doubleBuffer.Width / 2 - 80, _doubleBuffer.Height / 2 - 32), Color.White);
                    break;
                case GameState.Choose:
                    int displace = _pads[0].Type == PadType.Mouse ? -128 : _pads[1].Type == PadType.Mouse ? 128 : 0;
                    _spriteBatch.Draw(_mouse, new Rectangle(_doubleBuffer.Width / 2 - 32 + displace, _doubleBuffer.Height / 2 - 192, 64, 64), Color.White);

                    displace = _pads[0].Type == PadType.Keyboard ? -128 : _pads[1].Type == PadType.Keyboard ? 128 : 0;
                    _spriteBatch.Draw(_keyboard, new Rectangle(_doubleBuffer.Width / 2 - 32 + displace, _doubleBuffer.Height / 2 - 128, 64, 64), Color.White);

                    for (int i = 0; i < 4; i++)
                    {
                        if (!GamePad.GetState(i).IsConnected) continue;
                        displace = _pads[0].Type == (PadType) i ? -128 : _pads[1].Type == (PadType) i ? 128 : 0;
                        _spriteBatch.Draw(_gamepad, new Rectangle(_doubleBuffer.Width / 2 - 32 + displace, _doubleBuffer.Height / 2 - 64 + i * 64, 64, 64), Color.White);
                    }
                    break;
                case GameState.Start:
                    break;
                case GameState.Play:
                case GameState.CheckEnd:
                    _spriteBatch.Draw(_texture, _ball.Box, Color.White);

                    _spriteBatch.Draw(_texture, _paddles[0].Box, Color.White);
                    _spriteBatch.Draw(_texture, _paddles[1].Box, Color.White);

                    _spriteBatch.DrawString(_font, _scores[0].ToString(), new Vector2(64, 0), Color.White);
                    _spriteBatch.DrawString(_font, _scores[1].ToString(), new Vector2(_doubleBuffer.Width - 102, 0), Color.White);
                    break;
            }

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_doubleBuffer, _renderRectangle, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
