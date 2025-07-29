using UnityEngine;
using DS.Core;
using System;
using System.Collections;

public class ScriptDriver : MonoBehaviour, IIRExecuter
{
    public string FilePath;
    public string StartLabel = "start";
    public bool ClickToContinue = true;
    public float WaitingTime = 2f;
    public ChatBubble DialogueBubble;
    public ChatBubble NarratorBubble;
    public GameObject OptionBubblePrefab;

    public RuntimeEnv Runtime { get; private set; } = new();
    protected readonly Compiler compiler = new();
    private IIRExecuter Executer => this;
    private WaitState waitState = WaitState.None;
    private bool _isRun = false;
    private bool _readyForNext = true;
    private float _waitingTime = 0;
    private IR_Menu _currentMenu = null;

    void Awake()
    {
        if (DialogueBubble == null)
        {
            Debug.LogError("DialogueBubble is not assigned and could not be found in the scene.");

        }
        if (NarratorBubble == null)
        {
            Debug.LogError("NarratorBubble is not assigned and could not be found in the scene.");

        }
        if (OptionBubblePrefab == null)
        {
            Debug.LogError("OptionBubblePrefab is not assigned and could not be found in the scene.");
        }
        Runtime.Functions.AddFunction<int, int, int>("Add", Add);
        Runtime.Functions.AddFunction<string>("Print", Print);
    }

    void Update()
    {
        PostDialogue();

        // Run
        if (_isRun)
        {
            if (_readyForNext)
            {
                if (Runtime.HasNext)
                {
                    var instruction = Runtime.Pop();
                    Executer.Execute(instruction, Runtime);
                }
                else
                {
                    _isRun = false;
                    Debug.Log("Script execution completed.");
                }
            }
        }
        else
        {
            DialogueBubble.Hide = true;
            NarratorBubble.Hide = true;
        }
    }

    public void ExecuteDialogue(IR_Dialogue instruction, RuntimeEnv runtime)
    {
        _readyForNext = false;
        if (instruction.HasSpeaker)
        {
            NarratorBubble.Hide = true;
            DialogueBubble.Clear();
            DialogueBubble.Hide = false;
            DialogueBubble.SetText(instruction.SpeakerName + ": \n\t");
            PushFstringNodeToBubble(instruction.TextNode, DialogueBubble, runtime);
            waitState = WaitState.WaitDialogue;
        }
        else
        {
            DialogueBubble.Hide = true;
            NarratorBubble.Clear();
            NarratorBubble.Hide = false;
            PushFstringNodeToBubble(instruction.TextNode, NarratorBubble, runtime);
            waitState = WaitState.WaitDialogue;
        }
    }

    private void PostDialogue()
    {
        if (!_readyForNext && waitState == WaitState.WaitDialogue && DialogueBubble.IsAllPushed && NarratorBubble.IsAllPushed)
        {
            if (ClickToContinue)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    waitState = WaitState.None;
                    _readyForNext = true;
                }
            }
            else
            {
                _waitingTime += Time.deltaTime;
                if (_waitingTime > WaitingTime)
                {
                    waitState = WaitState.None;
                    _waitingTime = 0;
                    _readyForNext = true;
                }
            }
        }
    }

    public void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime)
    {
        _readyForNext = false;
        _currentMenu = instruction;
        ChatBubble currentBubble;
        if (!DialogueBubble.Hide)
        {
            currentBubble = DialogueBubble;
        }
        else
        {
            currentBubble = NarratorBubble;
        }

        int index = 0;
        foreach (var option in instruction.OptionTextNodes)
        {
            var optionBubble = Instantiate(OptionBubblePrefab, currentBubble.transform);
            var chatBubble = optionBubble.GetComponent<ChatBubble>();
            chatBubble.Clear();
            chatBubble.SetText(Convert.ToString(option.Evaluate(runtime)));
            chatBubble.Hide = false;
            chatBubble.OptionNumber = index;
            chatBubble.CanSelected = true;
            chatBubble.OnOptionSelected = SelectOption;
            currentBubble.SubBubbles.Add(chatBubble);
            index++;
        }
    }

    public void SelectOption(int choice)
    {
        if (_currentMenu == null)
        {
            Debug.LogError("No current menu to select option from.");
            return;
        }

        if (!_readyForNext)
        {
            Debug.Log($"Selected option {choice} from the current menu: {_currentMenu}");
            var block = _currentMenu.Blocks[choice];
            if (block == null)
            {
                Debug.LogError($"No block found for choice {choice} in the current menu.");
                return;
            }

            foreach (var sub in DialogueBubble.SubBubbles)
            {
                sub.Hide = true;
                Destroy(sub.gameObject, 2f);
            }
            DialogueBubble.SubBubbles.Clear();
            foreach (var sub in NarratorBubble.SubBubbles)
            {
                sub.Hide = true;
                Destroy(sub.gameObject, 2f);
            }
            NarratorBubble.SubBubbles.Clear();

            Runtime.Enqueue(block, true);
            _readyForNext = true;
        }
    }

    public void Run()
    {
        var script = compiler.Compile(FilePath);
        if (script == null)
        {
            Debug.LogError("Failed to compile script.");
            return;
        }

        Runtime.ClearLabels();
        Runtime.ClearQueue();
        Runtime.Read(script);
        Runtime.Load(StartLabel);
        _isRun = true;
    }

    private void PushFstringNodeToBubble(FStringNode node, ChatBubble bubble, RuntimeEnv runtime)
    {
        int embedIndex = 0;
        var EmbedExpr = node.EmbedExpr;
        foreach (var fragment in node.Fragments)
        {
            if (fragment == FStringNode.EmbedSign)
            {
                if (embedIndex < EmbedExpr.Count)
                {
                    var embedValue = EmbedExpr[embedIndex].Evaluate(runtime);
                    if (embedValue is string embedString)
                    {
                        bubble.PushText(embedString);
                    }
                    else if (embedValue is null)
                    {
                    }
                    else if (Convert.ToString(embedValue) is string embedConverted)
                    {
                        bubble.PushText(embedConverted);
                    }
                    embedIndex++;
                }
                else
                {
                    throw new InvalidOperationException("Not enough embedded expressions provided for FString.");
                }
            }
            else
            {
                bubble.PushText(fragment);
            }
        }
        if (embedIndex < EmbedExpr.Count)
        {
            throw new InvalidOperationException("Too many embedded expressions provided for FString.");
        }
    }

    private enum WaitState
    {
        None,
        WaitDialogue,
    }

    public int Add(int a, int b)
    {
        return a + b;
    }

    public void Print(string message)
    {
        Debug.Log(message);
    }
}
