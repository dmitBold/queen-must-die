using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODCleaningSound : MonoBehaviour
{
    private EventInstance CleaningInstance;

    private static int Barrelcount = 0;
    private static int Dishescount = 0;
    private static int Cratecount = 0;
    private static int Bookscount = 0;


    void Barrel_1()
    { 
        
        // FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Barrel_1", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Barrel_1");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);
      
        if(Barrelcount == 0)
        {
            Barrelcount++;
            CleaningInstance.start();          
            CleaningInstance.release();
        }
        else
        {
            Barrelcount--;
        }
        
    }
    void Barrel_2()
    {
        // RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Barrel_2", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Barrel_2");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Barrelcount == 1)
        {
            Barrelcount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Barrelcount--;
        }

    }
    void Dishes_1()
    {
        //  RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Dishes_1", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Dishes_1");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Dishescount == 0)
        {
            Dishescount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Dishescount--;
        }
    }
    void Dishes_2()
    {
        // RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Dishes_2", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Dishes_2");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Dishescount == 1)
        {
            Dishescount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Dishescount--;
        }
    }
    void Dishes_3()
    {
        //  FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Dishes_3", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Dishes_3");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Dishescount == 2)
        {
            Dishescount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Dishescount--;
        }
    }
    void Crate_1()
    {
        //  FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Crate_1", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Crate_1");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);
       

        if (Cratecount == 0)
        {
            Cratecount++;
            CleaningInstance.start();
            CleaningInstance.release();
           
        }
        else
        {
            Cratecount--;
           
        }
    }
    void Crate_2()
    {
        // FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Crate_2", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Crate_2");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);
        

        if (Cratecount == 1)
        {
            Cratecount++;
            CleaningInstance.start();
            CleaningInstance.release();
           
        }
        else
        {
            Cratecount--;
            
        }
    }
    void Books_1()
    {
        //  FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Books_1", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Books_1");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Bookscount == 0)
        {
            Bookscount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Bookscount--;
        }
    }
    void Books_2()
    {
        //  FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Books_2", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Books_2");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Bookscount == 1)
        {
            Bookscount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Bookscount--;
        }
    }
    void Books_3()
    {
        //  FMODUnity.RuntimeManager.PlayOneShot("event:/Scenes/Hom/Cleaning/Books_3", gameObject.transform.position);
        CleaningInstance = RuntimeManager.CreateInstance("event:/Scenes/Hom/Cleaning/Books_3");
        RuntimeManager.AttachInstanceToGameObject(CleaningInstance, transform);

        if (Bookscount == 2)
        {
            Bookscount++;
            CleaningInstance.start();
            CleaningInstance.release();
        }
        else
        {
            Bookscount--;
        }
    }

}
