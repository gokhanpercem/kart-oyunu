using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public enum GamePhase { Round1_Placement, Round1_Combat, Round2_To_4_Action, Round5_EndlessWar, GameOver }
public enum TurnOwner { Player, NPC }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Oyun Sonu Arayüzü (UI)")]
    public GameObject gameOverPanel;
    public TMPro.TextMeshProUGUI gameOverText;

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
        Debug.Log("Oyun Başladı! 1. Tur: Sırayla 3'er kart yerleştirme.");
        canPlayerPlaceCard = true;
        canPlayerAttack = false;

        // Oyun başladığında emin olmak için GameOver panelini kapatıyoruz
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // Sadece oyuncu turundaysa ve saldırma izni varsa tıklamaları dinle
        if (currentTurn == TurnOwner.Player && canPlayerAttack && currentPhase != GamePhase.GameOver)
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
                    if (clickedCard != null && clickedCard.assignedSlot != null)
                    {
                        CardClicked(clickedCard, clickedCard.cardOwner);
                    }
                }
            }
        }
    }

    public bool IsValidTarget(CardDisplay target)
    {
        if (target == null || target.assignedSlot == null) return false;

        int targetSlot = target.assignedSlot.slotIndex;

        // EĞER OYUNCU NPC'YE SALDIRIYORSA
        if (targetSlot == 10) return !IsSlotOccupied(7);
        if (targetSlot == 11) return !IsSlotOccupied(8);
        if (targetSlot == 12) return !IsSlotOccupied(9);

        // EĞER NPC OYUNCUYA SALDIRIYORSA
        if (targetSlot == 1) return !IsSlotOccupied(4);
        if (targetSlot == 2) return !IsSlotOccupied(5);
        if (targetSlot == 3) return !IsSlotOccupied(6);

        return true;
    }

    private bool IsSlotOccupied(int slotIndex)
    {
        foreach (var slot in playerSlots)
            if (slot != null && slot.slotIndex == slotIndex) return slot.isOccupied;

        foreach (var slot in npcSlots)
            if (slot != null && slot.slotIndex == slotIndex) return slot.isOccupied;

        return false;
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
            if (playerPlacedCount >= 3 && npcPlacedCount >= 3)
            {
                Debug.Log("1. Tur yerleştirmeleri bitti! Şimdi karşılıklı 1'er kez saldırı fazı.");
                currentPhase = GamePhase.Round1_Combat;
                canPlayerPlaceCard = false;
                canPlayerAttack = true;
                currentTurn = TurnOwner.Player;
            }
            else
            {
                SwitchTurn();
            }
        }
        else if (currentPhase == GamePhase.Round2_To_4_Action)
        {
            if (owner == TurnOwner.Player)
            {
                canPlayerAttack = true;
                Debug.Log("Kart yerleştirildi. Şimdi saldırmak için bir kartınızı seçin.");
            }
        }
    }

    public void ActionExecuted()
    {
        canPlayerAttack = false;
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        if (currentPhase == GamePhase.GameOver) return; // Oyun bittiyse tur değişimini durdur

        currentTurn = (currentTurn == TurnOwner.Player) ? TurnOwner.NPC : TurnOwner.Player;

        if (currentTurn == TurnOwner.Player)
        {
            if (currentPhase == GamePhase.Round1_Placement)
            {
                canPlayerPlaceCard = true;
                canPlayerAttack = false;
            }
            else if (currentPhase == GamePhase.Round1_Combat)
            {
                AdvanceRound();
                return;
            }
            else if (currentPhase == GamePhase.Round2_To_4_Action)
            {
                AdvanceRound();
                return;
            }
            else if (currentPhase == GamePhase.Round5_EndlessWar)
            {
                currentRound++;
                canPlayerPlaceCard = false;
                canPlayerAttack = true;
                Debug.Log($"{currentRound}. Tur (Sonsuz Savaş): Saldırı sırası sizde.");
            }
        }

        if (currentTurn == TurnOwner.NPC)
        {
            StartCoroutine(NPCTurnRoutine());
        }
    }

    private void AdvanceRound()
    {
        currentRound++;
        if (currentRound >= 5 || playerPlacedCount >= 6 || npcPlacedCount >= 6)
        {
            currentPhase = GamePhase.Round5_EndlessWar;
            canPlayerPlaceCard = false;
            canPlayerAttack = true;
            Debug.Log($"{currentRound}. Tur: Sonsuz Savaş Başladı! Artık kart koymak yok, sadece saldırı.");
        }
        else
        {
            currentPhase = GamePhase.Round2_To_4_Action;
            canPlayerPlaceCard = true;
            canPlayerAttack = false;
            Debug.Log($"{currentRound}. Tur: 1 Kart Koy + 1 Saldırı yap.");
        }
        currentTurn = TurnOwner.Player;
    }

    private IEnumerator NPCTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        // --- YERLEŞTİRME FAZI (NPC) ---
        if (currentPhase == GamePhase.Round1_Placement)
        {
            BoardSlot targetSlot = GetFirstEmptySlot(npcSlots);
            if (targetSlot != null) SpawnNPCCard(targetSlot);
            CardPlaced(TurnOwner.NPC);
            yield break;
        }
        else if (currentPhase == GamePhase.Round2_To_4_Action && npcPlacedCount < 6)
        {
            BoardSlot targetSlot = GetFirstEmptySlot(npcSlots);
            if (targetSlot != null) SpawnNPCCard(targetSlot);
            yield return new WaitForSeconds(0.5f);
        }

        // --- SALDIRI FAZI (NPC YAPAY ZEKASI) ---
        List<CardDisplay> npcAttackerPool = GetActiveCards(npcSlots);
        List<CardDisplay> validPlayerTargets = GetValidTargetsForNPC();

        if (npcAttackerPool.Count > 0 && validPlayerTargets.Count > 0)
        {
            CardDisplay npcAttacker = npcAttackerPool[Random.Range(0, npcAttackerPool.Count)];
            CardDisplay playerTarget = validPlayerTargets[Random.Range(0, validPlayerTargets.Count)];

            Debug.Log($"NPC, {npcAttacker.gameObject.name} ile sizin {playerTarget.gameObject.name} kartınıza saldırıyor!");
            yield return StartCoroutine(AttackAnimationRoutine(npcAttacker, playerTarget));
        }
        else
        {
            ActionExecuted();
        }
    }

    private List<CardDisplay> GetActiveCards(List<BoardSlot> slots)
    {
        List<CardDisplay> activeCards = new List<CardDisplay>();
        foreach (var slot in slots)
        {
            if (slot.isOccupied && slot.currentCard != null)
                activeCards.Add(slot.currentCard);
        }
        return activeCards;
    }

    private List<CardDisplay> GetValidTargetsForNPC()
    {
        List<CardDisplay> validTargets = new List<CardDisplay>();
        foreach (var slot in playerSlots)
        {
            if (slot.isOccupied && slot.currentCard != null)
            {
                if (IsValidTarget(slot.currentCard))
                    validTargets.Add(slot.currentCard);
            }
        }
        return validTargets;
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

    public void CardClicked(CardDisplay clickedCard, TurnOwner cardOwner)
    {
        if (currentTurn != TurnOwner.Player) return;

        CardMovement moveScript = clickedCard.GetComponent<CardMovement>();
        if (moveScript != null && !moveScript.isPlacedOnSlot) return;

        if (selectedAttackerCard == null && cardOwner == TurnOwner.Player)
        {
            selectedAttackerCard = clickedCard;
            Debug.Log($"Saldırmak için {clickedCard.cardData.cardName} seçildi. Şimdi bir NPC kartına tıklayın.");
        }
        else if (selectedAttackerCard != null && cardOwner == TurnOwner.NPC)
        {
            if (!IsValidTarget(clickedCard))
            {
                Debug.LogWarning("HATA: Önce ön sıradaki koruyan kartı yok etmelisiniz!");
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
        Vector3 startPos = attacker.transform.position;
        Vector3 targetPos = target.transform.position;
        Vector3 attackDestination = Vector3.MoveTowards(startPos, targetPos, Vector3.Distance(startPos, targetPos) - 0.5f);

        float elapsedTime = 0f;
        float duration = 0.15f;

        // 1. İleri Atılma
        while (elapsedTime < duration)
        {
            if (attacker == null) yield break;
            attacker.transform.position = Vector3.Lerp(startPos, attackDestination, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (attacker != null) attacker.transform.position = attackDestination;

        // 2. Sadece hedefe hasar ver (Tek taraflı)
        if (attacker != null && target != null)
        {
            target.TakeDamage(attacker.currentAttack);
        }

        yield return new WaitForSeconds(0.05f);

        // 3. Geri Dönüş
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

        // 4. Oyun Bitiş Kontrolü
        CheckGameOver();

        // 5. Oyun devam ediyorsa turu atlat
        if (currentPhase != GamePhase.GameOver)
        {
            ActionExecuted();
        }
    }

    // --- OYUN BİTİŞ KONTROLÜ ---
    public void CheckGameOver()
    {
        if (currentPhase == GamePhase.Round1_Placement)
            return;

        int playerAliveCards = GetActiveCards(playerSlots).Count;
        int npcAliveCards = GetActiveCards(npcSlots).Count;

        if (playerAliveCards <= 0)
        {
            currentPhase = GamePhase.GameOver;
            canPlayerAttack = false;
            canPlayerPlaceCard = false;

            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "OYUN BİTTİ\nTÜM KARTLARINIZ YOK EDİLDİ!\n[P]";
        }
        else if (npcAliveCards <= 0)
        {
            currentPhase = GamePhase.GameOver;
            canPlayerAttack = false;
            canPlayerPlaceCard = false;

            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "ZAFER!\nDÜŞMAN KARTLARI YOK EDİLDİ!\n[P]";
        }
    }
}