using UnityEngine;
using UnityEngine.UI;

public class DragIconController : MonoBehaviour
{
    [SerializeField] Image itemIcon;
    [SerializeField] Image canApplyIcon;
    [SerializeField] Image cannotApplyIcon;
    //[SerializeField] DayCycleController controller;

    public void SetItem(Sprite icon)
    {
        itemIcon.sprite = icon;
    }

    public void SetResult(bool? canApply)
    {
        if (canApply == null)
        {
            canApplyIcon.gameObject.SetActive(false);
            cannotApplyIcon.gameObject.SetActive(false);
            return;
        }

        /*Color color1 = cannotApplyIcon.color;
        color1.a = 1f;
        cannotApplyIcon.color = color1;

        Color color2 = canApplyIcon.color;
        color2.a = 1f;
        canApplyIcon.color = color2;*/

        canApplyIcon.gameObject.SetActive(canApply.Value);
        cannotApplyIcon.gameObject.SetActive(!canApply.Value);
    }
}
