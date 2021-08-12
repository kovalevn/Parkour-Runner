using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public float followSpeed = 0.1f;

    private Transform myTransform;
    private Vector3 cameraFollowVelocity = Vector3.zero;

    private void Awake()
    {
        myTransform = transform;
    }
    public void FollowTarget(Transform targetTransform, float delta)
    {
        Vector3 targetPosition =
            Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
        myTransform.position = targetPosition;
    }
}
