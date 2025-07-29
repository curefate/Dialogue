# DialogueScripter

## Overview
`Dialoguescripter` is a scripting language for processing game scripts. This project aim to lower the threshold for writing interactive game scripts, let people who without programming background can easily read and write scripts.

![](show.gif)

The [script](Assets/Scripts/DS/Unity/TutorialScript.txt) used in the above figure.

***
## Grammar

DialogueScripter follows a strict intent grammar, somewhat similar to Python, but is designed specifically for interactive game script. You can use it in a variety of game types. The original script will be parsed by Antlr4 first, you can find the [lexer](Assets/Scripts/DS/Grammar/DSLexer.g4) and [parser](Assets/Scripts/DS/Grammar/DSParser.g4) definition here.

### Dialogue Sentence

Dialogue Sentence is usually the most used type of sentence.

The basic dialogue scentence is looks like:

`"Hi, nice to meet you."`

To add a spaeker for this sentence to make it more like a conversation, just add the speaker's name above.

`Amy "Hi, nice to meet you."`

You can also add some tags for this sentence, use `@` to indicate tag at the end.

`Amy "Hi, nice to meet you." @happy @anytag`
