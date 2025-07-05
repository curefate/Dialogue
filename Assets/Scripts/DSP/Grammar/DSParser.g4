parser grammar DSParser;

options {
    tokenVocab = DSLexer;
}

program
    : label_block* EOF
    ;

label_block
    : LABEL label=ID COLON NEWLINE INDENT* statement+ DEDENT*
    ;

statement
    : dialogue_stmt
    | menu_stmt
    | jump_stmt
    | tour_stmt
    | call_stmt
    | set_stmt
    | if_stmt
    ;

// ====================== dialogue ======================
dialogue_stmt
    : SYNC? speaker=ID? text=STRING tags+=TAG* NEWLINE
    ;

// ====================== menu ==========================
menu_stmt
    : options+=menu_item+
    ;

menu_item
    : text=STRING COLON NEWLINE block
    ;

// ====================== jump ==========================
jump_stmt
    : JUMP label=ID NEWLINE
    ;

// ====================== tour ==========================
tour_stmt
    : TOUR label=ID NEWLINE
    ;

// ====================== call ==========================
call_stmt
    : CALL func_name=ID LPAR (args+=expression (COMMA args+=expression)*)? RPAR NEWLINE
    ;

// ====================== set ===========================
set_stmt
    : VARIABLE eq=(EQUAL | PLUSEQUAL | MINEQUAL | STAREQUAL | SLASHEQUAL | PERCENTEQUAL) value=expression NEWLINE
    ;

// ====================== if ============================
if_stmt
    : IF conditions+=expression COLON NEWLINE blocks+=block (ELIF conditions+=expression COLON NEWLINE blocks+=block)* (ELSE COLON NEWLINE blocks+=block)?
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
    : expr_unary ((STAR | SLASH | PERCENT) expr_unary)*
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
    : INDENT+ statement+ DEDENT+
    ;

// ====================== backup ========================
/*
arg_pos
    : expression | STRING | BOOL | NUMBER | VARIABLE
    ;

arg_key
    : key=ID EQUAL value=(expression | STRING | BOOL | NUMBER | VARIABLE)
    ;

argument
    : expression | STRING | BOOL | NUMBER | VARIABLE
    ;
*/