using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using Nyancat.Graphics;
using Nyancat.Graphics.Colors;
using Nyancat.Media;

namespace Nyancat.Scenes
{
    public class NyancatScene : IScene
    {
        private readonly IGraphicsDevice Graphics;
        private readonly NyancatOptions SceneOptions;

        private readonly Stopwatch counter = new Stopwatch();

        private readonly Dictionary<char, string> colors = new Dictionary<char, string>();
        private int _frameId = -1;

        private readonly List<ITexture> _textureBatch = new List<ITexture>();

        private ITexture _currentTexture;

        private MediaPlayer _mediaPlayer;

        private Dictionary<char, Color> _colorMap = new Dictionary<char, Color>()
        {
            { ',', ColorConvert.FromHex("#00005f") },   /* Blue background */
            { '.', Color.White },                       /* White stars */
            { '\'', Color.Black },                      /* Black border */
            { '@', ColorConvert.FromHex("ffffd7") },    /* Tan poptart */
            { '$', Color.Pink },                        /* Pink poptart */
            { '-', ColorConvert.FromHex("#d70087") },   /* Red poptart */
            { '>', Color.Red },                         /* Red rainbow */
            { '&', Color.Orange },                      /* Orange rainbow */
            { '+', Color.Yellow },                      /* Yellow Rainbow */
            { '#', ColorConvert.FromHex("#87ff00") },   /* Green rainbow */
            { '=', ColorConvert.FromHex("#0087ff") },   /* Light blue rainbow */
            { ';', ColorConvert.FromHex("#0000af") },   /* Dark blue rainbow */
            { '*', ColorConvert.FromHex("#585858") },   /* Gray cat face */
            { '%', ColorConvert.FromHex("#d787af") },   /* Pink cheeks */
        };

        public NyancatScene(IGraphicsDevice graphics, NyancatOptions sceneOptions)
        {
            Graphics = graphics;
            SceneOptions = sceneOptions;
        }

        public void Initialize()
        {
            foreach (var frame in NyancatFrames.Frames)
            {
                _textureBatch.Add(new AsciiTexture(frame, _colorMap, 2));
            }

            if (SceneOptions.ShowTitle)
            {
                Graphics.Title = "Nyanyanyanyanyanyanya...";
            }

            if (SceneOptions.Sound)
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var wav = Path.Combine(path, "Resources", "nyancat.wav");
                using (FileStream stream = new FileStream(wav, FileMode.Open))
                {
                    _mediaPlayer = new MediaPlayer(stream);
                    _mediaPlayer.PlayLoop();
                }
            }

            counter.Reset();
            counter.Start();
        }

        public bool ShouldExit()
        {
            if (SceneOptions.Frames != int.MaxValue)
            {
                SceneOptions.Frames--;
            }

            if (SceneOptions.Frames == 0)
            {
                return true;
            }

            return false;
        }

        public void Update(IGameTime gameTime)
        {
            if (ShouldExit())
            {
                _mediaPlayer?.Stop();
                Graphics.Exit();
                return;
            }

            _frameId++;
            if (_frameId >= _textureBatch.Count)
                _frameId = 0;

            _currentTexture = _textureBatch[_frameId];
        }

        public void Render(IGameTime gameTime)
        {
            Graphics.Clear(',', _colorMap[',']);

            var framePosition = new Position
            {
                Row = (Graphics.Height - _currentTexture.Height) / 2,
                Col = (Graphics.Width - _currentTexture.Width) / 2,
            };

            if (framePosition.Col > 0)
            {
                var rainbowPosition = new Position
                {
                    Row = (Graphics.Height - 18) / 2,
                    Col = 0,
                };
                var rainbowTexture = new NyncatTailTexture(_frameId, framePosition.Col, _colorMap);

                Graphics.Draw(rainbowTexture, rainbowPosition);
            }

            Graphics.Draw(_currentTexture, framePosition);

            if (SceneOptions.ShowCounter)
            {

                var seconds = ((int)counter.Elapsed.TotalSeconds).ToString();
                var message = $"You have nyaned for {seconds} seconds!";
                var position = new Position
                {
                    Col = (Graphics.Width - message.Length) / 2,
                    Row = Graphics.Height - 1,
                };

                Graphics.Draw(message, position, Color.White, _colorMap[',']);
            }

            const int defaultSleep = 90;
            var sleep = (defaultSleep * 2) - gameTime.ElapsedGameTime.Milliseconds;
            Thread.Sleep(Math.Clamp(sleep, 0, defaultSleep));

            Graphics.Render();
        }
    }
}
