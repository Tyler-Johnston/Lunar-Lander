using System;
using CS5410.Controls;
using CS5410.HighScores;
using CS5410.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Random;

namespace CS5410.GamePlay

{
    public class GamePlayView : GameStateView
    {
        private SpriteFont m_font;
        private Texture2D m_background;
        private Texture2D m_lunarLander;
        private Song m_music;
        private SoundEffect m_explosion;
        private SoundEffect m_landed;
        private SoundEffect m_thrusters;
        private SoundEffectInstance thrustersInstance;
        private RandomMisc randomMisc = new RandomMisc();
        private LunarLander lunarLander;
        private TerrainGenerator terrainGenerator;
        private float m_landerScale;
        public enum GameStatus
        {
            Playing,
            Landed,
            Crashed,
            Won
        }
        private GameStatus gameStatus = GameStatus.Playing;
        private float minY = 0.01f;
        private float maxY = 0.60f;
        private int level = 1;
        private const int MAX_LEVEL = 2;
        private const int screenPadding = 10;
        private uint currentScore;
        private float countdown = 3;
        private float lastUpdateTime = 0;
        private float initialFuel;
        private bool explosionPlayed = false;
        private bool landedPlayed = false;
        private bool thrustKeyPressed = false;
        private bool rotateLeftPressed = false;
        private bool rotateRightPressed = false;
        private bool restartGamePressed = false;
        private bool highScoreRecorded = false;
        private ParticleSystem m_particleSystemFire;
        private ParticleSystem m_particleSystemSmoke;
        private ParticleSystem m_particleSystemFireThrust;
        private ParticleSystem m_particleSystemSmokeThrust;
        private ParticleSystemRenderer m_renderFire;
        private ParticleSystemRenderer m_renderSmoke;

        private ParticleSystemRenderer m_renderFireThrust;
        private ParticleSystemRenderer m_renderSmokeThrust;
        private ContentManager contentManager;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunarLander");
            m_music = contentManager.Load<Song>("Audio/myMusic");
            m_explosion = contentManager.Load<SoundEffect>("Audio/explosion");
            m_landed = contentManager.Load<SoundEffect>("Audio/landed");
            m_thrusters = contentManager.Load<SoundEffect>("Audio/thrusters");
            this.contentManager = contentManager;

            thrustersInstance = m_thrusters.CreateInstance();
            thrustersInstance.IsLooped = true;

            MediaPlayer.Play(m_music);
            MediaPlayer.IsRepeating = true;

            terrainGenerator = new TerrainGenerator(minY, maxY, level, randomMisc, m_graphics);
            terrainGenerator.InitializeTerrain();
            terrainGenerator.GenerateTerrainOutline();
            terrainGenerator.FillTerrain();

            m_particleSystemFire = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2),
                10, 4,
                0.12f, 0.05f,
                500, 100, 200);
            m_renderFire = new ParticleSystemRenderer("fire");

            m_particleSystemSmoke = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2),
                15, 4,
                0.07f, 0.05f,
                750, 300, 200);
            m_renderSmokeThrust = new ParticleSystemRenderer("smoke-2");

            m_particleSystemFireThrust = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth * 2, m_graphics.PreferredBackBufferHeight * 2),
                3, 2,
                0.12f, 0.05f,
                500, 75, 50);
            m_renderFireThrust = new ParticleSystemRenderer("fire");

            m_particleSystemSmokeThrust = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth * 2, m_graphics.PreferredBackBufferHeight * 2),
                2, 2,
                0.07f, 0.05f,
                750, 250, 50);
            m_renderSmoke = new ParticleSystemRenderer("smoke-2");

            m_renderFire.LoadContent(contentManager);
            m_renderSmoke.LoadContent(contentManager);
            m_renderFireThrust.LoadContent(contentManager);
            m_renderSmokeThrust.LoadContent(contentManager);
            InitializeLunarLander();
        }

        private Vector2 calculateThrusterEffectPosition()
        {
            Vector2 thrusterOffsetLocal = new Vector2(0, m_lunarLander.Height / 2 * m_landerScale);
            Vector2 rotatedOffset = RotateVector(thrusterOffsetLocal, lunarLander.Rotation);
            Vector2 thrusterEffectPosition = lunarLander.Center + rotatedOffset;
            return thrusterEffectPosition;
        }

        private Vector2 RotateVector(Vector2 vector, float angle)
        {
            float cosAngle = (float)Math.Cos(angle);
            float sinAngle = (float)Math.Sin(angle);
            return new Vector2(
                vector.X * cosAngle - vector.Y * sinAngle,
                vector.X * sinAngle + vector.Y * cosAngle);
        }

        private void InitializeLunarLander()
        {
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            float desiredHeightPercentage = 0.05f;

            // calculate the scale factor based on the desired height of the lunar lander relative to screen height
            float landerHeightAtScale = screenHeight * desiredHeightPercentage;
            m_landerScale = landerHeightAtScale / m_lunarLander.Height;

            // calculate the Y-coordinate position to ensure it's above maxY by a margin and below the top of the screen
            float maxYScreenPosition = screenHeight * (1 - maxY);
            float landerYPositionMargin = maxYScreenPosition * 0.4f;

            // randomize X position
            float landerX = (float)randomMisc.nextDoubleInRange(0, screenWidth - (m_lunarLander.Width * m_landerScale));

            // generate a random rotation. Assuming full rotation is from 0 to 2*PI radians
            float randomRotation = (float)(randomMisc.nextDouble() * Math.PI * 2);

            const float BaseFuel = 20;
            const float a = 0.15f;
            float adjustedFuel = BaseFuel / (1 + a * (level - 1));
            initialFuel = adjustedFuel;
    
            lunarLander = new LunarLander(new Vector2(landerX, landerYPositionMargin - (m_lunarLander.Height * m_landerScale)), randomRotation, m_landerScale, adjustedFuel, m_lunarLander);
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            thrustKeyPressed = keyboardState.IsKeyDown(ControlsManager.Controls["Thrust"]);
            rotateLeftPressed = keyboardState.IsKeyDown(ControlsManager.Controls["RotateLeft"]);
            rotateRightPressed = keyboardState.IsKeyDown(ControlsManager.Controls["RotateRight"]);

            if (gameStatus == GameStatus.Crashed && keyboardState.IsKeyDown(Keys.Y))
            {
                restartGamePressed = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }
            return GameStateEnum.GamePlay;
        }

        private void renderWon()
        {
            if (gameStatus == GameStatus.Landed && level == MAX_LEVEL)
            {
                string winMessage = "You won! Congratulations!";
                Vector2 winMessageSize = m_font.MeasureString(winMessage);
                Vector2 winMessagePosition = new Vector2((m_graphics.GraphicsDevice.Viewport.Width - winMessageSize.X) / 2, m_graphics.GraphicsDevice.Viewport.Height / 2 - winMessageSize.Y - 20);
                m_spriteBatch.DrawString(m_font, winMessage, winMessagePosition, Color.White);

                string scoreMessage = $"Your score is: {currentScore}";
                Vector2 scoreMessageSize = m_font.MeasureString(scoreMessage);
                Vector2 scoreMessagePosition = new Vector2((m_graphics.GraphicsDevice.Viewport.Width - scoreMessageSize.X) / 2, winMessagePosition.Y + winMessageSize.Y + 10); // Adjust for space after win message
                m_spriteBatch.DrawString(m_font, scoreMessage, scoreMessagePosition, Color.White);

                // string continueMessage = "New Game? Press Y to restart";
                // Vector2 continueMessageSize = m_font.MeasureString(continueMessage);
                // Vector2 continueMessagePosition = new Vector2(
                //     (m_graphics.GraphicsDevice.Viewport.Width - continueMessageSize.X) / 2,
                //     scoreMessagePosition.Y + scoreMessageSize.Y + 20);
                // m_spriteBatch.DrawString(m_font, continueMessage, continueMessagePosition, Color.White);
            }
        }
        private void renderHUD()
        {
            string levelText = $"Level: {level}";
            Vector2 levelTextPosition = new Vector2(screenPadding, screenPadding);
            m_spriteBatch.DrawString(m_font, levelText, levelTextPosition, Color.White);

            string fuelText = $"Fuel: {lunarLander.Fuel:F2}";
            if (lunarLander.Fuel < .05)
            {
                fuelText = $"Fuel: {0}";
            }
            string speedText = $"Speed: {lunarLander.Speed:F2} m/s";
            string rotationText = $"Angle: {lunarLander.RotationInDegrees:F0}";

            Color fuelColor = lunarLander.Fuel > 0.05 ? Color.Green : Color.White;
            Color speedColor = lunarLander.Speed > 2 ? Color.White : Color.Green;
            Color rotationColor = (lunarLander.RotationInDegrees <= 5 || lunarLander.RotationInDegrees >= 355) ? Color.Green : Color.White;

            Vector2 fuelTextSize = m_font.MeasureString(fuelText);
            Vector2 speedTextSize = m_font.MeasureString(speedText);
            Vector2 rotationTextSize = m_font.MeasureString(rotationText);
            Vector2 fuelTextPosition = new Vector2(m_graphics.GraphicsDevice.Viewport.Width - fuelTextSize.X - screenPadding, screenPadding);
            Vector2 speedTextPosition = new Vector2(m_graphics.GraphicsDevice.Viewport.Width - speedTextSize.X - screenPadding, fuelTextPosition.Y + fuelTextSize.Y);
            Vector2 rotationTextPosition = new Vector2(m_graphics.GraphicsDevice.Viewport.Width - rotationTextSize.X - screenPadding, speedTextPosition.Y + speedTextSize.Y);

            m_spriteBatch.DrawString(m_font, fuelText, fuelTextPosition, fuelColor);
            m_spriteBatch.DrawString(m_font, speedText, speedTextPosition, speedColor);
            m_spriteBatch.DrawString(m_font, rotationText, rotationTextPosition, rotationColor);
        }

        private void renderCrashed()
        {
            if (gameStatus == GameStatus.Crashed)
            {
                string continueMessage = "New Game? Press Y to restart";
                Vector2 continueMessageSize = m_font.MeasureString(continueMessage);
                Vector2 continueMessagePosition = new Vector2(
                (m_graphics.GraphicsDevice.Viewport.Width - continueMessageSize.X) / 2,
                (m_graphics.GraphicsDevice.Viewport.Height - continueMessageSize.Y) / 2);
                m_spriteBatch.DrawString(m_font, continueMessage, continueMessagePosition, Color.White);
            }
        }

        private void renderLanded()
        {
            if (gameStatus == GameStatus.Landed && level != MAX_LEVEL)
            {
                string winMessage = "You've landed successfully!";
                Vector2 winMessageSize = m_font.MeasureString(winMessage);
                Vector2 winMessagePosition = new Vector2((m_graphics.GraphicsDevice.Viewport.Width - winMessageSize.X) / 2, (m_graphics.GraphicsDevice.Viewport.Height / 2) - 20);
                m_spriteBatch.DrawString(m_font, winMessage, winMessagePosition, Color.White);

                string countdownText = countdown > 0 ? countdown.ToString("F0") : "Go!";
                Vector2 countdownTextSize = m_font.MeasureString(countdownText);
                Vector2 countdownTextPosition = new Vector2((m_graphics.GraphicsDevice.Viewport.Width - countdownTextSize.X) / 2, winMessagePosition.Y + 40);
                m_spriteBatch.DrawString(m_font, countdownText, countdownTextPosition, Color.White);
            }
        }

        private void renderLunarLander()
        {
            if (gameStatus != GameStatus.Crashed)
            {
                Vector2 origin = new Vector2(m_lunarLander.Width / 2, m_lunarLander.Height / 2);
                m_spriteBatch.Draw(
                    m_lunarLander,
                    position: lunarLander.Position + origin * m_landerScale,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: lunarLander.Rotation,
                    origin: origin,
                    scale: m_landerScale,
                    effects: SpriteEffects.None,
                    layerDepth: 0f);
            }
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height), Color.White);
            m_spriteBatch.End();

            foreach (EffectPass pass in terrainGenerator.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                m_graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, 
                    terrainGenerator.Vertices, 0, terrainGenerator.Vertices.Length, 
                    terrainGenerator.Indices, 0, terrainGenerator.Indices.Length / 3);

                m_graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineList, terrainGenerator.Outline, 0, terrainGenerator.Outline.Length / 2);
            }

            m_spriteBatch.Begin();
            renderHUD();
            renderLunarLander();
            renderCrashed();
            renderLanded();
            renderWon();
            m_spriteBatch.End();

            if (gameStatus == GameStatus.Playing && lunarLander.Fuel > .05)
            {
                m_renderSmoke.draw(m_spriteBatch, m_particleSystemSmokeThrust);
                m_renderFire.draw(m_spriteBatch, m_particleSystemFireThrust); 
            }

            if (gameStatus == GameStatus.Crashed)
            {
                m_renderSmoke.draw(m_spriteBatch, m_particleSystemSmoke);
                m_renderFire.draw(m_spriteBatch, m_particleSystemFire); 
            }
        }

        private void resetGameState()
        {
            level += 1;
            gameStatus = GameStatus.Playing;
            countdown = 3;
            lastUpdateTime = 0;
            explosionPlayed = false;
            landedPlayed = false;
            highScoreRecorded = false;

            terrainGenerator = new TerrainGenerator(minY, maxY, level, randomMisc, m_graphics);
            terrainGenerator.InitializeTerrain();
            terrainGenerator.GenerateTerrainOutline();
            terrainGenerator.FillTerrain();
            InitializeLunarLander();

            m_particleSystemFire = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2),
                10, 4,
                0.12f, 0.05f,
                500, 100, 100);
            m_renderFire = new ParticleSystemRenderer("fire");

            m_particleSystemSmoke = new ParticleSystem(
                new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2),
                15, 4,
                0.07f, 0.05f,
                750, 300, 100);
            m_renderSmoke = new ParticleSystemRenderer("smoke-2");

            m_renderFire.LoadContent(this.contentManager);
            m_renderSmoke.LoadContent(this.contentManager);
        }

        private void updatePlayingState(GameTime gameTime)
        {
            if (gameStatus == GameStatus.Playing)
            {
                lunarLander.update(gameTime);
                if (thrustKeyPressed)
                {
                    if ((thrustersInstance.State != SoundState.Playing) && lunarLander.Fuel > .05)
                    {
                        thrustersInstance.Play();
                    }
                    lunarLander.ApplyThrust(gameTime);

                    Vector2 thrusterEffectPosition = calculateThrusterEffectPosition();
                    m_particleSystemFireThrust.m_center = thrusterEffectPosition;
                    m_particleSystemSmokeThrust.m_center = thrusterEffectPosition;

                    m_particleSystemFireThrust.shipThrust(15);
                    m_particleSystemSmokeThrust.shipThrust(15);
                }
                else
                {
                    if (thrustersInstance.State == SoundState.Playing)
                    {
                        thrustersInstance.Stop();
                    }
                }
                if (rotateLeftPressed)
                {
                    lunarLander.Rotate(-0.05f);
                }
                if (rotateRightPressed)
                {
                    lunarLander.Rotate(0.05f);
                }
                rotateLeftPressed = false;
                rotateRightPressed = false;
                thrustKeyPressed = false;
            }
        }

        private void updateCrashedState(GameTime gameTime)
        {
            if (gameStatus == GameStatus.Crashed) 
            {
                m_particleSystemFire.update(gameTime);
                m_particleSystemSmoke.update(gameTime);
                if (!explosionPlayed) 
                {
                    m_explosion.Play();
                    explosionPlayed = true;
                    thrustersInstance.Stop();
                }
                if (restartGamePressed)
                {
                    level = 0;
                    resetGameState();
                }
                restartGamePressed = false;
            }
        }


        private void updateLandedState(GameTime gameTime)
        {
            if (gameStatus == GameStatus.Landed)
            {
                if (level >= MAX_LEVEL)
                {
                    gameStatus = GameStatus.Won;
                    EvaluateAndRecordHighScore();
                }
                else
                {
                    float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
                    if (lastUpdateTime == 0)
                    {
                        lastUpdateTime = currentTime;
                    }
                    else if (currentTime - lastUpdateTime >= 1)
                    {
                        countdown--;
                        lastUpdateTime = currentTime;
                    }
                    if (!landedPlayed)
                    {
                        m_landed.Play();
                        landedPlayed = true;
                    }
                    if (countdown <= 0)
                    {
                        resetGameState();
                    }
                }
            }
        }

        private void EvaluateAndRecordHighScore()
        {
            if (gameStatus == GameStatus.Won && !highScoreRecorded)
            {
                float fuelUsed = initialFuel - lunarLander.Fuel;
                uint score = (uint)Math.Max(0, (initialFuel - fuelUsed) * 100);
                currentScore = score;
                HighScore newHighScore = new HighScore()
                {
                    Score = score,
                    TimeStamp = DateTime.Now
                };
                HighScoreManager.AddHighScore(newHighScore);
                highScoreRecorded = true;
            }
        }

        private void checkCollision()
        {
            int scaleX = m_graphics.PreferredBackBufferWidth;
            int scaleY = m_graphics.PreferredBackBufferHeight;
            var terrain = terrainGenerator.Terrain;

            for (int i = 0; i < terrain.Count - 1; i++)
            {
                Vector2 start = new Vector2(terrain[i].X * scaleX, scaleY - (terrain[i].Y * scaleY));
                Vector2 end = new Vector2(terrain[i + 1].X * scaleX, scaleY - (terrain[i + 1].Y * scaleY));
                bool isFlatSurface = Math.Abs(start.Y - end.Y) <= 1;

                Vector2 collisionCenter = lunarLander.Position + new Vector2(m_lunarLander.Width * m_landerScale / 2, m_lunarLander.Height * m_landerScale / 2);
                float lunarLanderRadius = m_lunarLander.Width * m_landerScale / 2;

                if (CollisionDetection.LineCircleIntersection(start, end, new Circle(collisionCenter, lunarLanderRadius)))
                {
                    if ((lunarLander.RotationInDegrees <= 5 || lunarLander.RotationInDegrees >= 355) && lunarLander.Speed < 2 && isFlatSurface)
                    {
                        gameStatus = GameStatus.Landed;
                    }
                    else
                    {
                        gameStatus = GameStatus.Crashed;
                        m_particleSystemFire.m_center = lunarLander.Position;
                        m_particleSystemSmoke.m_center = lunarLander.Position;
                    }
                }
            }
        }

        public override void update(GameTime gameTime)
        {
            updatePlayingState(gameTime);
            updateCrashedState(gameTime);
            updateLandedState(gameTime);
            checkCollision();
            m_particleSystemFireThrust.update(gameTime);
            m_particleSystemSmokeThrust.update(gameTime);
        }
    }
}