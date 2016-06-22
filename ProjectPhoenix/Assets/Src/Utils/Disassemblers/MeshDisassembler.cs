using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshDisassembler : MonoBehaviour
{
    //public Material FadeOutMaterial;
    public bool RootColliderAlwaysOn;
    public float MassForeachElement = 10;

    private Rigidbody rootRigidbody;
    private Collider rootCollider;
    private Vector3 lastPosition;

    private List<GameObject> targets;
    private Dictionary<GameObject, AssemblerInfo> targetsInfo;
    private List<Collider> remainingColliders;
    private List<MeshRenderer> meshRenderers;

    public bool IsNotReady { get; private set; }

    private void Awake()
    {
        //if (FadeOutMaterial == null)
        //    throw new UnityException("Give Me the FadeOut Material!");
        IsNotReady = false;

        rootRigidbody = this.GetComponent<Rigidbody>();
        rootCollider = this.GetComponent<Collider>();

        targetsInfo = new Dictionary<GameObject, AssemblerInfo>();
        targets = new List<GameObject>();
        remainingColliders = new List<Collider>();
        meshRenderers = new List<MeshRenderer>();

        targets = this.GetComponentsInChildren<Transform>().Select(hT => hT.gameObject).Where(GO => GO.GetComponent<MeshRenderer>() != null).ToList();
        remainingColliders = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<MeshRenderer>() == null).Select(GO => GO.GetComponent<Collider>()).Where(hC => hC != null && hC as WheelCollider == null).ToList();

        meshRenderers = targets.Select(hGO => hGO.GetComponent<MeshRenderer>()).ToList();
        meshRenderers.ForEach(hMR =>
        {
            List<Material> originalMaterials = hMR.materials.ToList();
            //FadeOutMaterial.mainTexture = originalMaterials.First().mainTexture;
            //originalMaterials.Add(FadeOutMaterial);
            hMR.materials = originalMaterials.ToArray();
        });

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

           Rigidbody hRb;
           if (!info.hasInitialRigidbody)
           {
               hRb = hT.AddComponent<Rigidbody>();
           }
           else
           {
               hRb = hT.GetComponent<Rigidbody>();
           }
       });

        remainingColliders.ForEach(hC => hC.enabled = false);
        IsNotReady = true;

        if (rootCollider != null)
            if (!RootColliderAlwaysOn)
                rootCollider.enabled = false;

        FadeOutMeshRenderers();
    }

    public void Explode(float MinExplosionForce, float MaxExplosionForce)
    {
        lastPosition = this.transform.position;

        IsNotReady = true;
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

        remainingColliders.ForEach(hC => hC.enabled = false);

        if (rootCollider != null)
            if (!RootColliderAlwaysOn)
                rootCollider.enabled = false;

        FadeOutMeshRenderers();
    }

    public void Reassemble()
    {

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

        TurnOnMeshRenderers();
    }

    private void FadeOutMeshRenderers()
    {
        StartCoroutine(WaitForFadeOut(3f));
    }
    //private void SwapMaterials()
    //{
    //    meshRenderers.ForEach(hMR =>
    //    {
    //        Material tmp = hMR.material;
    //        hMR.material = hMR.materials[hMR.materials.Length - 1];
    //        hMR.materials[hMR.materials.Length - 1] = tmp;
    //    });
    //}
    private void TurnOnMeshRenderers()
    {
        //SwapMaterials();
        targets.ForEach(hT => LeanTween.alpha(hT, 255f, 0f));
    }
    private IEnumerator WaitForFadeOut(float duration)
    {
        yield return new WaitForSeconds(duration);
        //SwapMaterials();
        targets.ForEach(hT => LeanTween.alpha(hT, 0f, 3f).setOnComplete(x => IsNotReady = false));
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