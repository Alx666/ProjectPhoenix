using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshDisassembler : MonoBehaviour
{
	private bool                                    isAssembled;
    private Vector3                                 lastPosition;
	private List<GameObject>                        toDisassemble;
    private Dictionary<GameObject, AssemblerInfo>   children;
	void Awake()
	{
		isAssembled                 = true;
		children	                = new Dictionary<GameObject, AssemblerInfo>();
		toDisassemble		                = this.GetComponentsInChildren<Transform>().Select(hT => hT.gameObject).Where(GO => GO.GetComponent<MeshRenderer>() != null ).ToList();

		toDisassemble.ForEach( hC => 
		{
            Transform parent        = hC.transform.parent;
            Vector3 position        = hC.transform.localPosition;
            Quaternion rotation     = hC.transform.localRotation;
            Vector3 scale           = hC.transform.localScale;

            MeshCollider collider   = hC.GetComponent<MeshCollider>();
            bool hasCollider        = false;
			if(collider == null )
			{
                collider            = hC.AddComponent<MeshCollider>();
                collider.convex     = true;
                collider.enabled    = false;
			}
            else
            {
                hasCollider = true;
            }

            bool hasRigidbody       = hC.GetComponent<Rigidbody>() != null;

            children.Add(hC, new AssemblerInfo(parent, position, rotation, scale, collider, hasRigidbody, hasCollider));
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
		toDisassemble.ForEach( hC =>
		{
			AssemblerInfo info      = children[hC];
			hC.transform.parent     = null;

			MeshCollider collider   = info.generatedCollider;
            if (collider != null )
			{
                collider.enabled = true;
			}

            Vector3 explosionForce = UnityEngine.Random.Range(30f, 100f) * UnityEngine.Random.onUnitSphere;
            if ( !info.hasInitialRigidbody )
			{
				hC.AddComponent<Rigidbody>().velocity = explosionForce;
			}
            else
            {
                hC.GetComponent<Rigidbody>().velocity = explosionForce;
            }
		} );

	}

	void Reassemble()
	{
        toDisassemble.ForEach(hC =>
        {
            AssemblerInfo info          = children[hC];

            hC.transform.parent         = info.originalParent;
            hC.transform.localPosition  = info.originalPosition;
            hC.transform.localRotation  = info.originalRotation;
            hC.transform.localScale     = info.originalScale;

            MeshCollider collider       = info.generatedCollider;
            if(!info.hasInitialCollider)
            {
                collider.enabled = false;
            }

            if (!info.hasInitialRigidbody)
            {
                Destroy(hC.GetComponent<Rigidbody>());
            }

        });
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
