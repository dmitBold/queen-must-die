using UnityEngine;

public class Doc_paper : MonoBehaviour
{
    // Поле в инспекторе для выбора FMOD-ивента (сюда назначим Signature)
    public FMODUnity.EventReference SignatureEvent;

    // Этот метод вызовем из Animation Event
    public void PlaySignatureSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(SignatureEvent, transform.position);
    }
}