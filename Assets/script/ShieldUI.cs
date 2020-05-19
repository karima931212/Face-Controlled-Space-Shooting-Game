using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldUI : MonoBehaviour {

    [SerializeField] RectTransform bar;
    float maxWidth;

    void Awake() {

        maxWidth = bar.rect.width;
    }
    void OnEnable() {
        EventHandle.OnTakeDamage += UpdateShieldDisplay;

    }

    void OnDisable() {
        EventHandle.OnTakeDamage -= UpdateShieldDisplay;
    }

    public void UpdateShieldDisplay(float percentage) {

        bar.sizeDelta = new Vector2(maxWidth * percentage, 10f);
    }
}
