using UnityEngine;
using UnityEngine.InputSystem;

public class CardMovement : MonoBehaviour
{
    [Header("Kart Durumu")]
    public bool isPlacedOnSlot = false;

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
    }

    void Update()
    {
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
                        // KURAL KİLİDİ: GameManager "artık kart koyamazsın" diyorsa sürüklemeyi reddet!
                        if (!GameManager.Instance.canPlayerPlaceCard)
                        {
                            Debug.Log("Kart koyma hakkınız bitti veya 6 karta ulaştınız! Önce saldırmalısınız.");
                            return;
                        }

                        isDragging = true;
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

            GameManager.Instance.CardPlaced(TurnOwner.Player);
        }
        else
        {
            transform.position = startPosition;
        }
    }
}