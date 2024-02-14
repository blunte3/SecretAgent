using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    public Slider hungerSlider;
    public Transform agentHead;
    public float yOffset = 0.5f; // Adjust this value to position the slider closer or further from the head

    private void Update()
    {
        // Position the slider above the agent's head
        Vector3 screenPos = Camera.main.WorldToScreenPoint(agentHead.position + Vector3.up * yOffset);
        hungerSlider.transform.position = screenPos;

        // Adjust slider size
        hungerSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 10f); // Adjust width and height as needed

        // Change slider color based on fill amount
        Color barColor = Color.green; // Default color
        if (hungerSlider.value < 0.3f)
        {
            barColor = Color.red;
        }
        else if (hungerSlider.value < 0.6f)
        {
            barColor = Color.yellow;
        }
        hungerSlider.fillRect.GetComponent<Image>().color = barColor;
    }
}