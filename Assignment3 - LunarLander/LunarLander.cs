using System;
using Microsoft.Xna.Framework;
public class LunarLander
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; private set; }
    public float Rotation { get; private set; }
    public float Scale { get; set; }
    private const float Gravity = 0.35f;
    private const float Thrust = -0.03f;

    public LunarLander(Vector2 startPosition, float startRotation, float scale)
    {
        Position = startPosition;
        Rotation = startRotation;
        Scale = scale;
        Velocity = Vector2.Zero;
    }

    public void update(GameTime gameTime)
    {
        Velocity += new Vector2(0, Gravity * (float)gameTime.ElapsedGameTime.TotalSeconds);
        Position += Velocity;
        
    }

    public float Speed
    {
        get
        {
            float speed = (float)Math.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y);
            return speed;
        }
    }

    public float RotationInDegrees
    {
        get
        {
            float degrees = MathHelper.ToDegrees(Rotation);
            // Normalize the angle to be within 0 to 360 degrees
            degrees = (degrees + 360) % 360;
            return degrees;
        }
    }

    public void ApplyThrust()
    {
        Vector2 thrustDirection = new Vector2((float)Math.Cos(Rotation + MathHelper.PiOver2), (float)Math.Sin(Rotation + MathHelper.PiOver2));
        Velocity += thrustDirection * Thrust;
    }

    public void Rotate(float rotationAmount)
    {
        Rotation += rotationAmount;
    }
}
