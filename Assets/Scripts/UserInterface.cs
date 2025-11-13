using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    // Components
    private Slider healthBar;
    private Slider manaBar;
    private Slider staminaBar;

    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        
        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        healthBar.maxValue = player.maxHealth;
        healthBar.value = player.GetHealth();

        manaBar = GameObject.Find("Mana Bar").GetComponent<Slider>();
        manaBar.maxValue = player.maxMana;
        manaBar.value = player.GetMana();

        staminaBar = GameObject.Find("Stamina Bar").GetComponent<Slider>();
        staminaBar.maxValue = player.maxStamina;
        staminaBar.value = player.GetStamina();
        
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = player.GetHealth();
        manaBar.value = player.GetMana();
        staminaBar.value = player.GetStamina();
    }
}
