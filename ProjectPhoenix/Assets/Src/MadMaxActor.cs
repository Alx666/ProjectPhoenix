using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MadMaxActor : Actor
{
    [SyncVar]
    private float currentHealth;

	private Canvas HealthBar;
    private Slider m_hHpSlider;
    private RectTransform m_hSliderRectTransform;
    //private MeshDisassembler m_hDisassembler;
    private Rigidbody m_hRigidbody;
    private MonoBehaviour m_hController;
    private InputProviderPCStd m_hProvider;
    private MonoBehaviour m_hWeapon;
    private List<Renderer> m_hRenderers;
    private List<Collider> m_hColliders;

    [Header("Health Bar Config")]
    public HealthBarMode HpBarMode = HealthBarMode.WorldSpace;
    [Range(1f, 100f)]
    public float HPBarOffset = 10f;
    [Range(1f, 100f)]
    public float HpBarLerp = 10f;
    [Range(0.01f, 1f)]
    public float HpBarScale = 0.01f;



    void Awake()
	{
        #region Initialize stuff
        currentHealth = Hp;
        HealthBar = this.GetComponentInChildren<Canvas>();
        HealthBar.name = "HealthBar_" + this.gameObject.name;
        HealthBar.transform.parent = null;
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


    void Update()
    {
        m_hHpSlider.value = Mathf.Clamp(currentHealth, 0f, base.Hp); //TODO: Make RPC
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
        Die(LastActor);
    }

    public override void Die(Actor Killer)
    {
		GameManager.Instance.WoW( Killer, this );
        //m_hDisassembler.Explode(10f, 20f);
        m_hRigidbody.isKinematic = true;
        m_hWeapon.enabled = false;
        m_hWeapon.GetComponent<Weapon>().Reset();


        if (isLocalPlayer)
        {
            m_hController.enabled = false;
            m_hProvider.enabled = false;
        }

        m_hRenderers.ForEach(hR => hR.enabled = false);
        m_hColliders.ForEach(hC => hC.enabled = false);
        StartCoroutine(WaitForRespawn(GameManager.Instance.RespawnTime));
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
        m_hRenderers.ForEach(hR => hR.enabled = true);
        m_hColliders.ForEach(hC => hC.enabled = true);
        m_hWeapon.enabled = true;

        if(isLocalPlayer)
        {
            m_hController.enabled = true;
            m_hProvider.enabled = true;
        }

        //m_hDisassembler.Reassemble();
        currentHealth = Hp;
        HealthBar.enabled = true;
    }

    public enum HealthBarMode
    {
        Overlay,
        WorldSpace,
    }
}
