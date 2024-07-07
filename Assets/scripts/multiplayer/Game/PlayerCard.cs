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
    [HideInInspector] public Image CardImage;
    [HideInInspector] public bool dragging;
    [SerializeField] private GameObject visualcard_prefab;
    [SerializeField] public GameObject visualcard;
    [HideInInspector] public Transform visualcardparent;
    [HideInInspector] public Transform ParentAfterDrag;
    private void Start()
    {
        CardImage = GetComponent<Image>();
        visualcardparent = GameObject.FindWithTag("PlayerCards").transform;
        visualcard = Instantiate(visualcard_prefab, visualcardparent);
        if (!CanDrag && Placed)
        {
            visualcard.transform.position = new Vector2(350, 460); //OPPONENT COORDS
        }
        else
        {
            visualcard.transform.position = new Vector2(700, -100);
        }
        Color selfcolor = transform.GetComponent<Image>().color;
        visualcard.GetComponent<Image>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 255);
        transform.GetComponent<Image>().color = new Color(selfcolor.r, selfcolor.g, selfcolor.b, 0);
        //visualcard.transform.position = transform.position;
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
        if (stopEndDrag || !CanDrag) return;
        if (ParentAfterDrag == null) { return; }
        if (matchcontroller.currentPlayer != null) { if (!matchcontroller.currentPlayer.isLocalPlayer) return; }


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
        obj.GetComponent<Image>().enabled = false;
        StartCoroutine(Deletevisualcard(obj));
    }
    IEnumerator Deletevisualcard(Transform obj)
    {
        yield return new WaitForSeconds(0.5f);
        obj.GetComponent<ActionCard>().visualcard.SetActive(false);
    }
}