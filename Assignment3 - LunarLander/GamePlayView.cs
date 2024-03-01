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
        private const string MESSAGE = "Isn't this game fun!";
        Texture2D pixelTexture; // Add this field to your class
        private RandomMisc randomMisc = new RandomMisc();
        private List<Vector2> terrain;


        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            // Create a 1x1 white texture for drawing lines
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
            Vector2 startPoint = new Vector2(0, (float)randomMisc.nextDouble());
            Vector2 endPoint = new Vector2(1, (float)randomMisc.nextDouble());
            int iterations = 7;
            float roughness = 0.4f;
            float safeZoneWidth = 0.1f;

            terrain = RandomMidpointDisplacement(startPoint, endPoint, roughness, iterations);
            // add the starting and ending points to the terrain
            terrain.Insert(0, startPoint);
            terrain.Add(endPoint);

            // foreach (var thing in terrain)
            // {
            //     Console.WriteLine(thing);
            // }

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

            // ensure the elevation does not dip below 0 by getting the max of the new displacement or 0
            float displacement = Math.Max(roughness * (float)randomMisc.nextGaussian(0, 1), 0);
            midY += displacement;
            
            Vector2 midPoint = new Vector2(midX, midY);

            List<Vector2> left = RandomMidpointDisplacement(start, midPoint, roughness / 2, iterations - 1);
            List<Vector2> right = RandomMidpointDisplacement(midPoint, end, roughness / 2, iterations - 1);

            // add the midpoint and the right side terrain to list 'left' and return that
            left.Add(midPoint);
            left.AddRange(right);
            return left;
        }

        private List<Vector2> AddSafeZone(List<Vector2> terrain, float safeZoneWidth)
        {
            float terrainLength = terrain[terrain.Count - 1].X - terrain[0].X;
            float minStart = 0.15f * terrainLength;
            float maxStart = (1 - 0.15f) * terrainLength - safeZoneWidth;
            float safeZoneStart = randomMisc.nextRange((int)minStart, (int)maxStart);
            float safeZoneEnd = safeZoneStart + safeZoneWidth;
            float safeZoneElevation = 0f;
            bool elevationSet = false;

            for (int i = 0; i < terrain.Count - 1; i++)
            {
                if (safeZoneStart <= terrain[i].X && terrain[i].X <= safeZoneEnd)
                {
                    if (!elevationSet)
                    {
                        safeZoneElevation = terrain[i].Y;
                        elevationSet = true;
                    }
                    terrain[i] = new Vector2(terrain[i].X, safeZoneElevation);
                }
                else if (terrain[i].X < safeZoneStart && safeZoneStart <= terrain[i + 1].X && !elevationSet)
                {
                    safeZoneElevation = terrain[i].Y;
                    elevationSet = true;
                }
                else if (terrain[i].X < safeZoneEnd && safeZoneEnd < terrain[i + 1].X)
                {
                    terrain[i + 1] = new Vector2(terrain[i + 1].X, safeZoneElevation);
                }
            }

            return terrain;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Draw the message
            Vector2 stringSize = m_font.MeasureString(MESSAGE);
            m_spriteBatch.DrawString(m_font, MESSAGE,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y), Color.Yellow);

            // Calculate scaling and translation factors
            float scaleX = m_graphics.PreferredBackBufferWidth;
            float scaleY = m_graphics.PreferredBackBufferHeight;
            Vector2 previousPoint = Vector2.Zero;

            // Ensure there's at least one point to start with
            if (terrain.Count > 0)
            {
                previousPoint = new Vector2(terrain[0].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[0].Y * scaleY)); // Flip Y for screen coordinates
            }

            // Draw lines between points in the terrain
            for (int i = 1; i < terrain.Count; i++)
            {
                Vector2 currentPoint = new Vector2(terrain[i].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[i].Y * scaleY)); // Scale and flip Y
                DrawLine(previousPoint, currentPoint, Color.White, 2); // Assuming you have an extension method DrawLine
                previousPoint = currentPoint;
            }

            m_spriteBatch.End();
        }


        // public override void render(GameTime gameTime)
        // {
        //     m_spriteBatch.Begin();

        //     Vector2 stringSize = m_font.MeasureString(MESSAGE);
        //     m_spriteBatch.DrawString(m_font, MESSAGE,
        //         new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y), Color.Yellow);

        //     m_spriteBatch.End();
        // }

        // Assuming you have a 1x1 white texture pixel called pixelTexture
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
