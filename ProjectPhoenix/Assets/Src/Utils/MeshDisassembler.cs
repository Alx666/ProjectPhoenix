using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshDisassembler : MonoBehaviour
{
    public float MinExplosionForce = 1f;
    public float MaxExplosionForce = 10f;
    public float MassForeachElement = 1f;
    public bool RootColliderAlwaysOn;

    private Rigidbody rootRigidbody;
    private Collider rootCollider;
    private Vector3 lastPosition;

    private List<GameObject> targets;
    private Dictionary<GameObject, AssemblerInfo> targetsInfo;
    private List<Collider> remainingColliders;
    private bool isAssembled;

    void Awake()
    {
        isAssembled = true;
        rootRigidbody = this.GetComponent<Rigidbody>();
        targetsInfo = new Dictionary<GameObject, AssemblerInfo>();
        targets = this.GetComponentsInChildren<Transform>().Select(hT => hT.gameObject).Where(GO => GO.GetComponent<MeshRenderer>() != null).ToList();
        remainingColliders = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<MeshRenderer>() == null).Select(GO => GO.GetComponent<Collider>()).Where(hC => hC != null && hC as WheelCollider == null).ToList();

        rootCollider = this.GetComponent<Collider>();

        targets.ForEach(hT =>
       {
           Transform parent = hT.transform.parent;
           Vector3 position = hT.transform.localPosition;
           Quaternion rotation = hT.transform.localRotation;
           Vector3 scale = hT.transform.localScale;

           MeshCollider collider = hT.GetComponent<MeshCollider>();
           bool hasCollider = false;
           if (collider == null)
           {
               collider = hT.AddComponent<MeshCollider>();
               collider.convex = true;
               collider.enabled = false;
           }
           else
           {
               hasCollider = true;
           }

           bool hasRigidbody = hT.GetComponent<Rigidbody>() != null;

           targetsInfo.Add(hT, new AssemblerInfo(parent, position, rotation, scale, collider, hasRigidbody, hasCollider));
       });
    }


    public void Disassemble()
    {

        lastPosition = this.transform.position;

        if (isAssembled)
        {
            targets.ForEach(hT =>
           {
               AssemblerInfo info = targetsInfo[hT];

           //
           Transform parent = hT.transform.parent;
               Vector3 position = hT.transform.localPosition;
               Quaternion rotation = hT.transform.localRotation;
               Vector3 scale = hT.transform.localScale;

               info.originalPosition = position;
               info.originalRotation = rotation;
               info.originalScale = scale;
           //

           hT.transform.parent = null;

               info.generatedCollider.enabled = true;

               Vector3 explosionForce = UnityEngine.Random.Range(MinExplosionForce, MaxExplosionForce) * UnityEngine.Random.onUnitSphere;

               Rigidbody hRb;
               if (!info.hasInitialRigidbody)
               {
                   hRb = hT.AddComponent<Rigidbody>();
                   hRb.mass = MassForeachElement;
                   hRb.velocity = explosionForce;
               }
               else
               {
                   hRb = hT.GetComponent<Rigidbody>();
                   hRb.mass = MassForeachElement;
                   hRb.velocity = explosionForce;
               }
           });

        }
        remainingColliders.ForEach(hC => hC.enabled = false);
        isAssembled = false;

        if (rootCollider != null)
            if (!RootColliderAlwaysOn)
                rootCollider.enabled = false;


    }

    void Reassemble()
    {
        isAssembled = true;

        this.transform.position = lastPosition;

        targets.ForEach(hT =>
        {
            AssemblerInfo info = targetsInfo[hT];

            hT.transform.parent = info.originalParent;
            hT.transform.localPosition = info.originalPosition;
            hT.transform.localRotation = info.originalRotation;
            hT.transform.localScale = info.originalScale;

            MeshCollider collider = info.generatedCollider;
            if (!info.hasInitialCollider)
            {
                collider.enabled = false;
            }

            if (!info.hasInitialRigidbody)
            {
                Rigidbody hRb = hT.GetComponent<Rigidbody>();
                hRb.velocity = Vector3.zero;
                hRb.angularVelocity = Vector3.zero;
                Destroy(hRb);
            }

        });

        rootRigidbody.velocity = Vector3.zero;
        rootRigidbody.angularVelocity = Vector3.zero;

        remainingColliders.ForEach(hC => hC.enabled = true);

        if (rootCollider != null)
            if (!RootColliderAlwaysOn)
                rootCollider.enabled = true;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ControllerPlayerWheels>())
            Disassemble();
    }

    private struct AssemblerInfo
    {
        public Transform originalParent;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;
        public MeshCollider generatedCollider;
        public bool hasInitialRigidbody;
        public bool hasInitialCollider;

        public AssemblerInfo(Transform originalParent, Vector3 originalPosition, Quaternion originalRotation, Vector3 originalScale, MeshCollider generatedCollider, bool hasInitialRigidbody, bool hasInitialCollider)
        {
            this.originalParent = originalParent;
            this.originalPosition = originalPosition;
            this.originalRotation = originalRotation;
            this.originalScale = originalScale;
            this.generatedCollider = generatedCollider;
            this.hasInitialRigidbody = hasInitialRigidbody;
            this.hasInitialCollider = hasInitialCollider;
        }
    }
}
