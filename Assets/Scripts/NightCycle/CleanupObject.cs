using System.Collections.Generic;
using UnityEngine;

public class CleanupObject : MonoBehaviour
{
    [Header("Settings")]
    //[SerializeField] private GameObject cleanupEffect; // Префаб частиц
    [SerializeField] private AudioClip cleanupSound;   // Звук уборки
    [SerializeField] private GameObject cleanObject;

    [SerializeField] CleanupStage parentStage;

    [SerializeField] private GameObject clear_GROUP;


    public void Clean()
    {
        if (cleanupSound != null)
        {
            SoundManager.Instance.PlaySound(cleanupSound);
        }

        //Эффект
        //if (cleanupEffect != null)
        //{
          //  Instantiate(cleanupEffect, transform.position, Quaternion.identity);
        //}

        if (parentStage != null)
        {
            parentStage.OnObjectCleaned();
        }

        gameObject.SetActive(false);
        if(clear_GROUP != null){
            clear_GROUP.SetActive(false);
        }
        cleanObject.SetActive(true);
    }
}