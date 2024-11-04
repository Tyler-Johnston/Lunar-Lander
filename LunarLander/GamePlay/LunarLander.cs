using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.GamePlay
{
    public class LunarLander
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; private set; }
        public float Rotation { get; private set; }
        public float Scale { get; set; }
        public float Fuel { get; private set; }
        private Texture2D LunarLanderImage {get; set; }
        private const float Gravity = 0.35f;
        private const float Thrust = -0.03f;
        private const float FuelConsumptionRate = 1.1f;

        public LunarLander(Vector2 startPosition, float startRotation, float scale, float initialFuel, Texture2D m_lunarLander)
        {
            Position = startPosition;
            Rotation = startRotation;
            Scale = scale;
            Velocity = Vector2.Zero;
            Fuel = initialFuel;
            LunarLanderImage = m_lunarLander;

        }

        public void update(GameTime gameTime)
        {
            Velocity += new Vector2(0, Gravity * (float)gameTime.ElapsedGameTime.TotalSeconds);
            Position += Velocity;
        }
        
        public Vector2 Center
        {
            get
            {
                return new Vector2(
                    Position.X + LunarLanderImage.Width / 2 * Scale, 
                    Position.Y + LunarLanderImage.Height / 2 * Scale);
            }
        }

        public float Speed
        {
            get
            {
                float speed = (float)Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y);
                return speed * 9;
            }
        }

        public float RotationInDegrees
        {
            get
            {
                float degrees = MathHelper.ToDegrees(Rotation);
                degrees = (degrees + 360) % 360;
                return degrees;
            }
        }

        public void ApplyThrust(GameTime gameTime)
        {
            if (Fuel <= 0.01)
            {
                return;
            }

            Vector2 thrustDirection = new Vector2((float)Math.Cos(Rotation + MathHelper.PiOver2), (float)Math.Sin(Rotation + MathHelper.PiOver2));
            float fuelConsumed = FuelConsumptionRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Fuel - fuelConsumed >= 0)
            {
                Fuel -= fuelConsumed;
                Velocity += thrustDirection * Thrust;
            }
        }

        public void Rotate(float rotationAmount)
        {
            Rotation += rotationAmount;
        }
    }

}