using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public class MadMaxActor : Actor
{
    //TODO: this is a field added to set up initial HP from automated script "CarImportTool". Temp solution.
    [HideInInspector]
    public float HpToSet;

    [SyncVar]
    private float currentHealth;

    private Canvas HealthBar;
    private Slider m_hHpSlider;
    private RectTransform m_hSliderRectTransform;
    //private MeshDisassembler m_hDisassembler;
    private Rigidbody m_hRigidbody;
    private MonoBehaviour m_hController;
    private InputProviderPCStd m_hProvider;
    //DeathBomb not used anymore in 2016/2017 version
    //private DeathBomb m_hBomb;
    private MonoBehaviour m_hWeapon;
    private List<Renderer> m_hRenderers;
    private List<Collider> m_hColliders;
    //private Vector3 PlayableCenterOfMass;
    private MadMaxCarAudio m_hAudioCtrl;
    //private DMGImageMGR dmgImgMGR;

    [Header("Health Bar Config")]
    public HealthBarMode HpBarMode = HealthBarMode.WorldSpace;
    [Range(1f, 100f)]
    public float HPBarOffset = 10f;
    [Range(1f, 100f)]
    public float HpBarLerp = 10f;
    [Range(0.01f, 1f)]
    public float HpBarScale = 0.01f;

    public GameObject DeathExplosionPrefab;

    //DAMAGE ON IMPACT REGION
    [Tooltip("Max Damage get by Impact between to MadMaxActors")]
    public float ImpactMaxDamage = 10f;
    [Tooltip("Cool Down Calculated in Seconds")]
    public float ImpactCoolDownTime = 1f;
    private LinkedList<MadMaxActor> impactCoolDownActors;

    private ControllerWheels wheels;

    public bool CanTakeDamage { get; private set; }

    void Awake()
    {
        #region Initialize stuff
        //Dangerous code, health hp can be set freely
        Hp = HpToSet;

        currentHealth = Hp;
        HealthBar = this.GetComponentInChildren<Canvas>();
        HealthBar.name = "HealthBar_" + this.gameObject.name;
        //HealthBar.transform.parent = null;
        HealthBar.transform.SetParent(null, false);
        m_hHpSlider = HealthBar.GetComponentInChildren<Slider>();
        m_hSliderRectTransform = m_hHpSlider.GetComponent<RectTransform>();
        //m_hDisassembler             = GetComponent<MeshDisassembler>();
        m_hRigidbody = GetComponent<Rigidbody>();
        m_hController = GetComponent<IControllerPlayer>() as MonoBehaviour;
        m_hProvider = GetComponent<InputProviderPCStd>();
        m_hWeapon = GetComponent<IWeapon>() as MonoBehaviour;
        m_hRenderers = new List<Renderer>(GetComponents<Renderer>());
        m_hRenderers.AddRange(GetComponentsInChildren<Renderer>());
        m_hColliders = new List<Collider>(GetComponents<Collider>());
        m_hColliders.AddRange(GetComponentsInChildren<Collider>());
        //m_hBomb = GetComponent<DeathBomb>();
        m_hAudioCtrl = this.GetComponent<MadMaxCarAudio>();

        impactCoolDownActors = new LinkedList<MadMaxActor>();
        //PlayableCenterOfMass = m_hRigidbody.centerOfMass;

        wheels = GetComponent<ControllerWheels>();

        //dmgImgMGR = GameManager.Instance.GetComponentInChildren<DMGImageMGR>();

        CanTakeDamage = true;
        #endregion

        if (HpBarMode == HealthBarMode.WorldSpace)
        {
            HealthBar.renderMode = RenderMode.WorldSpace;
        }
        else
        {
            HealthBar.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    void Start()
    {
        if (!isServer && localPlayerAuthority)
        {
            CmdSyncPlayer();
        }
        StartCoroutine(WaitForInit());
    }

    public IEnumerator WaitForInit()
    {
        yield return new WaitForSeconds(5.0f);
        //dmgImgMGR = GameManager.Instance.GetComponentInChildren<DMGImageMGR>();
    }

    #region Sexy Code
    [Command]
    void CmdSyncPlayer()
    {
        NetworkInstanceId[] hId = GameManager.Instance.scores.Keys.Select(hA => hA.netId).ToArray();
        GameManager.Instance.RpcSyncPlayer(hId);
    }
    #endregion

    void Update()
    {
        m_hHpSlider.value = Mathf.Clamp(currentHealth, 0f, base.Hp); //TODO: Make RPC

        //debug
        //if (Input.GetKeyDown(KeyCode.Space))
        //    this.Die(this);

        //if (Input.GetKeyDown(KeyCode.H))
        //    dmgImgMGR.FlashIn();

    }

    void LateUpdate()
    {
        Vector3 vHealthPosition;

        if (HpBarMode == HealthBarMode.WorldSpace)
        {
            HealthBar.transform.localScale = Vector3.one * HpBarScale;
            HealthBar.transform.forward = -Camera.main.transform.forward;
            vHealthPosition = this.transform.position + new Vector3(0.0f, HPBarOffset, 0.0f);
        }
        else
        {
            vHealthPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position) + new Vector2(0.0f, HPBarOffset);
        }

        Vector3 vCurrentHealthPosition = m_hSliderRectTransform.position;
        m_hSliderRectTransform.position = Vector3.Lerp(vCurrentHealthPosition, vHealthPosition, Time.deltaTime * HpBarLerp);
    }

    public override void Damage(IDamageSource hSource)
    {
        if (hSource.Owner == this)
            return;

        if (!CanTakeDamage)
            return;

        //dmgImgMGR.FlashIn();

        LastActor = hSource.Owner;

        this.currentHealth -= hSource.GetDamage(this.Armor);

        if (this.currentHealth <= 0f)
        {
            HealthBar.enabled = false;
            RpcDie(hSource.Owner.netId);
        }
    }

    public override void OnFlippedState()
    {
        if (LastActor != null)
            Die(LastActor);
        else
            Die(this);
    }

    public void OnDeathBombTimeout()
    {
        if (LastActor != null)
            Die(LastActor);
        else
            Die(this);
    }

    public override void Die(Actor Killer)
    {
        GameManager.Instance.WoW(Killer, this);

        if (this.DeathExplosionPrefab != null)
        {
            ParticleVfx vExplosion = GlobalFactory.GetInstance<ParticleVfx>(DeathExplosionPrefab);
            Vector3 vDirection = Random.insideUnitSphere;
            vDirection.y = 0;
            vDirection.Normalize();
            vExplosion.PlayEffect(this.transform.position, vDirection, 1.0f, false);
        }

        //m_hDisassembler.Explode(10f, 20f);
        //m_hRigidbody.isKinematic = true;
        m_hWeapon.enabled = false;
        m_hWeapon.GetComponent<Weapon>().Reset();
        //m_hBomb.enabled = false;
        HealthBar.enabled = false;
        m_hAudioCtrl.enabled = false;

        if (isLocalPlayer)
        {
            m_hController.enabled = false;
            m_hProvider.enabled = false;
        }

        //m_hRenderers.ForEach(hR => hR.enabled = false);
        //m_hColliders.ForEach(hC => hC.enabled = false);

        //m_hRigidbody.ResetCenterOfMass();
        m_hRigidbody.AddForce(Vector3.up * 12f, ForceMode.VelocityChange);
        m_hRigidbody.AddTorque(Random.rotation.eulerAngles * 10f, ForceMode.VelocityChange);
        Physics.OverlapSphere(this.transform.position, 10f).Select(x => x.GetComponent<Rigidbody>()).Where(x => x != null).ToList().ForEach(x => x.AddExplosionForce(10f, this.transform.position, 0f));
        //this.m_hController.enabled = false;

        CanTakeDamage = false;

        StartCoroutine(WaitForRespawn(GameManager.Instance.RespawnTime));

        this.IsDead = true;
    }

    [ClientRpc]
    public void RpcDie(NetworkInstanceId hID)
    {
        Die(ClientScene.FindLocalObject(hID).GetComponent<Actor>());
    }

    private IEnumerator WaitForRespawn(float duration)
    {

        //yield return new WaitWhile(() => m_hDisassembler.IsNotReady);
        yield return new WaitForSeconds(duration);
        this.Respawn();
    }

    private void Respawn()
    {
        this.transform.position = GameManager.Instance.GetRandomSpawnPoint();
        this.m_hRigidbody.isKinematic = false;
        //m_hRenderers.ForEach(hR => hR.enabled = true);
        //m_hColliders.ForEach(hC => hC.enabled = true);
        m_hWeapon.enabled = true;
        HealthBar.enabled = true;
        this.gameObject.transform.up = Vector3.up;
        //m_hRigidbody.centerOfMass = PlayableCenterOfMass;
        m_hRigidbody.velocity = Vector3.zero;
        m_hRigidbody.angularVelocity = Vector3.zero;
        m_hAudioCtrl.enabled = true;

        CanTakeDamage = true;

        //m_hBomb.Reset();
        if (wheels != null)
        {
            wheels.Reset();
        }

        if (isLocalPlayer)
        {
            m_hController.enabled = true;
            m_hProvider.enabled = true;
        }

        //m_hDisassembler.Reassemble();
        currentHealth = Hp;
        HealthBar.enabled = true;

        IsDead = false;
    }

    public enum HealthBarMode
    {
        Overlay,
        WorldSpace,
    }

    #region ON COLLISION ENTER
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<TerrainCollider>() != null)
            return;

        MadMaxActor other = collision.gameObject.GetComponent<MadMaxActor>();
        if (other != null && !impactCoolDownActors.Contains(other))
        {
            float damage = 0f;
            float damageRate = 0f;

            float dot = Mathf.Abs(Vector3.Dot(this.transform.forward, other.transform.forward));

            //NON PARALLELE (IMPULSIVE FORCE)
            if (dot < 0.5f)
            {
                damageRate = 1 - Mathf.Abs(Vector3.Dot(this.transform.forward, collision.impulse.normalized));
            }
            //PARALLELE (VELOCITY)
            else
            {
                float sum = m_hRigidbody.velocity.magnitude + other.m_hRigidbody.velocity.magnitude;
                float damageValue = sum - m_hRigidbody.velocity.magnitude;
                damageRate = damageValue / sum;
            }

            damage = ImpactMaxDamage * damageRate;
            this.Damage(other, damage);
            //COOLDOWN
            StartCoroutine(ImpactCoolDown(other));
        }
    }

    private void Damage(MadMaxActor hActor, float dmg)
    {
        if (hActor == this)
            return;

        LastActor = hActor;

        this.currentHealth -= hActor.GetImpactDamage(dmg);
        if (this.currentHealth <= 0f)
        {
            HealthBar.enabled = false;
            RpcDie(hActor.netId);
        }
    }
    public float GetImpactDamage(float dmg)
    {
        float damage = 0;

        switch (this.Armor)
        {
            case ArmorType.Light:
                damage = dmg;           //Danno da impatto TOTALE
                break;
            case ArmorType.Medium:
                damage = dmg * 0.5f;    //Danno da impatto DIMEZZATO
                break;
            case ArmorType.Heavy:
                damage = dmg * 0.3f;    //Danno da impatto di UN TERZO
                break;
            default:
                break;
        }
        return damage;
    }
    private IEnumerator ImpactCoolDown(object toCoolDownActor)
    {
        MadMaxActor actor = toCoolDownActor as MadMaxActor;

        this.impactCoolDownActors.AddLast(actor);
        yield return new WaitForSeconds(ImpactCoolDownTime);
        this.impactCoolDownActors.Remove(actor);
    }
    #endregion
}
