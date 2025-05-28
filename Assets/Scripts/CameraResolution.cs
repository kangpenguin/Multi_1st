using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    private void Awake()
    {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;
        float scaleHeight = (float)Screen.width / Screen.height / ((float)16 / 9); // 가로 / 세로
        float scaleWidth = 1f / scaleHeight;

        Screen.sleepTimeout = SleepTimeout.NeverSleep; //가만히 둬도 안꺼진다.
        //Screen.SetResolution(1280, 720, true); //풀 스크린 여부는 false

        if (scaleHeight < 1)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }
        camera.rect = rect;
    }
}