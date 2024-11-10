using UnityEngine;
using Combat;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 50f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private int attackDamage = 20;
        [SerializeField] private float attackCooldown = 0.3f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;
        private bool canAttack = true;
        private bool isAttacking;
        private Vector2 lastMoveDirection = Vector2.right;
        private LayerMask enemyLayer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            // Set up enemy layer explicitly
            enemyLayer = LayerMask.GetMask("Enemy");
            Debug.Log($"Enemy layer mask: {enemyLayer}"); // Debug layer setup
        }

        private void Update()
        {
            HandleMovement();
            HandleAttack();

            // Debug enemy detection
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
            if (nearbyEnemies.Length > 0)
            {
                Debug.Log($"Enemies in range: {nearbyEnemies.Length}");
                foreach (var enemy in nearbyEnemies)
                {
                    Debug.Log($"Enemy found: {enemy.name} at layer: {enemy.gameObject.layer}");
                }
            }
        }

        private void HandleMovement()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;

            if (moveInput != Vector2.zero)
            {
                lastMoveDirection = moveInput;
                spriteRenderer.flipX = moveInput.x < 0;
            }
        }

        private void HandleAttack()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (canAttack && !isAttacking)
                {
                    Debug.Log("Attack initiated"); // Debug attack start
                    StartCoroutine(PerformAttack());
                }
            }
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, moveInput * moveSpeed, Time.fixedDeltaTime * acceleration);
        }

        private System.Collections.IEnumerator PerformAttack()
        {
            isAttacking = true;
            canAttack = false;

            // Get all enemies in range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
            Debug.Log($"OverlapCircleAll found {hitEnemies.Length} enemies"); // Debug enemies found

            foreach (Collider2D enemyCollider in hitEnemies)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log($"Attacking enemy: {enemy.name}"); // Debug enemy hit
                    enemy.TakeDamage(attackDamage);
                }
            }

            // Visual feedback
            StartCoroutine(AttackVisualFeedback());

            yield return new WaitForSeconds(0.2f);
            isAttacking = false;

            yield return new WaitForSeconds(attackCooldown - 0.2f);
            canAttack = true;
        }

        private System.Collections.IEnumerator AttackVisualFeedback()
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        private void OnDrawGizmos()
        {
            // Draw attack range
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw lines to enemies in range during play mode
            if (Application.isPlaying && enemyLayer != 0)
            {
                Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
                foreach (var enemy in nearbyEnemies)
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    Gizmos.color = distance <= attackRange ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }
} 