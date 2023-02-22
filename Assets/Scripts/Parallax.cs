using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public bool isActive = false;
    public CameraFollow Camera;
    public UnitBaseModule Player;
    public Transform CamTrans;
    public Vector3 LastCamPOS;
    public float SpriteSizeX;
    public float SpriteSizeY;
    [SerializeField] public Vector2 PARALLAX_VAL;

    public bool ScaleX;
    public bool ScaleY;
    public void Initinate()
    {
        Camera = CameraFollow.Instance;
        Player = FindObjectOfType<UnitController>().GetComponent<UnitBaseModule>();

        CamTrans = Camera.GetComponent<Camera>().transform;
        LastCamPOS = CamTrans.position;
        Sprite SP = GetComponent<SpriteRenderer>().sprite;
        Texture2D TXT = SP.texture;
        SpriteSizeX = TXT.width / SP.pixelsPerUnit * transform.localScale.x;
        SpriteSizeY = TXT.height / SP.pixelsPerUnit * transform.localScale.y;

        isActive = true;
    }

    public void LateUpdate()
    {
        if (isActive)
        {
            Vector3 deltaMovement = CamTrans.position - LastCamPOS;
            transform.position += new Vector3(deltaMovement.x * PARALLAX_VAL.x, deltaMovement.y * PARALLAX_VAL.y, deltaMovement.z);
            LastCamPOS = CamTrans.position;

            if (ScaleX)
            {
                if (Mathf.Abs(CamTrans.position.x - transform.position.x) >= SpriteSizeX)
                {
                    float OffsetX = (CamTrans.position.x - transform.position.x) % SpriteSizeX;
                    transform.position = new Vector3(CamTrans.position.x + OffsetX, transform.position.y, deltaMovement.z);
                }
            }

            if (ScaleY)
            {
                if (Mathf.Abs(CamTrans.position.y - transform.position.y) >= SpriteSizeY)
                {
                    float OffsetY = (CamTrans.position.y - transform.position.y) % SpriteSizeY;
                    transform.position = new Vector3(CamTrans.position.x, transform.position.y + OffsetY, deltaMovement.z);
                }
            }
        }
    }
}
