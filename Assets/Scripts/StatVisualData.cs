using UnityEngine;

[CreateAssetMenu(fileName = "StatVisualData", menuName = "UI/StatVisualData")]
public class StatVisualData : ScriptableObject
{
    public WorldState.Stats stat;
    public Sprite icon;
    public Color fillColor = Color.white;
    public Color backgroundColor = Color.gray;

    //test
    public Sprite lockedIcon;
    public Color lockColor = Color.white;
    public RuntimeAnimatorController lockAnimator;
    //test

}
