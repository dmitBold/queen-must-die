using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class NoteControllerInstaller : MonoInstaller
    {
        [SerializeField] NoteController noteController;
        public override void InstallBindings()
        {
            Container.Bind<NoteController>().FromInstance(noteController).AsSingle();
        }
    }
}