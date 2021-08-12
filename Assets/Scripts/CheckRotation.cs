using UnityEngine;

public static class CheckRotation
{
    public static bool FacingForward(Transform obj)
    {
        if (obj.transform.localRotation.eulerAngles.y > 370 || obj.transform.localRotation.eulerAngles.y < 10) return true;
        else return false;
    }
    public static bool FacingBackward(Transform obj)
    {
        if (obj.transform.localRotation.eulerAngles.y > 170 && obj.transform.localRotation.eulerAngles.y < 190) return true;
        else return false;
    }
    public static float ColliderVerticalAngle(Transform obj)
    {
        return Quaternion.Angle(obj.transform.rotation, Quaternion.Euler(0, obj.transform.rotation.eulerAngles.y, 0));
    }
}
