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
        private float minY = 0.01f;
        private float maxY = 0.49f;
        Texture2D terrainTexture;
        private Texture2D m_background;
        private RandomMisc randomMisc = new RandomMisc();
        private List<Vector2> terrain;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/background");
            terrainTexture = new Texture2D(m_graphics.GraphicsDevice, 1, 1);
            terrainTexture.SetData(new[] { Color.White });
            InitializeTerrain();

            m_graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.CullClockwiseFace,   // CullMode.None If you want to not worry about triangle winding order
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
            int vertexCount = (terrainCount - 1) * 6; // Each segment forms two triangles (6 vertices)
            m_vertsTris = new VertexPositionColor[vertexCount];
            m_indexTris = new int[vertexCount]; // Each vertex will be directly indexed


            for (int i = 0, v = 0; i < terrainCount - 1; i++, v += 6)
            {
                Vector3 bottomLeft = new Vector3(terrain[i].X * scaleX, bottomHeight, 0);
                Vector3 topLeft = new Vector3(terrain[i].X * scaleX, scaleY - (terrain[i].Y * scaleY), 0);
                Vector3 topRight = new Vector3(terrain[i + 1].X * scaleX, scaleY - (terrain[i + 1].Y * scaleY), 0);
                Vector3 bottomRight = new Vector3(terrain[i + 1].X * scaleX, bottomHeight, 0);

                // First Triangle: bottomLeft -> topLeft -> topRight
                m_vertsTris[v].Position = bottomLeft;
                m_vertsTris[v + 1].Position = topLeft;
                m_vertsTris[v + 2].Position = topRight;

                // Second Triangle: bottomLeft -> topRight -> bottomRight
                m_vertsTris[v + 3].Position = bottomLeft;
                m_vertsTris[v + 4].Position = topRight;
                m_vertsTris[v + 5].Position = bottomRight;

                // Apply colors (optional)
                for (int j = 0; j < 6; j++)
                {
                    m_vertsTris[v + j].Color = Color.Gray; // Set your desired color
                }

                // Setting up indices (direct mapping in this case)
                m_indexTris[v] = v;
                m_indexTris[v + 1] = v + 1;
                m_indexTris[v + 2] = v + 2;
                m_indexTris[v + 3] = v + 3;
                m_indexTris[v + 4] = v + 4;
                m_indexTris[v + 5] = v + 5;
            }


            // m_vertsTris = new VertexPositionColor[12];

            // // Define the position and color for each vertex - Triangle 1
            // // winding order: bottom, top, right
            // m_vertsTris[0].Position = new Vector3(terrain[0].X * scaleX, bottomHeight, 0);
            // m_vertsTris[0].Color = Color.Red;
            // m_vertsTris[1].Position = new Vector3(terrain[0].X * scaleX, scaleY - (terrain[0].Y * scaleY), 0);
            // m_vertsTris[1].Color = Color.Red;
            // m_vertsTris[2].Position = new Vector3(terrain[1].X * scaleX, scaleY - (terrain[1].Y * scaleY), 0);
            // m_vertsTris[2].Color = Color.Red;

            // // Define the position and color for each vertex - Triangle 2
            // // winding order: bottom, top, right
            // m_vertsTris[3].Position = new Vector3(terrain[0].X * scaleX, bottomHeight, 0);
            // m_vertsTris[3].Color = Color.Blue;
            // m_vertsTris[4].Position = new Vector3(terrain[1].X * scaleX, scaleY - (terrain[1].Y * scaleY), 0);
            // m_vertsTris[4].Color = Color.Blue;
            // m_vertsTris[5].Position = new Vector3(terrain[1].X * scaleX, bottomHeight, 0);
            // m_vertsTris[5].Color = Color.Blue;

            // // Define the position and color for each vertex - Triangle 3
            // // winding order: bottom, top, right
            // m_vertsTris[6].Position = new Vector3(terrain[1].X * scaleX, bottomHeight, 0);
            // m_vertsTris[6].Color = Color.Green;
            // m_vertsTris[7].Position = new Vector3(terrain[1].X * scaleX, scaleY - (terrain[1].Y * scaleY), 0);
            // m_vertsTris[7].Color = Color.Green;
            // m_vertsTris[8].Position = new Vector3(terrain[2].X * scaleX, scaleY - (terrain[2].Y * scaleY), 0);
            // m_vertsTris[8].Color = Color.Green;

            // // Define the position and color for each vertex - Triangle 4
            // // winding order: bottom, top, right
            // m_vertsTris[9].Position = new Vector3(terrain[1].X * scaleX, bottomHeight, 0);
            // m_vertsTris[9].Color = Color.Yellow;
            // m_vertsTris[10].Position = new Vector3(terrain[2].X * scaleX, scaleY - (terrain[2].Y * scaleY), 0);
            // m_vertsTris[10].Color = Color.Yellow;
            // m_vertsTris[11].Position = new Vector3(terrain[2].X * scaleX, bottomHeight, 0);
            // m_vertsTris[11].Color = Color.Yellow;

            // // Create an array that holds the 'index' of each vertex
            // // for each triangle, in groups of 3
            // m_indexTris = new int[12];

            // // Triangle 1 - Indexes
            // m_indexTris[0] = 0;
            // m_indexTris[1] = 1;
            // m_indexTris[2] = 2;

            // // Triangle 2 - Indexes
            // m_indexTris[3] = 3;
            // m_indexTris[4] = 4;
            // m_indexTris[5] = 5;

            // // Triangle 3 - Indexes
            // m_indexTris[6] = 6;
            // m_indexTris[7] = 7;
            // m_indexTris[8] = 8;

            // // Triangle 4 - Indexes
            // m_indexTris[9] = 9;
            // m_indexTris[10] = 10;
            // m_indexTris[11] = 11;



            // for each terrain point, we need two vertices: one at the terrain Y and another at the bottom
            // int vertexCount = terrain.Count * 2;
            // m_vertsTriList = new VertexPositionColor[vertexCount];

            // int scaleX = m_graphics.PreferredBackBufferWidth;
            // int scaleY = m_graphics.PreferredBackBufferHeight;
            // int bottomHeight = m_graphics.PreferredBackBufferHeight;

            // for (int i = 0, v = 0; i < terrain.Count; i++, v += 2)
            // {
            //     // corresponding vertex at the bottom
            //     m_vertsTriList[v].Position = new Vector3(terrain[i].X * scaleX, bottomHeight, 0);
            //     m_vertsTriList[v].Color = Color.LightGray;

            //     // vertex at the terrain point
            //     m_vertsTriList[v+1].Position = new Vector3(terrain[i].X * scaleX, scaleY - (terrain[i].Y * scaleY), 0);
            //     m_vertsTriList[v+1].Color = Color.LightGray;
            // }

            // m_indexTriList = new int[vertexCount];
            // for (int i = 0; i < vertexCount; i++) 
            // {
            //     m_indexTriList[i] = i;
            // }
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

            // wanted to make sure the terrain line doesn't go above halfway of the screen's height or below the screen to ensure room for space ship
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

        // TODO: if time permits adjust this so it isn't being added after the random midpoint displacement algortihm
        private void AddSafeZone(List<Vector2> terrain, float safeZoneWidth)
        {
            float terrainLength = terrain[terrain.Count - 1].X - terrain[0].X;
            float buffer = 0.15f * terrainLength; // 15% buffer from each end
            float minStart = terrain[0].X + buffer;
            float maxStart = terrain[terrain.Count - 1].X - buffer - safeZoneWidth;

            // calculate safeZoneStart within the adjusted range
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

            // ensure there's at least one point to start with
            if (terrain.Count > 0)
            {
                previousPoint = new Vector2(terrain[0].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[0].Y * scaleY));
            }

            // draw lines between points in the terrain
            for (int i = 1; i < terrain.Count; i++)
            {
                Vector2 currentPoint = new Vector2(terrain[i].X * scaleX, m_graphics.PreferredBackBufferHeight - (terrain[i].Y * scaleY));
                DrawLine(previousPoint, currentPoint, Color.White, 2);
                previousPoint = currentPoint;
            }

            m_spriteBatch.End();

            foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                m_graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, 
                    m_vertsTris, 0, m_vertsTris.Length, 
                    m_indexTris, 0, m_indexTris.Length / 3);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            m_spriteBatch.Draw(terrainTexture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
                null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        public override void update(GameTime gameTime)
        {
        }
    }
}
