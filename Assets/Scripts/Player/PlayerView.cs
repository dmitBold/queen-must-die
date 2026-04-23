using NightCycle;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private PlayerInteraction _playerInteraction;
    public PlayerInteraction PlayerInteraction => _playerInteraction;

    [SerializeField] private PlayerStateController _playerStateController;
    public PlayerStateController PlayerStateController => _playerStateController;

    [SerializeField] private FirstPersonController _fpc;
    public FirstPersonController FirstPersonController => _fpc;

    [SerializeField] private GameObject _lantern;
    
    
    public void SetLanternActivity(bool value)
    {
        _lantern.SetActive(value);
    }
}