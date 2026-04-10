using System.Collections.Generic;
using UnityEngine;

public class ChoiceImpactUI : MonoBehaviour
{
    public enum ImpactVisualMode
    {
        IconOnly,
        ScaleOnly,
        IconAndScale
    }

    [SerializeField] ImpactVisualMode visualMode;
    [SerializeField] List<ImpactIcon> icons;
    [SerializeField] WorldState worldState;


    public void Show(Choice choice)
    {
        foreach (var icon in icons)
            icon.HideImmediate();

        foreach (var effect in choice.effects)
        {
            if (effect.type != CardEffectType.ModifyStat)
                continue;

            if (worldState.IsStatLocked(effect.stat))
                continue;   

            ImpactIcon icon = GetIcon(effect.stat);
            if (icon == null)
                continue;

            icon.Setup(effect.value, visualMode);
            icon.Show();
        }
    }

    public void Hide()
    {
        foreach (var icon in icons)
            //icon.Hide();
            icon.HideImmediate();
    }

    public void Hide_Anim()
    {
        foreach (var icon in icons)
            icon.Hide();
            //icon.HideImmediate();
    }

    ImpactIcon GetIcon(WorldState.Stats stat)
    {
        foreach (var icon in icons)
        {
            if (icon.Stat == stat)
                return icon;
        }
        return null;
    }
}
