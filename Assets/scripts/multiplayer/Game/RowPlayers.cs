using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class RowPlayers : MonoBehaviour, IDropHandler
{
    public RowType row;
    public bool is_opponent;
    public MatchController matchcontroller;
    //public MatchController matchController;
    public void OnDrop(PointerEventData eventData)
    {
        Transform parentbeforedrag = transform; // transform = null
        GameObject obj = eventData.pointerDrag;
        if (obj.GetComponent<PlayerCard>() != null && transform.childCount<4 && !is_opponent)
        {
            PlayerCard card = obj.GetComponent<PlayerCard>();

            if (card.ParentAfterDrag == transform)
            {
                card.ParentAfterDrag = null;
                card.dragging = false;
                card.CardImage.raycastTarget = true;
                card.transform.SetParent(transform);
                card.transform.SetSiblingIndex(card.siblingindex);
                return;
            }
            if (!card.dragging) { return; }

            parentbeforedrag = card.ParentAfterDrag;

            card.ParentAfterDrag = transform;
            card.Placed = true;

            int parentbefore = -1;
            if (parentbeforedrag.GetComponent<RowPlayers>() != null)
            {
                parentbefore = parentbeforedrag.GetComponent<RowPlayers>().row == RowType.Attack ? 1 : 0;
            }
            int parentnow = row == RowType.Attack ? 1 : 0;
            //Debug.Log((card.index, parentnow, transform.childCount, parentbefore, card.siblingindex), transform);
            
            MoveMessage move = new MoveMessage();
            move.row_to = parentnow;
            move.col_to = transform.childCount;
            move.row_from = parentbefore;
            move.col_from = card.siblingindex;

            matchcontroller.MakePlay(MoveType.Player, card.index, move);
        }
        
        
    }
}
