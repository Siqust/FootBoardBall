using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "ActionCard", order = 1)]
public class ActionCard : ScriptableObject
{
    public CardType cardtype;
    public int modif;
}