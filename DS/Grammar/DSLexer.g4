lexer grammar DSLexer;

options {
    language=CSharp;
}

@header {
using System.Collections.Generic;
}

tokens { 
    INDENT, DEDENT, NL,
    STRING_START, STRING_CONTEXT, STRING_ESCAPE, STRING_END,
    PATH
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
            if (token.Type != NEWLINE && token.Type != DEDENT && _tokenList.Count == 0)
			{
				var newlineToken = new CommonToken(NEWLINE, "\n");
				_tokenList.Add(newlineToken);
				_pre_token = token;
				return token;
			}

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

        /*
        if (token.Channel == 0)
            System.Console.WriteLine($"[{token.Channel}] {Vocabulary.GetSymbolicName(token.Type)}: {token.Text}: {token.Line}");
        */

        _pre_token = token;
        return token;
    }
}

// ====================== expr =========================
LPAR         : '(';
RPAR         : ')';
LBRACE       : '{';
RBRACE       : '}';
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
COMMA  : ',';
CALL   : 'call';
IF     : 'if';
NOT    : 'not';
ELIF   : 'elif';
ELSE   : 'else';
// WHILE  : 'while';
// MATCH  : 'match';
// CASE   : 'case';
JUMP   : 'jump' | '->';
TOUR   : 'tour' | '-><';
LABEL  : 'label' | '~';
IMPORT : 'import' -> pushMode(PATH_MODE);

// ===================== literals ======================
BOOL         : TRUE | FALSE;
TRUE         : 'true';
FALSE        : 'false';
NUMBER       : MINUS? (INTEGER | FLOAT);
ID           : ALPHABET CHAR*;
TAG          : AT CHAR+;
VARIABLE     : '$' ID;
STRING_START : '"' -> pushMode(STRING_MODE);

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
ERROR_CHAR    : .               -> channel(HIDDEN);
NEWLINE       : '\r'? '\n'      { HandleNewline(); };

// ===================== mode ==========================
mode STRING_MODE;
EMBED_START       : LBRACE -> pushMode(EMBED_EXPR_MODE), type(LBRACE);
STRING_ESCAPE     : '\\' [btnfr'"\\] | '{{' | '}}';
STRING_CONTEXT    : ~["\\\r\n{}]+;
STRING_END        : '"' -> popMode;
STRING_NEWLINE    : ('\r'? '\n') -> more;

mode EMBED_EXPR_MODE;
EMBED_END          : RBRACE -> popMode, type(RBRACE);
EMBED_CALL         : CALL -> type(CALL);
EMBED_VAR          : VARIABLE -> type(VARIABLE);
EMBED_WS           : WS -> channel(HIDDEN);
EMBED_LPAR         : LPAR -> type(LPAR);
EMBED_RPAR         : RPAR -> type(RPAR);
EMBED_COMMA        : COMMA -> type(COMMA);
EMBED_ID           : ID -> type(ID);
EMBED_NUMBER       : NUMBER -> type(NUMBER);
EMBED_BOOL         : BOOL -> type(BOOL);
EMBED_EXCLAMATION  : EXCLAMATION -> type(EXCLAMATION);
EMBED_PLUS         : PLUS -> type(PLUS);
EMBED_MINUS        : MINUS -> type(MINUS);
EMBED_STAR         : STAR -> type(STAR);
EMBED_SLASH        : SLASH -> type(SLASH);
EMBED_LESS         : LESS -> type(LESS);
EMBED_GREATER      : GREATER -> type(GREATER);
EMBED_PERCENT      : PERCENT -> type(PERCENT);
EMBED_EQEQUAL      : EQEQUAL -> type(EQEQUAL);
EMBED_NOTEQUAL     : NOTEQUAL -> type(NOTEQUAL);
EMBED_LESSEQUAL    : LESSEQUAL -> type(LESSEQUAL);
EMBED_GREATEREQUAL : GREATEREQUAL -> type(GREATEREQUAL);
EMBED_AND          : AND -> type(AND);
EMBED_OR           : OR -> type(OR);
EMBED_STRING_START : '"' -> pushMode(STRING_MODE), type(STRING_START);

mode PATH_MODE;
PATH_WS       : WS -> channel(HIDDEN);
QUOTED_PATH   : '"' (~["\\] | '\\' .)* '"' -> type(PATH);
UNQUOTED_PATH : ~[ *?<>|\t\r\n"]+ -> type(PATH);
PATH_NEWLINE  : ('\r'? '\n') -> popMode, type(NEWLINE);

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