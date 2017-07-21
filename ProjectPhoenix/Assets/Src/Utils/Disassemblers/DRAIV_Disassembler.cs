using UnityEngine;

public class DRAIV_Disassembler : MonoBehaviour, IDamageable
{
    public ArmorType Armor = ArmorType.Light;
    public float Mass = 1;

    public bool DEBUG_DISASSEMBLED;
    private MeshCollider hCollider;

    //DELETE?
    private Rigidbody hRigidbody;
    //DELETE?

    public void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Placeable");
        hCollider = this.GetComponent<MeshCollider>();
        if (hCollider == null)
        {
            hCollider = this.gameObject.AddComponent<MeshCollider>();
        }
        hCollider.convex = true;

        hRigidbody = AddRigidbody();
    }

    public void Damage(IDamageSource hSource)
    {
        //DELETE?
        hRigidbody.useGravity = true;


        //DELETE?

        //Rigidbody hRigidbody = AddRigidbody();
        hRigidbody.velocity = hSource.GetDamage(Armor) * hSource.Owner.gameObject.transform.forward;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<TerrainCollider>() != null)
            return;

        if (collision.gameObject.GetComponent<DRAIV_Disassembler>() != null)
            return;

        if (collision.gameObject.GetComponent<DRAIV_TerrainCollider>() != null)
            return;

        //DELETE?
        hRigidbody.useGravity = true;
        DEBUG_DISASSEMBLED = true;

        hRigidbody.drag = 1f;
        hRigidbody.angularDrag = 1f;

        //DELETE?

        //AddRigidbody();
    }

    private Rigidbody AddRigidbody()
    {
        //this.gameObject.transform.SetParent(null);

        Rigidbody hRigidbody = this.GetComponent<Rigidbody>();
        if (hRigidbody == null)
            hRigidbody = this.gameObject.AddComponent<Rigidbody>();

        hRigidbody.mass = Mass;

        //DELETE?
        hRigidbody.useGravity = false;
        //DELETE?

        hRigidbody.drag = 5f;
        hRigidbody.angularDrag = 5f;

        return hRigidbody;
    }
    public void SetMass(float mass)
    {
        hRigidbody.mass = mass;
    }
}