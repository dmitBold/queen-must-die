using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GlobalAudioConfigInstaller", menuName = "Installers/GlobalAudioConfigInstaller")]
public class GlobalAudioConfigInstaller : ScriptableObjectInstaller<GlobalAudioConfigInstaller>
{
    [SerializeField] private GlobalAudioConfig globalAudioConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(globalAudioConfig).IfNotBound();
    }
}
