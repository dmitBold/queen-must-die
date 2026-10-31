using Core;
using UnityEngine;

namespace Choices
{
    [CreateAssetMenu(fileName = "EffectData", menuName = "Scriptable Objects/EffectData")]
    public class EffectData : ScriptableObject
    {
        public WorldState.Stats stat;
        public int value;
    }
}
