using UnityEngine;
using UnityEngine.UI;
using Player;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private TextMeshProUGUI healthText;
        private PlayerHealth playerHealth;

        private void Start()
        {
            // Create health text if it doesn't exist
            if (healthText == null)
            {
                CreateHealthText();
            }

            // Find player and get health component
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        private void CreateHealthText()
        {
            // Create a new UI Text object
            GameObject healthTextObj = new GameObject("HealthText");
            healthTextObj.transform.SetParent(transform);
            
            // Add TextMeshProUGUI component
            healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
            
            // Set text properties
            healthText.fontSize = 36;
            healthText.color = Color.white;
            healthText.text = "Health: 100";
            
            // Position the text in the top-left corner
            RectTransform rectTransform = healthText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
            rectTransform.sizeDelta = new Vector2(200, 50);
        }

        public void UpdateUI()
        {
            if (healthText != null && playerHealth != null)
            {
                healthText.text = $"Health: {playerHealth.CurrentHealth}";
            }
        }
    }
} 