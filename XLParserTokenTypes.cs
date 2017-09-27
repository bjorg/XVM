// $ANTLR 2.7.2: "xl.g" -> "XLParser.cs"$

	using System.Collections;
	using antlr;
	
	using Module = XL.Declaration.Module;
	using Schema = XL.Declaration.Schema;
	using Value = XL.Declaration.Value;
	using Pattern = XL.Declaration.Pattern;
	using Process = XL.Declaration.Process;
	using Action = XL.Declaration.Action;

namespace XL.Parser
{
	public class XLParserTokenTypes
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int KW_PROCESS = 4;
		public const int KW_SCHEMA = 5;
		public const int KW_VOID = 6;
		public const int KW_BOOL = 7;
		public const int KW_INT = 8;
		public const int KW_FLOAT = 9;
		public const int KW_CHAR = 10;
		public const int KW_STRING = 11;
		public const int KW_NEW = 12;
		public const int KW_SELECT = 13;
		public const int KW_FORK = 14;
		public const int KW_IF = 15;
		public const int KW_ELSE = 16;
		public const int KW_SWITCH = 17;
		public const int KW_CASE = 18;
		public const int KW_TRUE = 19;
		public const int KW_FALSE = 20;
		public const int LPAREN = 21;
		public const int RPAREN = 22;
		public const int LCURLY = 23;
		public const int RCURLY = 24;
		public const int LSQUARE = 25;
		public const int RSQUARE = 26;
		public const int STAR = 27;
		public const int PLUS = 28;
		public const int PAR = 29;
		public const int ASSIGN = 30;
		public const int COLON = 31;
		public const int SEMI = 32;
		public const int COMMA = 33;
		public const int DOT = 34;
		public const int FLAT = 35;
		public const int WS = 36;
		public const int SL_COMMENT = 37;
		public const int ML_COMMENT = 38;
		public const int CHAR_LITERAL = 39;
		public const int STRING_LITERAL = 40;
		public const int ESC = 41;
		public const int HEX_DIGIT = 42;
		public const int VOCAB = 43;
		public const int DIGIT = 44;
		public const int INT = 45;
		public const int ID = 46;
		
	}
}
