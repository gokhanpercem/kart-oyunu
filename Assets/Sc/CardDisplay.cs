using UnityEngine;
using TMPro;
using System.Collections; 

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public TurnOwner cardOwner = TurnOwner.Player;

    // ... kodun geri kalanı aynı şekilde devam ediyor ...

    [Header("Kartın Canlı İstatistikleri")]
    public int currentHealth;
    public int currentAttack;

    [Header("Kart Üzerindeki Arayüz Elemanları")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;

    [Header("Aktif Etkiler")]
    public bool hasSerumActive = false; // Kartta serum var mı?

    // KARTIN DURDUĞU SLOT REFERANSI (Yeni eklendi)
    [HideInInspector] public BoardSlot assignedSlot;

    void Start()
    {
        if (cardData != null)
        {
            currentHealth = cardData.healthPoints;
            currentAttack = cardData.attackPower;
            UpdateUI();
        }
    }

    // Kart hasar aldığında bu fonksiyonu çağıracağız
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // HASAR ALDIĞINDA TİTREMEYİ BAŞLAT (Yeni Eklendi)
        // 0.2 saniye boyunca, 0.1f şiddetinde sarsılır. Değerleri zevkine göre değiştirebilirsin.
        StartCoroutine(ShakeAnimation(0.2f, 0.1f));

        if (currentHealth <= 0)
        {
            // Kart ölmeden önce titremenin bitmesini istersen Die() fonksiyonunu
            // küçük bir gecikme ile çağırabilirsin, ancak şu anki haliyle de çalışacaktır.
            Die();
        }
    }

    public void UpdateUI()
    {
        if (attackText != null) attackText.text = currentAttack.ToString();
        if (healthText != null) healthText.text = currentHealth.ToString();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " öldü.");

        // KART ÖLÜRKEN DURDUĞU SLOTU BOŞALTIYOR (Yeni eklendi - Kritik Bug Çözümü)
        if (assignedSlot != null)
        {
            assignedSlot.ClearSlot();
        }

        Destroy(gameObject);
    }
    // --- HASAR SARSINTISI (SHAKE) EFEKTİ ---
    public IEnumerator ShakeAnimation(float duration, float magnitude)
    {
        // Kartın titremeye başlamadan önceki orijinal konumunu kaydediyoruz
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // X ve Z eksenlerinde rastgele küçük sapmalar oluşturuyoruz (Masaüstünde titremesi için)
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetZ = Random.Range(-1f, 1f) * magnitude;

            // Kartın Y (yükseklik) değerini bozmuyoruz ki masanın içine girmesin
            transform.position = new Vector3(originalPosition.x + offsetX, originalPosition.y, originalPosition.z + offsetZ);

            elapsed += Time.deltaTime;
            yield return null; // Bir sonraki frame'i bekle
        }

        // Süre bittiğinde kartı tam olarak eski orijinal yerine oturtuyoruz
        transform.position = originalPosition;
    }

    // Her tur başında GameManager bu fonksiyonu çağıracak
    public void ApplySerumEffect()
    {
        if (hasSerumActive)
        {
            currentAttack += 1;
            currentHealth += 1;
            UpdateUI();
            Debug.Log($"{cardData.cardName} serum sayesinde +1/+1 kazandı!");
        }
    }



}