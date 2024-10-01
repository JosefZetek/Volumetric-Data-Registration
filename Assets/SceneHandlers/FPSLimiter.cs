using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{

    public int FPS_Limit = 60;
    private void LimitFPS()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FPS_Limit;
    }

    private void Awake()
    {
        LimitFPS();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Application.targetFrameRate != FPS_Limit)
        //    LimitFPS();
    }
}
