using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DMGImageMGR : MonoBehaviour {

    public float FlashSpeed = 10.0f;

    private Image dmgImage;


    void Awake()
    {
        dmgImage = this.GetComponent<Image>();
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        dmgImage.color = Color.Lerp(dmgImage.color, Color.clear, FlashSpeed * Time.deltaTime);
    }

    public void FlashIn()
    {
        dmgImage.color = Color.white;
    }
}
