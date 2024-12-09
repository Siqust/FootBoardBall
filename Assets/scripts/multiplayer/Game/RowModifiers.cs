using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class RowModifiers : MonoBehaviour, IDropHandler
{
    public RowType row;
    public bool is_opponent;
    public TextMeshProUGUI modif_text;
    public MatchController matchcontroller;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject obj = eventData.pointerDrag;
        if (obj.GetComponent<ActionCard>() != null)
        {
            ActionCard card = obj.GetComponent<ActionCard>();
            if (card.cardtype != ActionCardType.ActRow) { return; }

            if (!card.dragging) { return; }
            Transform parentbeforedrag = card.ParentAfterDrag;
            card.ParentAfterDrag = transform;
            card.visualcard.SetActive(false);

            int parentnow = row == RowType.Attack ? 1 : 0;
            //Debug.Log((card.index, parentnow, is_opponent, -1), transform);

            MoveMessage move = new MoveMessage();
            move.row_to = parentnow;
            move.is_opponent = is_opponent;

            
            matchcontroller.MakePlay(MoveType.ActionOnRow, card.index, move);

        }
    }
    public void offModifsImages()
    {
        Transform obj = transform.GetChild(transform.childCount - 1);
        obj.GetComponent<RawImage>().enabled = false;
        StartCoroutine(Deletevisualcard(obj));
    }
    IEnumerator Deletevisualcard(Transform obj)
    {
        yield return new WaitForSeconds(1f);
        obj.GetComponent<ActionCard>().visualcard.SetActive(false);
    }
}