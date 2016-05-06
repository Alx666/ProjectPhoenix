using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshDisassembler : MonoBehaviour
{
	private bool                                    isAssembled;
    private Vector3                                 lastPosition;
	private List<GameObject>                        targets;
    private Dictionary<GameObject, AssemblerInfo>   targetsInfo;
    private List<Collider>                          remainingColliders;
	void Awake()
	{
		isAssembled                 = true;
		targetsInfo	                = new Dictionary<GameObject, AssemblerInfo>();
		targets		                = this.GetComponentsInChildren<Transform>().Select(hT => hT.gameObject).Where(GO => GO.GetComponent<MeshRenderer>() != null ).ToList();
        remainingColliders          = this.GetComponentsInChildren<Transform>().Where(hT => hT.GetComponent<MeshRenderer>() == null).Select(GO => GO.GetComponent<Collider>()).Where(hC => hC != null && hC as WheelCollider == null).ToList();

        Collider rootCollider = this.GetComponent<Collider>();
        if(rootCollider != null)
            remainingColliders.Add(rootCollider);

        targets.ForEach( hT => 
		{
            Transform parent        = hT.transform.parent;
            Vector3 position        = hT.transform.localPosition;
            Quaternion rotation     = hT.transform.localRotation;
            Vector3 scale           = hT.transform.localScale;

            MeshCollider collider   = hT.GetComponent<MeshCollider>();
            bool hasCollider        = false;
			if(collider == null )
			{
                collider            = hT.AddComponent<MeshCollider>();
                collider.convex     = true;
                collider.enabled    = false;
			}
            else
            {
                hasCollider = true;
            }

            bool hasRigidbody       = hT.GetComponent<Rigidbody>() != null;

            targetsInfo.Add(hT, new AssemblerInfo(parent, position, rotation, scale, collider, hasRigidbody, hasCollider));
		} );
	}
	void Start ()
	{
	
	}
	
	void Update ()
	{
		if ( Input.GetKeyDown(KeyCode.Space ))
		{
			if ( isAssembled )
			{
                lastPosition = this.transform.position;
                Disassemble();
				isAssembled = false;
			}
			else
			{
                this.transform.position = lastPosition;
				Reassemble();
				isAssembled = true;
			}
		}
	
	}

	public void Disassemble()
	{
		targets.ForEach( hT =>
		{
			AssemblerInfo info      = targetsInfo[hT];
            hT.transform.parent     = null;

			MeshCollider collider   = info.generatedCollider;
            if (collider != null )
			{
                collider.enabled = true;
			}

            Vector3 explosionForce = UnityEngine.Random.Range(30f, 100f) * UnityEngine.Random.onUnitSphere;
            if ( !info.hasInitialRigidbody )
			{
                hT.AddComponent<Rigidbody>().velocity = explosionForce;
			}
            else
            {
                hT.GetComponent<Rigidbody>().velocity = explosionForce;
            }
		} );

        remainingColliders.ForEach(hC => hC.enabled = false);

	}

	void Reassemble()
	{
        targets.ForEach(hT =>
        {
            AssemblerInfo info          = targetsInfo[hT];

            hT.transform.parent         = info.originalParent;
            hT.transform.localPosition  = info.originalPosition;
            hT.transform.localRotation  = info.originalRotation;
            hT.transform.localScale     = info.originalScale;

            MeshCollider collider       = info.generatedCollider;
            if(!info.hasInitialCollider)
            {
                collider.enabled = false;
            }

            if (!info.hasInitialRigidbody)
            {
                Destroy(hT.GetComponent<Rigidbody>());
            }

        });

        remainingColliders.ForEach(hC => hC.enabled = true);

    }

    private struct AssemblerInfo
	{
        public Transform    originalParent;
        public Vector3      originalPosition;
        public Quaternion   originalRotation;
        public Vector3      originalScale;
        public MeshCollider generatedCollider;
		public bool         hasInitialRigidbody;
		public bool         hasInitialCollider;

        public AssemblerInfo( Transform originalParent, Vector3 originalPosition, Quaternion originalRotation, Vector3 originalScale, MeshCollider generatedCollider, bool hasInitialRigidbody, bool hasInitialCollider)
		{
            this.originalParent         = originalParent;
            this.originalPosition       = originalPosition;
            this.originalRotation       = originalRotation;
            this.originalScale          = originalScale;
			this.generatedCollider      = generatedCollider;
            this.hasInitialRigidbody    = hasInitialRigidbody;
            this.hasInitialCollider     = hasInitialCollider;
        }					 
	}
}
