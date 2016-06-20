using UnityEngine;

public class DRAIV_Disassembler : MonoBehaviour, IDamageable
{
    public ArmorType Armor = ArmorType.Light;
    public float Mass = 1;
    private MeshCollider hCollider;

    public void Awake()
    {
        hCollider = this.GetComponent<MeshCollider>();
        if (hCollider == null)
        {
            hCollider = this.gameObject.AddComponent<MeshCollider>();
        }
        hCollider.convex = true;
    }

    public void Damage(IDamageSource hSource)
    {
        this.gameObject.transform.SetParent(null);

        Rigidbody hRigidbody = this.GetComponent<Rigidbody>();
        if (hRigidbody == null)
            hRigidbody = this.gameObject.AddComponent<Rigidbody>();

        hRigidbody.mass = Mass;

        hRigidbody.velocity = hSource.GetDamage(Armor) * hSource.Owner.gameObject.transform.forward;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<DRAIV_Disassembler>() != null || collision.gameObject.GetComponent<TerrainCollider>() != null)
            return;

        this.gameObject.transform.SetParent(null);

        Rigidbody hRigidbody = this.GetComponent<Rigidbody>();
        if (hRigidbody == null)
            hRigidbody = this.gameObject.AddComponent<Rigidbody>();

        hRigidbody.mass = Mass;
    }
}