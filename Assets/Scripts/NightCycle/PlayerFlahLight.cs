using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    public static PlayerFlashlight instance;

    [SerializeField] Light flashlight;
    [SerializeField] GameObject model;
    float baseIntensity = 1.0f;

    [SerializeField] float swayAmount = 0.04f;
    [SerializeField] float swaySmooth = 8f;

    Vector3 initialLocalPos;


    void Awake()
    {
        instance = this;
        Disable();
        initialLocalPos = model.transform.localPosition;
        baseIntensity = flashlight.intensity;
    }

    public void Enable()
    {
        model.SetActive(true);
        //initialLocalPos = model.transform.localPosition;
        flashlight.enabled = true;
    }

    public void Disable()
    {
        model.SetActive(false);
        flashlight.enabled = false;
    }
    private void Update()
    {
        if (!flashlight.enabled) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        //model.transform.localPosition = initialLocalPos;
        Vector3 targetPos = initialLocalPos +
             new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0f);

         model.transform.localPosition =
             Vector3.Lerp(model.transform.localPosition, targetPos, Time.deltaTime * swaySmooth);

        flashlight.intensity = baseIntensity + Mathf.Sin(Time.time * 2f) * 0.5f*baseIntensity;
    }

}
