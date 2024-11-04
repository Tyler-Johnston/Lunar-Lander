using System;
using Microsoft.Xna.Framework;

public static class CollisionDetection
{
    public static bool LineCircleIntersection(Vector2 pt1, Vector2 pt2, Circle circle)
    {
        Vector2 v1 = pt2 - pt1;
        Vector2 v2 = pt1 - circle.Center;
        float b = -2 * (v1.X * v2.X + v1.Y * v2.Y);
        float c = 2 * (v1.X * v1.X + v1.Y * v1.Y);
        float d = (float)Math.Sqrt(b * b - 2 * c * (v2.X * v2.X + v2.Y * v2.Y - circle.Radius * circle.Radius));
        if (float.IsNaN(d)) // no intercept
        {
            return false;
        }
        // These represent the unit distance of point one and two on the line
        float u1 = (b - d) / c;
        float u2 = (b + d) / c;
        return (u1 <= 1 && u1 >= 0) || (u2 <= 1 && u2 >= 0);
    }
}

public class Circle
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }

    public Circle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}
