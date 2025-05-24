lexer grammar DSLexer;

options {
    language=CSharp;
}

@header {
using System.Collections.Generic;
}

tokens { 
    INDENT, DEDENT, STRING, NL
}

@lexer::members {
    private Stack<int> _indentStack = new();
    private List<IToken> _tokenList = new();
    private int _currentIndent = 0;
    private IToken _pre_token = null;
	private bool _fbl = true;

    private void HandleNewline() 
    {
        int newIndent = 0;
        while (InputStream.LA(1) == ' ' || InputStream.LA(1) == '\t') 
        {
            newIndent += (InputStream.LA(1) == '\t') ? 4 : 1;
            InputStream.Consume();
        }
        if (InputStream.LA(1) == '\r' || InputStream.LA(1) == '\n' || InputStream.LA(1) == Eof)
			return;

        newIndent /= 4;
        if (newIndent > _currentIndent)
        {
            var token = new CommonToken(INDENT, "INDENT");
            _tokenList.Add(token);
            _indentStack.Push(_currentIndent);
            _currentIndent = newIndent;
        } 
        else if (newIndent < _currentIndent)
        {
            while (_currentIndent > newIndent)
            {
                var token = new CommonToken(DEDENT, "DEDENT");
                _tokenList.Add(token);
                _currentIndent = _indentStack.Count > 0 ? _indentStack.Pop() : 0;
            }
        }
    }

    public override IToken NextToken()
    {
        IToken token = null;
        if (_tokenList.Count > 0)
        {
            token = _tokenList[0];
            _tokenList.RemoveAt(0);
        }
        else
        {
            token = base.NextToken();
            if (_fbl && token.Channel == 0 && token.Type != NEWLINE)
				_fbl = false;
        }

        if (InputStream.LA(1) == Eof)
		{
			while (_indentStack.Count > 0)
			{
				var tokenDedent = new CommonToken(DEDENT, "DEDENT");
				_tokenList.Add(tokenDedent);
				_indentStack.Pop();
			}
		}

        if (_fbl && token.Type == NEWLINE)
			return NextToken();

        if (_pre_token != null && _pre_token.Type == NEWLINE && token.Type == NEWLINE)
			return NextToken();

        if (token.Channel == 0)
            UnityEngine.Debug.Log($"[{token.Channel}] {Vocabulary.GetSymbolicName(token.Type)}: {token.Text}: {token.Line}");
        _pre_token = token;
        return token;
    }
}

// ====================== expr =========================
LPAR         : '('; // OPEN_PAREN
RPAR         : ')'; // CLOSE_PAREN
EXCLAMATION  : '!';
PLUS         : '+';
MINUS        : '-';
STAR         : '*';
SLASH        : '/';
LESS         : '<';
GREATER      : '>';
EQUAL        : '=';
PERCENT      : '%';
EQEQUAL      : '==';
NOTEQUAL     : '!=';
LESSEQUAL    : '<=';
GREATEREQUAL : '>=';
PLUSEQUAL    : '+=';
MINEQUAL     : '-=';
STAREQUAL    : '*=';
SLASHEQUAL   : '/=';
PERCENTEQUAL : '%=';
AND          : '&&' | 'and';
OR           : '||' | 'or';

// ===================== keyworkds =====================
COLON  : ':';
CALL   : 'call';
PLAY   : 'play';
HIDE   : 'hide';
SHOW   : 'show';
WAIT   : 'wait';
IF     : 'if';
ELSE   : 'else';
WHILE  : 'while';
JUMP   : 'jump';
LABEL  : 'label';
SYNC   : 'sync';
MENU   : 'menu';
// IMPORT : 'import';
// MATCH  : 'match';
// CASE   : 'case';

// ===================== literals ======================
BOOL         : TRUE | FALSE;
TRUE         : 'true';
FALSE        : 'false';
NUMBER       : MINUS? (INTEGER | FLOAT);
ID           : ALPHABET CHAR*;
TAG          : AT CHAR+;
VARIABLE     : '$' ID (DOT ID)*;
STRING_START : '"' -> pushMode(STRING_MODE), more;

// ===================== fragment ======================
fragment INTEGER        : DIGIT | (NON_ZERO_DIGIT DIGIT+);
fragment FLOAT          : INTEGER DOT INTEGER*;
fragment NON_ZERO_DIGIT : [1-9];
fragment DIGIT          : [0-9];
fragment DOT            : '.';
fragment AT             : '@';
fragment ALPHABET       : [a-zA-Z_];
fragment CHAR           : [a-zA-Z0-9_];

// ===================== others ========================
WS            : [ \t\f]         -> channel(HIDDEN);
LINE_COMMENT  : '#' ~[\r\n]*    -> channel(HIDDEN);
BLOCK_COMMENT : '"""' .*? '"""' -> channel(HIDDEN);
ERROR_CHAR    : .               -> channel(HIDDEN);
NEWLINE       : '\r'? '\n'      { HandleNewline(); };

// ===================== mode ==========================
mode STRING_MODE;
STRING_CONTENT : ~["\\\r\n]+      -> more;  // 匹配非引号、非转义字符
STRING_ESCAPE  : '\\' [btnfr"'\\] -> more;  // 处理转义字符，如 \n, \"
STRING_END     : '"'              -> popMode, type(STRING);  // 遇到闭合引号，退出模式并生成STRING Token
STRING_NEWLINE : ('\r'? '\n')     -> more;  // 处理字符串内的换行（跨行支持）

// ===================== backup ========================
// SEMI             : ';';
// VBAR             : '|';
// AMPER            : '&';
// TILDE            : '~';
// CIRCUMFLEX       : '^';
// LEFTSHIFT        : '<<';
// RIGHTSHIFT       : '>>';
// DOUBLESTAR       : '**';
// AMPEREQUAL       : '&=';
// VBAREQUAL        : '|=';
// CIRCUMFLEXEQUAL  : '^=';
// LEFTSHIFTEQUAL   : '<<=';
// RIGHTSHIFTEQUAL  : '>>=';
// DOUBLESTAREQUAL  : '**=';
// DOUBLESLASH      : '//';
// DOUBLESLASHEQUAL : '//=';
// ATEQUAL          : '@=';
// RARROW           : '->';
// ELLIPSIS         : '...';
// COLONEQUAL       : ':=';
// LSQB             : '['; // OPEN_BRACK
// RSQB             : ']'; // CLOSE_BRACK
// LBRACE           : '{'; // OPEN_BRACE
// RBRACE           : '}'; // CLOSE_BRACE
// COMMA            : ',';