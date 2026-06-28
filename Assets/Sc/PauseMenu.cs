using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Yeni klavye sistemi için
using StarterAssets; // Karakteri dondurabilmek için

public class PauseMenu : MonoBehaviour
{
    [Header("Arayüz Elemanları")]
    public GameObject pauseMenuPanel;
    public GameObject playerCapsule;

    private bool isPaused = false;
    private bool wasControllerEnabled = true;

    void Start()
    {
        // Oyun başında menünün kapalı olduğundan emin olalım
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // ESC tuşuna basıldığında
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Zamanı normale döndür

        // Karakter yürüyüş modundaysa kontrolleri geri aç ve fareyi gizle
        var controller = playerCapsule.GetComponent<FirstPersonController>();
        if (controller != null && wasControllerEnabled)
        {
            controller.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Dünyadaki tüm zamanı (ve yapay zekayı) dondur

        // Karakterin o anki yürüme durumunu kaydet ve yürümeyi dondur
        var controller = playerCapsule.GetComponent<FirstPersonController>();
        if (controller != null)
        {
            wasControllerEnabled = controller.enabled;
            controller.enabled = false;
        }

        // Fare imlecini menüyü tıklayabilmemiz için serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // ÇOK ÖNEMLİ: Zamanı çözmezsek ana menü de donuk kalır!
        SceneManager.LoadScene(0); // Build settings'deki 0 indeksli sahneyi (MainMenu) açar
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit(); // Derlenmiş oyunu kapatır
    }
}