using UnityEngine;

[CreateAssetMenu(menuName = "UI/Stat Impact Visuals")]
public class StatImpactVisualData : ScriptableObject
{
    public WorldState.Stats stat;

    public Sprite positiveSmall;
    public Sprite positiveMedium;
    public Sprite positiveLarge;

    public Sprite negativeSmall;
    public Sprite negativeMedium;
    public Sprite negativeLarge;

    public enum ImpactSize
    {
        Small,
        Medium,
        Large
    }

    public Sprite GetSprite(int value, ImpactSize size)
    {
        bool positive = value > 0;

        return (positive, size) switch
        {
            (true, ImpactSize.Small) => positiveSmall,
            (true, ImpactSize.Medium) => positiveMedium,
            (true, ImpactSize.Large) => positiveLarge,
            (false, ImpactSize.Small) => negativeSmall,
            (false, ImpactSize.Medium) => negativeMedium,
            (false, ImpactSize.Large) => negativeLarge,
            _ => null
        };
    }
}
