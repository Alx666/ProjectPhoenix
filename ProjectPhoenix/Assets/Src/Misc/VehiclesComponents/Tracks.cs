using UnityEngine;
using System.Collections;

internal class Tracks : MonoBehaviour
{
    private float tracksSpeed;
    private Material material;
    internal float TracksSpeed
    {
        get
        {
            return tracksSpeed;
        }
        set
        {
            tracksSpeed = Mathf.Clamp(value, -100f, 100f);
        }
    }

	void Awake ()
    {
        material = this.GetComponent<MeshRenderer>().material;
	}
	
	void Update ()
    {
        Vector2 offset = material.mainTextureOffset;

        if (offset.x == 1f)
            offset.x = 0f;
        else if (offset.x == 0f)
            offset.x = 1f;
        offset.x = Mathf.Clamp01(offset.x - this.tracksSpeed * Time.deltaTime);

        material.mainTextureOffset = offset;
        
	}
}
