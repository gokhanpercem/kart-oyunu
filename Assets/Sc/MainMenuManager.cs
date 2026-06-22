using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menü Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject aboutPanel;    // Eski SettingsPanel
    public GameObject gameplayPanel; // Yeni Oynanış Paneli
    public GameObject storyPanel;    // Yeni Hikaye Paneli

    void Start()
    {
        // Oyun açıldığında sadece Ana Menü açık olsun
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (storyPanel != null) storyPanel.SetActive(false);
    }

    // --- ANA MENÜ BUTONLARI ---
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenAboutMenu()
    {
        mainMenuPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit();
    }

    // --- "OYUN HAKKINDA" MENÜSÜ BUTONLARI ---
    public void OpenGameplay()
    {
        aboutPanel.SetActive(false);
        gameplayPanel.SetActive(true);
    }

    public void OpenStory()
    {
        aboutPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    public void CloseAboutMenu() // Geri butonu (Ana menüye döner)
    {
        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- ALT PANELLERİN GERİ BUTONLARI ---
    public void CloseGameplay() // Oynanıştan Oyun Hakkında'ya döner
    {
        gameplayPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void CloseStory() // Hikayeden Oyun Hakkında'ya döner
    {
        storyPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }
}