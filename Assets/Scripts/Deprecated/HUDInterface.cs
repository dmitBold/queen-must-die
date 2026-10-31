using NightCycle;
using UnityEngine;

public class HUDInterface : MonoBehaviour
{

    public HUDController controller;

    void Start()
    {
        controller = HUDController.instance;
    }

    public void DisableHUD()
    {
        controller.SetCrosshairActivity(false);
    }

    public void EnableHUD()
    {
        controller.SetCrosshairActivity(false);
    }

}
