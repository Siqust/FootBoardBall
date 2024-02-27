using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputCardPlacement : MonoBehaviour, IDropHandler
{
    public CardType cardtype;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        PlayerCard card = dropped.GetComponent<MonoPlayerCard>().card;
        if (card.cardtype == cardtype)
        {
            DraggbleItem draggbleItem = dropped.GetComponent<DraggbleItem>();
            draggbleItem.ParentAfterDrag = transform;
            draggbleItem.CanDrag = false;
        }

    }
}
