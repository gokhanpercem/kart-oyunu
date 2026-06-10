using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public enum GamePhase { Round1_Placement, Round2_To_4_Action, Round5_EndlessWar, GameOver }
public enum TurnOwner { Player, NPC }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Oyunun Şu Anki Durumu")]
    public GamePhase currentPhase = GamePhase.Round1_Placement;
    public TurnOwner currentTurn = TurnOwner.Player;
    public int currentRound = 1;

    [Header("Kart Sayıcıları (Maks 6)")]
    public int playerPlacedCount = 0;
    public int npcPlacedCount = 0;

    [Header("Tur İçi İzin Kilitleri")]
    public bool canPlayerPlaceCard = true;
    public bool canPlayerAttack = false;

    [Header("Tahtadaki Slotlar")]
    public List<BoardSlot> playerSlots = new List<BoardSlot>();
    public List<BoardSlot> npcSlots = new List<BoardSlot>();

    [Header("NPC Kart Havuzu")]
    public List<GameObject> npcCardPool = new List<GameObject>();

    [Header("Savaş Sistemi")]
    public CardDisplay selectedAttackerCard;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("Oyun Başladı! 1. Tur: Sırayla 3'er kart.");
        canPlayerPlaceCard = true;
        canPlayerAttack = false;
    }

    void Update()
    {
        if (currentPhase != GamePhase.Round1_Placement && currentTurn == TurnOwner.Player)
        {
            Mouse currentMouse = Mouse.current;
            if (currentMouse != null && currentMouse.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = currentMouse.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                int cardLayerMask = LayerMask.GetMask("Card");

                if (Physics.Raycast(ray, out RaycastHit hit, 100f, cardLayerMask))
                {
                    CardDisplay clickedCard = hit.collider.GetComponent<CardDisplay>();
                    if (clickedCard != null) CardClicked(clickedCard, clickedCard.cardOwner);
                }
            }
        }
    }

    public void CardPlaced(TurnOwner owner)
    {
        if (owner == TurnOwner.Player)
        {
            playerPlacedCount++;
            canPlayerPlaceCard = false;
        }
        else
        {
            npcPlacedCount++;
        }

        if (currentPhase == GamePhase.Round1_Placement)
        {
            if (playerPlacedCount >= 3 && npcPlacedCount >= 3) AdvanceRound();
            else SwitchTurn();
        }
        else if (currentPhase == GamePhase.Round2_To_4_Action)
        {
            if (playerPlacedCount >= 6)
            {
                currentPhase = GamePhase.Round5_EndlessWar;
                canPlayerPlaceCard = false;
                Debug.Log("6 Karta ulaşıldı! Sonsuz savaş devrede.");
            }

            if (owner == TurnOwner.Player && !canPlayerPlaceCard && !canPlayerAttack)
            {
                SwitchTurn();
            }
        }
    }

    public void ActionExecuted()
    {
        if (currentTurn == TurnOwner.Player)
        {
            canPlayerAttack = false;

            if (currentPhase == GamePhase.Round2_To_4_Action)
            {
                if (!canPlayerPlaceCard && !canPlayerAttack) SwitchTurn();
            }
            else if (currentPhase == GamePhase.Round5_EndlessWar)
            {
                SwitchTurn();
            }
        }
        else
        {
            SwitchTurn();
        }
    }

    public void SwitchTurn()
    {
        currentTurn = (currentTurn == TurnOwner.Player) ? TurnOwner.NPC : TurnOwner.Player;

        if (currentTurn == TurnOwner.Player)
        {
            if (currentPhase == GamePhase.Round1_Placement)
            {
                canPlayerPlaceCard = (playerPlacedCount < 3);
                canPlayerAttack = false;
            }
            else if (currentPhase == GamePhase.Round2_To_4_Action)
            {
                canPlayerPlaceCard = (playerPlacedCount < 6);
                canPlayerAttack = true;
            }
            else if (currentPhase == GamePhase.Round5_EndlessWar)
            {
                canPlayerPlaceCard = false;
                canPlayerAttack = true;
            }
        }

        if (currentTurn == TurnOwner.NPC)
        {
            StartCoroutine(NPCTurnRoutine());
        }
    }

    private IEnumerator NPCTurnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (npcCardPool.Count == 0 || npcSlots.Count == 0)
        {
            Debug.LogError("DİKKAT: NPC Kart Havuzu veya Slotları boş! Unity Inspector'dan tekrar bağlayın.");
        }

        if (currentPhase == GamePhase.Round1_Placement)
        {
            BoardSlot targetSlot = GetFirstEmptySlot(npcSlots);
            if (targetSlot != null) SpawnNPCCard(targetSlot);

            CardPlaced(TurnOwner.NPC); // Kilit kırıcı: İşlem bitince sırayı zorla devret
        }
        else if (currentPhase == GamePhase.Round2_To_4_Action)
        {
            if (npcPlacedCount < 6)
            {
                BoardSlot targetSlot = GetFirstEmptySlot(npcSlots);
                if (targetSlot != null) SpawnNPCCard(targetSlot);
                yield return new WaitForSeconds(0.5f);
            }
            ActionExecuted(); // Kilit kırıcı
        }
        else if (currentPhase == GamePhase.Round5_EndlessWar)
        {
            ActionExecuted(); // Kilit kırıcı
        }
    }

    private BoardSlot GetFirstEmptySlot(List<BoardSlot> slots)
    {
        foreach (BoardSlot slot in slots)
            if (slot != null && !slot.isOccupied) return slot;
        return null;
    }

    private void SpawnNPCCard(BoardSlot targetSlot)
    {
        if (npcCardPool.Count > 0)
        {
            int randomIndex = Random.Range(0, npcCardPool.Count);
            GameObject spawnedCard = Instantiate(npcCardPool[randomIndex], targetSlot.transform.position + Vector3.up * 0.2f, targetSlot.transform.rotation);
            CardDisplay display = spawnedCard.GetComponent<CardDisplay>();
            if (display != null)
            {
                targetSlot.PlaceCard(display);
                CardMovement npcMovement = spawnedCard.GetComponent<CardMovement>();
                if (npcMovement != null) npcMovement.isPlacedOnSlot = true;
            }
        }
    }

    public void AdvanceRound()
    {
        currentRound++;
        currentPhase = GamePhase.Round2_To_4_Action;
        currentTurn = TurnOwner.Player;

        canPlayerPlaceCard = true;
        canPlayerAttack = true;
        Debug.Log("2. Tura Geçildi! 1 Kart Koy + 1 Savaş hakkınız var.");
    }

    public void CardClicked(CardDisplay clickedCard, TurnOwner cardOwner)
    {
        if (currentTurn != TurnOwner.Player) return;

        CardMovement moveScript = clickedCard.GetComponent<CardMovement>();
        if (moveScript != null && !moveScript.isPlacedOnSlot) return;

        if (selectedAttackerCard == null && cardOwner == TurnOwner.Player)
        {
            selectedAttackerCard = clickedCard;
        }
        else if (selectedAttackerCard != null && cardOwner == TurnOwner.NPC)
        {
            if (!canPlayerAttack)
            {
                Debug.Log("Bu el saldırı hakkınız bitti!");
                selectedAttackerCard = null;
                return;
            }

            StartCoroutine(AttackAnimationRoutine(selectedAttackerCard, clickedCard));
            selectedAttackerCard = null;
        }
        else if (cardOwner == TurnOwner.Player)
        {
            selectedAttackerCard = clickedCard;
        }
    }

    private IEnumerator AttackAnimationRoutine(CardDisplay attacker, CardDisplay target)
    {
        canPlayerAttack = false;

        Vector3 startPos = attacker.transform.position;
        Vector3 targetPos = target.transform.position;
        Vector3 attackDestination = Vector3.MoveTowards(startPos, targetPos, Vector3.Distance(startPos, targetPos) - 0.5f);

        float elapsedTime = 0f;
        float duration = 0.15f;

        while (elapsedTime < duration)
        {
            if (attacker == null) yield break;
            attacker.transform.position = Vector3.Lerp(startPos, attackDestination, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (attacker != null) attacker.transform.position = attackDestination;

        if (attacker != null && target != null)
        {
            target.TakeDamage(attacker.currentAttack);
            attacker.TakeDamage(target.currentAttack);
        }

        yield return new WaitForSeconds(0.05f);

        elapsedTime = 0f;
        float returnDuration = 0.2f;

        while (elapsedTime < returnDuration)
        {
            if (attacker == null) break;
            attacker.transform.position = Vector3.Lerp(attackDestination, startPos, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (attacker != null) attacker.transform.position = startPos;

        ActionExecuted();
    }
}