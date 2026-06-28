using UnityEngine;
using UnityEngine.InputSystem;

public class CardMovement : MonoBehaviour
{
    [Header("Kart Durumu")]
    public bool isPlacedOnSlot = false;

    [Header("Geri Dönüş Hafızası")]
    public Vector3 originalHandPosition;

    [Header("Titreme Efekti (Pusula)")]
    public bool isShaking = false; // Kart şu an titriyor mu?
    public float shakeSpeed = 25f; // Titreme hızı
    public float shakeAmount = 3f; // Ne kadar şiddetli titreyeceği (Açı)
    private Quaternion originalRotation; // Kartın ilk düz açısı

    private bool isDragging = false;
    private Vector3 startPosition;
    private CardDisplay cardDisplay;
    private Collider cardCollider;
    private int cardLayerMask;
    private float zCoord;
    private Vector3 offset;

    void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
        cardCollider = GetComponent<Collider>();
        startPosition = transform.position;
        cardLayerMask = LayerMask.GetMask("Card");

        originalHandPosition = transform.position;

        // Kartın oyun başındaki düz duruş açısını hafızaya al
        originalRotation = transform.rotation;
    }

    void Update()
    {
        // === TİTREME MOTORU ===
        // Eğer kartın titremesi gerekiyorsa ve o an fareyle sürüklenmiyorsa salla
        if (isShaking && !isDragging)
        {
            float angle = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, angle);
        }
        else if (!isShaking && transform.rotation != originalRotation && !isDragging)
        {
            // Titremesi bittiyse ve düz değilse, açıyı sıfırla
            transform.rotation = originalRotation;
        }
        // =======================

        if (GameManager.Instance.currentTurn == TurnOwner.NPC)
        {
            if (isDragging) HandleMouseUp();
            return;
        }

        Mouse currentMouse = Mouse.current;
        if (currentMouse == null) return;

        Vector2 mousePosition = currentMouse.position.ReadValue();

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cardLayerMask))
            {
                if (hit.collider == cardCollider)
                {
                    if (isPlacedOnSlot)
                    {
                        GameManager.Instance.CardClicked(cardDisplay, cardDisplay.cardOwner);
                    }
                    else
                    {
                        if (cardDisplay != null && cardDisplay.cardData != null)
                        {
                            string currentCardName = cardDisplay.cardData.cardName.Trim().ToLower();

                            if (currentCardName == "kayıp amiral" && !GameManager.Instance.isAdmiralForced)
                            {
                                Debug.LogWarning("Önce Amiralin Pusulası strateji kartını kullanmalısın!");
                                return;
                            }

                            if (GameManager.Instance.isAdmiralForced && currentCardName != "kayıp amiral")
                            {
                                Debug.LogWarning("Zorunlu olarak Kayıp Amiral'i sahaya sürmelisin!");
                                return;
                            }
                        }

                        if (!GameManager.Instance.canPlayerPlaceCard)
                        {
                            Debug.Log("Kart koyma hakkınız bitti veya 6 karta ulaştınız! Önce saldırmalısınız.");
                            return;
                        }

                        isDragging = true;

                        // Sürüklemeye başlandığı an titremeyi iptal et ve kartı düzelt
                        transform.rotation = originalRotation;

                        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
                        offset = transform.position - GetMouseWorldPos(mousePosition);
                    }
                }
            }
        }

        if (isDragging && currentMouse.leftButton.isPressed)
        {
            Vector3 targetPosition = GetMouseWorldPos(mousePosition) + offset;
            transform.position = new Vector3(targetPosition.x, startPosition.y + 0.1f, targetPosition.z);
        }

        if (isDragging && currentMouse.leftButton.wasReleasedThisFrame)
        {
            HandleMouseUp();
        }
    }

    private Vector3 GetMouseWorldPos(Vector2 mousePos)
    {
        Vector3 mousePoint = new Vector3(mousePos.x, mousePos.y, zCoord);
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void HandleMouseUp()
    {
        isDragging = false;
        CheckForNearestSlot();
    }

    void CheckForNearestSlot()
    {
        BoardSlot closestSlot = null;
        float closestDistance = Mathf.Infinity;

        foreach (BoardSlot slot in GameManager.Instance.playerSlots)
        {
            if (slot.isOccupied) continue;

            float distance = Vector3.Distance(transform.position, slot.transform.position);
            if (distance < closestDistance && distance < 1.5f)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        if (closestSlot != null)
        {
            closestSlot.PlaceCard(cardDisplay);
            startPosition = transform.position;
            isPlacedOnSlot = true;

            // KART MASAYA KONULDU: Titremeyi tamamen durdur
            isShaking = false;
            transform.rotation = originalRotation;

            GameManager.Instance.CardPlaced(TurnOwner.Player);
        }
        else
        {
            transform.position = startPosition;
            Debug.LogWarning("Kart yuvaya oturtulamadı! Ya masaya çok uzak bıraktın ya da slotlar hala dolu.");
        }
    }

    public void ReturnToHand()
    {
        if (isPlacedOnSlot && cardDisplay != null && cardDisplay.assignedSlot != null)
        {
            cardDisplay.assignedSlot.isOccupied = false;
            cardDisplay.assignedSlot.currentCard = null;
            cardDisplay.assignedSlot = null;
        }

        transform.position = originalHandPosition;
        startPosition = originalHandPosition;
        isPlacedOnSlot = false;

        // KART ELİMİZE DÖNDÜ: Titremeyi Başlat!
        isShaking = true;
    }
}