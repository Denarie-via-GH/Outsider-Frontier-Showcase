using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public Vector3 Offset;
    public float smoothSpeed;
    public Transform playerTransform;
    public Vector3 min, max;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Initiate()
    {
        playerTransform = FindObjectOfType<UnitController>().transform;
    }

    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 desiredPOS = playerTransform.position + Offset;
            Vector3 Bound = new Vector3(Mathf.Clamp(desiredPOS.x, min.x, max.x), Mathf.Clamp(desiredPOS.y, min.y, max.y), -10);
            Vector3 smooethPOS = Vector3.Lerp(transform.position, Bound, smoothSpeed);
            transform.position = smooethPOS;
        }
    }
}
