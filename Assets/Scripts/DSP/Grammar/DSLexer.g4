lexer grammar DSLexer;

options {
    language=CSharp;
}

@header {
using System.Collections.Generic;
}

tokens { 
    INDENT, DEDENT, STRING, LINEMK
}

@lexer::members {
    private Stack<int> _indentStack = new Stack<int>();
    private int _currentIndent = 0;

    private void HandleNewline() {
        int newIndent = 0;
         while (InputStream.LA(1) == ' ' || InputStream.LA(1) == '\t') {
             newIndent += (InputStream.LA(1) == '\t') ? 4 : 1;
            InputStream.Consume();
        }
        if (InputStream.LA(1) == '\r' || InputStream.LA(1) == '\n' || InputStream.LA(1) == Eof) {
			Skip();
			return;
		}

        newIndent /= 4;
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
        else {
            Emit(new CommonToken(LINEMK, "LINEMK"));
        }
    }

    public override IToken NextToken() {
        var token = base.NextToken();
        UnityEngine.Debug.Log($"[{token.Channel}] {Vocabulary.GetSymbolicName(token.Type)}: {token.Text}: {token.Line}");
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
// IMPORT: 'import';

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
NEWLINE: '\r'? '\n' { HandleNewline(); };
WS: [ \t]+ -> channel(HIDDEN);
LINE_COMMENT: '#' ~[\r\n]* -> skip;
BLOCK_COMMENT: '"""' .*? '"""' -> skip;

// 转义序列
fragment ESCAPE_SEQ: '\\' [btnfr"'\\];

// 捕获错误词法
ERROR_CHAR: . -> channel(HIDDEN);

mode STRING_MODE;
// 字符串内容（支持跨行）
STRING_CONTENT: ~["\\\r\n]+ -> more;  // 匹配非引号、非转义字符
STRING_ESCAPE: '\\' [btnfr"'\\] -> more;  // 处理转义字符，如 \n, \"
STRING_END: '"' -> popMode, type(STRING);  // 遇到闭合引号，退出模式并生成STRING Token
STRING_NEWLINE: ('\r'? '\n') -> more;  // 处理字符串内的换行（跨行支持）