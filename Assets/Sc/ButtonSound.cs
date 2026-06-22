using UnityEngine;
using UnityEngine.EventSystems; // Fare etkileşimlerini algılamak için şart!

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Ses Efektleri")]
    public AudioClip hoverSound; // Üzerine gelince çalacak ses
    public AudioClip clickSound; // Tıklayınca çalacak ses

    private AudioSource uiAudioSource;

    void Start()
    {
        // Sahnede sesleri çalacak bir AudioSource arıyoruz (Genelde Main Camera'da olur)
        if (Camera.main != null)
        {
            uiAudioSource = Camera.main.GetComponent<AudioSource>();

            // Eğer kamerada AudioSource yoksa, hata vermemesi için kodla otomatik ekliyoruz
            if (uiAudioSource == null)
            {
                uiAudioSource = Camera.main.gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // Fare imleci butonun ÜZERİNE GELDİĞİNDE çalışır
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    // Fare ile butona TIKLANDIĞINDA (basıldığı an) çalışır
    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
}