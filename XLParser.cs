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
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	
	public 	class XLParser : antlr.LLkParser
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
		
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected XLParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public XLParser(TokenBuffer tokenBuf) : this(tokenBuf,1)
		{
		}
		
		protected XLParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public XLParser(TokenStream lexer) : this(lexer,1)
		{
		}
		
		public XLParser(ParserSharedInputState state) : base(state,1)
		{
			initialize();
		}
		
	public Module  main() //throws RecognitionException, TokenStreamException
{
		Module result = new Module();
		
		Process.Base p;
		
		try {      // for error handling
			{ // ( ... )+
			int _cnt5=0;
			for (;;)
			{
				bool synPredMatched4 = false;
				if (((LA(1)==KW_PROCESS)))
				{
					int _m4 = mark();
					synPredMatched4 = true;
					inputState.guessing++;
					try {
						{
							match(KW_PROCESS);
							name();
						}
					}
					catch (RecognitionException)
					{
						synPredMatched4 = false;
					}
					rewind(_m4);
					inputState.guessing--;
				}
				if ( synPredMatched4 )
				{
					process_definition();
				}
				else if ((LA(1)==KW_PROCESS)) {
					p=process();
					if (0==inputState.guessing)
					{
						result.Processes.Add(p);
					}
				}
				else if ((LA(1)==KW_SCHEMA)) {
					schema_definition();
				}
				else
				{
					if (_cnt5 >= 1) { goto _loop5_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt5++;
			}
_loop5_breakloop:			;
			}    // ( ... )+
			match(Token.EOF_TYPE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_0_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public string  name() //throws RecognitionException, TokenStreamException
{
		string result = null;
		
		Token  n = null;
		
		try {      // for error handling
			n = LT(1);
			match(ID);
			if (0==inputState.guessing)
			{
				result = n.getText();
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_1_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Process.Base  process_definition() //throws RecognitionException, TokenStreamException
{
		Process.Base result = null;
		
		
		try {      // for error handling
			match(KW_PROCESS);
			name();
			match(LPAREN);
			pattern();
			match(RPAREN);
			result=process_statement();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Process.Base  process() //throws RecognitionException, TokenStreamException
{
		Process.Base result = null;
		
		
		try {      // for error handling
			match(KW_PROCESS);
			result=process_statement();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public void schema_definition() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(KW_SCHEMA);
			name();
			match(ASSIGN);
			schema();
			match(SEMI);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public Pattern.Base  pattern() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		
		try {      // for error handling
			result=pattern_unordered();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Process.Base  process_statement() //throws RecognitionException, TokenStreamException
{
		Process.Base result = null;
		
		ArrayList l;
		
		try {      // for error handling
			match(LCURLY);
			l=statement_list();
			match(RCURLY);
			if (0==inputState.guessing)
			{
				result = new Process.Sequence((Action.Base[])l.ToArray(typeof(Action.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_4_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		
		try {      // for error handling
			result=schema_unordered();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public string  tag() //throws RecognitionException, TokenStreamException
{
		string result = null;
		
		Token  t = null;
		
		try {      // for error handling
			t = LT(1);
			match(ID);
			if (0==inputState.guessing)
			{
				result = t.getText();
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_6_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_NEW:
			case KW_TRUE:
			case KW_FALSE:
			case CHAR_LITERAL:
			case STRING_LITERAL:
			case INT:
			case ID:
			{
				result=value_unordered();
				break;
			}
			case RPAREN:
			case RSQUARE:
			case SEMI:
			{
				if (0==inputState.guessing)
				{
					result = Value.Void.Instance;
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_unordered() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		ArrayList l = new ArrayList(); Value.Base i;
		
		try {      // for error handling
			i=value_ordered();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PAR))
					{
						match(PAR);
						i=value_ordered();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop14_breakloop;
					}
					
				}
_loop14_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Value.Base)l[0] : new Value.Unordered((Value.Base[])l.ToArray(typeof(Value.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_ordered() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		ArrayList l = new ArrayList(); Value.Base i;
		
		try {      // for error handling
			i=value_simple();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						i=value_simple();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop17_breakloop;
					}
					
				}
_loop17_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Value.Base)l[0] : new Value.Ordered((Value.Base[])l.ToArray(typeof(Value.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_7_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_simple() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_TRUE:
			case KW_FALSE:
			case CHAR_LITERAL:
			case STRING_LITERAL:
			case INT:
			{
				result=value_literal();
				break;
			}
			case KW_NEW:
			{
				result=value_new();
				break;
			}
			default:
				bool synPredMatched20 = false;
				if (((LA(1)==ID)))
				{
					int _m20 = mark();
					synPredMatched20 = true;
					inputState.guessing++;
					try {
						{
							tag();
							match(LSQUARE);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched20 = false;
					}
					rewind(_m20);
					inputState.guessing--;
				}
				if ( synPredMatched20 )
				{
					result=value_element();
				}
				else if ((LA(1)==ID)) {
					result=value_variable();
				}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			break; }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_literal() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_TRUE:
			case KW_FALSE:
			{
				result=value_boolean();
				break;
			}
			case CHAR_LITERAL:
			{
				result=value_character();
				break;
			}
			case STRING_LITERAL:
			{
				result=value_string();
				break;
			}
			default:
				bool synPredMatched26 = false;
				if (((LA(1)==INT)))
				{
					int _m26 = mark();
					synPredMatched26 = true;
					inputState.guessing++;
					try {
						{
							match(INT);
							match(DOT);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched26 = false;
					}
					rewind(_m26);
					inputState.guessing--;
				}
				if ( synPredMatched26 )
				{
					result=value_floating_point();
				}
				else if ((LA(1)==INT)) {
					result=value_integer();
				}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			break; }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_element() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		string t; Value.Base v;
		
		try {      // for error handling
			t=tag();
			match(LSQUARE);
			v=value();
			match(RSQUARE);
			if (0==inputState.guessing)
			{
				result = new Value.Element(t, v);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_new() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		Schema.Base s;
		
		try {      // for error handling
			match(KW_NEW);
			s=schema_name();
			if (0==inputState.guessing)
			{
				result = new Value.New(s);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_variable() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		string n;
		
		try {      // for error handling
			n=name();
			if (0==inputState.guessing)
			{
				result = new Value.Variable(n);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_name() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_BOOL:
			{
				match(KW_BOOL);
				if (0==inputState.guessing)
				{
					result = Schema.Boolean.Instance;
				}
				break;
			}
			case KW_INT:
			{
				match(KW_INT);
				if (0==inputState.guessing)
				{
					result = Schema.Integer.Instance;
				}
				break;
			}
			case KW_FLOAT:
			{
				match(KW_FLOAT);
				if (0==inputState.guessing)
				{
					result = Schema.FloatingPoint.Instance;
				}
				break;
			}
			case KW_CHAR:
			{
				match(KW_CHAR);
				if (0==inputState.guessing)
				{
					result = Schema.Character.Instance;
				}
				break;
			}
			case KW_STRING:
			{
				match(KW_STRING);
				if (0==inputState.guessing)
				{
					result = Schema.String.Instance;
				}
				break;
			}
			case ID:
			{
				result=schema_ref();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_9_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_boolean() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_TRUE:
			{
				match(KW_TRUE);
				if (0==inputState.guessing)
				{
					result = new Value.Boolean(true);
				}
				break;
			}
			case KW_FALSE:
			{
				match(KW_FALSE);
				if (0==inputState.guessing)
				{
					result = new Value.Boolean(false);
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_floating_point() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			match(INT);
			match(DOT);
			{
				switch ( LA(1) )
				{
				case INT:
				{
					match(INT);
					break;
				}
				case RPAREN:
				case RSQUARE:
				case PAR:
				case SEMI:
				case COMMA:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_integer() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		Token  i = null;
		
		try {      // for error handling
			i = LT(1);
			match(INT);
			if (0==inputState.guessing)
			{
				result = new Value.Integer(Int32.Parse(i.getText()));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_character() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		
		try {      // for error handling
			match(CHAR_LITERAL);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Value.Base  value_string() //throws RecognitionException, TokenStreamException
{
		Value.Base result = null;
		
		Token  s = null;
		
		try {      // for error handling
			s = LT(1);
			match(STRING_LITERAL);
			if (0==inputState.guessing)
			{
				result = new Value.String(XL.Declaration.Util.UnquoteString(s.getText()));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_unordered() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		ArrayList l = new ArrayList(); Pattern.Base i;
		
		try {      // for error handling
			i=pattern_ordered();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PAR))
					{
						match(PAR);
						i=pattern_ordered();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop36_breakloop;
					}
					
				}
_loop36_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Pattern.Base)l[0] : new Pattern.Unordered((Pattern.Base[])l.ToArray(typeof(Pattern.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_ordered() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		ArrayList l = new ArrayList(); Pattern.Base i;
		
		try {      // for error handling
			i=pattern_simple();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						i=pattern_simple();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop39_breakloop;
					}
					
				}
_loop39_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Pattern.Base)l[0] : new Pattern.Ordered((Pattern.Base[])l.ToArray(typeof(Pattern.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_simple() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_VOID:
			{
				result=pattern_void();
				break;
			}
			case LPAREN:
			{
				match(LPAREN);
				result=pattern();
				match(RPAREN);
				break;
			}
			default:
				bool synPredMatched42 = false;
				if (((LA(1)==ID)))
				{
					int _m42 = mark();
					synPredMatched42 = true;
					inputState.guessing++;
					try {
						{
							tag();
							match(LSQUARE);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched42 = false;
					}
					rewind(_m42);
					inputState.guessing--;
				}
				if ( synPredMatched42 )
				{
					result=pattern_element();
				}
				else {
					bool synPredMatched44 = false;
					if (((tokenSet_11_.member(LA(1)))))
					{
						int _m44 = mark();
						synPredMatched44 = true;
						inputState.guessing++;
						try {
							{
								schema_name();
								match(FLAT);
							}
						}
						catch (RecognitionException)
						{
							synPredMatched44 = false;
						}
						rewind(_m44);
						inputState.guessing--;
					}
					if ( synPredMatched44 )
					{
						result=pattern_discard();
					}
					else {
						bool synPredMatched46 = false;
						if (((tokenSet_11_.member(LA(1)))))
						{
							int _m46 = mark();
							synPredMatched46 = true;
							inputState.guessing++;
							try {
								{
									schema_name();
									name();
								}
							}
							catch (RecognitionException)
							{
								synPredMatched46 = false;
							}
							rewind(_m46);
							inputState.guessing--;
						}
						if ( synPredMatched46 )
						{
							result=pattern_variable();
						}
					else
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					}}break; }
				}
				catch (RecognitionException ex)
				{
					if (0 == inputState.guessing)
					{
						reportError(ex);
						consume();
						consumeUntil(tokenSet_12_);
					}
					else
					{
						throw;
					}
				}
				return result;
			}
			
	public Pattern.Base  pattern_void() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		
		try {      // for error handling
			match(KW_VOID);
			if (0==inputState.guessing)
			{
				result = Pattern.Void.Instance;
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_12_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_element() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		string t; Pattern.Base p;
		
		try {      // for error handling
			t=tag();
			match(LSQUARE);
			p=pattern();
			match(RSQUARE);
			if (0==inputState.guessing)
			{
				result = new Pattern.Element(t, p);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_12_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_discard() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		Schema.Base s;
		
		try {      // for error handling
			s=schema_name();
			match(FLAT);
			if (0==inputState.guessing)
			{
				result = new Pattern.Discard(s);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_12_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Pattern.Base  pattern_variable() //throws RecognitionException, TokenStreamException
{
		Pattern.Base result = null;
		
		Schema.Base s; string n;
		
		try {      // for error handling
			s=schema_name();
			n=name();
			if (0==inputState.guessing)
			{
				result = new Pattern.Variable(s, n);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_12_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_unordered() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		ArrayList l = new ArrayList(); Schema.Base i;
		
		try {      // for error handling
			i=schema_choice();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PAR))
					{
						match(PAR);
						i=schema_choice();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop54_breakloop;
					}
					
				}
_loop54_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Schema.Base)l[0] : new Schema.Unordered((Schema.Base[])l.ToArray(typeof(Schema.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_choice() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		ArrayList l = new ArrayList(); Schema.Base i;
		
		try {      // for error handling
			i=schema_repeat();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PLUS))
					{
						match(PLUS);
						i=schema_repeat();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop57_breakloop;
					}
					
				}
_loop57_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Schema.Base)l[0] : new Schema.Choice((Schema.Base[])l.ToArray(typeof(Schema.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_7_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_repeat() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		
		try {      // for error handling
			result=schema_ordered();
			{
				switch ( LA(1) )
				{
				case STAR:
				{
					match(STAR);
					break;
				}
				case RPAREN:
				case RSQUARE:
				case PLUS:
				case PAR:
				case SEMI:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_13_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_ordered() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		ArrayList l = new ArrayList(); Schema.Base i;
		
		try {      // for error handling
			i=schema_simple();
			if (0==inputState.guessing)
			{
				l.Add(i);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						i=schema_simple();
						if (0==inputState.guessing)
						{
							l.Add(i);
						}
					}
					else
					{
						goto _loop62_breakloop;
					}
					
				}
_loop62_breakloop:				;
			}    // ( ... )*
			if (0==inputState.guessing)
			{
				result = (l.Count == 1) ? (Schema.Base)l[0] : new Schema.Ordered((Schema.Base[])l.ToArray(typeof(Schema.Base)));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_14_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_simple() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		
		try {      // for error handling
			bool synPredMatched65 = false;
			if (((LA(1)==ID)))
			{
				int _m65 = mark();
				synPredMatched65 = true;
				inputState.guessing++;
				try {
					{
						tag();
						match(LSQUARE);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched65 = false;
				}
				rewind(_m65);
				inputState.guessing--;
			}
			if ( synPredMatched65 )
			{
				result=schema_element();
			}
			else if ((tokenSet_11_.member(LA(1)))) {
				result=schema_name();
			}
			else if ((LA(1)==LPAREN)) {
				match(LPAREN);
				result=schema();
				match(RPAREN);
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_15_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_element() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		string t; Schema.Base s;
		
		try {      // for error handling
			t=tag();
			match(LSQUARE);
			s=schema();
			match(RSQUARE);
			if (0==inputState.guessing)
			{
				result = new Schema.Element(t, s);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_15_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Schema.Base  schema_ref() //throws RecognitionException, TokenStreamException
{
		Schema.Base result = null;
		
		Token  n = null;
		
		try {      // for error handling
			n = LT(1);
			match(ID);
			if (0==inputState.guessing)
			{
				result = new Schema.Reference(n.getText());
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_9_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public ArrayList  statement_list() //throws RecognitionException, TokenStreamException
{
		ArrayList result = new ArrayList();
		
		Action.Base i;
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_16_.member(LA(1))))
					{
						i=statement();
						if (0==inputState.guessing)
						{
							result.Add(i);
						}
					}
					else
					{
						goto _loop71_breakloop;
					}
					
				}
_loop71_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_17_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_SELECT:
			{
				result=select_statement();
				break;
			}
			case KW_IF:
			{
				result=if_statement();
				break;
			}
			default:
				bool synPredMatched75 = false;
				if (((LA(1)==ID)))
				{
					int _m75 = mark();
					synPredMatched75 = true;
					inputState.guessing++;
					try {
						{
							match(ID);
							match(LPAREN);
							pattern();
							match(RPAREN);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched75 = false;
					}
					rewind(_m75);
					inputState.guessing--;
				}
				if ( synPredMatched75 )
				{
					result=receive_statement();
					match(SEMI);
				}
				else {
					bool synPredMatched77 = false;
					if (((LA(1)==KW_FORK||LA(1)==DOT)))
					{
						int _m77 = mark();
						synPredMatched77 = true;
						inputState.guessing++;
						try {
							{
								fork();
								name();
								match(LPAREN);
								pattern();
								match(RPAREN);
							}
						}
						catch (RecognitionException)
						{
							synPredMatched77 = false;
						}
						rewind(_m77);
						inputState.guessing--;
					}
					if ( synPredMatched77 )
					{
						result=fork_receive_statement();
					}
					else {
						bool synPredMatched79 = false;
						if (((LA(1)==ID)))
						{
							int _m79 = mark();
							synPredMatched79 = true;
							inputState.guessing++;
							try {
								{
									match(ID);
									match(LPAREN);
									value();
									match(RPAREN);
								}
							}
							catch (RecognitionException)
							{
								synPredMatched79 = false;
							}
							rewind(_m79);
							inputState.guessing--;
						}
						if ( synPredMatched79 )
						{
							result=send_or_call_statement();
							match(SEMI);
						}
						else {
							bool synPredMatched81 = false;
							if (((LA(1)==KW_FORK||LA(1)==DOT)))
							{
								int _m81 = mark();
								synPredMatched81 = true;
								inputState.guessing++;
								try {
									{
										fork();
										match(KW_SWITCH);
									}
								}
								catch (RecognitionException)
								{
									synPredMatched81 = false;
								}
								rewind(_m81);
								inputState.guessing--;
							}
							if ( synPredMatched81 )
							{
								result=switch_statement();
							}
							else {
								bool synPredMatched83 = false;
								if (((LA(1)==KW_FORK||LA(1)==DOT)))
								{
									int _m83 = mark();
									synPredMatched83 = true;
									inputState.guessing++;
									try {
										{
											fork();
											match(LCURLY);
										}
									}
									catch (RecognitionException)
									{
										synPredMatched83 = false;
									}
									rewind(_m83);
									inputState.guessing--;
								}
								if ( synPredMatched83 )
								{
									result=fork_statement();
								}
								else if ((LA(1)==KW_FORK||LA(1)==DOT)) {
									result=fork_send_or_call_statement();
									match(SEMI);
								}
								else if ((tokenSet_18_.member(LA(1)))) {
									result=let_statement();
									match(SEMI);
								}
							else
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							}}}}break; }
						}
						catch (RecognitionException ex)
						{
							if (0 == inputState.guessing)
							{
								reportError(ex);
								consume();
								consumeUntil(tokenSet_19_);
							}
							else
							{
								throw;
							}
						}
						return result;
					}
					
	public Action.Base  receive_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		string n; Pattern.Base p;
		
		try {      // for error handling
			n=name();
			match(LPAREN);
			p=pattern();
			match(RPAREN);
			if (0==inputState.guessing)
			{
				result = new Action.Receive(n, p);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_20_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public void fork() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case KW_FORK:
			{
				match(KW_FORK);
				break;
			}
			case DOT:
			{
				match(DOT);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_21_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public Action.Base  fork_receive_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		string n; Pattern.Base p; ArrayList l;
		
		try {      // for error handling
			fork();
			n=name();
			match(LPAREN);
			p=pattern();
			match(RPAREN);
			match(LCURLY);
			l=statement_list();
			match(RCURLY);
			if (0==inputState.guessing)
			{
				l.Insert(0, new Action.Receive(n, p)); result = new Action.Fork(new Process.Sequence((Action.Base[])l.ToArray(typeof(Action.Base))));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  send_or_call_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		
		try {      // for error handling
			name();
			match(LPAREN);
			value();
			match(RPAREN);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_20_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  switch_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		
		try {      // for error handling
			fork();
			match(KW_SWITCH);
			match(LPAREN);
			value();
			match(RPAREN);
			match(LCURLY);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==KW_CASE))
					{
						switch_case();
					}
					else
					{
						goto _loop95_breakloop;
					}
					
				}
_loop95_breakloop:				;
			}    // ( ... )*
			match(RCURLY);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  fork_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		Process.Base p;
		
		try {      // for error handling
			fork();
			p=process_statement();
			if (0==inputState.guessing)
			{
				result = new Action.Fork(p);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  fork_send_or_call_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		string n; Value.Base v;
		
		try {      // for error handling
			fork();
			n=name();
			match(LPAREN);
			v=value();
			match(RPAREN);
			if (0==inputState.guessing)
			{
				result = new Action.Fork(new Process.Send(n, v));
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_20_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  select_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		
		try {      // for error handling
			match(KW_SELECT);
			match(LCURLY);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==KW_FORK||LA(1)==DOT))
					{
						fork_receive_statement();
					}
					else
					{
						goto _loop90_breakloop;
					}
					
				}
_loop90_breakloop:				;
			}    // ( ... )*
			match(RCURLY);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  if_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		
		try {      // for error handling
			match(KW_IF);
			match(LPAREN);
			value();
			match(RPAREN);
			fork();
			process_statement();
			match(KW_ELSE);
			fork();
			process_statement();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Base  let_statement() //throws RecognitionException, TokenStreamException
{
		Action.Base result = null;
		
		Pattern.Base p; Value.Base v;
		
		try {      // for error handling
			p=pattern();
			match(ASSIGN);
			v=value();
			if (0==inputState.guessing)
			{
				result = new Action.Let(p, v);
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_20_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	public Action.Switch.Case  switch_case() //throws RecognitionException, TokenStreamException
{
		Action.Switch.Case result = null;
		
		
		try {      // for error handling
			match(KW_CASE);
			pattern();
			match(COLON);
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_16_.member(LA(1))))
					{
						statement();
					}
					else
					{
						goto _loop98_breakloop;
					}
					
				}
_loop98_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_22_);
			}
			else
			{
				throw;
			}
		}
		return result;
	}
	
	private void initializeFactory()
	{
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""process""",
		@"""schema""",
		@"""void""",
		@"""bool""",
		@"""int""",
		@"""float""",
		@"""char""",
		@"""string""",
		@"""new""",
		@"""select""",
		@"""fork""",
		@"""if""",
		@"""else""",
		@"""switch""",
		@"""case""",
		@"""true""",
		@"""false""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""LCURLY""",
		@"""RCURLY""",
		@"""LSQUARE""",
		@"""RSQUARE""",
		@"""STAR""",
		@"""PLUS""",
		@"""PAR""",
		@"""ASSIGN""",
		@"""COLON""",
		@"""SEMI""",
		@"""COMMA""",
		@"""DOT""",
		@"""FLAT""",
		@"""WS""",
		@"""comment""",
		@"""ML_COMMENT""",
		@"""char""",
		@"""string""",
		@"""ESC""",
		@"""HEX_DIGIT""",
		@"""VOCAB""",
		@"""DIGIT""",
		@"""integer""",
		@"""identifier"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 16716398592L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 50L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 3292528640L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 70385943310322L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 4366270464L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 33554432L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 4903141376L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 13493075968L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = { 70416999645184L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = { 3829399552L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	private static long[] mk_tokenSet_11_()
	{
		long[] data = { 70368744181632L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_11_ = new BitSet(mk_tokenSet_11_());
	private static long[] mk_tokenSet_12_()
	{
		long[] data = { 12419334144L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_12_ = new BitSet(mk_tokenSet_12_());
	private static long[] mk_tokenSet_13_()
	{
		long[] data = { 5171576832L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_13_ = new BitSet(mk_tokenSet_13_());
	private static long[] mk_tokenSet_14_()
	{
		long[] data = { 5305794560L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_14_ = new BitSet(mk_tokenSet_14_());
	private static long[] mk_tokenSet_15_()
	{
		long[] data = { 13895729152L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_15_ = new BitSet(mk_tokenSet_15_());
	private static long[] mk_tokenSet_16_()
	{
		long[] data = { 70385926205376L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_16_ = new BitSet(mk_tokenSet_16_());
	private static long[] mk_tokenSet_17_()
	{
		long[] data = { 16777216L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_17_ = new BitSet(mk_tokenSet_17_());
	private static long[] mk_tokenSet_18_()
	{
		long[] data = { 70368746278848L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_18_ = new BitSet(mk_tokenSet_18_());
	private static long[] mk_tokenSet_19_()
	{
		long[] data = { 70385943244736L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_19_ = new BitSet(mk_tokenSet_19_());
	private static long[] mk_tokenSet_20_()
	{
		long[] data = { 4294967296L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_20_ = new BitSet(mk_tokenSet_20_());
	private static long[] mk_tokenSet_21_()
	{
		long[] data = { 70368752697344L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_21_ = new BitSet(mk_tokenSet_21_());
	private static long[] mk_tokenSet_22_()
	{
		long[] data = { 17039360L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_22_ = new BitSet(mk_tokenSet_22_());
	
}
}
