using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image bubbleImage;
    [SerializeField]
    private TextMeshProUGUI bubbleText;
    [SerializeField]
    private int MaxHorizontalPadding = 300;
    [SerializeField]
    private int MinHorizontalPadding = 100;
    [SerializeField]
    private int MaxVerticalPadding = 300;
    [SerializeField]
    private bool CanHighLight = false;

    private Color _color = Color.white;
    public readonly List<ChatBubble> _options = new();

    void Start()
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
            Mathf.Min(textSize.y * lineCount, MaxVerticalPadding)
        );
        bubbleImage.rectTransform.sizeDelta = Vector2.Lerp(bubbleImage.rectTransform.sizeDelta, bubbleSize * 1.2f, Time.deltaTime * 10);
        bubbleText.rectTransform.sizeDelta = Vector2.Lerp(bubbleText.rectTransform.sizeDelta, bubbleSize, Time.deltaTime * 10);

        // HighLight
        if (CanHighLight)
        {
            bubbleImage.color = Color.Lerp(bubbleImage.color, _color, Time.deltaTime * 10);
        }

        // Arange position of option bubbles
        if (_options.Count > 0)
        {
            float offsetY = 0f;
            foreach (var option in _options)
            {
                option.transform.localPosition = new Vector3(
                    transform.localPosition.x + bubbleImage.rectTransform.sizeDelta.x / 2 + option.bubbleImage.rectTransform.sizeDelta.x / 2,
                    transform.localPosition.y + offsetY,
                    transform.localPosition.z
                );
                offsetY -= option.bubbleImage.rectTransform.sizeDelta.y * 1.2f; // Adjust the spacing as needed
            }
        }
    }

    public void Clear()
    {
        bubbleText.text = string.Empty;
    }

    public void PushText(string text)
    {
        StartCoroutine(PushTextCoroutine(text));
    }

    public void SetText(string text)
    {
        bubbleText.text = text;
    }

    private IEnumerator PushTextCoroutine(string text)
    {
        foreach (char c in text)
        {
            bubbleText.text += c;
            yield return new WaitForSeconds(0.05f); // Adjust the delay as needed
        }
    }

    public void Fade(float duration = 0.5f)
    {
        duration = Mathf.Max(0.1f, duration);
        StartCoroutine(FadeCoroutine(duration));
    }

    private IEnumerator FadeCoroutine(float duration)
    {
        float elapsed = 0f;

        Color originalColor = bubbleImage.color;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / duration);
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
            // Handle left click
            Debug.Log("Click");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanHighLight)
        {
            _color = new Color(1f, 1f, 0.5f, 1f); // Highlight color
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CanHighLight)
        {
            _color = Color.white; // Reset to original color
        }
    }
}