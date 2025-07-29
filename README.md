# DialogueScripter

## Overview
`Dialoguescripter` is a scripting language for processing game scripts. This project aim to lower the threshold for writing interactive game scripts, let people who without programming background can easily read and write scripts.

![](show.gif)

The [script](Assets/Scripts/DS/Unity/TutorialScript.txt) used in the above figure.

***
## Grammar

DialogueScripter follows a strict intent grammar, somewhat similar to Python, but is designed specifically for interactive game scripts. You can use it in a variety of game types.

The parser is based on Antlr4, you can find the [lexer](Assets/Scripts/DS/Grammar/DSLexer.g4) and [parser](Assets/Scripts/DS/Grammar/DSParser.g4) definition here.

### Label

Label is to declare a label block. All the statements are must be included under a Label. For example:

```
label labelName:
# statements below...

~ anotherLabel:
# Allows use '~' to instead "label" keyword.
```

### Dialogue Statement

Dialogue statement is usually the most used type of statements.

The basic dialogue scentence is looks like:

```"Hi, nice to meet you."```

To add a spaeker for this statement to make it more like a conversation, just add the speaker's name above.

```Amy "Hi, nice to meet you."```

You can also add some tags for this statement, use `@` to indicate tag at the end.

```Amy "Hi, nice to meet you." @happy @anytag```

### Menu Statement

The Menu statement is used to add branches, it is often used after Dialogue statement, for example:

```
Amy "Do you like me?"
"Yes":
  # Note that each branch option sub-block needs to be indented.
  Amy "Great! I like you too!" @happy
"No":
  Amy "Okay...That's fine..."
Amy "See you tomorrow."
```

### Jump Statement

Used to flow control, jump to target label, for example:

```
label A:
"Here is label A."
jump B

label B:
"Here is label B."
-> C # Allows use "->" to instead "jump" keyword.

~ C:
"Here is label C."
```

### Tour Statement

Used to flow control, jump to target label then go back to current position, for example:

```
label A:
"Here is label A."
tour B # Or use "-><" to instead "tour" keyword.
"Back to label A!"

label B:
"Here is label B."
```

### Set Statement

Used to define or modify variables， for example:

```
$a = 10
$b = 15
$bool = a > b
$myname = "Amy"
```

Variabels can be embed in string by using `{}`, in this case they will be convert to string automatically.

```
Amy "My name is {$Amy}!"
```

You can access these variabels by `RuntimeEnv.Variables.Get(string varName)`.

### Call Statement

Call statement is used to call a function, for example:

```
call FunctionA()
call FunctionB($param, 1, "any param")
```

Call statement can also be embed in string, even in Dialogue statement. If the called function has a return value, it will be convert to string automatically when it is in a string.

```
$a = 10
"{$a}^2 equals {call Pow($a)}"
"({$a}^2)^2 equals {call Pow({call Pow{$a}})}"
```

The called function must be registered in the registry, use `RuntimeEnv.Functions.AddFunction(string FuncName, Delegate delegate)` to regist.

### If Statement

Used for flow control, enter different branches based on conditions, for example:

```
if $score>100:
  jump win
elif $score>60:
  jump normal
else:
  jump lose
```

Note that each branch's sub-block needs to be indented.

## Others

- This project is in a early stages and I'd appreciate any feedback.
