using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem; // YENİ: Yeni klavye sistemi için eklendi

public class TableInteract : MonoBehaviour
{
    [Header("Kameralar ve Karakter")]
    public GameObject firstPersonCamera;
    public GameObject boardCamera;
    public GameObject playerCapsule;

    [Header("Masa Düzeni")]
    public GameObject cardSetupParent;

    [Header("Ayarlar")]
    public float interactDistance = 15f;  // DİKKAT: Odan devasa olduğu için bu mesafeyi 4'ten 15'e çıkardık!
    private bool isPlayingCards = false;

    void Start()
    {
        if (cardSetupParent != null) cardSetupParent.SetActive(false);
        if (boardCamera != null) boardCamera.SetActive(false);
    }

    void Update()
    {
        // YENİ: Yeni Input System ile P tuşunu dinliyoruz
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            float distance = Vector3.Distance(playerCapsule.transform.position, transform.position);

            if (distance <= interactDistance && !isPlayingCards)
            {
                StartCardGame();
            }
            else if (isPlayingCards)
            {
                ExitCardGame();
            }
            else
            {
                Debug.Log($"Masaya çok uzaksın! Şu anki mesafe: {distance}. Yaklaşman gereken: {interactDistance}");
            }
        }
    }

    public void StartCardGame()
    {
        isPlayingCards = true;

        firstPersonCamera.SetActive(false);
        boardCamera.SetActive(true);
        cardSetupParent.SetActive(true);

        playerCapsule.GetComponent<FirstPersonController>().enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitCardGame()
    {
        isPlayingCards = false;

        boardCamera.SetActive(false);
        firstPersonCamera.SetActive(true);
        cardSetupParent.SetActive(false);

        playerCapsule.GetComponent<FirstPersonController>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ==========================================
        // YENİ EKLENEN: Masadan kalkınca UI ekranını gizle!
        // ==========================================
        if (GameManager.Instance != null && GameManager.Instance.gameOverPanel != null)
        {
            GameManager.Instance.gameOverPanel.SetActive(false);
        }
    }
}