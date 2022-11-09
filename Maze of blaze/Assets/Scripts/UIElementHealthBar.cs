using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementHealthBar : UIelement
{
    float maxSize;
    Health playerHealth;
    RectTransform rectTransform;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        maxSize = rectTransform.sizeDelta.x;
        playerHealth = GameManager.instance.player.GetComponent<Health>();
    }
    public override void UpdateUI()
    {
        float size = playerHealth.currentHealth / playerHealth.maxHealth * maxSize;
        rectTransform.sizeDelta = new Vector2(size, rectTransform.sizeDelta.y);
    }
}
