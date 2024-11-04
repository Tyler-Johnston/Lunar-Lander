using Microsoft.Xna.Framework;

namespace CS5410.GamePlay
{
    public class SafeZone
{
    public Vector2 Start;
    public Vector2 End;

    public SafeZone(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }
}

}