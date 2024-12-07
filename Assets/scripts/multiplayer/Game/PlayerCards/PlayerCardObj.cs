using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Player", menuName = "PlayerCard", order = 1)]
public class PlayerCardObj : ScriptableObject
{
    public PlayerAbility playerability;
    public Texture CardImg;
    public Color color;
    public int attack;
    public int defence;
}