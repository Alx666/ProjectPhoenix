using UnityEngine;
using System.Collections;

public class HelicopterRotor : MonoBehaviour 
{
    ControllerPlayerHeli2 heli;
    float rotateDegree;
    float currentRPM;
    public float MaxRPM;

    void Awake()
    {
        currentRPM = 0.0f;
        heli = this.GetComponentInParent<ControllerPlayerHeli2>();
        MaxRPM = heli.MaxHeight * 500;
    }

    void Update()
    {
        Rotate(currentRPM);

        if (!heli.isGrounded && currentRPM <= MaxRPM)
        {
            IncreaseRPM(MaxRPM / 5);
        }
        if (heli.isGrounded && currentRPM >= 0.0f)
        {
            DecreaseRPM(MaxRPM / 5);
        }
    }

    void Rotate(float rpm)
    {
        rpm += this.transform.localPosition.y;
        rotateDegree += rpm * Time.deltaTime;
        rotateDegree %= 360f;
        transform.localRotation = Quaternion.Euler(0f, rotateDegree, 0f);
    }

    public void IncreaseRPM(float toAdd)
    {
        currentRPM += toAdd * Time.deltaTime;
    }

    public void DecreaseRPM(float toDecrease)
    {
        currentRPM -= toDecrease * Time.deltaTime;
    }
}
