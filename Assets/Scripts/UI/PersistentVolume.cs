using UnityEngine;

public class PersistentVolume : MonoBehaviour
{
    private static PersistentVolume instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // ”ничтожаем дубликат, если вернулись в меню
        }
    }
}