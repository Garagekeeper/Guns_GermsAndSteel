using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    private float smoothSpeed = 6f;
    public Vector3 TargetPos { get; set; }
    private void Awake()
    {
        TargetPos = transform.position;
    }
    public void MoveCameraWithLerp()
    {
        transform.position = Vector3.Lerp(transform.position, TargetPos, smoothSpeed * Time.fixedDeltaTime);
    }

    public void MoveCameraWithoutLerp(Vector3 pos)
    {
        TargetPos = pos;
        transform.position = pos;
    }
    private void LateUpdate()
    {
        MoveCameraWithLerp();
    }
}
