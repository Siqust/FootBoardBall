using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "PlayerCard", order = 1)]
public class PlayerCard : ScriptableObject
{
    public CardType cardtype;
    public PlayerAbility playerability;
    public int attack;
    public int defence;
}