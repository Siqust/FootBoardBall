using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggbleItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform ParentAfterDrag;
    public ScriptableObject Card;
    public CardType cardType;
    public bool CanDrag;
    private int siblingindex;
    private bool stopEndDrag;
    Image card;
    private void Start()
    {
        CanDrag = true;
        card = GetComponent<Image>();
        try
        {
            cardType = ((PlayerCard)Card).cardtype;
        }
        catch { }
        try
        {
            cardType = ((ActionCard)Card).cardtype;
        }
        catch { }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        ParentAfterDrag = transform.parent;
        siblingindex = transform.GetSiblingIndex();
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        card.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (stopEndDrag) return;
        transform.parent = ParentAfterDrag;
        transform.SetSiblingIndex(siblingindex);
        card.raycastTarget = true;
        transform.localPosition = Vector3.zero;
        if (CanDrag == false)
        {
            stopEndDrag = true;
            if (cardType != CardType.ActPlayer)
            {
                transform.SetAsFirstSibling();
            }
        }
    }
}
