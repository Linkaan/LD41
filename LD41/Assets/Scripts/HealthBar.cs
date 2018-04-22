using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    public RectTransform canvasRect;
    public Slider healthSlider;
    public float health;
    public float maxHealth;
    public Transform entity;

    private Vector2 uiOffset;

    void Start () {
        uiOffset = new Vector2((float)canvasRect.sizeDelta.x / 2f, (float)canvasRect.sizeDelta.y / 2f);
    }

    void Update() {
        if (Mathf.Abs(maxHealth) - 0.0f < Mathf.Epsilon) return;
        healthSlider.value = health / maxHealth;
    }

    void LateUpdate() {
        if (entity == null) return;
        //Vector3 screenPos = Camera.main.WorldToScreenPoint(entity.position);
        //screenPos.x += widthOffset;
        //screenPos.y += heightOffset;
        /*
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(entity.position);
        Vector2 screenPos = new Vector2(
            ((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
        GetComponent<RectTransform> ().anchoredPosition = screenPos;
        */
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(entity.position);
        Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvasRect.sizeDelta.x, viewportPosition.y * canvasRect.sizeDelta.y);
        GetComponent<RectTransform> ().localPosition = proportionalPosition - uiOffset;
    }

}
