using UnityEngine;

public class PenWritingSound : MonoBehaviour
{
    // ѕоле в инспекторе дл€ выбора FMOD-ивента (сюда назначим Signature)
    public FMODUnity.EventReference SignatureEvent;

    // Ётот метод вызовем из Animation Event на клипе feather_writing
    public void PlaySignatureSound()
    {
        // PlayOneShot Ч одноразовый проигрыш ивента
        // transform.position Ч 3D-позици€ объекта pen, на котором висит анимаци€
        FMODUnity.RuntimeManager.PlayOneShot(SignatureEvent, transform.position);
    }
}