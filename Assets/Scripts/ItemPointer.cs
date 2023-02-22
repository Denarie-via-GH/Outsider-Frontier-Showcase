using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Personal.Utils;
public class ItemPointer : MonoBehaviour
{
    [SerializeField] private Camera uiCamera;
    public Vector3 targetPosition;
    public RectTransform pointerRect;
    public Image PointerImage;
    public Sprite Mark;
    public Sprite Point;

    public void Initiate(Vector3 target)
    {
        uiCamera = CameraFollow.Instance.GetComponent<Camera>();
        targetPosition = target;
        pointerRect = transform.Find("Pointer").GetComponent<RectTransform>();
        PointerImage = pointerRect.GetComponent<Image>();
    }

    private void Update()
    {
        Vector3 toPosition = targetPosition;
        Vector3 fromPosition = uiCamera.transform.position;
        fromPosition.z = 0f;
        Vector3 dir = (toPosition - fromPosition).normalized;
        float angle = Utility.GetAngleFromVectorFloat(dir);
        pointerRect.localEulerAngles = new Vector3(0, 0, angle);

        
        float borderSize = 100f;
        Vector3 targetPositionScreenPoint = uiCamera.WorldToScreenPoint(targetPosition);
        bool isOffScreen = targetPositionScreenPoint.x <= borderSize || targetPositionScreenPoint.x >= Screen.width - borderSize || targetPositionScreenPoint.y <= borderSize || targetPositionScreenPoint.y >= Screen.height - borderSize;
        
        if (isOffScreen)
        {
            PointerImage.sprite = Point;
            Vector3 cappedTargetScreenPOS = targetPositionScreenPoint;
            if (cappedTargetScreenPOS.x <= borderSize)
                cappedTargetScreenPOS.x = borderSize;
            if (cappedTargetScreenPOS.x >= Screen.width - borderSize)
                cappedTargetScreenPOS.x = Screen.width - borderSize;

            if (cappedTargetScreenPOS.y <= borderSize)
                cappedTargetScreenPOS.y = borderSize;
            if(cappedTargetScreenPOS.y >= Screen.height - borderSize)
                cappedTargetScreenPOS.y = Screen.height - borderSize;

            //Vector3 pointerWorldPOS = uiCamera.ScreenToWorldPoint(cappedTargetScreenPOS);
            pointerRect.position = cappedTargetScreenPOS;
            pointerRect.localPosition = new Vector3(pointerRect.localPosition.x,pointerRect.localPosition.y, 0);
            
        }
        else
        {
            PointerImage.sprite = Mark;
            pointerRect.position = targetPositionScreenPoint;
            pointerRect.localPosition = new Vector3(pointerRect.localPosition.x, pointerRect.localPosition.y, 0);
        }
        
    }
}
