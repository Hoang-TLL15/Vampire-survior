using UnityEngine;

public class BossHit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by boss attack!");
            PlayerStats player = other.gameObject.GetComponent<PlayerStats>();
            player.TakeDamage(10);
            if(player.CurrentHealth<= 0)
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
