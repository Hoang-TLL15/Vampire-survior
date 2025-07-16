using UnityEngine;

public class BossSkillAttack : MonoBehaviour
{
    public BoxCollider2D attackCollider; // K�o collider v�o Inspector n?u kh�ng t? l?y
    public float lifeTime = 1f; // Th?i gian t?n t?i c?a skill

    void Start()
    {
        if (attackCollider == null)
            attackCollider = GetComponent<BoxCollider2D>();
        attackCollider.enabled = false; // T?t collider l�c ??u
        Destroy(gameObject, lifeTime); // T? h?y sau khi animation xong
    }

    // H�m n�y s? ???c g?i b?i Animation Event
    public void EnableSkillCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    // H�m n�y s? ???c g?i b?i Animation Event
    public void DisableSkillCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by boss attack!");
            PlayerStats player = other.gameObject.GetComponent<PlayerStats>();
            player.TakeDamage(10);
            if (player.CurrentHealth <= 0)
            {
                PlayerStats playerStats = FindObjectOfType<PlayerStats>();
                PlayerInventory inventory = playerStats.GetComponent<PlayerInventory>();
                GameManager.instance.AssignLevelReachedUI(playerStats.level);
                GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
                Debug.Log("You have completed all the waves!");
                GameManager.instance.GameOver();
            }
            // Ensure health does not go below zero
        }
    }
}