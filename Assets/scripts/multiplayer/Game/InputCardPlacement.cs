using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputCardPlacement : MonoBehaviour, IDropHandler
{
    public CardType cardtype;
    public void OnDrop(PointerEventData eventData)
    {
        if (cardtype == CardType.Player && transform.childCount >= 3) return;
        GameObject dropped = eventData.pointerDrag;
        DraggbleItem draggbleItem = dropped.GetComponent<DraggbleItem>();
        if (draggbleItem.cardType == cardtype)
        {
            draggbleItem.ParentAfterDrag = transform;
            draggbleItem.CanDrag = false;
        }

    }
}
