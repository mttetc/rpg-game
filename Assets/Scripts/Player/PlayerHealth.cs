using UnityEngine;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        public int CurrentHealth => currentHealth;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            FindObjectOfType<UI.UIManager>()?.UpdateUI();
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player died!");
        }
    }
} 