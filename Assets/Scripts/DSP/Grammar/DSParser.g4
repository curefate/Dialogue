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
    | jump_stmt
    | call_stmt
    | assign_stmt
    | if_stmt
    ;

// ====================== dialogue ======================
dialogue_stmt
    : SYNC? speaker=ID? text=STRING tags+=TAG* NEWLINE
    ;

// ====================== menu ==========================
menu_stmt
    : MENU COLON NEWLINE INDENT intro=dialogue_stmt? option+=menu_item+ DEDENT
    ;

menu_item
    : text=STRING COLON NEWLINE block
    ;

// ====================== jump ==========================
jump_stmt
    : JUMP label=ID NEWLINE
    ;

// ====================== call ==========================
call_stmt
    : SYNC? (call_command | call_function) args_p+=call_arg_pos* args_k+=call_arg_key* NEWLINE
    ;

call_command
    : PLAY
    | HIDE
    | SHOW
    | WAIT
    ;

call_function
    : CALL func=ID
    ;

call_arg_pos
    : STRING | BOOL | NUMBER | VARIABLE
    ;

call_arg_key
    : ID EQUAL (expression | STRING | BOOL | NUMBER | VARIABLE)
    ;

// ====================== assign ========================
assign_stmt
    : VARIABLE (EQUAL | PLUSEQUAL | MINEQUAL | STAREQUAL | SLASHEQUAL | PERCENTEQUAL) expression NEWLINE
    ;

// ====================== if ============================
if_stmt
    : IF expression COLON NEWLINE if_block=block (ELSE COLON NEWLINE else_block=block)?
    ;

// ====================== others ========================
expression
    : expr_logical_and (OR expr_logical_and)*
    ;

expr_logical_and
    : expr_equality (AND expr_equality)*
    ;

expr_equality
    : expr_comparison ((EQEQUAL | NOTEQUAL) expr_comparison)*
    ;

expr_comparison
    : expr_term ((GREATER | LESS | GREATEREQUAL | LESSEQUAL) expr_term)*
    ;

expr_term
    : expr_factor ((PLUS | MINUS) expr_factor)*
    ;

expr_factor
    : expr_unary ((STAR | SLASH) expr_unary)*
    ;

expr_unary
    : (PLUS | MINUS | EXCLAMATION)? expr_primary
    ;

expr_primary
    : VARIABLE
    | NUMBER
    | BOOL
    | STRING
    | LPAR expression RPAR
    ;

block
    : INDENT statement+ DEDENT
    ;