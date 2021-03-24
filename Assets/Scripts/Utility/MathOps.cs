using UnityEngine;

public class MathOps
{
    public static Vector3 RotateByQuaternion(Vector3 vector, Quaternion quaternion)
    {
        Quaternion inverseQ = Quaternion.Inverse(quaternion);
        Quaternion compVector = new Quaternion(vector.x, vector.y, vector.z, 0);
        Quaternion qNion = quaternion * compVector * inverseQ;
        return new Vector3(qNion.x, qNion.y, qNion.z);
    }

    /// <summary>
    /// Remaps any given agnle to be between the range 0 - 360.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float RemapAngle(float angle)
    {
        angle = angle % 360;
        angle += angle < 0 ? 360 : 0;
        return angle;
    }
}