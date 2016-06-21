using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class MadMaxActor : Actor
{
    [SyncVar]
    private float currentHealth;

	private Canvas HealthBar;
    private Slider m_hHpSlider;
    private RectTransform m_hSliderRectTransform;


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
		currentHealth = Hp;
        HealthBar                   = this.GetComponentInChildren<Canvas>();
        HealthBar.name              = "HealthBar_" + this.gameObject.name;
        HealthBar.transform.parent  = null;
        m_hHpSlider                 = HealthBar.GetComponentInChildren<Slider>();
        m_hSliderRectTransform      = m_hHpSlider.GetComponent<RectTransform>();

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
        this.currentHealth -= hSource.GetDamage(this.Armor);
        
        if (this.currentHealth <= 0f)
        {
            HealthBar.enabled = false;
            RpcDie(hSource.Owner.netId);
        }
    }

    public override void Die(Actor Killer)
    {
		GameManager.Instance.WoW( Killer, this );
    }
    
    [ClientRpc]
    public void RpcDie(NetworkInstanceId hID)
    {
        Die(ClientScene.FindLocalObject(hID).GetComponent<Actor>());
    }


    public enum HealthBarMode
    {
        Overlay,
        WorldSpace,
    }
}
