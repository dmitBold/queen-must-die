using UnityEngine;

public class AnimDesynch : MonoBehaviour
{
    public Animator anim;

    public void SetRandomOffset(string paramName)
    {
        float randomValue = Random.Range(0f, 1f);
        anim.SetFloat(paramName, randomValue);
    }

}
