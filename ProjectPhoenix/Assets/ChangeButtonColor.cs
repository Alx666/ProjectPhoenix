using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonColor : MonoBehaviour
{
    public Color DeselectionColor;
    public float FadeDuration = 0.1f;
    Button button;

    private Color selectionColor;

    void Awake()
    {
        button = GetComponent<Button>();
        selectionColor = button.colors.highlightedColor;
    }

    public void OnDeselect()
    {
        LeanTween.color(this.gameObject, DeselectionColor, FadeDuration).setOnUpdateColor(val =>
        {
            ColorBlock colors = button.colors;
            colors.highlightedColor = val;
            button.colors = colors;
        });
    }

    public void OnSelect()
    {
        LeanTween.color(this.gameObject, selectionColor, FadeDuration).setOnUpdateColor(val =>
        {
            ColorBlock colors = button.colors;
            colors.highlightedColor = val;
            button.colors = colors;
        });
    }
}
