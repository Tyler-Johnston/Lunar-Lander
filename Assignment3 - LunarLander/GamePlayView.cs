using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Random;

namespace CS5410

{
    public class GamePlayView : GameStateView
    {
        private SpriteFont m_font;
        private BasicEffect m_effect;
        private VertexPositionColor[] m_vertsTris;
        private int[] m_indexTris;

        private VertexPositionColor[] m_outline;
        private float minY = 0.01f;
        private float maxY = 0.60f;
        private int level = 1;
        private Texture2D m_background;
        private LunarLander lunarLander;
        private Texture2D m_lunarLander;

        private RandomMisc randomMisc = new RandomMisc();
        private float m_landerScale;

        private List<SafeZone> safeZones;
        private List<Vector2> terrain;

        public enum GameStatus
        {
            Playing,
            Landed,
            Crashed
        }
        private GameStatus gameStatus = GameStatus.Playing;


        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunarLander");
            InitializeTerrain();
            GenerateTerrainOutline();
            FillTerrain();
            InitializeLunarLander();
        }

        private void InitializeTerrain()
        {
            Vector2 startPoint = new Vector2(0, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            Vector2 endPoint = new Vector2(1, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            int iterations = 7;
            float roughness = 4.0f;
            float safeZoneDistance = .08f;
            safeZones = new List<SafeZone>();
            if (level == 1)
            {
                AddSafeZones(2, safeZoneDistance);
            }
            else
            {
                AddSafeZones(1, safeZoneDistance - .02f);
            }
            terrain = RandomMidpointDisplacement(startPoint, endPoint, roughness, iterations);
            terrain.Insert(0, startPoint);
            terrain.Add(endPoint);
        }

        private void GenerateTerrainOutline()
        {
            int terrainCount = terrain.Count;
            m_outline = new VertexPositionColor[(terrainCount - 1) * 2];
            int scaleX = m_graphics.PreferredBackBufferWidth;
            int scaleY = m_graphics.PreferredBackBufferHeight;

            for (int i = 0; i < terrainCount - 1; i++)
            {
                int index = i * 2;
                Vector2 start = terrain[i];
                Vector2 end = terrain[i + 1];

                m_outline[index] = new VertexPositionColor(new Vector3(start.X * scaleX, scaleY - (start.Y * scaleY), 0), Color.Black);
                m_outline[index + 1] = new VertexPositionColor(new Vector3(end.X * scaleX, scaleY - (end.Y * scaleY), 0), Color.Black);
            }
        }

        private void FillTerrain()
        {
            m_graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.CullClockwiseFace,
                MultiSampleAntiAlias = true,
            };

            m_effect = new BasicEffect(m_graphics.GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up),

                Projection = Matrix.CreateOrthographicOffCenter(
                    0, m_graphics.GraphicsDevice.Viewport.Width,
                    m_graphics.GraphicsDevice.Viewport.Height, 0,   // doing this to get it to match the default of upper left of (0, 0)
                    0.1f, 2)
            };

            int scaleX = m_graphics.PreferredBackBufferWidth;
            int scaleY = m_graphics.PreferredBackBufferHeight;
            int bottomHeight = m_graphics.PreferredBackBufferHeight;

            int terrainCount = terrain.Count;
            int vertexCount = (terrainCount - 1) * 6;
            m_vertsTris = new VertexPositionColor[vertexCount];
            m_indexTris = new int[vertexCount];

            for (int i = 0, v = 0; i < terrainCount - 1; i++, v += 6)
            {
                Vector3 bottomLeft = new Vector3(terrain[i].X * scaleX, bottomHeight, 0);
                Vector3 topLeft = new Vector3(terrain[i].X * scaleX, scaleY - (terrain[i].Y * scaleY), 0);
                Vector3 topRight = new Vector3(terrain[i + 1].X * scaleX, scaleY - (terrain[i + 1].Y * scaleY), 0);
                Vector3 bottomRight = new Vector3(terrain[i + 1].X * scaleX, bottomHeight, 0);

                m_vertsTris[v].Position = bottomLeft;
                m_vertsTris[v + 1].Position = topLeft;
                m_vertsTris[v + 2].Position = topRight;

                m_vertsTris[v + 3].Position = bottomLeft;
                m_vertsTris[v + 4].Position = topRight;
                m_vertsTris[v + 5].Position = bottomRight;

                // apply colors
                for (int j = 0; j < 6; j++)
                {
                    m_vertsTris[v + j].Color = Color.LightGray; 
                }

                // set up the indices
                m_indexTris[v] = v;
                m_indexTris[v + 1] = v + 1;
                m_indexTris[v + 2] = v + 2;
                m_indexTris[v + 3] = v + 3;
                m_indexTris[v + 4] = v + 4;
                m_indexTris[v + 5] = v + 5;
            }
        }

        private void InitializeLunarLander()
        {
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            float desiredHeightPercentage = 0.05f;

            // Calculate the scale factor based on the desired height of the lunar lander relative to screen height
            float landerHeightAtScale = screenHeight * desiredHeightPercentage;
            m_landerScale = landerHeightAtScale / m_lunarLander.Height;

            // Calculate the Y-coordinate position to ensure it's above maxY by a margin and below the top of the screen
            float maxYScreenPosition = screenHeight * (1 - maxY); // Convert maxY to screen coordinates
            float landerYPositionMargin = maxYScreenPosition * 0.4f; // 40% above the maxY terrain height

            // Randomize X position
            float landerX = (float)randomMisc.nextDoubleInRange(0, screenWidth - (m_lunarLander.Width * m_landerScale));

            // Generate a random rotation. Assuming full rotation is from 0 to 2*PI radians
            float randomRotation = (float)(randomMisc.nextDouble() * Math.PI * 2);
            
            lunarLander = new LunarLander(new Vector2(landerX, landerYPositionMargin - (m_lunarLander.Height * m_landerScale)), randomRotation, m_landerScale);
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.GamePlay;
        }

        private List<Vector2> RandomMidpointDisplacement(Vector2 start, Vector2 end, float roughness, int iterations)
        {
            if (iterations == 0)
            {
                return new List<Vector2>();
            }

            float midX = (start.X + end.X) / 2;
            float midY = (start.Y + end.Y) / 2;
            SafeZone mySafeZone = getSafeZone(new Vector2(midX, midY));
            if (mySafeZone == null)
            {
                float displacement = roughness * (float)randomMisc.nextGaussian(0, .375) * Math.Abs(end.X - start.X);
                midY += displacement;
                midY = Math.Min(Math.Max(midY, minY), maxY);
            }
            else
            {
                midY = mySafeZone.Start.Y;
            }
            Vector2 midPoint = new Vector2(midX, midY);

            List<Vector2> left = RandomMidpointDisplacement(start, midPoint, roughness / 1.1f, iterations - 1);
            List<Vector2> right = RandomMidpointDisplacement(midPoint, end, roughness / 1.1f, iterations - 1);

            left.Add(midPoint);
            left.AddRange(right);
            return left;
        }

        private void AddSafeZones(int count, float distance)
        {
            for (int i=0; i < count; i++)
            {
                Vector2 startPoint = new Vector2((float)randomMisc.nextDoubleInRange(.15, .85), minY + (float)randomMisc.nextDouble() * (maxY - minY));
                Vector2 endPoint = new Vector2(startPoint.X + distance, startPoint.Y);
                safeZones.Add(new SafeZone(startPoint, endPoint));
            }
        }

        private SafeZone getSafeZone(Vector2 point)
        {
            foreach (var zone in safeZones)
            {
                if (point.X >= zone.Start.X && point.X <= zone.End.X)
                {
                    return zone;
                }
            }
            return null;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height), Color.White);
            m_spriteBatch.End();
            
            foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                m_graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, 
                    m_vertsTris, 0, m_vertsTris.Length, 
                    m_indexTris, 0, m_indexTris.Length / 3);

                m_graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineList, m_outline, 0, m_outline.Length / 2);
            }
            m_spriteBatch.Begin();

            string speedText = $"Speed: {lunarLander.Speed:F2} m/s";
            string rotationText = $"Angle: {lunarLander.RotationInDegrees:F0}";

            Color speedColor = lunarLander.Speed > 2 ? Color.White : Color.Green;
            Color rotationColor = (lunarLander.RotationInDegrees <= 5 || lunarLander.RotationInDegrees >= 355) ? Color.Green : Color.White;

            Vector2 speedTextSize = m_font.MeasureString(speedText);
            Vector2 rotationTextSize = m_font.MeasureString(rotationText);
            int screenPadding = 10;
            Vector2 speedTextPosition = new Vector2(m_graphics.GraphicsDevice.Viewport.Width - speedTextSize.X - screenPadding, screenPadding);
            Vector2 rotationTextPosition = new Vector2(m_graphics.GraphicsDevice.Viewport.Width - rotationTextSize.X - screenPadding, screenPadding + speedTextSize.Y);

            // Draw the text strings with conditional coloring
            m_spriteBatch.DrawString(m_font, speedText, speedTextPosition, speedColor);
            m_spriteBatch.DrawString(m_font, rotationText, rotationTextPosition, rotationColor);

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

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
            if (gameStatus == GameStatus.Playing)
            {
                lunarLander.update(gameTime);
                var keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                {
                    lunarLander.ApplyThrust();
                }
                if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                {
                    lunarLander.Rotate(-0.05f);
                }
                if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                {
                    lunarLander.Rotate(0.05f);
                }
            }
            int scaleX = m_graphics.PreferredBackBufferWidth;
            int scaleY = m_graphics.PreferredBackBufferHeight;
            // Check for collision between the lunar lander and the terrain
            for (int i = 0; i < terrain.Count - 1; i++)
            {
                Vector2 start = new Vector2(terrain[i].X * scaleX, scaleY - (terrain[i].Y * scaleY));
                Vector2 end = new Vector2(terrain[i + 1].X * scaleX, scaleY - (terrain[i + 1].Y * scaleY));
                bool isFlatSurface = Math.Abs(start.Y - end.Y) <= 1;
                Vector2 bottomCenter = lunarLander.Position + new Vector2(0, m_lunarLander.Height * m_landerScale / 2);
                float lunarLanderRadius = m_lunarLander.Width / 2 * m_landerScale;
                if (CollisionDetection.LineCircleIntersection(start, end, new Circle(bottomCenter, lunarLanderRadius)))
                {
                    if ((lunarLander.RotationInDegrees <= 5 || lunarLander.RotationInDegrees >= 355) && lunarLander.Speed < 2 && isFlatSurface)
                    {
                        gameStatus = GameStatus.Landed;
                    }
                    else
                    {
                        gameStatus = GameStatus.Crashed;
                    }
                }
            }
        }
    }
}