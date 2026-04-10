using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController Instance { get; private set; }

    public enum PlayerMode
    {
        FreeMovement,
        //ItemSelection,
        //Assembly,
        DoorState,
        Focused // общий режим
    }

    public PlayerMode CurrentMode { get; private set; } = PlayerMode.FreeMovement;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void SetMode(PlayerMode newMode)
    {
        CurrentMode = newMode;

        switch (newMode)
        {
            case PlayerMode.FreeMovement:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;


            //TEST TEST
            case PlayerMode.DoorState:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            //TEST TEST
            //case PlayerMode.InventoryView:
            /*case PlayerMode.ItemSelection:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case PlayerMode.Assembly:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;*/
            case PlayerMode.Focused:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    public bool CanMove()
    {
        return CurrentMode == PlayerMode.FreeMovement || CurrentMode == PlayerMode.DoorState;
    }

    public bool CanRotate()
    {
        return CurrentMode == PlayerMode.FreeMovement;
    }
}
