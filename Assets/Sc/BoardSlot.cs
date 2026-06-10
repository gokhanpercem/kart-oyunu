using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public int slotIndex; // 1 ile 12 arasında hangi slot olduğu
    public bool isOccupied = false; // Bu slotta kart var mı?
    public CardDisplay currentCard; // Şu an bu slotta duran kartın kodu

    // Slota kart yerleştirildiğinde çağrılacak
    public void PlaceCard(CardDisplay card)
    {
        currentCard = card;
        isOccupied = true;
        // Kartı tam olarak bu slotun merkezine ışınla/yerleştir
        // Kartı slotun koordinatına yerleştir ama masanın üstünde durması için Y ekseninde yukarı kaldır!
        // Buradaki 0.2f değerini kartın kalınlığına göre 0.1f veya 0.3f gibi değiştirebilirsin.
        card.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        
        
    }

    // Kart öldüğünde veya yer değiştirdiğinde slotu boşaltmak için
    public void ClearSlot()
    {
        currentCard = null;
        isOccupied = false;
    }
}