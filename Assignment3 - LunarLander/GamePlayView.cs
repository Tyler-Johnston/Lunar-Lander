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
        private float minY = 0.01f;
        private float maxY = 0.49f;
        Texture2D pixelTexture;
        private Texture2D m_background;
        private RandomMisc randomMisc = new RandomMisc();
        private List<Vector2> terrain;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
            pixelTexture = new Texture2D(m_graphics.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            InitializeTerrain();
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.GamePlay;
        }

        private void InitializeTerrain()
        {
            Vector2 startPoint = new Vector2(0, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            Vector2 endPoint = new Vector2(1, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            int iterations = 7;
            float roughness = 4.0f;
            float safeZoneWidth = 0.08f;

            terrain = RandomMidpointDisplacement(startPoint, endPoint, roughness, iterations);
            terrain.Insert(0, startPoint);
            terrain.Add(endPoint);

            float actualSafeZoneWidth = (terrain[^1].X - terrain[0].X) * safeZoneWidth;
            AddSafeZone(terrain, actualSafeZoneWidth);
        }

        private List<Vector2> RandomMidpointDisplacement(Vector2 start, Vector2 end, float roughness, int iterations)
        {
            if (iterations == 0)
            {
                return new List<Vector2>();
            }

            float midX = (start.X + end.X) / 2;
            float midY = (start.Y + end.Y) / 2;

            float displacement = roughness * (float)randomMisc.nextGaussian(0, .375) * Math.Abs(end.X - start.X);
            midY += displacement;

            // Adjust minY and maxY to reflect the correct bounds for terrain generation (above bottom and below halfway point)
            float minY = 0.01f;
            float maxY = 0.49f;
            midY = Math.Min(Math.Max(midY, minY), maxY);

            Vector2 midPoint = new Vector2(midX, midY);

            List<Vector2> left = RandomMidpointDisplacement(start, midPoint, roughness / 1.1f, iterations - 1);
            List<Vector2> right = RandomMidpointDisplacement(midPoint, end, roughness / 1.1f, iterations - 1);

            left.Add(midPoint);
            left.AddRange(right);
            return left;
        }

        private void AddSafeZone(List<Vector2> terrain, float safeZoneWidth)
        {
            float terrainLength = terrain[terrain.Count - 1].X - terrain[0].X;
            float buffer = 0.15f * terrainLength; // 15% buffer from each end
            float minStart = terrain[0].X + buffer;
            float maxStart = terrain[terrain.Count - 1].X - buffer - safeZoneWidth;

            // Calculate safeZoneStart within the adjusted range
            float safeZoneStart = minStart + (float)(randomMisc.nextDouble() * (maxStart - minStart));
            float safeZoneEnd = safeZoneStart + safeZoneWidth;
            float safeZoneElevation = 0f;
            bool elevationSet = false;

            // Iterate through each point in the terrain, except the last one, to adjust elevations for the safe zone
            for (int i = 0; i < terrain.Count - 1; i++)
            {
                // Check if the current point is within the safe zone boundaries
                if (safeZoneStart <= terrain[i].X && terrain[i].X <= safeZoneEnd)
                {
                    if (!elevationSet)
                    {
                        safeZoneElevation = terrain[i].Y;
                        elevationSet = true;
                    }
                    terrain[i] = new Vector2(terrain[i].X, safeZoneElevation);
                }
                // If the safe zone elevation has not been set and the current point is immediately before the safe zone starts
                else if (!elevationSet && terrain[i].X < safeZoneStart && safeZoneStart < terrain[i + 1].X)
                {
                    safeZoneElevation = terrain[i].Y;
                    elevationSet = true;
                    terrain[i] = new Vector2(terrain[i].X, safeZoneElevation);
                }
                // If the current iteration is at the point immediately before the safe zone ends
                else if (terrain[i].X < safeZoneEnd && safeZoneEnd < terrain[i + 1].X)
                {
                    terrain[i + 1] = new Vector2(terrain[i + 1].X, safeZoneElevation);
                }
            }
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height), Color.White);

            float scaleX = m_graphics.PreferredBackBufferWidth;
            float scaleY = m_graphics.PreferredBackBufferHeight;
            Vector2 previousPoint = Vector2.Zero;

            // Ensure there's at least one point to start with
            if (terrain.Count > 0)
            {
                previousPoint = new Vector2(terrain[0].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[0].Y * scaleY));
            }

            // Draw lines between points in the terrain
            for (int i = 1; i < terrain.Count; i++)
            {
                Vector2 currentPoint = new Vector2(terrain[i].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[i].Y * scaleY));
                DrawLine(previousPoint, currentPoint, Color.White, 2);
                previousPoint = currentPoint;
            }
            m_spriteBatch.End();
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            m_spriteBatch.Draw(pixelTexture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
                null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        public override void update(GameTime gameTime)
        {
        }
    }
}
