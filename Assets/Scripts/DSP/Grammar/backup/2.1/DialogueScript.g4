parser grammar DialogueScript;

options {
    tokenVocab = DialogueScriptLexer;  // 分离词法规则（可选）
}

// 语法规则
program
    : (import_decl | statement)* EOF
    ;

import_decl
    : IMPORT file_path
    ;

statement
    : if_stmt
    | while_stmt
    | jump_stmt
    | label_decl
    | dialogue_stmt
    | call_stmt
    | menu_stmt
    | assignment_stmt
    ;

if_stmt
    : IF expression COLON block (ELSE COLON block)?
    ;

while_stmt
    : WHILE expression COLON block
    ;

jump_stmt
    : JUMP ID
    ;

label_decl
    : LABEL ID COLON
    ;

dialogue_stmt
    : SYNC? ID? STRING (WITH tags)?
    ;

call_stmt
    : SYNC? call_command call_arg_pos* call_arg_key*
    ;

call_command
    : CALL ID      # CallCustomCommand
    | PLAY         # PlayCommand
    | HIDE         # HideCommand
    | SHOW         # ShowCommand
    | WAIT         # WaitCommand
    ;

call_arg_pos
    : (literal | VARIABLE)
    ;

call_arg_key
    : ID EQ (expression | literal | VARIABLE)
    ;

menu_stmt
    : MENU COLON dialogue_stmt? menu_item+
    ;

menu_item
    : STRING COLON block
    ;

assignment_stmt
    : VARIABLE EQ expression
    ;

expression
    : logical_and (OR logical_and)*
    ;

logical_and
    : equality (AND equality)*
    ;

equality
    : comparison (EQ | NEQ) comparison
    | comparison
    ;

comparison
    : term ((GT | LT | GTE | LTE) term)*
    | term
    ;

term
    : factor ((PLUS | MINUS) factor)*
    | factor
    ;

factor
    : unary ((MUL | DIV) unary)*
    | unary
    ;

unary
    : (PLUS | MINUS | NOT)? primary
    ;

primary
    : VARIABLE
    | NUMBER
    | BOOL
    | STRING
    | LPAREN expression RPAREN
    ;

block
    : INDENT statement+ DEDENT
    | statement
    ;

tags
    : (ID | STRING) (WS (ID | STRING))*
    ;

file_path
    : ID (DOT ID)*
    ;

literal
    : NUMBER
    | BOOL
    | STRING
    ;