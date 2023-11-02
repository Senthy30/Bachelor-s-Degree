using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI fpsText = null;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    void Update() {
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime) {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameRate.ToString() + " FPS";
            time -= pollingTime;
            frameCount = 0;
        }
    }
}
