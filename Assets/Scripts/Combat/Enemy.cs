using UnityEngine;
using Player;
using System.Collections;
using TMPro;

namespace Combat
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private float moveSpeed = 2f;

        [Header("Movement")]
        [SerializeField] private float patrolRadius = 3f;
        [SerializeField] private float waitTimeAtPoint = 2f;
        [SerializeField] private float attackRange = 1f;

        [Header("UI")]
        [SerializeField] private float healthTextDisplayTime = 3f;
        [SerializeField] private float damageNumberDuration = 0.5f;
        [SerializeField] private float damageNumberRiseHeight = 1f;

        private int currentHealth;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector3 startPosition;
        private Vector3 currentTarget;
        private BoxCollider2D boxCollider;
        private bool isDead = false;
        private bool isWaiting = false;
        private bool isAttacking = false;
        private TextMeshProUGUI healthText;
        private float healthTextTimer;
        private string enemyName;

        private static readonly string[] EnemyNames = new string[] 
        {
            "Goblin", "Orc", "Troll", "Slime", "Bat", "Spider", "Skeleton", "Ghost",
            "Imp", "Kobold", "Rat", "Wolf", "Bandit", "Thief", "Rogue", "Witch"
        };

        private void Start()
        {
            InitializeComponents();
            SetupRandomName();
            StartCoroutine(PatrolBehavior());
        }

        private void InitializeComponents()
        {
            currentHealth = maxHealth;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            startPosition = transform.position;
            currentTarget = GetNewTargetPosition();

            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }

            CreateHealthText();
        }

        private void SetupRandomName()
        {
            enemyName = EnemyNames[Random.Range(0, EnemyNames.Length)];
            gameObject.name = $"{enemyName} ({gameObject.name})";
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            ShowDamageNumber(damage);

            // Show and update health text
            if (healthText != null)
            {
                healthText.text = $"{enemyName}\nHP: {currentHealth}/{maxHealth}";
                healthText.gameObject.SetActive(true);
                healthTextTimer = healthTextDisplayTime;
            }

            StartCoroutine(FlashRed());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void ShowDamageNumber(int damage)
        {
            var damageTextObj = new GameObject("DamageNumber");
            damageTextObj.transform.position = transform.position + Vector3.up * 0.5f;

            var damageText = damageTextObj.AddComponent<TextMeshPro>();
            damageText.text = damage.ToString();
            damageText.fontSize = 4;
            damageText.color = Color.red;
            damageText.alignment = TextAlignmentOptions.Center;

            StartCoroutine(AnimateDamageNumber(damageTextObj));
        }

        private IEnumerator AnimateDamageNumber(GameObject damageTextObj)
        {
            float elapsed = 0f;
            Vector3 startPos = damageTextObj.transform.position;
            TextMeshPro tmp = damageTextObj.GetComponent<TextMeshPro>();

            while (elapsed < damageNumberDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / damageNumberDuration;

                // Move up
                damageTextObj.transform.position = startPos + Vector3.up * (damageNumberRiseHeight * t);
                
                // Fade out
                Color color = tmp.color;
                color.a = 1 - t;
                tmp.color = color;

                yield return null;
            }

            Destroy(damageTextObj);
        }

        private void CreateHealthText()
        {
            var textObj = new GameObject("EnemyHealthText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.up; // Position above enemy

            healthText = textObj.AddComponent<TextMeshProUGUI>();
            healthText.fontSize = 16;
            healthText.color = Color.white;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Update health text position to follow enemy
            if (healthText != null && healthText.gameObject.activeSelf)
            {
                healthText.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
                
                // Hide health text after timer expires
                if (healthTextTimer > 0)
                {
                    healthTextTimer -= Time.deltaTime;
                    if (healthTextTimer <= 0)
                    {
                        healthText.gameObject.SetActive(false);
                    }
                }
            }

            if (!isDead && !isWaiting && !isAttacking)
            {
                // Move towards current target
                Vector3 moveDirection = (currentTarget - transform.position).normalized;
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget,
                    moveSpeed * Time.deltaTime
                );

                // Update sprite direction
                if (moveDirection.x != 0)
                {
                    spriteRenderer.flipX = moveDirection.x < 0;
                }

                // Check if reached target
                if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
                {
                    StartCoroutine(WaitAndGetNewTarget());
                }
            }
        }

        private IEnumerator WaitAndGetNewTarget()
        {
            isWaiting = true;
            yield return new WaitForSeconds(waitTimeAtPoint);
            currentTarget = GetNewTargetPosition();
            isWaiting = false;
        }

        private Vector3 GetNewTargetPosition()
        {
            Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
            Vector3 targetPos = startPosition + new Vector3(randomDirection.x, randomDirection.y, 0);
            return targetPos;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !isAttacking && !isDead)
            {
                StartCoroutine(AttackCoroutine(other.transform));
            }
        }

        private IEnumerator AttackCoroutine(Transform target)
        {
            isAttacking = true;

            // Attack wind-up
            yield return new WaitForSeconds(0.2f);

            // Check if player is still in range before dealing damage
            if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
            {
                var playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Enemy attacks player for {damage} damage!");
                }
            }

            // Attack recovery
            yield return new WaitForSeconds(0.3f);
            isAttacking = false;
        }

        private IEnumerator FlashRed()
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }

        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            Debug.Log("Enemy defeated!");
            boxCollider.enabled = false;
            rb.simulated = false;
            Destroy(gameObject, 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw patrol radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, patrolRadius);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw current target if in play mode
            if (Application.isPlaying && currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget);
                Gizmos.DrawWireSphere(currentTarget, 0.2f);
            }
        }
    }
}