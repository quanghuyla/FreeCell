using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AutoMove : MonoBehaviour, IPointerClickHandler
{
    private Card card;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    void Awake()
    {
        card = GetComponent<Card>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            TryAutoMove();
            lastClickTime = 0f;
        }
        else
        {
            lastClickTime = Time.time;
        }
    }

    private void TryAutoMove()
    {
        if (!IsTopCard())
        {
            return;
        }

        ICardContainer currentContainer = transform.parent.GetComponent<ICardContainer>();
        if (currentContainer == null)
        {
            return;
        }

        if (TryMoveToFoundation(currentContainer))
        {
            return;
        }

        if (TryMoveToTableau(currentContainer))
        {
            return;
        }

        TryMoveToFreeCell(currentContainer);
    }

    private bool IsTopCard()
    {
        TableauColumn tableau = transform.parent.GetComponent<TableauColumn>();
        if (tableau != null)
        {
            return tableau.cards.Count > 0 && tableau.cards[tableau.cards.Count - 1] == card;
        }

        FreeCell freeCell = transform.parent.GetComponent<FreeCell>();
        if (freeCell != null)
        {
            return freeCell.card == card;
        }

        Foundation foundation = transform.parent.GetComponent<Foundation>();
        if (foundation != null)
        {
            return foundation.cards.Count > 0 && foundation.cards[foundation.cards.Count - 1] == card;
        }

        return false;
    }

    private bool TryMoveToFoundation(ICardContainer from)
    {
        foreach (Foundation foundation in GameManager.Instance.foundations)
        {
            if (foundation.CanPlaceCard(card))
            {
                from.RemoveCard(card);
                foundation.AddCard(card);

                GameManager.Instance.RegisterMove(new List<Card> { card }, from, foundation);
                return true;
            }
        }
        return false;
    }

    private bool TryMoveToTableau(ICardContainer from)
    {
        foreach (TableauColumn tableau in GameManager.Instance.tableauColumns)
        {
            if (ReferenceEquals(tableau, from))
            {
                continue;
            }

            if (tableau.CanPlaceCard(card))
            {
                from.RemoveCard(card);
                tableau.AddCard(card);

                GameManager.Instance.RegisterMove(new List<Card> { card }, from, tableau);
                return true;
            }
        }
        return false;
    }

    private bool TryMoveToFreeCell(ICardContainer from)
    {
        if (from is FreeCell)
        {
            return false;
        }

        foreach (FreeCell freeCell in GameManager.Instance.freeCells)
        {
            if (freeCell.CanPlaceCard(card))
            {
                from.RemoveCard(card);
                freeCell.AddCard(card);

                GameManager.Instance.RegisterMove(new List<Card> { card }, from, freeCell);
                return true;
            }
        }
        return false;
    }
}
