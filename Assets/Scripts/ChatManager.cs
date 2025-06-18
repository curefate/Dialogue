using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class ChatManager : MonoBehaviour
{
    [SerializeField] private GameObject ChatBubblePrefab;
    [SerializeField] private GameObject OptionBubblePrefab;

    private Dictionary<DCharacter, ChatBubble> _chatBubbles = new();

    private void AddChatBubble(DCharacter character)
    {
        if (_chatBubbles.ContainsKey(character))
        {
            Debug.LogWarning($"Chat bubble for {character.name} already exists.");
            return;
        }

        GameObject bubbleObject = Instantiate(ChatBubblePrefab, transform);
        ChatBubble chatBubble = bubbleObject.GetComponent<ChatBubble>();
        chatBubble.FollowTarget = character.gameObject;
        chatBubble.OptionNumber = -1;
        _chatBubbles[character] = chatBubble;
    }

    public void Say(DCharacter character, string text)
    {
        if (character == null || string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Character or text is null or empty.");
            return;
        }

        if (!_chatBubbles.ContainsKey(character))
        {
            Debug.LogWarning($"Chat bubble for {character.name} does not exist. Creating a new one.");
            AddChatBubble(character);
        }

        StartCoroutine(SayCoroutine(character, text));
    }

    private IEnumerator SayCoroutine(DCharacter character, string text)
    {
        if (!_chatBubbles.TryGetValue(character, out ChatBubble chatBubble))
        {
            Debug.LogWarning($"Chat bubble for {character.name} does not exist. Creating a new one.");
            AddChatBubble(character);
            chatBubble = _chatBubbles[character];
        }
        chatBubble.SetText(character.ShowName + ":\n");
        chatBubble.ShowIn();
        chatBubble.PushText(text);
        yield return null;
    }

    public void FadeExcept(DCharacter[] character, float duration = 0.5f)
    {
        foreach (var kvp in _chatBubbles)
        {
            if (character == null || !System.Array.Exists(character, c => c == kvp.Key))
            {
                kvp.Value.FadeOut(duration);
            }
        }
    }
}
