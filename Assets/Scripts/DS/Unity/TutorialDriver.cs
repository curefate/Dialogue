using UnityEngine;
using DS.Core;
using System;
using System.Collections;
using Mono.Cecil.Cil;

public class TutorialDriver : MonoBehaviour, IIRExecuter
{
    public string FilePath;
    public string StartLabel = "start";
    public bool ClickToContinue = true;
    public float WaitingTime = 2f;
    public ChatBubble DialogueBubble;
    public ChatBubble NarratorBubble;
    public GameObject OptionBubblePrefab;

    public GameObject MrCube;
    public ParticleSystem[] particles;

    public RuntimeEnv Runtime { get; private set; } = new();
    protected readonly Compiler compiler = new();
    private IIRExecuter Executer => this;
    private bool _isRun = false;
    private bool _readyForNext = true;
    private float _waitingTime = 0;
    private IRInstruction _currentInst = null;

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
        Runtime.Functions.AddFunction<int>("SetCubeState", SetCubeState);
        Runtime.Functions.AddFunction("Emit", Emit);
        Runtime.Functions.AddFunction<int, int, int>("Mult", Mult);
        Runtime.Functions.AddFunction("RandInt", RandInt);
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
                    if (instruction is IR_Dialogue || instruction is IR_Menu)
                    {
                        _currentInst = instruction;
                    }
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
            if (instruction.SpeakerName == "MrCube")
            {
                SetCubeState(1);
            }
            DialogueBubble.SetText(instruction.SpeakerName + ": \n\t");
            PushFstringNodeToBubble(instruction.TextNode, DialogueBubble, runtime);
        }
        else
        {
            DialogueBubble.Hide = true;
            NarratorBubble.Clear();
            NarratorBubble.Hide = false;
            PushFstringNodeToBubble(instruction.TextNode, NarratorBubble, runtime);
        }
    }

    private void PostDialogue()
    {
        if (!_readyForNext && DialogueBubble.IsAllPushed && NarratorBubble.IsAllPushed && _currentInst is IR_Dialogue)
        {
            if (Runtime.HasNext && Runtime.LA(0) is IR_Menu)
            {
                _readyForNext = true;
            }
            else if (ClickToContinue)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    _readyForNext = true;
                }
            }
            else
            {
                _waitingTime += Time.deltaTime;
                if (_waitingTime > WaitingTime)
                {
                    _waitingTime = 0;
                    _readyForNext = true;
                }
            }
            SetCubeState(0);
        }
    }

    public void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime)
    {
        _readyForNext = false;
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
            var optionBubble = Instantiate(OptionBubblePrefab, currentBubble.transform.parent);
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
        if (!_readyForNext && _currentInst is IR_Menu menu)
        {
            var block = menu.Blocks[choice];
            if (block == null)
            {
                Debug.LogError($"No block found for choice {choice} in the current menu.");
                return;
            }

            foreach (var sub in DialogueBubble.SubBubbles)
            {
                sub.gameObject.SetActive(false);
                StartCoroutine(CorDestroy(sub.gameObject, 1f));
            }
            DialogueBubble.SubBubbles.Clear();
            foreach (var sub in NarratorBubble.SubBubbles)
            {
                sub.gameObject.SetActive(false);
                StartCoroutine(CorDestroy(sub.gameObject, 1f));

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

    public IEnumerator CorDestroy(GameObject obj, float delay = 1f)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    public int Mult(int a, int b)
    {
        return a * b;
    }

    public void SetCubeState(int state)
    {
        if (MrCube != null)
        {
            var animator = MrCube.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetInteger("state", state);
            }
        }
    }

    public void Emit()
    {
        if (particles != null)
        {
            foreach (var particle in particles)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }
        }
    }

    public int RandInt()
    {
        return UnityEngine.Random.Range(-100, 100);
    }
}
