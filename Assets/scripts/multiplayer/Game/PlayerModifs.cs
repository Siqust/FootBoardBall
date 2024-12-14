using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PlayerModifs : MonoBehaviour, IDropHandler
{
    public MatchController matchcontroller;
    private PlayerCard card;
    void Start()
    {
        card = GetComponent<PlayerCard>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject obj = eventData.pointerDrag;
        if (card.Placed && obj.GetComponent<ActionCard>() != null)
        {
            ActionCard putted_card = obj.GetComponent<ActionCard>();
            if (!(putted_card.cardtype == ActionCardType.ActPlayerLT || putted_card.cardtype == ActionCardType.ActPlayerOT)) return;

            if (!putted_card.dragging) {return;}

            //Transform parentbeforedrag = putted_card.ParentAfterDrag;
            putted_card.ParentAfterDrag = transform;
            obj.GetComponent<RawImage>().enabled = false;
            StartCoroutine(TurnOffVisualCard(putted_card.visualcard));


            RowPlayers rowscript = transform.parent.GetComponent<RowPlayers>();
            int parentnow = rowscript.row == RowType.Attack ? 1 : 0;
            //Debug.Log((card.index, parentnow, transform.GetSiblingIndex(), rowscript.is_opponent, -1), transform);

            MoveMessage move = new MoveMessage();
            move.row_to = parentnow;
            move.col_to = transform.GetSiblingIndex();
            move.is_opponent = rowscript.is_opponent;
            move.card_index = card.index;

            matchcontroller.MakePlay(MoveType.ActionOnPlayer, putted_card.index, move);
        }
    }


    private IEnumerator TurnOffVisualCard(GameObject visualcard)
    {
        yield return new WaitForSeconds(0.5f);
        visualcard.GetComponent<RawImage>().enabled = false;
    }
}
