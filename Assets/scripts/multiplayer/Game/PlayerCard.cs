using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PlayerCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public MatchController matchcontroller;
    public int index;
    public bool CanDrag = true;
    public bool Placed = false;
    [HideInInspector] public int siblingindex;
    private bool stopEndDrag;
    [HideInInspector] public RawImage CardImage;
    [HideInInspector] public bool dragging;
    [HideInInspector] public Vector2 visualcard_pos;
    [SerializeField] private GameObject visualcard_prefab;
    [SerializeField] public GameObject visualcard;
    [HideInInspector] public Transform visualcardparent;
    [HideInInspector] public Transform ParentAfterDrag;
    private void Start()
    {
        CardImage = GetComponent<RawImage>();
        visualcardparent = GameObject.FindWithTag("PlayerCards").transform;
        visualcard = Instantiate(visualcard_prefab, visualcardparent);
        if (visualcard_pos != null) { visualcard.transform.position = visualcard_pos; }
        Color selfcolor = transform.GetComponent<RawImage>().color;
        visualcard.GetComponent<RawImage>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 255);
        transform.GetComponent<RawImage>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 0);
        //visualcard.transform.position = transform.position;
        FollowCard visualcardscript = visualcard.GetComponent<FollowCard>();
        visualcardscript.target_card = transform;
        visualcardscript.player_attack_text.gameObject.SetActive(true); visualcardscript.player_defence_text.gameObject.SetActive(true);
        visualcardscript.player_attack_text.text = matchcontroller.players_cards[index].attack.ToString();
        visualcardscript.player_defence_text.text = matchcontroller.players_cards[index].defence.ToString();
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
        if (stopEndDrag || !CanDrag || !dragging) return;
        if (ParentAfterDrag == null) { return; }
        //if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }

        dragging = false;
        visualcard.transform.SetAsFirstSibling();
        transform.SetParent(ParentAfterDrag);
        if (ParentAfterDrag.GetComponent<RowPlayers>() == null)
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
        }
    }
    public void delete()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<ActionCard>().delete();
        }
        Destroy(visualcard);
        Destroy(gameObject);
    }
    public void offModifsImages()
    {
        Transform obj = transform.GetChild(transform.childCount - 1);
        obj.GetComponent<RawImage>().enabled = false;
        StartCoroutine(Deletevisualcard(obj));
    }
    IEnumerator Deletevisualcard(Transform obj)
    {
        yield return new WaitForSeconds(0.5f);
        obj.GetComponent<ActionCard>().visualcard.SetActive(false);
    }
}