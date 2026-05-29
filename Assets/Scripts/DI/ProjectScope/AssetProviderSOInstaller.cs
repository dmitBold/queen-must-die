using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

[CreateAssetMenu(fileName = "AssetProviderSOInstaller", menuName = "Installers/AssetProviderSOInstaller")]
public class AssetProviderSOInstaller : ScriptableObjectInstaller<AssetProviderSOInstaller>
{
    [SerializeField] private GlobalAudioConfig globalAudioConfig;
    [SerializeField] private PlayerView _playerPrefab;
    //TEST
    [SerializeField] private ItemDatabase _itemDatabase;
    //TEST


    public override void InstallBindings()
    {
        Container.BindInstance(globalAudioConfig).IfNotBound();
        Container.BindInstance(_playerPrefab).AsSingle();
        Container.BindInstance(_itemDatabase).AsSingle();   
    }
}
