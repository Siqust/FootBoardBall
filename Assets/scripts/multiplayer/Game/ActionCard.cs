using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    public MatchController matchcontroller;
    public int index;
    public ActionCardType cardtype;
    public bool CanDrag;
    [HideInInspector] public int siblingindex;
    private bool stopEndDrag;
    Image CardImage;
    public bool dragging;
    [SerializeField] private GameObject visualcard_prefab;
    [HideInInspector] public GameObject visualcard;
    [HideInInspector] public Transform visualcardparent;
    [HideInInspector] public Transform ParentAfterDrag;
    private void Start()
    {
        CardImage = GetComponent<Image>();
        visualcardparent = GameObject.FindWithTag("PlayerCards").transform;
        CanDrag = true;
        visualcard = Instantiate(visualcard_prefab, visualcardparent);
        Color selfcolor = transform.GetComponent<Image>().color;
        visualcard.GetComponent<Image>().color = new Color(selfcolor.r,selfcolor.g,selfcolor.b, 255);
        transform.GetComponent<Image>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 0);
        visualcard.GetComponent<FollowCard>().target_card = transform;
        visualcard.transform.SetAsLastSibling();
        dragging = false;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }

        dragging = true;
        ParentAfterDrag = transform.parent;
        siblingindex = transform.GetSiblingIndex();
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        visualcard.transform.SetAsLastSibling();
        CardImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }

        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (stopEndDrag) return;
        if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }


        dragging = false;
        visualcard.transform.SetAsFirstSibling();
        transform.SetParent(ParentAfterDrag);
        if (ParentAfterDrag.GetComponent<PlayerModifs>() == null)
        {
            transform.SetSiblingIndex(siblingindex);
        }
        else
        {
            transform.SetAsLastSibling();
        }
        CardImage.raycastTarget = true;
        transform.localPosition = Vector3.zero;
        if (CanDrag == false)
        {
            stopEndDrag = true;
            if (cardtype != ActionCardType.ActPlayerLT || cardtype != ActionCardType.ActPlayerOT)
            {
                transform.SetAsFirstSibling();
            }
        }
    }
    public void delete()
    {
        Destroy(visualcard);
        Destroy(gameObject);
    }
}
