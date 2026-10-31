using UnityEngine;

namespace NightCycle
{
    public class Chase_Statue_Help : MonoBehaviour
    {

        public void StatueChase()
        {
            Debug.Log("StatueChase");
            StatueController[] Statues = Object.FindObjectsByType<StatueController>(FindObjectsSortMode.None);

            foreach (StatueController statue in Statues)
            {
                statue.advance_pose();
            }

        }

    }
}
