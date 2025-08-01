//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from DSLexer.g4 by ANTLR 4.13.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419


using System.Collections.Generic;

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public partial class DSLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		INDENT=1, DEDENT=2, NL=3, STRING_START=4, STRING_CONTEXT=5, STRING_ESCAPE=6, 
		STRING_END=7, LPAR=8, RPAR=9, LBRACE=10, RBRACE=11, EXCLAMATION=12, PLUS=13, 
		MINUS=14, STAR=15, SLASH=16, LESS=17, GREATER=18, EQUAL=19, PERCENT=20, 
		EQEQUAL=21, NOTEQUAL=22, LESSEQUAL=23, GREATEREQUAL=24, PLUSEQUAL=25, 
		MINEQUAL=26, STAREQUAL=27, SLASHEQUAL=28, PERCENTEQUAL=29, AND=30, OR=31, 
		COLON=32, COMMA=33, CALL=34, IF=35, ELIF=36, ELSE=37, WHILE=38, JUMP=39, 
		TOUR=40, LABEL=41, BOOL=42, TRUE=43, FALSE=44, NUMBER=45, ID=46, TAG=47, 
		VARIABLE=48, WS=49, LINE_COMMENT=50, BLOCK_COMMENT=51, ERROR_CHAR=52, 
		NEWLINE=53, EMBED_WS=54;
	public const int
		STRING_MODE=1, EMBED_EXPR_MODE=2;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE", "STRING_MODE", "EMBED_EXPR_MODE"
	};

	public static readonly string[] ruleNames = {
		"LPAR", "RPAR", "LBRACE", "RBRACE", "EXCLAMATION", "PLUS", "MINUS", "STAR", 
		"SLASH", "LESS", "GREATER", "EQUAL", "PERCENT", "EQEQUAL", "NOTEQUAL", 
		"LESSEQUAL", "GREATEREQUAL", "PLUSEQUAL", "MINEQUAL", "STAREQUAL", "SLASHEQUAL", 
		"PERCENTEQUAL", "AND", "OR", "COLON", "COMMA", "CALL", "IF", "ELIF", "ELSE", 
		"WHILE", "JUMP", "TOUR", "LABEL", "BOOL", "TRUE", "FALSE", "NUMBER", "ID", 
		"TAG", "VARIABLE", "STRING_START", "INTEGER", "FLOAT", "NON_ZERO_DIGIT", 
		"DIGIT", "DOT", "AT", "ALPHABET", "CHAR", "WS", "LINE_COMMENT", "BLOCK_COMMENT", 
		"ERROR_CHAR", "NEWLINE", "EMBED_START", "STRING_ESCAPE", "STRING_CONTEXT", 
		"STRING_END", "STRING_NEWLINE", "EMBED_END", "EMBED_CALL", "EMBED_VAR", 
		"EMBED_WS", "EMBED_LPAR", "EMBED_RPAR", "EMBED_COMMA", "EMBED_ID", "EMBED_NUMBER", 
		"EMBED_BOOL", "EMBED_EXCLAMATION", "EMBED_PLUS", "EMBED_MINUS", "EMBED_STAR", 
		"EMBED_SLASH", "EMBED_LESS", "EMBED_GREATER", "EMBED_PERCENT", "EMBED_EQEQUAL", 
		"EMBED_NOTEQUAL", "EMBED_LESSEQUAL", "EMBED_GREATEREQUAL", "EMBED_AND", 
		"EMBED_OR", "EMBED_STRING_START"
	};


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
	            if (token.Type != NEWLINE && _tokenList.Count == 0)
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

	        // if (token.Channel == 0)
	            // UnityEngine.Debug.Log($"[{token.Channel}] {Vocabulary.GetSymbolicName(token.Type)}: {token.Text}: {token.Line}");
	        _pre_token = token;
	        return token;
	    }


	public DSLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public DSLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, null, null, null, null, "'('", "')'", "'{'", "'}'", 
		"'!'", "'+'", "'-'", "'*'", "'/'", "'<'", "'>'", "'='", "'%'", "'=='", 
		"'!='", "'<='", "'>='", "'+='", "'-='", "'*='", "'/='", "'%='", null, 
		null, "':'", "','", "'call'", "'if'", "'elif'", "'else'", "'while'", null, 
		null, null, null, "'true'", "'false'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "INDENT", "DEDENT", "NL", "STRING_START", "STRING_CONTEXT", "STRING_ESCAPE", 
		"STRING_END", "LPAR", "RPAR", "LBRACE", "RBRACE", "EXCLAMATION", "PLUS", 
		"MINUS", "STAR", "SLASH", "LESS", "GREATER", "EQUAL", "PERCENT", "EQEQUAL", 
		"NOTEQUAL", "LESSEQUAL", "GREATEREQUAL", "PLUSEQUAL", "MINEQUAL", "STAREQUAL", 
		"SLASHEQUAL", "PERCENTEQUAL", "AND", "OR", "COLON", "COMMA", "CALL", "IF", 
		"ELIF", "ELSE", "WHILE", "JUMP", "TOUR", "LABEL", "BOOL", "TRUE", "FALSE", 
		"NUMBER", "ID", "TAG", "VARIABLE", "WS", "LINE_COMMENT", "BLOCK_COMMENT", 
		"ERROR_CHAR", "NEWLINE", "EMBED_WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "DSLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static DSLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	public override void Action(RuleContext _localctx, int ruleIndex, int actionIndex) {
		switch (ruleIndex) {
		case 54 : NEWLINE_action(_localctx, actionIndex); break;
		}
	}
	private void NEWLINE_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:  HandleNewline();  break;
		}
	}

	private static int[] _serializedATN = {
		4,0,54,533,6,-1,6,-1,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,
		7,5,2,6,7,6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,
		7,13,2,14,7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,
		7,20,2,21,7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,
		7,27,2,28,7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,
		7,34,2,35,7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,
		7,41,2,42,7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,
		7,48,2,49,7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,2,55,
		7,55,2,56,7,56,2,57,7,57,2,58,7,58,2,59,7,59,2,60,7,60,2,61,7,61,2,62,
		7,62,2,63,7,63,2,64,7,64,2,65,7,65,2,66,7,66,2,67,7,67,2,68,7,68,2,69,
		7,69,2,70,7,70,2,71,7,71,2,72,7,72,2,73,7,73,2,74,7,74,2,75,7,75,2,76,
		7,76,2,77,7,77,2,78,7,78,2,79,7,79,2,80,7,80,2,81,7,81,2,82,7,82,2,83,
		7,83,2,84,7,84,1,0,1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,
		1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,11,1,11,1,12,1,12,1,13,1,13,1,13,1,
		14,1,14,1,14,1,15,1,15,1,15,1,16,1,16,1,16,1,17,1,17,1,17,1,18,1,18,1,
		18,1,19,1,19,1,19,1,20,1,20,1,20,1,21,1,21,1,21,1,22,1,22,1,22,1,22,1,
		22,3,22,232,8,22,1,23,1,23,1,23,1,23,3,23,238,8,23,1,24,1,24,1,25,1,25,
		1,26,1,26,1,26,1,26,1,26,1,27,1,27,1,27,1,28,1,28,1,28,1,28,1,28,1,29,
		1,29,1,29,1,29,1,29,1,30,1,30,1,30,1,30,1,30,1,30,1,31,1,31,1,31,1,31,
		1,31,1,31,3,31,274,8,31,1,32,1,32,1,32,1,32,1,32,1,32,1,32,3,32,283,8,
		32,1,33,1,33,1,33,1,33,1,33,1,33,3,33,291,8,33,1,34,1,34,3,34,295,8,34,
		1,35,1,35,1,35,1,35,1,35,1,36,1,36,1,36,1,36,1,36,1,36,1,37,3,37,309,8,
		37,1,37,1,37,3,37,313,8,37,1,38,1,38,5,38,317,8,38,10,38,12,38,320,9,38,
		1,39,1,39,4,39,324,8,39,11,39,12,39,325,1,40,1,40,1,40,1,41,1,41,1,41,
		1,41,1,42,1,42,1,42,4,42,338,8,42,11,42,12,42,339,3,42,342,8,42,1,43,1,
		43,1,43,5,43,347,8,43,10,43,12,43,350,9,43,1,44,1,44,1,45,1,45,1,46,1,
		46,1,47,1,47,1,48,1,48,1,49,1,49,1,50,1,50,1,50,1,50,1,51,1,51,5,51,370,
		8,51,10,51,12,51,373,9,51,1,51,1,51,1,52,1,52,1,52,1,52,1,52,5,52,382,
		8,52,10,52,12,52,385,9,52,1,52,1,52,1,52,1,52,1,52,1,52,1,53,1,53,1,53,
		1,53,1,54,3,54,398,8,54,1,54,1,54,1,54,1,55,1,55,1,55,1,55,1,55,1,56,1,
		56,1,56,1,56,1,56,1,56,3,56,414,8,56,1,57,4,57,417,8,57,11,57,12,57,418,
		1,58,1,58,1,58,1,58,1,59,3,59,426,8,59,1,59,1,59,1,59,1,59,1,60,1,60,1,
		60,1,60,1,60,1,61,1,61,1,61,1,61,1,62,1,62,1,62,1,62,1,63,1,63,1,63,1,
		63,1,64,1,64,1,64,1,64,1,65,1,65,1,65,1,65,1,66,1,66,1,66,1,66,1,67,1,
		67,1,67,1,67,1,68,1,68,1,68,1,68,1,69,1,69,1,69,1,69,1,70,1,70,1,70,1,
		70,1,71,1,71,1,71,1,71,1,72,1,72,1,72,1,72,1,73,1,73,1,73,1,73,1,74,1,
		74,1,74,1,74,1,75,1,75,1,75,1,75,1,76,1,76,1,76,1,76,1,77,1,77,1,77,1,
		77,1,78,1,78,1,78,1,78,1,79,1,79,1,79,1,79,1,80,1,80,1,80,1,80,1,81,1,
		81,1,81,1,81,1,82,1,82,1,82,1,82,1,83,1,83,1,83,1,83,1,84,1,84,1,84,1,
		84,1,84,1,383,0,85,3,8,5,9,7,10,9,11,11,12,13,13,15,14,17,15,19,16,21,
		17,23,18,25,19,27,20,29,21,31,22,33,23,35,24,37,25,39,26,41,27,43,28,45,
		29,47,30,49,31,51,32,53,33,55,34,57,35,59,36,61,37,63,38,65,39,67,40,69,
		41,71,42,73,43,75,44,77,45,79,46,81,47,83,48,85,4,87,0,89,0,91,0,93,0,
		95,0,97,0,99,0,101,0,103,49,105,50,107,51,109,52,111,53,113,0,115,6,117,
		5,119,7,121,0,123,0,125,0,127,0,129,54,131,0,133,0,135,0,137,0,139,0,141,
		0,143,0,145,0,147,0,149,0,151,0,153,0,155,0,157,0,159,0,161,0,163,0,165,
		0,167,0,169,0,171,0,3,0,1,2,8,1,0,49,57,1,0,48,57,3,0,65,90,95,95,97,122,
		4,0,48,57,65,90,95,95,97,122,3,0,9,9,12,12,32,32,2,0,10,10,13,13,8,0,34,
		34,39,39,92,92,98,98,102,102,110,110,114,114,116,116,6,0,10,10,13,13,34,
		34,92,92,123,123,125,125,542,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,
		0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,
		0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,
		1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,
		0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,
		1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,
		0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,
		1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,
		0,0,103,1,0,0,0,0,105,1,0,0,0,0,107,1,0,0,0,0,109,1,0,0,0,0,111,1,0,0,
		0,1,113,1,0,0,0,1,115,1,0,0,0,1,117,1,0,0,0,1,119,1,0,0,0,1,121,1,0,0,
		0,2,123,1,0,0,0,2,125,1,0,0,0,2,127,1,0,0,0,2,129,1,0,0,0,2,131,1,0,0,
		0,2,133,1,0,0,0,2,135,1,0,0,0,2,137,1,0,0,0,2,139,1,0,0,0,2,141,1,0,0,
		0,2,143,1,0,0,0,2,145,1,0,0,0,2,147,1,0,0,0,2,149,1,0,0,0,2,151,1,0,0,
		0,2,153,1,0,0,0,2,155,1,0,0,0,2,157,1,0,0,0,2,159,1,0,0,0,2,161,1,0,0,
		0,2,163,1,0,0,0,2,165,1,0,0,0,2,167,1,0,0,0,2,169,1,0,0,0,2,171,1,0,0,
		0,3,173,1,0,0,0,5,175,1,0,0,0,7,177,1,0,0,0,9,179,1,0,0,0,11,181,1,0,0,
		0,13,183,1,0,0,0,15,185,1,0,0,0,17,187,1,0,0,0,19,189,1,0,0,0,21,191,1,
		0,0,0,23,193,1,0,0,0,25,195,1,0,0,0,27,197,1,0,0,0,29,199,1,0,0,0,31,202,
		1,0,0,0,33,205,1,0,0,0,35,208,1,0,0,0,37,211,1,0,0,0,39,214,1,0,0,0,41,
		217,1,0,0,0,43,220,1,0,0,0,45,223,1,0,0,0,47,231,1,0,0,0,49,237,1,0,0,
		0,51,239,1,0,0,0,53,241,1,0,0,0,55,243,1,0,0,0,57,248,1,0,0,0,59,251,1,
		0,0,0,61,256,1,0,0,0,63,261,1,0,0,0,65,273,1,0,0,0,67,282,1,0,0,0,69,290,
		1,0,0,0,71,294,1,0,0,0,73,296,1,0,0,0,75,301,1,0,0,0,77,308,1,0,0,0,79,
		314,1,0,0,0,81,321,1,0,0,0,83,327,1,0,0,0,85,330,1,0,0,0,87,341,1,0,0,
		0,89,343,1,0,0,0,91,351,1,0,0,0,93,353,1,0,0,0,95,355,1,0,0,0,97,357,1,
		0,0,0,99,359,1,0,0,0,101,361,1,0,0,0,103,363,1,0,0,0,105,367,1,0,0,0,107,
		376,1,0,0,0,109,392,1,0,0,0,111,397,1,0,0,0,113,402,1,0,0,0,115,413,1,
		0,0,0,117,416,1,0,0,0,119,420,1,0,0,0,121,425,1,0,0,0,123,431,1,0,0,0,
		125,436,1,0,0,0,127,440,1,0,0,0,129,444,1,0,0,0,131,448,1,0,0,0,133,452,
		1,0,0,0,135,456,1,0,0,0,137,460,1,0,0,0,139,464,1,0,0,0,141,468,1,0,0,
		0,143,472,1,0,0,0,145,476,1,0,0,0,147,480,1,0,0,0,149,484,1,0,0,0,151,
		488,1,0,0,0,153,492,1,0,0,0,155,496,1,0,0,0,157,500,1,0,0,0,159,504,1,
		0,0,0,161,508,1,0,0,0,163,512,1,0,0,0,165,516,1,0,0,0,167,520,1,0,0,0,
		169,524,1,0,0,0,171,528,1,0,0,0,173,174,5,40,0,0,174,4,1,0,0,0,175,176,
		5,41,0,0,176,6,1,0,0,0,177,178,5,123,0,0,178,8,1,0,0,0,179,180,5,125,0,
		0,180,10,1,0,0,0,181,182,5,33,0,0,182,12,1,0,0,0,183,184,5,43,0,0,184,
		14,1,0,0,0,185,186,5,45,0,0,186,16,1,0,0,0,187,188,5,42,0,0,188,18,1,0,
		0,0,189,190,5,47,0,0,190,20,1,0,0,0,191,192,5,60,0,0,192,22,1,0,0,0,193,
		194,5,62,0,0,194,24,1,0,0,0,195,196,5,61,0,0,196,26,1,0,0,0,197,198,5,
		37,0,0,198,28,1,0,0,0,199,200,5,61,0,0,200,201,5,61,0,0,201,30,1,0,0,0,
		202,203,5,33,0,0,203,204,5,61,0,0,204,32,1,0,0,0,205,206,5,60,0,0,206,
		207,5,61,0,0,207,34,1,0,0,0,208,209,5,62,0,0,209,210,5,61,0,0,210,36,1,
		0,0,0,211,212,5,43,0,0,212,213,5,61,0,0,213,38,1,0,0,0,214,215,5,45,0,
		0,215,216,5,61,0,0,216,40,1,0,0,0,217,218,5,42,0,0,218,219,5,61,0,0,219,
		42,1,0,0,0,220,221,5,47,0,0,221,222,5,61,0,0,222,44,1,0,0,0,223,224,5,
		37,0,0,224,225,5,61,0,0,225,46,1,0,0,0,226,227,5,38,0,0,227,232,5,38,0,
		0,228,229,5,97,0,0,229,230,5,110,0,0,230,232,5,100,0,0,231,226,1,0,0,0,
		231,228,1,0,0,0,232,48,1,0,0,0,233,234,5,124,0,0,234,238,5,124,0,0,235,
		236,5,111,0,0,236,238,5,114,0,0,237,233,1,0,0,0,237,235,1,0,0,0,238,50,
		1,0,0,0,239,240,5,58,0,0,240,52,1,0,0,0,241,242,5,44,0,0,242,54,1,0,0,
		0,243,244,5,99,0,0,244,245,5,97,0,0,245,246,5,108,0,0,246,247,5,108,0,
		0,247,56,1,0,0,0,248,249,5,105,0,0,249,250,5,102,0,0,250,58,1,0,0,0,251,
		252,5,101,0,0,252,253,5,108,0,0,253,254,5,105,0,0,254,255,5,102,0,0,255,
		60,1,0,0,0,256,257,5,101,0,0,257,258,5,108,0,0,258,259,5,115,0,0,259,260,
		5,101,0,0,260,62,1,0,0,0,261,262,5,119,0,0,262,263,5,104,0,0,263,264,5,
		105,0,0,264,265,5,108,0,0,265,266,5,101,0,0,266,64,1,0,0,0,267,268,5,106,
		0,0,268,269,5,117,0,0,269,270,5,109,0,0,270,274,5,112,0,0,271,272,5,45,
		0,0,272,274,5,62,0,0,273,267,1,0,0,0,273,271,1,0,0,0,274,66,1,0,0,0,275,
		276,5,116,0,0,276,277,5,111,0,0,277,278,5,117,0,0,278,283,5,114,0,0,279,
		280,5,45,0,0,280,281,5,62,0,0,281,283,5,60,0,0,282,275,1,0,0,0,282,279,
		1,0,0,0,283,68,1,0,0,0,284,285,5,108,0,0,285,286,5,97,0,0,286,287,5,98,
		0,0,287,288,5,101,0,0,288,291,5,108,0,0,289,291,5,126,0,0,290,284,1,0,
		0,0,290,289,1,0,0,0,291,70,1,0,0,0,292,295,3,73,35,0,293,295,3,75,36,0,
		294,292,1,0,0,0,294,293,1,0,0,0,295,72,1,0,0,0,296,297,5,116,0,0,297,298,
		5,114,0,0,298,299,5,117,0,0,299,300,5,101,0,0,300,74,1,0,0,0,301,302,5,
		102,0,0,302,303,5,97,0,0,303,304,5,108,0,0,304,305,5,115,0,0,305,306,5,
		101,0,0,306,76,1,0,0,0,307,309,3,15,6,0,308,307,1,0,0,0,308,309,1,0,0,
		0,309,312,1,0,0,0,310,313,3,87,42,0,311,313,3,89,43,0,312,310,1,0,0,0,
		312,311,1,0,0,0,313,78,1,0,0,0,314,318,3,99,48,0,315,317,3,101,49,0,316,
		315,1,0,0,0,317,320,1,0,0,0,318,316,1,0,0,0,318,319,1,0,0,0,319,80,1,0,
		0,0,320,318,1,0,0,0,321,323,3,97,47,0,322,324,3,101,49,0,323,322,1,0,0,
		0,324,325,1,0,0,0,325,323,1,0,0,0,325,326,1,0,0,0,326,82,1,0,0,0,327,328,
		5,36,0,0,328,329,3,79,38,0,329,84,1,0,0,0,330,331,5,34,0,0,331,332,1,0,
		0,0,332,333,6,41,0,0,333,86,1,0,0,0,334,342,3,93,45,0,335,337,3,91,44,
		0,336,338,3,93,45,0,337,336,1,0,0,0,338,339,1,0,0,0,339,337,1,0,0,0,339,
		340,1,0,0,0,340,342,1,0,0,0,341,334,1,0,0,0,341,335,1,0,0,0,342,88,1,0,
		0,0,343,344,3,87,42,0,344,348,3,95,46,0,345,347,3,87,42,0,346,345,1,0,
		0,0,347,350,1,0,0,0,348,346,1,0,0,0,348,349,1,0,0,0,349,90,1,0,0,0,350,
		348,1,0,0,0,351,352,7,0,0,0,352,92,1,0,0,0,353,354,7,1,0,0,354,94,1,0,
		0,0,355,356,5,46,0,0,356,96,1,0,0,0,357,358,5,64,0,0,358,98,1,0,0,0,359,
		360,7,2,0,0,360,100,1,0,0,0,361,362,7,3,0,0,362,102,1,0,0,0,363,364,7,
		4,0,0,364,365,1,0,0,0,365,366,6,50,1,0,366,104,1,0,0,0,367,371,5,35,0,
		0,368,370,8,5,0,0,369,368,1,0,0,0,370,373,1,0,0,0,371,369,1,0,0,0,371,
		372,1,0,0,0,372,374,1,0,0,0,373,371,1,0,0,0,374,375,6,51,1,0,375,106,1,
		0,0,0,376,377,5,34,0,0,377,378,5,34,0,0,378,379,5,34,0,0,379,383,1,0,0,
		0,380,382,9,0,0,0,381,380,1,0,0,0,382,385,1,0,0,0,383,384,1,0,0,0,383,
		381,1,0,0,0,384,386,1,0,0,0,385,383,1,0,0,0,386,387,5,34,0,0,387,388,5,
		34,0,0,388,389,5,34,0,0,389,390,1,0,0,0,390,391,6,52,1,0,391,108,1,0,0,
		0,392,393,9,0,0,0,393,394,1,0,0,0,394,395,6,53,1,0,395,110,1,0,0,0,396,
		398,5,13,0,0,397,396,1,0,0,0,397,398,1,0,0,0,398,399,1,0,0,0,399,400,5,
		10,0,0,400,401,6,54,2,0,401,112,1,0,0,0,402,403,3,7,2,0,403,404,1,0,0,
		0,404,405,6,55,3,0,405,406,6,55,4,0,406,114,1,0,0,0,407,408,5,92,0,0,408,
		414,7,6,0,0,409,410,5,123,0,0,410,414,5,123,0,0,411,412,5,125,0,0,412,
		414,5,125,0,0,413,407,1,0,0,0,413,409,1,0,0,0,413,411,1,0,0,0,414,116,
		1,0,0,0,415,417,8,7,0,0,416,415,1,0,0,0,417,418,1,0,0,0,418,416,1,0,0,
		0,418,419,1,0,0,0,419,118,1,0,0,0,420,421,5,34,0,0,421,422,1,0,0,0,422,
		423,6,58,5,0,423,120,1,0,0,0,424,426,5,13,0,0,425,424,1,0,0,0,425,426,
		1,0,0,0,426,427,1,0,0,0,427,428,5,10,0,0,428,429,1,0,0,0,429,430,6,59,
		6,0,430,122,1,0,0,0,431,432,3,9,3,0,432,433,1,0,0,0,433,434,6,60,5,0,434,
		435,6,60,7,0,435,124,1,0,0,0,436,437,3,55,26,0,437,438,1,0,0,0,438,439,
		6,61,8,0,439,126,1,0,0,0,440,441,3,83,40,0,441,442,1,0,0,0,442,443,6,62,
		9,0,443,128,1,0,0,0,444,445,3,103,50,0,445,446,1,0,0,0,446,447,6,63,1,
		0,447,130,1,0,0,0,448,449,3,3,0,0,449,450,1,0,0,0,450,451,6,64,10,0,451,
		132,1,0,0,0,452,453,3,5,1,0,453,454,1,0,0,0,454,455,6,65,11,0,455,134,
		1,0,0,0,456,457,3,53,25,0,457,458,1,0,0,0,458,459,6,66,12,0,459,136,1,
		0,0,0,460,461,3,79,38,0,461,462,1,0,0,0,462,463,6,67,13,0,463,138,1,0,
		0,0,464,465,3,77,37,0,465,466,1,0,0,0,466,467,6,68,14,0,467,140,1,0,0,
		0,468,469,3,71,34,0,469,470,1,0,0,0,470,471,6,69,15,0,471,142,1,0,0,0,
		472,473,3,11,4,0,473,474,1,0,0,0,474,475,6,70,16,0,475,144,1,0,0,0,476,
		477,3,13,5,0,477,478,1,0,0,0,478,479,6,71,17,0,479,146,1,0,0,0,480,481,
		3,15,6,0,481,482,1,0,0,0,482,483,6,72,18,0,483,148,1,0,0,0,484,485,3,17,
		7,0,485,486,1,0,0,0,486,487,6,73,19,0,487,150,1,0,0,0,488,489,3,19,8,0,
		489,490,1,0,0,0,490,491,6,74,20,0,491,152,1,0,0,0,492,493,3,21,9,0,493,
		494,1,0,0,0,494,495,6,75,21,0,495,154,1,0,0,0,496,497,3,23,10,0,497,498,
		1,0,0,0,498,499,6,76,22,0,499,156,1,0,0,0,500,501,3,27,12,0,501,502,1,
		0,0,0,502,503,6,77,23,0,503,158,1,0,0,0,504,505,3,29,13,0,505,506,1,0,
		0,0,506,507,6,78,24,0,507,160,1,0,0,0,508,509,3,31,14,0,509,510,1,0,0,
		0,510,511,6,79,25,0,511,162,1,0,0,0,512,513,3,33,15,0,513,514,1,0,0,0,
		514,515,6,80,26,0,515,164,1,0,0,0,516,517,3,35,16,0,517,518,1,0,0,0,518,
		519,6,81,27,0,519,166,1,0,0,0,520,521,3,47,22,0,521,522,1,0,0,0,522,523,
		6,82,28,0,523,168,1,0,0,0,524,525,3,49,23,0,525,526,1,0,0,0,526,527,6,
		83,29,0,527,170,1,0,0,0,528,529,5,34,0,0,529,530,1,0,0,0,530,531,6,84,
		0,0,531,532,6,84,30,0,532,172,1,0,0,0,22,0,1,2,231,237,273,282,290,294,
		308,312,318,325,339,341,348,371,383,397,413,418,425,31,5,1,0,0,1,0,1,54,
		0,5,2,0,7,10,0,4,0,0,3,0,0,7,11,0,7,34,0,7,48,0,7,8,0,7,9,0,7,33,0,7,46,
		0,7,45,0,7,42,0,7,12,0,7,13,0,7,14,0,7,15,0,7,16,0,7,17,0,7,18,0,7,20,
		0,7,21,0,7,22,0,7,23,0,7,24,0,7,30,0,7,31,0,7,4,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
