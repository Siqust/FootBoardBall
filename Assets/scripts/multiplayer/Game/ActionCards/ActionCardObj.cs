using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Action", menuName = "ActionCard", order = 1)]
public class ActionCardObj : ScriptableObject
{
    public ActionCardType cardtype;
    public ActionAbility ability;
    public Color color;
    public Texture CardImg;
    public int modif;
}