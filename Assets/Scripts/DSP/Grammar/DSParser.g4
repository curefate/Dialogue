parser grammar DSParser;

options {
    tokenVocab = DSLexer;
}

program
    : (statement | label_decl)* EOF
    ;

label_decl
    : LABEL label=ID COLON NEWLINE
    ;

statement
    : dialogue_stmt
    | menu_stmt
    ;

dialogue_stmt
    : SYNC? speaker=ID? text=STRING tags+=TAG* NEWLINE
    ;

menu_stmt
    : MENU COLON NEWLINE INDENT intro=dialogue_stmt? option+=menu_item+ DEDENT
    ;

menu_item
    : text=STRING COLON NEWLINE block
    ;

block
    : INDENT statement+ DEDENT
    ;