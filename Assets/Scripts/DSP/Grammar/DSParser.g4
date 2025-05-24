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

jump_stmt
    : JUMP label=ID NEWLINE
    ;

call_stmt
    : SYNC? call_command args_p+=call_arg_pos* args_k+=call_arg_key*
    ;

call_command
    : CALL func=ID # CallCustomCommand
    | PLAY         # PlayCommand
    | HIDE         # HideCommand
    | SHOW         # ShowCommand
    | WAIT         # WaitCommand
    ;

call_arg_pos
    : STRING | BOOL | NUMBER | VARIABLE
    ;

call_arg_key
    : ID EQUAL (expression | STRING | BOOL | NUMBER | VARIABLE)
    ;

assignment_stmt
    : VARIABLE (EQUAL | PLUSEQUAL | MINEQUAL | STAREQUAL | SLASHEQUAL | PERCENTEQUAL) expression
    ;

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