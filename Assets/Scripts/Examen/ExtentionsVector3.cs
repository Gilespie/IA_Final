using UnityEngine;

public static class ExtentionsVector3
{
    public static Vector3 NoY(this Vector3 v)
    {
        v.y = 0;
        return v;
    }
}
