using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Приглушает огонь FireIgnition, пока он находится в затемнённой DarknessZone.
    /// Работает где угодно: на стене, в руке и на полу, потому что зона ищется по текущей позиции огня.
    /// </summary>
    [RequireComponent(typeof(FireIgnition))]
    public class FireDarkness : MonoBehaviour
    {
        private const string ZoneLayerName = "Ignore Raycast";
        private const float ProbeRadius = 0.05f;

        private readonly Collider[] _hits = new Collider[8];
        private FireIgnition _fire;
        private int _zoneMask;

        private void Awake()
        {
            _fire = GetComponent<FireIgnition>();
            _zoneMask = LayerMask.GetMask(ZoneLayerName);
        }

        private void Update()
        {
            float level = 1f;
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, ProbeRadius, _hits, _zoneMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hits[i].TryGetComponent(out DarknessZone zone))
                {
                    level = Mathf.Min(level, zone.Level);
                }
            }

            _fire.SetDarkness(level);
        }

        private void OnDisable()
        {
            if (_fire != null)
            {
                _fire.SetDarkness(1f);
            }
        }
    }
}
