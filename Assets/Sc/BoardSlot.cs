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

        // KARTIN KENDİSİNE HANGİ SLOTTA OLDUĞUNU BİLDİRİYORUZ (Yeni eklendi)
        card.assignedSlot = this;

        // Kartı slotun koordinatına yerleştir ama masanın üstünde durması için Y ekseninde yukarı kaldır
        card.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
    }

    // Kart öldüğünde veya yer değiştirdiğinde slotu boşaltmak için
    public void ClearSlot()
    {
        currentCard = null;
        isOccupied = false;
    }
}