using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggbleItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform ParentAfterDrag;
    public bool CanDrag;
    private int siblingindex;
    private bool stopEndDrag;
    Image card;
    private void Start()
    {
        CanDrag = true;
        card = GetComponent<Image>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        Debug.Log("BeginDrag");
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
        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag");
        if (stopEndDrag) return;
        transform.parent = ParentAfterDrag;
        transform.SetSiblingIndex(siblingindex);
        card.raycastTarget = true;
        transform.localPosition = Vector3.zero;
        if (CanDrag == false)
        {
            stopEndDrag = true;
            transform.SetAsLastSibling();
        }
    }
}
