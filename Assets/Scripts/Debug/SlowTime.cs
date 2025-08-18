using UnityEngine;

public class SlowTime : MonoBehaviour
{
    public bool isSlowMotionActive = false;
    [Range(0f, 1f)]
    public float slowMotionScale = 0.5f;
    
    void Update()
    {
        Time.timeScale = isSlowMotionActive ? slowMotionScale : 1f;
    }
}