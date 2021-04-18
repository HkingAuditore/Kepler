using UnityEngine;

public class TrainCamera : MonoBehaviour
{
    public  Transform target;
    public  Vector3   offset;
    public  float     speed = 1f;
    private Transform trs;

    private void Awake()
    {
        trs = transform;
    }

    private void LateUpdate()
    {
        var targetPos = target.position + target.right * offset.x + target.up * offset.y + target.forward * offset.z;
        var rotation  = Quaternion.LookRotation(targetPos - trs.position);
        trs.rotation = Quaternion.Slerp(trs.rotation, rotation, Time.deltaTime * speed);
    }
}