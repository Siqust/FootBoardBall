using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "PlayerCard", order = 1)]
public class PlayerCardObj : ScriptableObject
{
    public PlayerAbility playerability;
    public int attack;
    public int defence;
}