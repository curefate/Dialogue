parser grammar DSParser;

options {
    tokenVocab = DSLexer;
}

// 语法规则
program
    : statement* EOF
    ;

/*
statement
    : if_stmt (NEWLINE | EOF)
    | while_stmt (NEWLINE | EOF)
    | jump_stmt (NEWLINE | EOF)
    | label_decl (NEWLINE | EOF)
    | dialogue_stmt (NEWLINE | EOF)
    | call_stmt (NEWLINE | EOF)
    | menu_stmt (NEWLINE | EOF)
    | assignment_stmt (NEWLINE | EOF)
    ;
*/

statement
    : dialogue_stmt
    | menu_stmt
    ;

if_stmt
    : IF expression COLON block (ELSE COLON block)?
    ;

while_stmt
    : WHILE expression COLON block
    ;

jump_stmt
    : JUMP label=ID
    ;

label_decl
    : LABEL label=ID COLON
    ;

dialogue_stmt
    : SYNC? speaker=ID? text=STRING tags+=TAG* (LINEMK | EOF)
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
    : (literal | VARIABLE | ID)
    ;

call_arg_key
    : ID EQ (expression | literal | VARIABLE)
    ;

menu_stmt
    : MENU COLON LINEMK INDENT intro=dialogue_stmt? menu_item+ LINEMK DEDENT
    ;

menu_item
    : option=STRING COLON LINEMK block
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
    ;

literal
    : NUMBER
    | BOOL
    | STRING
    ;