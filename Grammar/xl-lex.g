// ------------------------
// --- X-Language Lexer ---
// ------------------------

// -------------
// --- Lexer ---
// -------------

options {
	mangleLiteralPrefix = "TK_";
	language = "CSharp";
	namespace = "XL.Parser";
}

class XLLexer extends Lexer;
options {
	k=2;
	exportVocab=XL;
	charVocabulary = '\u0000'..'\u007F';
}

tokens {
	KW_PROCESS="process";
	KW_SCHEMA="schema";
	KW_VOID="void";
	KW_BOOL="bool";
	KW_INT="int";
	KW_FLOAT="float";
	KW_CHAR="char";
	KW_STRING="string";
	KW_NEW="new";
	KW_SELECT="select";
	KW_FORK="fork";
	KW_IF="if";
	KW_ELSE="else";
	KW_SWITCH="switch";
	KW_CASE="case";
	KW_TRUE="true";
	KW_FALSE="false";
}

// operators
LPAREN	:	'('	;
RPAREN	:	')'	;
LCURLY	:	'{'	;
RCURLY	:	'}'	;
LSQUARE	:	'['	;
RSQUARE	:	']'	;
STAR	:	'*'	;
PLUS	:	'+'	;
PAR	:	'|'	;
ASSIGN	:	'='	;
COLON	:	':'	;
SEMI	:	';'	;
COMMA	:	','	;
DOT	:	'.'	;
FLAT	:	'_'	;

// whitespace -- ignored
WS	:	(	' '
		|	'\t'
		|	'\f'
		// handle newlines
		|	(	options {generateAmbigWarnings=false;}
			:	"\r\n"  // DOS
			|	'\r'    // Macintosh
			|	'\n'    // Unix
			)
			{ newline(); }
		)+
		{ _ttype = Token.SKIP; }
	;

// single-line comments
SL_COMMENT
options {
	paraphrase = "comment";
}
	:	"//" (~('\n'|'\r'))* ('\n'|'\r'('\n')?) {$setType(Token.SKIP); newline();}
	;

// multiple-line comments
ML_COMMENT
	:	"/*"
		(	/*	'\r' '\n' can be matched in one alternative or by matching
				'\r' in one iteration and '\n' in another.  I am trying to
				handle any flavor of newline that comes in, but the language
				that allows both "\r\n" and "\r" and "\n" to all be valid
				newline is ambiguous.  Consequently, the resulting grammar
				must be ambiguous.  I'm shutting this warning off.
			 */
			options {
				generateAmbigWarnings=false;
			}
		:
			{ LA(2)!='/' }? '*'
		|	'\r' '\n'		{newline();}
		|	'\r'			{newline();}
		|	'\n'			{newline();}
		|	~('*'|'\n'|'\r')
		)*
		"*/"
		{$setType(Token.SKIP);}
	;

// character literals
CHAR_LITERAL
options {
	paraphrase = "char";
}
	:	'\'' (ESC|~'\'') '\''
	;

// string literals
STRING_LITERAL
options {
	paraphrase = "string";
}
	:	'"' (ESC|~('"'|'\\'))* '"'
	;

// escape sequence -- note that this is protected; it can only be called
//   from another lexer rule -- it will not ever directly return a token to
//   the parser
// There are various ambiguities hushed in this rule.  The optional
// '0'...'9' digit matches should be matched here rather than letting
// them go back to STRING_LITERAL to be matched.  ANTLR does the
// right thing by matching immediately; hence, it's ok to shut off
// the FOLLOW ambig warnings.
protected
ESC
	:	'\\'
		(	'n'
		|	'r'
		|	't'
		|	'b'
		|	'f'
		|	'"'
		|	'\''
		|	'\\'
		|	('u')+ HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT 
		|	'0'..'3'
			(
				options {
					warnWhenFollowAmbig = false;
				}
			:	'0'..'7'
				(	
					options {
						warnWhenFollowAmbig = false;
					}
				:	'0'..'7'
				)?
			)?
		|	'4'..'7'
			(
				options {
					warnWhenFollowAmbig = false;
				}
			:	'0'..'7'
			)?
		)
	;

// hexadecimal digit (again, note it's protected!)
protected
HEX_DIGIT
	:	('0'..'9'|'A'..'F'|'a'..'f')
	;

// a dummy rule to force vocabulary to be all characters (except special
//   ones that ANTLR uses internally (0 to 2))
protected
VOCAB
	:	'\3'..'\377'
	;

protected
DIGIT
	:	'0'..'9'
	;

INT
options {
	paraphrase = "integer";
}
	:	(DIGIT)+
	;

// an identifier.  Note that testLiterals is set to true!  This means
// that after we match the rule, we look in the literals table to see
// if it's a literal or really an identifer
ID
options {
	testLiterals = true;
	paraphrase = "identifier";
}
	:	('a'..'z'|'A'..'Z'|'$'|'@') ('a'..'z'|'A'..'Z'|'_'|'0'..'9'|'$')*
	;
