using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    static List<CinemachineCamera> cameras = new List<CinemachineCamera>();

    public static CinemachineCamera ActiveCamera = null;

    public static bool IsActiveCamera(CinemachineCamera camera)
    {
        return ActiveCamera == camera;
    }

    public static void RegisterCamera(CinemachineCamera camera)
    {
        if (!cameras.Contains(camera))
        {
            cameras.Add(camera);
        }
    }

    public static void UnregisterCamera(CinemachineCamera camera)
    {
        if (cameras.Contains(camera))
        {
            cameras.Remove(camera);
        }
    }

    public static void SwitchCamera(CinemachineCamera newCamera)
    {
        newCamera.Priority = 10;
        ActiveCamera = newCamera;

        foreach (CinemachineCamera cam in cameras)
        {
            if (cam != ActiveCamera)
            {
                cam.Priority = 0;
            }
        }
    }
}
