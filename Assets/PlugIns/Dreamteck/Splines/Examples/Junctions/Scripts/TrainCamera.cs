using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float speed = 1f;
    Transform trs;

    private void Awake()
    {
        trs = transform;
    }

    void LateUpdate()
    {
        Vector3 targetPos = target.position + target.right * offset.x + target.up * offset.y + target.forward * offset.z;
        Quaternion rotation = Quaternion.LookRotation(targetPos - trs.position);
        trs.rotation = Quaternion.Slerp(trs.rotation, rotation, Time.deltaTime * speed);
    }
}
