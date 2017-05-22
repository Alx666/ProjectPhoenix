using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;

public class CarPrefabCreationTool : EditorWindow
{
    //public scriptable object to get references from
    public static CarPrefabConfiguration Preset;

    private static GameObject CarAudioCurve;
    private static GameObject WeaponPrefab;
    private static GameObject ExplosionPrefab;
    private static GameObject Wheel_FR_model;
    private static GameObject Wheel_FL_model;
    private static GameObject Wheel_RR_model;
    private static GameObject Wheel_RL_model;
    private static GameObject FrontRightWheel_model;
    private static GameObject FrontLeftWheel_model;
    private static GameObject RearRightWheel_model;
    private static GameObject RearLeftWheel_model;

    private static Transform[] WeaponLocatorsPosition;
    private static Transform GunTransform;

    private static NetworkTransform NetTransform;
    private static NetworkTransformChild NetTransformChild;

    private static ControllerWheels ControllerWheels;
    private static Weapon Weapon;
    private static MadMaxCarAudio CarAudio;
    private static MadMaxActor MadMaxActor;
    private static VehicleTurret Turret;

    private static AudioClip AccelHigh;
    private static AudioClip DecelHigh;
    private static AudioClip AccelLow;
    private static AudioClip DecelLow;

    private static AnimationCurve EngineAudioCurve;
    private static AnimationCurve RolloffAudioCurve;

    private static JointSpring GenericSpring;
    private static WheelFrictionCurve FrictionCurve;

    private static ArmorType Armor;

    [MenuItem("DRAIVTools/CarPrefabConfiguration")]
    private static void OpenWindow()
    {
        CarPrefabCreationTool Wnd = EditorWindow.GetWindow<CarPrefabCreationTool>();
    }

    private void OnGUI()
    {
        Preset = (CarPrefabConfiguration)EditorGUILayout.ObjectField("WeaponReference", Preset, typeof(CarPrefabConfiguration), false);

        if (GUILayout.Button("GenerateCarPrefabs"))
        {
            if (Preset != null)
                GenerateCarPrefab();
            else
                Debug.Log("Please specify an import tool preset file!!!");
        }
    }


    static void GenerateCarPrefab()
    {
        //getting references from scriptable object
        WeaponPrefab = Preset.WeaponPrefab;
        ExplosionPrefab = Preset.ExplosionPrefab;
        CarAudioCurve = Preset.CarAudioCurve;
        AccelHigh = Preset.AccelHigh;
        AccelLow = Preset.AccelLow;
        DecelHigh = Preset.DecelHigh;
        DecelLow = Preset.DecelLow;
        Armor = Preset.Armor;

        //getting audio curves
        MadMaxCarAudio tempCarAudio = CarAudioCurve.GetComponent<MadMaxCarAudio>();
        EngineAudioCurve = tempCarAudio.Curve;
        RolloffAudioCurve = tempCarAudio.RolloffCurve;

        //creating JointSpring data struct
        //targetPosition it's the same for every wheel for now...
        GenericSpring = new JointSpring();
        GenericSpring.targetPosition = Preset.TargetPosition;

        //creating WheelFrictionCurve data struct
        FrictionCurve = new WheelFrictionCurve();


        //getting all the cars to setup and prefab
        GameObject[] Objects = Preset.CarsToSetUp.ToArray();

        foreach (var obj in Objects)
        {
            GameObject tempCar = GameObject.Instantiate(obj);

            //adding and getting all components and gameobjects
            ControllerWheels = tempCar.AddComponent<ControllerWheels>();
            Weapon = tempCar.AddComponent<Weapon>();
            NetTransform = tempCar.AddComponent<NetworkTransform>();
            NetTransformChild = tempCar.AddComponent<NetworkTransformChild>();
            MadMaxActor = tempCar.AddComponent<MadMaxActor>();
            CarAudio = tempCar.AddComponent<MadMaxCarAudio>();

            Turret = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Turret").Select(y => y.gameObject.AddComponent<VehicleTurret>()).FirstOrDefault();
            WeaponLocatorsPosition = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "WeaponLocator").ToArray();
            GunTransform = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Gun").FirstOrDefault();
            FrontRightWheel_model = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Wheel_FR").Select(y => y.gameObject).FirstOrDefault();
            FrontLeftWheel_model = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Wheel_FL").Select(y => y.gameObject).FirstOrDefault();
            RearRightWheel_model = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Wheel_RR").Select(y => y.gameObject).FirstOrDefault();
            RearLeftWheel_model = tempCar.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name == "Wheel_RL").Select(y => y.gameObject).FirstOrDefault();

            //starting all setup methods
            SetupRigidBody(tempCar);
            SetupWeapon();
            SetupControllerWheels();
            SetupNetworkIdentity(tempCar);
            SetupNetworkTransform();
            SetupNetworkTransformChild();
            SetupMadMaxActor();
            SetupMadMaxCarAudio(tempCar);
            SetUpTurret();
            SetupWheelColliders(tempCar);
            SavePrefab(tempCar);
        }
    }

    private static void SetupRigidBody(GameObject obj)
    {
        //rigidbody was automatically added by ControllerWheels script
        Rigidbody rBody = obj.GetComponent<Rigidbody>();
        if (rBody != null)
        {
            rBody.mass = 1850.0f;
            rBody.drag = 0.08f;
            rBody.angularDrag = 0.6f;
        }
    }

    private static void SetupWeapon()
    {
        Weapon.BulletPrefab = WeaponPrefab;
        //TODO: add all weapon locators
        Weapon.ShootLocators = new List<GameObject>(WeaponLocatorsPosition.Length);
        for (int i = 0; i < WeaponLocatorsPosition.Length; i++)
        {
            Weapon.ShootLocators.Add(WeaponLocatorsPosition[i].gameObject);
        }
    }

    private static void SetupControllerWheels()
    {
        ControllerWheels.Hp = 10000.0f;
        ControllerWheels.MaxSpeed = 200.0f;
    }

    private static void SetupNetworkIdentity(GameObject obj)
    {
        obj.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
    }

    private static void SetupNetworkTransform()
    {
        NetTransform.sendInterval = 0.07f;
        NetTransform.movementTheshold = 0.01f;
        NetTransform.snapThreshold = 12.0f;
        NetTransform.interpolateMovement = 0.5f;
        NetTransform.interpolateRotation = 20.0f;
        NetTransform.rotationSyncCompression = NetworkTransform.CompressionSyncMode.High;
        NetTransform.syncSpin = true;
    }

    private static void SetupNetworkTransformChild()
    {
        NetTransformChild.sendInterval = 0.1f;
        NetTransformChild.target = Turret.gameObject.transform;
        NetTransformChild.movementThreshold = 0.01f;
        NetTransformChild.syncRotationAxis = NetworkTransform.AxisSyncMode.AxisY;
        NetTransformChild.rotationSyncCompression = NetworkTransform.CompressionSyncMode.High;
    }

    private static void SetupMadMaxActor()
    {
        MadMaxActor.HpToSet = 100.0f;
        MadMaxActor.Armor = Armor;
        MadMaxActor.HPBarOffset = 4.0f;
        MadMaxActor.HpBarLerp = 100.0f;
        MadMaxActor.HpBarScale = 0.017f;
        MadMaxActor.DeathExplosionPrefab = ExplosionPrefab;
    }

    private static void SetupMadMaxCarAudio(GameObject obj)
    {
        CarAudio.HighAccelClip = AccelHigh;
        CarAudio.HighDecelClip = DecelHigh;
        CarAudio.LowAccelClip = AccelLow;
        CarAudio.LowDecelClip = DecelLow;
        CarAudio.HighPitchMultiplier = 0.5f;
        CarAudio.MaxRolloffDistance = 250.0f;
        CarAudio.Curve = EngineAudioCurve;
        CarAudio.RolloffCurve = RolloffAudioCurve;
    }

    private static void SetUpTurret()
    {
        Turret.AxeY = Turret.transform;
        Turret.AxeX = GunTransform;
    }

    private static void SetupWheelColliders(GameObject obj)
    {
        //creating Wheel Colliders GameObjects
        GameObject WheelC_FR = new GameObject("WheelC_FR");
        GameObject WheelC_FL = new GameObject("WheelC_FL");
        GameObject WheelC_RR = new GameObject("WheelC_RR");
        GameObject WheelC_RL = new GameObject("WheelC_RL");

        //parenting Wheel Colliders to Car GO
        WheelC_FR.transform.SetParent(obj.transform);
        WheelC_FL.transform.SetParent(obj.transform);
        WheelC_RR.transform.SetParent(obj.transform);
        WheelC_RL.transform.SetParent(obj.transform);

        //adding Sphere Colliders in order to calculate Wheels radius
        SphereCollider SphereC_FR = FrontRightWheel_model.AddComponent<SphereCollider>();
        SphereCollider SphereC_FL = FrontLeftWheel_model.AddComponent<SphereCollider>();
        SphereCollider SphereC_RR = RearRightWheel_model.AddComponent<SphereCollider>();
        SphereCollider SphereC_RL = RearLeftWheel_model.AddComponent<SphereCollider>();

        //adding Whell Collider components
        WheelCollider ColliderFR = WheelC_FR.AddComponent<WheelCollider>();
        WheelCollider ColliderFL = WheelC_FL.AddComponent<WheelCollider>();
        WheelCollider ColliderRR = WheelC_RR.AddComponent<WheelCollider>();
        WheelCollider ColliderRL = WheelC_RL.AddComponent<WheelCollider>();

        //setting up position with offset in order to align both WheelColliders and Wheel meshes
        WheelC_FR.transform.position = FrontRightWheel_model.transform.position + new Vector3(0.0f, ColliderFR.suspensionDistance / 2.0f, 0.0f);
        WheelC_FL.transform.position = FrontLeftWheel_model.transform.position + new Vector3(0.0f, ColliderFR.suspensionDistance / 2.0f, 0.0f);
        WheelC_RR.transform.position = RearRightWheel_model.transform.position + new Vector3(0.0f, ColliderFR.suspensionDistance / 2.0f, 0.0f);
        WheelC_RL.transform.position = RearLeftWheel_model.transform.position + new Vector3(0.0f, ColliderFR.suspensionDistance / 2.0f, 0.0f);

        //ColliderFR setup
        ColliderFR.mass = Preset.FrontWheelMass;
        ColliderFR.radius = SphereC_FR.radius;
        ColliderFR.wheelDampingRate = Preset.FrontDampingRate;

        GenericSpring.spring = Preset.FrontSpring;
        GenericSpring.damper = Preset.FrontDamper;
        ColliderFR.suspensionSpring = GenericSpring;

        FrictionCurve.extremumSlip = Preset.FrontForwardFrictionExtSlip;
        FrictionCurve.extremumValue = Preset.FrontForwardFrictionExtVal;
        FrictionCurve.asymptoteSlip = Preset.FrontForwardFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.FrontForwardFrictionAsyVal;
        ColliderFR.forwardFriction = FrictionCurve;

        FrictionCurve.extremumSlip   = Preset.FrontSideFrictionExtSlip;
        FrictionCurve.extremumValue  = Preset.FrontSideFrictionExtVal;
        FrictionCurve.asymptoteSlip  = Preset.FrontSideFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.FrontSideFrictionAsyVal;
        ColliderFR.sidewaysFriction  = FrictionCurve;

        //ColliderFL setup
        ColliderFL.mass = Preset.FrontWheelMass;
        ColliderFL.radius = SphereC_FL.radius;
        ColliderFL.wheelDampingRate = Preset.FrontDampingRate;

        GenericSpring.spring = Preset.FrontSpring;
        GenericSpring.damper = Preset.FrontDamper;
        ColliderFL.suspensionSpring = GenericSpring;

        FrictionCurve.extremumSlip = Preset.FrontForwardFrictionExtSlip;
        FrictionCurve.extremumValue = Preset.FrontForwardFrictionExtVal;
        FrictionCurve.asymptoteSlip = Preset.FrontForwardFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.FrontForwardFrictionAsyVal;
        ColliderFL.forwardFriction = FrictionCurve;

        FrictionCurve.extremumSlip = Preset.FrontSideFrictionExtSlip;
        FrictionCurve.extremumValue = Preset.FrontSideFrictionExtVal;
        FrictionCurve.asymptoteSlip = Preset.FrontSideFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.FrontSideFrictionAsyVal;
        ColliderFL.sidewaysFriction = FrictionCurve;

        //ColliderRR setup
        ColliderRR.mass = Preset.RearWheelMass;
        ColliderRR.radius = SphereC_RR.radius;
        ColliderRR.wheelDampingRate = Preset.RearDampingRate;
        
        GenericSpring.spring        = Preset.RearSpring;
        GenericSpring.damper        = Preset.RearDamper;
        ColliderRR.suspensionSpring = GenericSpring;
        
        FrictionCurve.extremumSlip   = Preset.RearForwardFrictionExtSlip;
        FrictionCurve.extremumValue  = Preset.RearForwardFrictionExtVal;
        FrictionCurve.asymptoteSlip  = Preset.RearForwardFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.RearForwardFrictionAsyVal;
        ColliderRR.forwardFriction   = FrictionCurve;

        FrictionCurve.extremumSlip   = Preset.RearSideFrictionExtSlip;
        FrictionCurve.extremumValue  = Preset.RearSideFrictionExtVal;
        FrictionCurve.asymptoteSlip  = Preset.RearSideFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.RearSideFrictionAsyVal;
        ColliderRR.sidewaysFriction  = FrictionCurve;

        //ColliderRL setup
        ColliderRL.mass = Preset.RearWheelMass;
        ColliderRL.radius = SphereC_RL.radius;
        ColliderRL.wheelDampingRate = Preset.RearDampingRate;
        
        GenericSpring.spring = Preset.RearSpring;
        GenericSpring.damper = Preset.RearDamper;
        ColliderRL.suspensionSpring = GenericSpring;

        FrictionCurve.extremumSlip = Preset.RearForwardFrictionExtSlip;
        FrictionCurve.extremumValue = Preset.RearForwardFrictionExtVal;
        FrictionCurve.asymptoteSlip = Preset.RearForwardFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.RearForwardFrictionAsyVal;
        ColliderRL.forwardFriction = FrictionCurve;

        FrictionCurve.extremumSlip = Preset.RearSideFrictionExtSlip;
        FrictionCurve.extremumValue = Preset.RearSideFrictionExtVal;
        FrictionCurve.asymptoteSlip = Preset.RearSideFrictionAsySlip;
        FrictionCurve.asymptoteValue = Preset.RearSideFrictionAsyVal;
        ColliderRL.sidewaysFriction = FrictionCurve;

        //removing sphere colliders from Wheel models
        DestroyImmediate(SphereC_FR);
        DestroyImmediate(SphereC_FL);
        DestroyImmediate(SphereC_RR);
        DestroyImmediate(SphereC_RL);
    }

    private static void SavePrefab(GameObject obj)
    {
        string FileLocation = "Assets/Raw/_MadMaxArena/Prefabs/Cars/Game1617/" + obj.name + ".prefab";
        

        Object prefab = PrefabUtility.CreateEmptyPrefab(FileLocation);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }

}
