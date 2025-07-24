using UnityEngine;
using Assets.Scripts.DS.Core;
using Unity.VisualScripting;
using System;
using NUnit.Framework.Constraints;

public class BubblePusher : MonoBehaviour
{
    private enum BubbleType
    {
        Narration,
        Dialogue,
        None,
    }

    public bool ClickToNext = true;
    public GameObject OptionBubblePrefab;
    public ChatBubble NameBubble;
    public ChatBubble NarrationBubble;
    public ChatBubble DialogueBubble;

    private bool _isClicked;
    private BubbleType _currentBubbleType = BubbleType.None;

    void Start()
    {
        if (OptionBubblePrefab == null)
        {
            Debug.LogError("OptionBubblePrefab is not assigned in BubblePusher.");
        }
        if (NameBubble == null)
        {
            Debug.LogError("NameBubble is not assigned in BubblePusher.");
        }
        if (NarrationBubble == null)
        {
            Debug.LogError("NarrationBubble is not assigned in BubblePusher.");
        }
        if (DialogueBubble == null)
        {
            Debug.LogError("DialogueBubble is not assigned in BubblePusher.");
        }
    }

    void Update()
    {
        if (ClickToNext && !_isClicked)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                _isClicked = true;
            }
        }
    }

    public void PushDialogue(Interpreter interpreter, IR_Dialogue dialogue)
    {
        _isClicked = false;
        if (dialogue.SpeakerName != null)
        {
            NameBubble.Clear();
            DialogueBubble.Clear();
            _currentBubbleType = BubbleType.Dialogue;
            NarrationBubble.FadeOut();
            NameBubble.SetText(dialogue.SpeakerName);
            NameBubble.ShowIn();
            DialogueBubble.ShowIn();
            DialogueBubble.PushText(dialogue.TextNode.GetText(interpreter));
            while (!DialogueBubble.IsAllPushed)
            {
            }
        }
        else
        {
            NarrationBubble.Clear();
            _currentBubbleType = BubbleType.Narration;
            NameBubble.FadeOut();
            DialogueBubble.FadeOut();
            NarrationBubble.ShowIn();
            NarrationBubble.PushText(dialogue.TextNode.GetText(interpreter));
            while (!NarrationBubble.IsAllPushed)
            {
            }
        }

        if (ClickToNext)
        {
            while (!_isClicked)
            {
            }
            NarrationBubble.FadeOut();
            NameBubble.FadeOut();
            DialogueBubble.FadeOut();
        }
        else
        {
            DialogueBubble.FadeOut();
            NameBubble.FadeOut();
            NarrationBubble.FadeOut();
        }
    }

    public int PushMenu(Interpreter interpreter, IR_Menu menu)
    {
        if (_currentBubbleType == BubbleType.Dialogue)
        {
            DialogueBubble.ShowIn();
            int choice = 0;
            foreach (var optionTextNode in menu.OptionTextNodes)
            {
                var opt = Instantiate(OptionBubblePrefab, DialogueBubble.transform);
                var optBubble = opt.GetComponent<ChatBubble>();
                optBubble.SetText(optionTextNode.GetText(interpreter));
                optBubble.ShowIn();
                optBubble.ParentBubble = DialogueBubble;
                optBubble.OptionNumber = choice++;
                DialogueBubble.SubBubbles.Add(opt.GetComponent<ChatBubble>());
            }
            while (DialogueBubble.OptionNumber == -1)
            {
            }
            var ret = DialogueBubble.OptionNumber;
            DialogueBubble.OptionNumber = -1; // Reset for next use
            DialogueBubble.FadeOut();
            foreach (var subBubble in DialogueBubble.SubBubbles)
            {
                Destroy(subBubble.gameObject);
            }
            DialogueBubble.SubBubbles.Clear();
            return ret;
        }
        else if (_currentBubbleType == BubbleType.Narration || _currentBubbleType == BubbleType.None)
        {
            NarrationBubble.ShowIn();
            int choice = 0;
            foreach (var optionTextNode in menu.OptionTextNodes)
            {
                var opt = Instantiate(OptionBubblePrefab, NarrationBubble.transform);
                var optBubble = opt.GetComponent<ChatBubble>();
                optBubble.SetText(optionTextNode.GetText(interpreter));
                optBubble.ShowIn();
                optBubble.ParentBubble = NarrationBubble;
                optBubble.OptionNumber = choice++;
                NarrationBubble.SubBubbles.Add(opt.GetComponent<ChatBubble>());
            }
            while (NarrationBubble.OptionNumber == -1)
            {
            }
            var ret = NarrationBubble.OptionNumber;
            NarrationBubble.OptionNumber = -1; // Reset for next use
            NarrationBubble.FadeOut();
            foreach (var subBubble in NarrationBubble.SubBubbles)
            {
                Destroy(subBubble.gameObject);
            }
            NarrationBubble.SubBubbles.Clear();
            return ret;
        }
        else
        {
            throw new InvalidOperationException("Invalid bubble type for menu.");
        }
    }
}