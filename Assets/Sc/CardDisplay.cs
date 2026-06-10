using UnityEngine;
using TMPro; // TextMeshPro kullanacağımız için şart

public class CardDisplay : MonoBehaviour
{
    // Senin ScriptableObject verin
    public CardData cardData;
    public TurnOwner cardOwner = TurnOwner.Player; // Düşman kartı Prefab'inde bunu NPC yapacaksın

    [Header("Kartın Canlı İstatistikleri")]
    public int currentHealth;
    public int currentAttack;

    [Header("Kart Üzerindeki Arayüz Elemanları")]
    public TextMeshProUGUI attackText; // Canvas altındaki AttackText
    public TextMeshProUGUI healthText; // Canvas altındaki HealthText

    void Start()
    {
        if (cardData != null)
        {
            // Senin CardData içindeki tam isimlerle eşitledik:
            currentHealth = cardData.healthPoints;
            currentAttack = cardData.attackPower;

            // Ekrandaki yazıları ilk kez dolduruyoruz
            UpdateUI();
        }
    }

    // Kart hasar aldığında bu fonksiyonu çağıracağız
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Yazıları güncel tutan yardımcı fonksiyon
    public void UpdateUI()
    {
        if (attackText != null) attackText.text = currentAttack.ToString();
        if (healthText != null) healthText.text = currentHealth.ToString();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " öldü.");
        Destroy(gameObject);
    }
}