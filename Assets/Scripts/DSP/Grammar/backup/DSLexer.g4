lexer grammar DSLexer;

options {
    language=CSharp;
}

@header {
using System.Collections.Generic;
}

tokens { 
    INDENT, DEDENT, STRING
}

@lexer::members {
    private Stack<int> _indentStack = new Stack<int>();
    private int _currentIndent = 0;

    private void HandleNewline() {
        // 跳过换行符
        Skip();
        
        // 计算新行的缩进空格数
        int newIndent = 0;
         while (InputStream.LA(1) == ' ' || InputStream.LA(1) == '\t') {
             newIndent += (InputStream.LA(1) == '\t') ? 4 : 1;
            InputStream.Consume();
        }
        newIndent /= 4;

        // 处理缩进变化
        if (newIndent > _currentIndent) {
            Emit(new CommonToken(INDENT, "INDENT"));
            _indentStack.Push(_currentIndent);
            _currentIndent = newIndent;
        } 
        else if (newIndent < _currentIndent) {
            while (_currentIndent > newIndent) {
                Emit(new CommonToken(DEDENT, "DEDENT"));
                _currentIndent = _indentStack.Count > 0 ? _indentStack.Pop() : 0;
            }
        }
        // 缩进不变时不生成任何 Token
    }

    public override IToken NextToken() {
        var token = base.NextToken();
        if (token.Type == Eof) {
            while (_currentIndent > 0) {
                Emit(new CommonToken(DEDENT, "DEDENT"));
                _currentIndent = _indentStack.Count > 0 ? _indentStack.Pop() : 0;
            }
        }
        return token;
    }
}

// ========= 词法规则 =========
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
SYNC: 'sync';
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

// 字面量
TAG: '@' [a-zA-Z0-9_]+;
ID: [a-zA-Z_] [a-zA-Z0-9_]*;
VARIABLE: '$' ID ('.' ID)*;
NUMBER: '-'? [0-9]+ ('.' [0-9]+)?;
STRING_START: '"' -> pushMode(STRING_MODE), more;
BOOL: TRUE | FALSE;

// 空白和注释
// WS: [ \t]+;
NEWLINE : '\r'? '\n' { HandleNewline(); };
LINE_COMMENT: '#' ~[\r\n]* -> skip;
BLOCK_COMMENT: '"""' .*? '"""' -> skip;

// 转义序列
fragment ESCAPE_SEQ: '\\' [btnfr"'\\];

mode STRING_MODE;
// 字符串内容（支持跨行）
STRING_CONTENT: ~["\\\r\n]+ -> more;  // 匹配非引号、非转义字符
STRING_ESCAPE: '\\' [btnfr"'\\] -> more;  // 处理转义字符，如 \n, \"
STRING_END: '"' -> popMode, type(STRING);  // 遇到闭合引号，退出模式并生成STRING Token
STRING_NEWLINE: ('\r'? '\n') -> more;  // 处理字符串内的换行（跨行支持）