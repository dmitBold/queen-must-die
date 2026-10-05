using UnityEngine;

public static class PendingSpawn
{
    public static Vector3? Position;
    public static Quaternion? Rotation;

    public static void Set(Vector3 pos, Quaternion rot)
    {
        Position = pos;
        Rotation = rot;
    }

    public static bool TryConsume(out Vector3 pos, out Quaternion rot)
    {
        if (Position.HasValue && Rotation.HasValue)
        {
            pos = Position.Value;
            rot = Rotation.Value;
            Position = null;
            Rotation = null;
            return true;
        }
        pos = default;
        rot = default;
        return false;
    }
}