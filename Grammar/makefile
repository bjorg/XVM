all: ..\XLLexer.cs ..\XLParser.cs

..\XLLexer.cs: xl-lex.g
	java antlr.Tool xl-lex.g
	move /Y XLLexer.cs ..\XLLexer.cs
	move /Y XLTokenTypes.cs ..\XLTokenTypes.cs

..\XLParser.cs: xl.g
	java antlr.Tool xl.g
	move /Y XLParser.cs ..\XLParser.cs
	move /Y XLParserTokenTypes.cs ..\XLParserTokenTypes.cs

clean:
	del ..\XLLexer.cs ..\XLTokenTypes.cs ..\XLParser.cs ..\XLParserTokenTypes.cs

rebuild: clean all
