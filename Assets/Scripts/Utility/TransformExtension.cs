using UnityEngine;

public static class TransformExtension
{
    public static void SetPositionX(this Transform self, float x)
    {
        var position = self.position;
        position.x = x;
        self.position = position;
    }

    public static void SetPositionY(this Transform self, float y)
    {
        var position = self.position;
        position.y = y;
        self.position = position;
    }

    public static void SetPositionZ(this Transform self, float z)
    {
        var position = self.position;
        position.z = z;
        self.position = position;
    }

    public static void SetLocalPositionX(this Transform self, float x)
    {
        var position = self.localPosition;
        position.x = x;
        self.localPosition = position;
    }

    public static void SetLocalPositionY(this Transform self, float y)
    {
        var position = self.localPosition;
        position.y = y;
        self.localPosition = position;
    }

    public static void SetLocalPositionZ(this Transform self, float z)
    {
        var position = self.localPosition;
        position.z = z;
        self.localPosition = position;
    }

    public static void AddPositionX(this Transform self, float x)
    {
        var position = self.position;
        position.x += x;
        self.position = position;
    }

    public static void AddPositionY(this Transform self, float y)
    {
        var position = self.position;
        position.y += y;
        self.position = position;
    }

    public static void AddPositionZ(this Transform self, float z)
    {
        var position = self.position;
        position.z += z;
        self.position = position;
    }

    public static void AddLocalPositionX(this Transform self, float x)
    {
        var position = self.localPosition;
        position.x += x;
        self.localPosition = position;
    }

    public static void AddLocalPositionY(this Transform self, float y)
    {
        var position = self.localPosition;
        position.y += y;
        self.localPosition = position;
    }

    public static void AddLocalPositionZ(this Transform self, float z)
    {
        var position = self.localPosition;
        position.z += z;
        self.localPosition = position;
    }
}
