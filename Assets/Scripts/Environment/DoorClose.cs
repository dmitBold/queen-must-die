using UnityEngine;
using System.Collections;

namespace NightCycle
{
    [RequireComponent(typeof(Rigidbody), typeof(HingeJoint))]
    public class DoorCloser : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("—корость автоматического закрыти€ двери")]
        [SerializeField] private float custom_rotation_open = 90f;

        [SerializeField] private float closeSpeed = 5f;
        [SerializeField] private float openSpeed = 5f;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private Rigidbody _rb;
        private HingeJoint _hinge;
        private bool _isClosing = false;
        private bool _isOpening = false;

        private void Awake()
        {
            // Ќа старте запоминаем текущую ротацию как "закрытое положение"
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, custom_rotation_open, 0f);
            _rb = GetComponent<Rigidbody>();
            _hinge = GetComponent<HingeJoint>();
        }

        public void CloseDoorEvent()
        {
            if (!_isClosing)
            {
                StartCoroutine(CloseRoutine());
            }
        }

        public void OpenDoorEvent()
        {
            if (!_isOpening)
            {
                StartCoroutine(OpenRoutine());
            }
        }

        public void de_lock()
        {
            // 1. Ќа вс€кий случай останавливаем корутину закрыти€, если вдруг разблокировали дверь во врем€ анимации
            StopAllCoroutines();
            _isClosing = false;

            // 2. ¬озвращаем физике контроль над объектом
            _rb.isKinematic = false;

            // 3. ќЅя«ј“≈Ћ№Ќќ: сбрасываем инерцию. 
            // Ѕез этого при отключении isKinematic дверь может "выстрелить" или забаговатьс€
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            // 4. ќЅя«ј“≈Ћ№Ќќ: принудительно будим физику. 
            // Ёто лечит залипание двери в крайних положени€х.
            _rb.WakeUp();

            // 5. ќтключаем замок, чтобы твой DragDoor снова рисовал иконку руки, а не замка
            /*var lockDoor = GetComponent<LockDoor>();
            if (lockDoor != null)
            {
                lockDoor.enabled = false;
            }*/

            // ≈сли у теб€ на HingeJoint в инспекторе »«Ќј„јЋ№Ќќ сто€ла галочка Use Spring 
            // (чтобы дверь немного пружинила), раскомментируй строку ниже:
            // if (_hinge != null) _hinge.useSpring = true; 
        }

        private IEnumerator CloseRoutine()
        {
            _isClosing = true;

            if (_hinge != null)
            {
                _hinge.useMotor = false;
                _hinge.useSpring = false;
            }


            _rb.isKinematic = true;

            while (Quaternion.Angle(transform.rotation, _closedRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, _closedRotation, Time.deltaTime * closeSpeed);
                yield return null;
            }

            transform.rotation = _closedRotation;

            /*var lockDoor = GetComponent<LockDoor>();
            if (lockDoor != null)
            {
                lockDoor.enabled = true;
            }*/
        }

        private IEnumerator OpenRoutine()
        {
            _isOpening = true;

            if (_hinge != null)
            {
                _hinge.useMotor = false;
                _hinge.useSpring = false;
            }


            _rb.isKinematic = true;

            while (Quaternion.Angle(transform.rotation, _openRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, _openRotation, Time.deltaTime * openSpeed);
                yield return null;
            }

            transform.rotation = _openRotation;

        }
    }
}