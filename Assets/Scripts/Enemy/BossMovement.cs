using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    EnemyStats enemy; // Add this - this is what makes the boss killable
    Transform player;
    Vector2 knockbackVelocity;
    float knockbackDuration;

    [Header("Combat Settings")]
    public float attackRangeX = 3f; // Tầm tấn công theo chiều ngang (3)
    public float attackRangeY = 1.5f; // Tầm tấn công theo chiều dọc
    public float attackDuration = 0.7f; // Thời gian tấn công
    public float attackCooldown = 1f; // Thời gian nghỉ giữa các đợt tấn công

    [Header("Animation")]
    public Animator animator;

    [Header("Attack Collider")]
    public PolygonCollider2D attackCollider; // Reference to the attack collider

    [Header("States")]
    public bool isAttacking = false;
    public bool isOnCooldown = false;

    [Header("Skill Attack")]
    public GameObject skillAttackPrefab; // Kéo prefab vào Inspector

    // Facing direction
    private bool facingRight = true;
    private Vector3 originalScale;

    void Start()
    {
        enemy = GetComponent<EnemyStats>(); // Get the EnemyStats component
        player = FindObjectOfType<PlayerMovement>().transform;

        // Lấy Animator nếu chưa được gán
        if (animator == null)
            animator = GetComponent<Animator>();

        // Lưu scale gốc để flip
        originalScale = transform.localScale;

        if (attackCollider == null)
            attackCollider = GetComponent<PolygonCollider2D>();
        attackCollider.enabled = false; // Disable at start

        StartCoroutine(AutoSkillAttackRoutine());
    }

    void Update()
    {
        if(enemy.currentHealth <= 0f)
        {
            Destroy(gameObject);
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            PlayerInventory inventory = playerStats.GetComponent<PlayerInventory>();
            GameManager.instance.AssignLevelReachedUI(playerStats.level);
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
            Debug.Log("You have completed all the waves!");
            GameManager.instance.GameOver(); // Notify the game manager that the boss is killed
        }

        // If we are currently being knocked back, then process the knockback.
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            // Luôn quay mặt về player
            FacePlayer();

            // Kiểm tra player có trong tầm tấn công hình chữ nhật không
            if (IsInAttackRange() && !isAttacking && !isOnCooldown)
            {
                StartCoroutine(AttackSequence());
            }
            // Nếu không tấn công thì di chuyển về phía player
            else if (!isAttacking && !isOnCooldown)
            {
                MoveTowardsPlayer();
                SetWalkAnimation();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Di chuyển về phía player using enemy.currentMoveSpeed
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            enemy.currentMoveSpeed * Time.deltaTime
        );
    }

    void FacePlayer()
    {
        // Tính toán hướng đến player
        Vector2 direction = player.position - transform.position;

        // Nếu player ở bên phải và đang quay trái
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        // Nếu player ở bên trái và đang quay phải
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = originalScale;
        newScale.x *= facingRight ? -1 : 1;
        transform.localScale = newScale;
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Chuyển sang animation attack
        SetAttackAnimation();

        // Chờ trong thời gian tấn công (boss không di chuyển)
        yield return new WaitForSeconds(attackDuration);

        // Disable attack collider
        if (attackCollider != null)
            attackCollider.enabled = false;

        // Kết thúc tấn công
        isAttacking = false;
        isOnCooldown = true;

        // Chuyển về animation walk
        SetWalkAnimation();

        // Chờ cooldown
        yield return new WaitForSeconds(attackCooldown);

        // Kết thúc cooldown
        isOnCooldown = false;
    }

    void SetWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isAttacking", false);
        }
    }

    void SetAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);
        }
    }

    // This is meant to be called from other scripts to create knockback.
    public void Knockback(Vector2 velocity, float duration)
    {
        // Ignore the knockback if the duration is greater than 0.
        if (knockbackDuration > 0) return;

        // Begins the knockback.
        knockbackVelocity = velocity;
        knockbackDuration = duration;

        // Ngừng tấn công nếu đang tấn công
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            isOnCooldown = false;
            SetWalkAnimation();
        }
    }

    // Hàm tiện ích để kiểm tra trạng thái
    public bool IsInAttackRange()
    {
        Vector2 distanceToPlayer = player.position - transform.position;

        // Kiểm tra player có trong hình chữ nhật không
        return Mathf.Abs(distanceToPlayer.x) <= attackRangeX &&
               Mathf.Abs(distanceToPlayer.y) <= attackRangeY;
    }

    public bool IsPlayerOnRight()
    {
        return player.position.x > transform.position.x;
    }

    public bool IsPlayerOnLeft()
    {
        return player.position.x < transform.position.x;
    }

    public void EnableAttackCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    IEnumerator AutoSkillAttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f); // Mỗi 3 giây

            if (player != null)
            {
                Instantiate(skillAttackPrefab, player.position, Quaternion.identity);
            }
        }
    }

    // Vẽ gizmos để debug
    void OnDrawGizmosSelected()
    {
        // Vẽ tầm tấn công hình chữ nhật
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(attackRangeX * 2, attackRangeY * 2, 0);
        Gizmos.DrawWireCube(center, size);
    }
}