using UnityEngine;

public class AssemblySocket : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] ItemData requiredItem;
    [SerializeField] GameObject visualPart; // нога
    [SerializeField] GameObject MainPart; // главная нога
    //[SerializeField] Renderer highlightRenderer;
    Outline outline;

    bool isFilled;

    public bool IsFilled => isFilled;
    public ItemData RequiredItem => requiredItem;

    void Awake()
    {
        if (visualPart != null) {
            visualPart.SetActive(false);
            MainPart.SetActive(false);
        }
        outline = GetComponent<Outline>();
        //outline.enabled = false;
        SetHighlight(false);
    }

    public bool CanAccept(ItemData item)
    {
        return !isFilled && item == requiredItem;
    }

    public void Apply(ItemData item)
    {
        if (!CanAccept(item))
            return;

        isFilled = true;

        if (visualPart != null)
        {
            visualPart.SetActive(true);
            MainPart.SetActive(true);
        }

        SetHighlight(false);
    }

    public void SetHighlight(bool state)
    {
        outline.enabled = state;
    }
}
