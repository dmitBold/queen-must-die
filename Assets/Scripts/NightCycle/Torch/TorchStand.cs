using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Настенный факел, который можно снять. После снятия на стене остаётся пустой держатель.
    /// </summary>
    [RequireComponent(typeof(Interactable), typeof(FireIgnition), typeof(MeshFilter))]
    public class TorchStand : MonoBehaviour
    {
        /// <summary>Факел, который появляется в момент снятия.</summary>
        [SerializeField] private TorchItem _torchPrefab;

        /// <summary>Меш держателя без факела.</summary>
        [SerializeField] private Mesh _emptyMesh;

        private PlayerTorch _player;
        private DiContainer _container;
        private Interactable _interactable;
        private FireIgnition _fire;
        private MeshFilter _meshFilter;
        private Collider _collider;
        private bool _taken;

        [Inject]
        private void Construct(PlayerTorch player, DiContainer container)
        {
            _player = player;
            _container = container;
        }

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            _fire = GetComponent<FireIgnition>();
            _meshFilter = GetComponent<MeshFilter>();
            _collider = GetComponent<Collider>();
            _interactable.onInteraction.AddListener(OnInteract);
        }

        // Огни холла включает HallGuide через FireIgnition, события о каждом переключении нет.
        private void Update()
        {
            if (!_taken) _interactable.enabled = _fire.IsLit;
        }

        private void OnInteract()
        {
            if (_taken || !_fire.IsLit || !_player.CanTake) return;

            var torch = _container.InstantiatePrefabForComponent<TorchItem>(_torchPrefab, transform.position, transform.rotation, null);
            torch.transform.localScale = transform.lossyScale;

            _taken = true;
            _fire.SetLit(false);
            _meshFilter.sharedMesh = _emptyMesh;
            _interactable.enabled = false;
            if (_collider != null) _collider.enabled = false;

            _player.Take(torch, -transform.forward);
        }
    }
}
