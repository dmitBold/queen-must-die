using UnityEngine;

public class CandleController : MonoBehaviour
{
    public Light candleLight;   
    public GameObject fireEffect; 
    public MeshRenderer mesh;   

    private Material candleMaterial;

    void Awake()
    {
        candleMaterial = mesh.material;
    }

    public void SetCandleState(bool isLit)
    {
        candleLight.enabled = isLit;

        fireEffect.SetActive(isLit);

        if (isLit)
        {
            candleMaterial.EnableKeyword("_EMISSION");

        }
        else
        {
            candleMaterial.DisableKeyword("_EMISSION");
        }
    }

    [ContextMenu("Turn Off")] void TestOff() => SetCandleState(false);
    [ContextMenu("Turn On")] void TestOn() => SetCandleState(true);
}
