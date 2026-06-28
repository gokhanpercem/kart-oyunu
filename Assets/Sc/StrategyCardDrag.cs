using UnityEngine;

public class StrategyCardDrag : MonoBehaviour
{
    [Header("Strateji Kartının Adı")]
    [Tooltip("Örn: Serum Askısı, Antik Ameliyat Masası, Pusula")]
    public string strategyName;

    private Vector3 startPosition;
    private Camera mainCamera;
    private float zDistance;
    private bool isDragging = false;

    void Start()
    {
        // Kartın raftaki ilk yerini kaydediyoruz ki işi bitince geri dönsün
        startPosition = transform.position;
        mainCamera = Camera.main;

        // 3D dünyada sürüklerken derinlik (Z) kaybolmasın diye mesafeyi ölçüyoruz
        zDistance = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    void OnMouseDown()
    {
        Debug.Log(strategyName + " kartına tıklandı!");

        if (GameManager.Instance.currentTurn != TurnOwner.Player)
        {
            Debug.LogWarning("Kartı çekemezsin: Sıra sende değil!");
            return;
        }
        if (!GameManager.Instance.canPlayerAttack)
        {
            Debug.LogWarning("Kartı çekemezsin: Saldırı/Aksiyon hakkın yok!");
            return;
        }

        isDragging = true;
        Debug.Log(strategyName + " sürükleniyor...");
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        // Farenin 3D dünyadaki yerini hesapla ve kartı oraya taşı
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance);
        transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int cardLayerMask = LayerMask.GetMask("Card");
        CardDisplay targetCard = null;

        // DİKKAT: Artık Raycast değil, RaycastAll kullanıyoruz. 
        // Fare imlecinin altındaki TÜM objeleri delip geçerek bir liste alıyoruz.
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, cardLayerMask);

        foreach (RaycastHit hit in hits)
        {
            // Eğer lazer "kendimize" (elimizde tuttuğumuz beyaz karta) ÇARPMADIYSA:
            if (hit.collider.gameObject != this.gameObject)
            {
                // Demek ki altımızdaki gerçek asker kartını bulduk!
                targetCard = hit.collider.GetComponent<CardDisplay>();
                break; // Askeri bulduğumuz için aramayı durdur.
            }
        }

        // Koşulları kontrol et ve etkiyi çalıştır
        bool success = TryActivateStrategy(targetCard);



        // Beyaz kartı yerine geri yolla
        transform.position = startPosition;

        // Başarılıysa turu geçir
        if (success)
        {
            GameManager.Instance.ActionExecuted();
        }
    }

    private bool TryActivateStrategy(CardDisplay targetCard)
    {
        // --- 1. SERUM ASKISI ---
        if (strategyName == "Serum Askısı")
        {
            if (!GameManager.Instance.IsCharacterOnBoard("Evelyn", TurnOwner.Player))
            {
                Debug.LogWarning("Sahada Evelyn olmadığı için Serum Askısı kullanılamaz!");
                return false;
            }
            if (targetCard == null || targetCard.cardOwner != TurnOwner.Player) return false;

            // Kartta zaten serum varsa ikinci kez kullanılmasını engelle (Opsiyonel ama mantıklı)
            if (targetCard.hasSerumActive)
            {
                Debug.LogWarning("Bu karta zaten serum bağlı!");
                return false;
            }

            Debug.Log($"{targetCard.cardData.cardName} kartına Serum bağlandı! Artık her tur güçlenecek.");
            targetCard.hasSerumActive = true; // Karta serum bağladık
            targetCard.ApplySerumEffect();    // İlk turun etkisini hemen ver
            return true;
        }

        // --- 2. ANTİK AMELİYAT MASASI ---
        else if (strategyName == "Antik Ameliyat Masası")
        {
            if (!GameManager.Instance.IsCharacterOnBoard("Dr. Morrow", TurnOwner.Player))
            {
                Debug.LogWarning("Sahada Dr. Morrow olmadığı için kullanılamaz!");
                return false;
            }
            if (targetCard == null || targetCard.cardOwner != TurnOwner.Player) return false;

            Debug.Log($"{targetCard.cardData.cardName} ameliyat edildi! Gücü 1.5 katına çıktı.");
            targetCard.currentAttack = Mathf.FloorToInt(targetCard.currentAttack * 1.5f);
            targetCard.currentHealth = Mathf.FloorToInt(targetCard.currentHealth * 1.5f);
            targetCard.UpdateUI();
            return true;
        }

        // --- 3. PUSULA (Saha Boşaltma) ---
        else if (strategyName == "Pusula")
        {
            if (!GameManager.Instance.IsCharacterOnBoard("Alexandria", TurnOwner.Player))
            {
                Debug.LogWarning("Sahada Alexandria olmadığı için Pusula kullanılamaz!");
                return false;
            }

            // Normalde strateji kartları bir hedefin üstüne bırakılır ama Pusula tüm sahaya etki eder.
            // Bu yüzden hedef askerin kim olduğu önemli değil, herhangi bir yere veya bir karta bırakılması yeterli.
            GameManager.Instance.RecallAllPlayerCards();
            return false;
        }

        // --- 4. AMİRALİN PUSULASI ---
        else if (strategyName == "Amiralin Pusulası")
        {
            if (!GameManager.Instance.IsCharacterOnBoard("Alexandria", TurnOwner.Player))
            {
                Debug.LogWarning("Sahada Alexandria olmadığı için Amiralin Pusulası kullanılamaz!");
                return false;
            }

            Debug.Log("Amiralin Pusulası kullanıldı! Bir sonraki el zorunlu olarak Kayıp Amiral yerleştirilecek.");

            // Kilidi açıyoruz (Zorunlu kılıyoruz)
            GameManager.Instance.isAdmiralForced = true;
            return true;
        }

        // --- 5. BOZUK PUSULA (Rakibi Karıştırma) ---
        else if (strategyName == "Bozuk Pusula")
        {
            if (!GameManager.Instance.IsCharacterOnBoard("Alexandria", TurnOwner.Player))
            {
                Debug.LogWarning("Sahada Alexandria olmadığı için Bozuk Pusula kullanılamaz!");
                return false;
            }

            GameManager.Instance.ShuffleNPCCards();
            return true;
        }

        return false;
    }
}