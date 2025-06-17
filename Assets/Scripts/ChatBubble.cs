using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
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

    void Start()
    {
        if (bubbleImage == null)
        {
            bubbleImage = GetComponentInChildren<Image>();
        }
        if (bubbleText == null)
        {
            bubbleText = GetComponentInChildren<TextMeshProUGUI>();
        }

        PushText("Hello, World!"); // Example text to start with
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
    }

    public void Clear()
    {
        bubbleText.text = string.Empty;
    }

    public void PushText(string text)
    {
        StartCoroutine(PushTextCoroutine(text));
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
}
