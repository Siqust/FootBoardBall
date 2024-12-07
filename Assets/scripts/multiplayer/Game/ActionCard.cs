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
    RawImage CardImage;
    public bool dragging;
    [HideInInspector] public Vector2 visualcard_pos;
    [SerializeField] private GameObject visualcard_prefab;
    [HideInInspector] public GameObject visualcard;
    [HideInInspector] public Transform visualcardparent;
    [HideInInspector] public Transform ParentAfterDrag;
    private void Start()
    {
        CardImage = GetComponent<RawImage>();
        visualcardparent = GameObject.FindWithTag("PlayerCards").transform;
        visualcard = Instantiate(visualcard_prefab, visualcardparent);
        if (visualcard_pos != null) { visualcard.transform.position = visualcard_pos; }
        Color selfcolor = matchcontroller.actions_cards[index].color;//transform.GetComponent<RawImage>().color;
        visualcard.GetComponent<RawImage>().color = new Color(selfcolor.r,selfcolor.g,selfcolor.b, 255);
        transform.GetComponent<RawImage>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 0);
        FollowCard visualcardscript = visualcard.GetComponent<FollowCard>();
        visualcardscript.target_card = transform;
        visualcardscript.ActionModifierText.gameObject.SetActive(true); visualcardscript.ActionTypeText.gameObject.SetActive(true);
        if (cardtype == ActionCardType.ActPlayerLT)
        {
            visualcardscript.ActionTypeText.text = "ÈÃÐÎÊ";
        }
        else if (cardtype == ActionCardType.ActRow)
        {
            visualcardscript.ActionTypeText.text = "ÐßÄ";
        }
        else if (cardtype == ActionCardType.ActPlayerOT)
        {
            visualcardscript.ActionTypeText.gameObject.SetActive(false);
            visualcardscript.ActionModifierText.gameObject.SetActive(false);
            visualcardscript.Deletion.SetActive(true);
        }
        visualcardscript.ActionModifierText.text = matchcontroller.actions_cards[index].modif.ToString();
        visualcard.transform.SetAsLastSibling();
        dragging = false;
    }
    private void Update()
    {
        if (dragging && !matchcontroller.currentPlayer.isLocalPlayer) { OnEndDrag(null); }
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
        if (stopEndDrag || matchcontroller.ended || !dragging) return;
        //if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }


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
