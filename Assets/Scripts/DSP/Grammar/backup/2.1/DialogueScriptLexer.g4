lexer grammar DialogueScriptLexer;

// 关键字
CALL: 'call';
PLAY: 'play';
HIDE: 'hide';
SHOW: 'show';
WAIT: 'wait';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
JUMP: 'jump';
LABEL: 'label';
IMPORT: 'import';
SYNC: 'sync';
WITH: 'with';
MENU: 'menu';
TRUE: 'true';
FALSE: 'false';

// 运算符
EQ: '==';
NEQ: '!=';
GT: '>';
LT: '<';
GTE: '>=';
LTE: '<=';
PLUS: '+';
MINUS: '-';
MUL: '*';
DIV: '/';
NOT: '!';
OR: '||';
AND: '&&';
ASSIGN: '=';  // 单独定义以禁止空格

// 分隔符
DOT: '.';
COLON: ':';
LPAREN: '(';
RPAREN: ')';
INDENT: '    ' -> pushMode(INDENT_MODE);  // 4空格缩进
DEDENT: -> popMode;

// 标识符和字面量
ID: [a-zA-Z_] [a-zA-Z0-9_]*;
VARIABLE: '$' ID ('.' ID)*;
NUMBER: '-'? [0-9]+ ('.' [0-9]+)?;
STRING: '"' (~["\\] | ESCAPE_SEQ)* '"';
BOOL: TRUE | FALSE;

// 空白和注释
WS: [ \t\r]+ -> skip;
NEWLINE: '\r'? '\n' -> skip;
LINE_COMMENT: '#' ~[\r\n]* -> skip;
BLOCK_COMMENT: '"""' .*? '"""' -> skip;

// 转义序列
fragment ESCAPE_SEQ: '\\' [btnfr"'\\];

// 缩进处理模式
mode INDENT_MODE;
I_WS: '    ' -> type(WS);  // 嵌套缩进
I_DEDENT: -> type(DEDENT), popMode;
I_TOKEN: -> skip, popMode;  // 其他token退出缩进模式