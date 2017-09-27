using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using Id = System.String;
using Sel = System.Int32;

using XL.Parser;

using Schema = XL.Declaration.Schema;
using Value = XL.Declaration.Value;
using Pattern = XL.Declaration.Pattern;
using Module = XL.Declaration.Module;
using Process = XL.Declaration.Process;
using Action = XL.Declaration.Action;

namespace XL {
    internal class App {

        [STAThread]
        private static void Main(string[] args) {
            Test3(args[0]);
        }

        private static void Test3(string filename) {
            StreamReader reader = File.OpenText(filename);
            Declaration.Module module = null;
            try {
                XLLexer lexer = new XLLexer(reader);
                lexer.setFilename(filename);
                XLParser parser = new XLParser(lexer);
                parser.setFilename(filename);
                module = parser.main();
                if(module == null) {
                    return;
                }
            } catch(Exception e) {
                Console.Error.WriteLine("EXCEPTION: {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
                return;
            } finally {
                reader.Close();
            }

            // defint module
            Util.DeclareGroundPortSchemas(module);

            // define port-types
            Util.ConvertClrType(module, typeof(Console), false);

            // bind module
            module.Bind();

            // generate code
            Compiler.CodeBuilder.Generate(module, "test");
        }

        private static void Test4(string filename) {
            StreamReader reader = File.OpenText(filename);
            try {
                XLLexer lexer = new XLLexer(reader);
                lexer.setFilename(filename);
                XLParser parser = new XLParser(lexer);
                parser.setFilename(filename);
                parser.main();
            } catch(Exception e) {
                Console.Error.WriteLine("EXCEPTION: {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
                return;
            } finally {
                reader.Close();
            }
        }

        private static Declaration.Pattern.Base Void() {
            return Declaration.Pattern.Void.Instance;
        }

        private static Declaration.Schema.Reference Ref(Id name) {
            return new Declaration.Schema.Reference(name);
        }

        private static Declaration.Value.Element Elem(string tag, Declaration.Value.Base value) {
            return new Declaration.Value.Element(tag, value);
        }

        private static Declaration.Value.Ordered Ordered(params Declaration.Value.Base[] values) {
            return new Declaration.Value.Ordered(values);
        }

        private static Declaration.Value.Variable Var(Id name) {
            return new Declaration.Value.Variable(name);
        }

        private static Declaration.Pattern.Discard Discard(Declaration.Schema.Base schema) {
            return new Declaration.Pattern.Discard(schema);
        }

        private static Declaration.Action.Receive Receive(Id name, Declaration.Pattern.Base pattern) {
            return new Declaration.Action.Receive(name, pattern);
        }

        private static Declaration.Action.Let Let(Declaration.Pattern.Base pattern, Declaration.Value.Base value) {
            return new Declaration.Action.Let(pattern, value);
        }

        private static Declaration.Pattern.Variable Def(Declaration.Schema.Base schema, Id name) {
            return new Declaration.Pattern.Variable(schema, name);
        }

        private static Declaration.Value.New New(Declaration.Schema.Base schema) {
            return new Declaration.Value.New(schema);
        }

        private static Declaration.Process.Send Send(Id name, Declaration.Value.Base value) {
            return new Declaration.Process.Send(name, value);
        }

        private static Declaration.Action.Fork Fork(Declaration.Process.Base process) {
            return new Declaration.Action.Fork(process);
        }

        private static Declaration.Process.Sequence Seq(params Declaration.Action.Base[] actions) {
            return new Declaration.Process.Sequence(actions);
        }
    }
}
