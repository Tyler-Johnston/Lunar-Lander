using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Random;

namespace CS5410.GamePlay
{
    public class TerrainGenerator
    {
        public List<SafeZone> SafeZones { get; private set; }
        public List<Vector2> Terrain { get; private set; }
        private float minY;
        private float maxY;
        private int level;
        private RandomMisc randomMisc = new RandomMisc();
        private GraphicsDeviceManager m_graphics;
        private BasicEffect m_effect;

        public VertexPositionColor[] Outline { get; private set; }
        public VertexPositionColor[] Vertices { get; private set; }
        public int[] Indices { get; private set; }
        public BasicEffect Effect
        {
            get { return m_effect; }
        }

        public TerrainGenerator(float minY, float maxY, int level, RandomMisc randomMisc, GraphicsDeviceManager m_graphics)
        {
            this.minY = minY;
            this.maxY = maxY;
            this.level = level;
            this.randomMisc = randomMisc;
            SafeZones = new List<SafeZone>();
            Terrain = new List<Vector2>();
            this.m_graphics = m_graphics;
        }

        public void InitializeTerrain()
        {
            Vector2 startPoint = new Vector2(0, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            Vector2 endPoint = new Vector2(1, minY + (float)randomMisc.nextDouble() * (maxY - minY));
            int iterations = 7;
            float roughness = 4.0f;
            float safeZoneDistance = .08f;
            if (level == 1)
            {
                AddSafeZones(2, safeZoneDistance);
            }
            else
            {
                AddSafeZones(1, safeZoneDistance - .02f);
            }
            Terrain = RandomMidpointDisplacement(startPoint, endPoint, roughness, iterations);
            Terrain.Insert(0, startPoint);
            Terrain.Add(endPoint);
        }

        public void GenerateTerrainOutline()
        {
            int TerrainCount = Terrain.Count;
            int scaleX = m_graphics.PreferredBackBufferWidth;
            int scaleY = m_graphics.PreferredBackBufferHeight;
            Outline = new VertexPositionColor[(TerrainCount - 1) * 2];

            for (int i = 0; i < TerrainCount - 1; i++)
            {
                int index = i * 2;
                Vector2 start = Terrain[i];
                Vector2 end = Terrain[i + 1];

                Outline[index] = new VertexPositionColor(new Vector3(start.X * scaleX, scaleY - (start.Y * scaleY), 0), Color.Crimson);
                Outline[index + 1] = new VertexPositionColor(new Vector3(end.X * scaleX, scaleY - (end.Y * scaleY), 0), Color.Crimson);
            }
        }

        public void FillTerrain()
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

            int TerrainCount = Terrain.Count;
            int vertexCount = (TerrainCount - 1) * 6;
            Vertices = new VertexPositionColor[vertexCount];
            Indices = new int[vertexCount];

            for (int i = 0, v = 0; i < TerrainCount - 1; i++, v += 6)
            {
                Vector3 bottomLeft = new Vector3(Terrain[i].X * scaleX, bottomHeight, 0);
                Vector3 topLeft = new Vector3(Terrain[i].X * scaleX, scaleY - (Terrain[i].Y * scaleY), 0);
                Vector3 topRight = new Vector3(Terrain[i + 1].X * scaleX, scaleY - (Terrain[i + 1].Y * scaleY), 0);
                Vector3 bottomRight = new Vector3(Terrain[i + 1].X * scaleX, bottomHeight, 0);

                Vertices[v].Position = bottomLeft;
                Vertices[v + 1].Position = topLeft;
                Vertices[v + 2].Position = topRight;

                Vertices[v + 3].Position = bottomLeft;
                Vertices[v + 4].Position = topRight;
                Vertices[v + 5].Position = bottomRight;

                // apply colors
                for (int j = 0; j < 6; j++)
                {
                    Vertices[v + j].Color = Color.LightGray; 
                }

                // set up the indices
                Indices[v] = v;
                Indices[v + 1] = v + 1;
                Indices[v + 2] = v + 2;
                Indices[v + 3] = v + 3;
                Indices[v + 4] = v + 4;
                Indices[v + 5] = v + 5;
            }
        }

        private void AddSafeZones(int count, float distance)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 startPoint = new Vector2((float)randomMisc.nextDoubleInRange(.15, .85), minY + (float)randomMisc.nextDouble() * (maxY - minY));
                Vector2 endPoint = new Vector2(startPoint.X + distance, startPoint.Y);
                SafeZones.Add(new SafeZone(startPoint, endPoint));
            }
        }

        private List<Vector2> RandomMidpointDisplacement(Vector2 start, Vector2 end, float roughness, int iterations)
        {
            if (iterations == 0)
            {
                return new List<Vector2>();
            }

            float midX = (start.X + end.X) / 2;
            float midY = (start.Y + end.Y) / 2;
            SafeZone mySafeZone = GetSafeZone(new Vector2(midX, midY));
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

        public SafeZone GetSafeZone(Vector2 point)
        {
            foreach (var zone in SafeZones)
            {
                if (point.X >= zone.Start.X && point.X <= zone.End.X)
                {
                    return zone;
                }
            }
            return null;
        }
    }
}
