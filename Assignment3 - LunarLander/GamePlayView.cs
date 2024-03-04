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
        Texture2D pixelTexture;
        private RandomMisc randomMisc = new RandomMisc();
        private List<Vector2> terrain;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
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
            // randomly generate the elevations for the starting and ending points
            Vector2 startPoint = new Vector2(0, (float)randomMisc.nextDouble());
            Vector2 endPoint = new Vector2(1, (float)randomMisc.nextDouble());
            int iterations = 7;
            float roughness = 4.0f;
            float safeZoneWidth = 0.1f;

            terrain = RandomMidpointDisplacement(startPoint, endPoint, roughness, iterations);
            // add the starting and ending points to the terrain
            terrain.Insert(0, startPoint);
            terrain.Add(endPoint);

            // convert safeZoneWidth from fraction to actual width based on terrain's X values
            float actualSafeZoneWidth = (terrain[^1].X - terrain[0].X) * safeZoneWidth;
            // add a safety zone
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
            midY = Math.Max(0, midY);
            
            Vector2 midPoint = new Vector2(midX, midY);

            List<Vector2> left = RandomMidpointDisplacement(start, midPoint, roughness / 1.1f, iterations - 1);
            List<Vector2> right = RandomMidpointDisplacement(midPoint, end, roughness / 1.1f, iterations - 1);

            // add the midpoint and the right side terrain to list 'left' and return that
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
                    // If the safe zone elevation has not been set yet, set it based on the current terrain point's elevation
                    if (!elevationSet)
                    {
                        safeZoneElevation = terrain[i].Y; // Capture the elevation of the first point within the safe zone
                        elevationSet = true; // Mark that the safe zone elevation has been determined
                    }
                    // Adjust the elevation of the current point to the safe zone elevation, making the terrain flat in this zone
                    terrain[i] = new Vector2(terrain[i].X, safeZoneElevation);
                }
                // If the safe zone elevation has not been set and the current point is immediately before the safe zone starts
                else if (!elevationSet && terrain[i].X < safeZoneStart && safeZoneStart < terrain[i + 1].X)
                {
                    safeZoneElevation = terrain[i].Y; // Use the elevation of the last point before the safe zone as the safe zone's elevation
                    elevationSet = true; // Indicate that the safe zone elevation has been set
                    // Ensure that the starting point of the safe zone matches the safe zone's elevation for a smooth transition
                    terrain[i] = new Vector2(terrain[i].X, safeZoneElevation);
                }
                // If the current iteration is at the point immediately before the safe zone ends
                else if (terrain[i].X < safeZoneEnd && safeZoneEnd < terrain[i + 1].X)
                {
                    // Adjust the elevation of the point immediately after the safe zone to match the safe zone's elevation
                    // This ensures a smooth transition back to the normal terrain
                    terrain[i + 1] = new Vector2(terrain[i + 1].X, safeZoneElevation);
                }
            }
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Calculate scaling and translation factors
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

        // draws a line between two points
        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            // Calculate the angle to rotate the line
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            m_spriteBatch.Draw(pixelTexture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
                null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        public override void update(GameTime gameTime)
        {
        }
    }
}
