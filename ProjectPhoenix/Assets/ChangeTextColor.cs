using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextColor : MonoBehaviour
{
    public Color SelectionColor;
    public float FadeDuration = 0.1f;
    private Color deselectionColor;

    Text text;
    Button button;

    void Awake()
    {
        text = GetComponent<Text>();
        button = GetComponentInParent<Button>();
        deselectionColor = text.color;
    }

    public void OnSelect()
    {
        LeanTween.color(this.gameObject, SelectionColor, FadeDuration).setOnUpdateColor(val => text.color = val);
    }

    public void OnDeselect()
    {
        LeanTween.color(this.gameObject, deselectionColor, FadeDuration).setOnUpdateColor(val => text.color = val);
    }
}
