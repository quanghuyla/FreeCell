using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCelebration : MonoBehaviour
{
    [Tooltip("How many cards to animate during the celebration")]
    public int cardsToCelebrate = 20;

    [Tooltip("Radius of the roaming animation")]
    public float roamRadius = 80f;

    [Tooltip("Speed of the roaming animation")]
    public float roamSpeed = 2f;

    private bool isPlaying;

    private class CardCelebrationState
    {
        public Card originalCard;
        public CanvasGroup originalCanvasGroup;
        public float originalAlpha;
        public GameObject clone;
        public RectTransform cloneRect;
        public Vector2 orbitCenter;
        public float angle;
        public Coroutine coroutine;
    }

    private readonly List<CardCelebrationState> celebrationStates = new List<CardCelebrationState>();
    private Transform celebrationLayer;

    public void Play()
    {
        if (isPlaying)
        {
            return;
        }

        StartCoroutine(PlayCelebration());
    }

    public void Stop()
    {
        StopAllCoroutines();
        isPlaying = false;

        foreach (CardCelebrationState state in celebrationStates)
        {
            if (state == null)
            {
                continue;
            }

            if (state.coroutine != null)
            {
                StopCoroutine(state.coroutine);
            }

            if (state.clone != null)
            {
                Destroy(state.clone);
            }

            if (state.originalCanvasGroup != null)
            {
                state.originalCanvasGroup.alpha = state.originalAlpha;
            }
        }

        celebrationStates.Clear();
    }

    private IEnumerator PlayCelebration()
    {
        isPlaying = true;
        celebrationStates.Clear();

        if (celebrationLayer == null)
        {
            Canvas canvas = GameManager.Instance.tableauParent.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                celebrationLayer = canvas.transform;
            }
        }

        List<Card> candidateCards = new List<Card>();
        foreach (Foundation foundation in GameManager.Instance.foundations)
        {
            candidateCards.AddRange(foundation.cards);
        }

        foreach (TableauColumn column in GameManager.Instance.tableauColumns)
        {
            if (column.cards.Count > 0)
            {
                candidateCards.Add(column.cards[column.cards.Count - 1]);
            }
        }

        int count = Mathf.Min(cardsToCelebrate, candidateCards.Count);
        for (int i = 0; i < count; i++)
        {
            Card card = candidateCards[i];
            if (card == null)
            {
                continue;
            }

            RectTransform originalRect = card.GetComponent<RectTransform>();
            if (originalRect == null)
            {
                continue;
            }

            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            }

            GameObject clone = Instantiate(card.gameObject, celebrationLayer);
            clone.name = card.name + "_Celebration";
            foreach (var behaviour in clone.GetComponents<MonoBehaviour>())
            {
                if (behaviour is Card || behaviour is DragDrop || behaviour is AutoMove)
                {
                    Destroy(behaviour);
                }
            }

            CanvasGroup cloneCanvas = clone.GetComponent<CanvasGroup>();
            if (cloneCanvas != null)
            {
                Destroy(cloneCanvas);
            }

            CardCelebrationState state = new CardCelebrationState
            {
                originalCard = card,
                originalCanvasGroup = canvasGroup,
                originalAlpha = canvasGroup.alpha,
                clone = clone,
                cloneRect = clone.GetComponent<RectTransform>(),
                angle = Random.Range(0f, Mathf.PI * 2f)
            };

            canvasGroup.alpha = 0f;

            if (state.cloneRect != null)
            {
                state.cloneRect.position = originalRect.position;
                state.orbitCenter = state.cloneRect.anchoredPosition;
                state.coroutine = StartCoroutine(OrbitCard(state));
                celebrationStates.Add(state);
            }
            else
            {
                Destroy(clone);
                canvasGroup.alpha = state.originalAlpha;
            }
        }

        yield return null;
    }

    private IEnumerator OrbitCard(CardCelebrationState state)
    {
        while (state.cloneRect != null)
        {
            state.angle += roamSpeed * Time.deltaTime;
            Vector2 offset = new Vector2(Mathf.Cos(state.angle), Mathf.Sin(state.angle)) * roamRadius;
            state.cloneRect.anchoredPosition = state.orbitCenter + offset;
            yield return null;
        }
    }
}
