using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GlowInterval : MonoBehaviour
{
    public float Delay;
    public float startPoint;
    public float endPoint;
    public bool reversed;
    public UnityEngine.Rendering.Universal.Light2D target;

    public bool ACTIVE = false;
    public float elapsedTime;
    public float duration;

    public void Start()
    {
        Invoke("Initiate", Delay);
    }
    private void Initiate()
    {
        ACTIVE = true;
    }
    void Update()
    {
        if (ACTIVE)
        {
            if (!reversed)
            {
                elapsedTime += Time.deltaTime;
                float distancePercent = elapsedTime / duration;

                target.intensity = Mathf.Lerp(startPoint, endPoint, distancePercent);
                if (target.intensity >= endPoint)
                {
                    elapsedTime = 0;
                    target.intensity = endPoint;
                    reversed = true;
                }
            }
            else if (reversed)
            {
                elapsedTime += Time.deltaTime;
                float distancePercent = elapsedTime / duration;

                target.intensity = Mathf.Lerp(endPoint, startPoint, distancePercent);
                if (target.intensity <= startPoint)
                {
                    elapsedTime = 0;
                    target.intensity = startPoint;
                    reversed = false;
                }
            }
        }
    }
}
