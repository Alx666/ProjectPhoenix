using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu()]
public class CarPrefabConfiguration : ScriptableObject
{
    public GameObject CarAudioCurve;
    public GameObject WeaponPrefab;
    public GameObject ExplosionPrefab;
    public GameObject GameCameraPrefab;
    public GameObject InGameGUIPrefab;
    public GameObject LightPrefab;
    public GameObject StopLightPrefab;

    public ArmorType Armor;
    public AudioClip AccelHigh;
    public AudioClip DecelHigh;
    public AudioClip AccelLow;
    public AudioClip DecelLow;

    public float FrontWheelMass = 20.0f;
    public float FrontDampingRate = 0.25f;
    public float FrontSpring = 40000.0f;
    public float FrontDamper = 8000f;
    public float TargetPosition = 0.5f;
    public float FrontForwardFrictionExtSlip = 0.5f;
    public float FrontForwardFrictionExtVal = 1.5f;
    public float FrontForwardFrictionAsySlip = 5.0f;
    public float FrontForwardFrictionAsyVal = 0.1f;
    public float FrontForwardFrictionStiff = 1.5f;

    public float FrontSideFrictionExtSlip = 0.1f;
    public float FrontSideFrictionExtVal = 5.0f;
    public float FrontSideFrictionAsySlip = 100.0f;
    public float FrontSideFrictionAsyVal = 1.5f;
    public float FrontSideFrictionStiff = 1.5f;

    public float RearWheelMass = 20.0f;
    public float RearDampingRate = 0.25f;
    public float RearSpring = 30000.0f;
    public float RearDamper = 5000f;
    public float RearForwardFrictionExtSlip = 0.4f;
    public float RearForwardFrictionExtVal = 1.5f;
    public float RearForwardFrictionAsySlip = 5.0f;
    public float RearForwardFrictionAsyVal = 0.1f;
    public float RearForwardFrictionStiff = 1.5f;

    public float RearSideFrictionExtSlip = 0.1f;
    public float RearSideFrictionStiff = 5.0f;
    public float RearSideFrictionExtVal = 100.0f;
    public float RearSideFrictionAsySlip = 1.5f;
    public float RearSideFrictionAsyVal = 1.5f;

    public List<GameObject> CarsToSetUp;

    public bool DestroyOnFinish;
}
