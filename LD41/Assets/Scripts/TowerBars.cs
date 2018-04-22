using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerBars : MonoBehaviour {

    public RectTransform canvasRect;
    public Slider healthSlider;
    public Slider timerSlider;
    public float health;
    public float maxHealth;
    public float time;
    public float maxTime;
    public float timerBarOffset;
    public Transform entity;

    private Vector2 uiOffset;

    void Start () {
        uiOffset = new Vector2((float)canvasRect.sizeDelta.x / 2f, (float)canvasRect.sizeDelta.y / 2f);
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localPosition = Vector2.zero;
    }

    void Update() {
        if (Mathf.Abs(maxHealth) - 0.0f < Mathf.Epsilon) return;
        healthSlider.value = health / maxHealth;
        if (Mathf.Abs(maxTime) - 0.0f < Mathf.Epsilon) return;
        timerSlider.value = time / maxTime;
    }

    void LateUpdate() {
        if (entity == null) return;
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(entity.position);
        Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvasRect.sizeDelta.x, viewportPosition.y * canvasRect.sizeDelta.y);
        Vector3 newPosition = proportionalPosition - uiOffset;
        healthSlider.GetComponent<RectTransform>().localPosition = newPosition;
        newPosition.y += timerBarOffset;
        timerSlider.GetComponent<RectTransform>().localPosition = newPosition;
    }

}
