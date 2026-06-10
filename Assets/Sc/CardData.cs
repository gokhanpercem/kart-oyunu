using UnityEngine;

public enum CardType { None, NormalCombat, SpecialCombat, StatusCard }
public enum SynergyGroup { None, EvelynGroup, MorrowGroup, AlexandriaGroup, KumandanGroup }

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Advanced Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public SynergyGroup synergyGroup; // Kartın hangi gruba ait olduğunu belirler

    public int attackPower;
    public int healthPoints;
    public Sprite cardImage;

    [Header("Durum Kartı Etkileri")]
    public bool requiresSpecificCharacter; // Çalışmak için sahada birini ister mi?
    public bool altersLayout;             // Yer değiştirme tetikler mi?
    public bool forcesNextSpawn;          // Sonraki el zorunlu spawn yapar mı?
}