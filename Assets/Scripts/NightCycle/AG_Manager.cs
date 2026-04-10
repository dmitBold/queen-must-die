using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AG_Manager : MonoBehaviour
{
    public List<CandleController> FirePlaces;
    public float FireTime;
    public float FirstDelay;

    IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(FirstDelay);

        for (int i = 0; i < FirePlaces.Count; i += 2)
        {
            yield return new WaitForSeconds(FireTime);

            FirePlaces[i].SetCandleState(false);
            if (i + 1 < FirePlaces.Count)
            {
                FirePlaces[i + 1].SetCandleState(false);
            }
        }
    }

    public void TurnOffFIRE()
    {
        StartCoroutine(FireRoutine());
    }

}
