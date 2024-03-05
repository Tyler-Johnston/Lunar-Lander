using Microsoft.Xna.Framework;
public class LunarLander
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; private set; }
    public float Rotation { get; private set; }
    public float Scale { get; set; }

    private const float Gravity = 0.1f; // Gravitational acceleration constant
    private const float Thrust = -0.2f; // Acceleration due to thrust

    public LunarLander(Vector2 startPosition, float startRotation, float scale)
    {
        Position = startPosition;
        Rotation = startRotation;
        Scale = scale;
        Velocity = Vector2.Zero;
    }


    public void update(GameTime gameTime)
    {
        // Apply gravity to the Y velocity
        Velocity += new Vector2(0, Gravity * (float)gameTime.ElapsedGameTime.TotalSeconds);

        // Update position based on velocity
        Position += Velocity;

        // Example of applying thrust (this could be triggered by user input)
        // Velocity += new Vector2(0, Thrust * (float)gameTime.ElapsedGameTime.TotalSeconds);

        // Example of rotating (this could be triggered by user input)
        // Rotation += 0.01f; // Rotate clockwise
    }

    public void ApplyThrust()
    {
        // Apply thrust upwards, reducing Y velocity
        Velocity += new Vector2(0, Thrust);
    }

    public void Rotate(float rotationAmount)
    {
        // Adjust rotation by the specified amount
        Rotation += rotationAmount;
    }
}
