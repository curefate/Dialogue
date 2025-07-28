using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image bubbleImage;
    [SerializeField]
    private TextMeshProUGUI bubbleText;

    private Color _color = Color.white;

    public int MaxHorizontalPadding = 300;
    public int MinHorizontalPadding = 100;
    public int MaxVerticalPadding = 300;
    public int MinVerticalPadding = 100;
    public Color OriginColor = Color.white;
    public Color SelectedColor = Color.yellow;
    public bool CanHighLight = false;
    public bool SubOnTop = false;
    public bool Hide = false;
    public float FadeOutDuration = 0.5f;
    public List<ChatBubble> SubBubbles = new();

    public bool IsAllPushed => _textQueue.Count == 0;
    [HideInInspector]
    public int OptionNumber = -1;
    [HideInInspector]
    public ChatBubble ParentBubble;
    private Queue<string> _textQueue = new();
    private bool _isPushing = false;
    private bool _isHided = false;



    void Awake()
    {
        if (bubbleImage == null)
        {
            bubbleImage = GetComponent<Image>();
        }
        if (bubbleText == null)
        {
            bubbleText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void FixedUpdate()
    {
        if (bubbleImage == null || bubbleText == null)
            return;

        // Calculate the size of the text
        Vector2 textSize = bubbleText.GetPreferredValues(bubbleText.text);
        // Calculate the line count
        int lineCount = Mathf.CeilToInt(textSize.x / bubbleText.rectTransform.rect.width);
        // Calculate the size of the bubble
        Vector2 bubbleSize = new(
            Mathf.Max(Mathf.Min(textSize.x, MaxHorizontalPadding), MinHorizontalPadding),
            Mathf.Max(Mathf.Min(textSize.y * lineCount, MaxVerticalPadding), MinVerticalPadding)
        );
        bubbleImage.rectTransform.sizeDelta = Vector2.Lerp(bubbleImage.rectTransform.sizeDelta, bubbleSize * 1.1f, Time.deltaTime * 15);
        bubbleText.rectTransform.sizeDelta = Vector2.Lerp(bubbleText.rectTransform.sizeDelta, bubbleSize, Time.deltaTime * 15);

        // HighLight
        if (CanHighLight)
        {
            bubbleImage.color = Color.Lerp(bubbleImage.color, _color, Time.deltaTime * 10);
        }

        // Arange position of sub bubbles
        if (SubBubbles.Count > 0)
        {
            float offsetY = 0f;
            foreach (var sub in SubBubbles)
            {
                if (!SubOnTop)
                {
                    sub.transform.localPosition = new Vector3(
                    transform.localPosition.x + bubbleImage.rectTransform.sizeDelta.x / 2 - sub.bubbleImage.rectTransform.sizeDelta.x / 2,
                    transform.localPosition.y - bubbleImage.rectTransform.sizeDelta.y / 2 - sub.bubbleImage.rectTransform.sizeDelta.y / 2 + offsetY,
                    transform.localPosition.z
                    );
                    offsetY -= sub.bubbleImage.rectTransform.sizeDelta.y * 1.1f;
                }
                else
                {
                    sub.transform.localPosition = new Vector3(
                    transform.localPosition.x + bubbleImage.rectTransform.sizeDelta.x / 2 - sub.bubbleImage.rectTransform.sizeDelta.x / 2,
                    transform.localPosition.y + bubbleImage.rectTransform.sizeDelta.y / 2 + sub.bubbleImage.rectTransform.sizeDelta.y / 2 + offsetY,
                    transform.localPosition.z
                    );
                    offsetY += sub.bubbleImage.rectTransform.sizeDelta.y * 1.1f;
                }
            }
        }

        // Push text
        if (_textQueue.Count > 0 && !_isPushing)
        {
            string text = _textQueue.Dequeue();
            StartCoroutine(PushTextCoroutine(text));
        }

        // Hide
        if (Hide)
        {
            var duration = Mathf.Max(0.1f, FadeOutDuration);
            if (!_isHided)
            {
                StartCoroutine(FadeCoroutine(duration));
            }
        }
        else
        {
            _isHided = false;
            float alpha = 1;
            bubbleImage.color = new Color(bubbleImage.color.r, bubbleImage.color.g, bubbleImage.color.b, alpha);
            bubbleText.color = new Color(bubbleText.color.r, bubbleText.color.g, bubbleText.color.b, alpha);
        }
    }

    public void Clear()
    {
        bubbleText.text = string.Empty;
    }

    public void PushText(string text)
    {
        _textQueue.Enqueue(text);
    }

    public void SetText(string text)
    {
        bubbleText.text = text;
    }

    private IEnumerator PushTextCoroutine(string text)
    {
        _isPushing = true;
        foreach (char c in text)
        {
            bubbleText.text += c;
            yield return new WaitForSeconds(0.05f); // Adjust the delay as needed
        }
        _isPushing = false;
    }

    private IEnumerator FadeCoroutine(float duration)
    {
        _isHided = true;
        float elapsed = 0f;
        Color originalColor = bubbleImage.color;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / duration) < .01f ? 0f : Mathf.Lerp(originalColor.a, 0f, elapsed / duration);
            bubbleImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            bubbleText.color = new Color(bubbleText.color.r, bubbleText.color.g, bubbleText.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ParentBubble.OptionNumber = OptionNumber;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanHighLight)
        {
            _color = SelectedColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CanHighLight)
        {
            _color = OriginColor;
        }
    }
}