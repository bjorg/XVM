using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using Id = System.String;
using Sel = System.Int32;
using Pair = System.Collections.DictionaryEntry;
using Schema = XL.Declaration.Schema;
using Value = XL.Declaration.Value;
using Pattern = XL.Declaration.Pattern;
using Module = XL.Declaration.Module;
using Process = XL.Declaration.Process;
using Action = XL.Declaration.Action;

namespace XL.Compiler {
    public sealed class CodeBuilder {

        //--- Constants ---
        private const string UNTYPED_CONTEXT = "_context";
        private const string CONTEXT = "_ctxt";
        private const string UNTYPED_MESSAGE = "_message";
        private const string MESSAGE = "_msg";
        private const string FIELD = "_field";
        private const string CHANNEL = "_channel";
        private const string CHANNEL_RECEIVE_METHOD = "Receive";
        private const string CHANNEL_SEND_METHOD = "Send";
        private const string SCHEDULER_FORK_METHOD = "Fork";
        private const string DELEGATE = "_delegate";
        private const string METHOD = "_action";
        private const string TYPE = "_type";
        private const string OUT_MESSAGE = "_out";
        private const string VAR = "_var";
        private const string PORT_TYPE = "_port_type";
        private const string PORT_CREATE_METHOD = "Create";
        private const string CLR_METHOD_PREFIX = "_clr_method";
        private const string PORT = "_port";
        private const string ARG = "_arg";
        private const string TARGET_PROPERTY = "Target";
        private const string REPLY = "_reply";
        private const string RETURN = "_return";
        private const string CLR_PORT_TYPE = "_clr_port_type";
        private const string GENERIC_PORT_TYPE = "_port_type";
        private const string CHANNELS = "_channels";
        private const string INSTANCE = "_instance";

        //--- Types ---
        private class Code {

            //--- Fields ---
            internal Declaration.Module Module;
            internal CodeCompileUnit Unit;
            internal CodeNamespace Namespace;
            internal CodeTypeDeclaration Main;
            internal CodeTypeReference ContextRef;
            private CodeMemberMethod method;
            private CodeMethodReferenceExpression method_ref;
            internal Hashtable /*<Declaration.Context,CodeTypeReference>*/ ContextMap;
            internal Hashtable /*<Schema.Base,CodeTypeReference>*/ SchemaMap;
            internal Hashtable /*<Schema.Port,CodeExpression>*/ PortTypeMap;
            internal Hashtable /*<Type,CodeExpression>*/ ClrInstanceFactoryMap;
            private Hashtable /*<string,int>*/ names;

            //--- Constructors ---
            internal Code(Declaration.Module module, string namespace_name) {
                this.Module = module;
                this.Unit = new CodeCompileUnit();

                // create namespace
                Namespace = new CodeNamespace(namespace_name);
                Unit.Namespaces.Add(Namespace);

                // create container class
                Main = new CodeTypeDeclaration("_main");
                Namespace.Types.Add(Main);

                // initialize containers
                this.ContextMap = new Hashtable();
                this.SchemaMap = new Hashtable();
                this.PortTypeMap = new Hashtable();
                this.ClrInstanceFactoryMap = new Hashtable();
                this.names = new Hashtable();
            }

            internal Code(Code code) {
                this.Module = code.Module;
                this.Unit = code.Unit;
                this.Namespace = code.Namespace;
                this.Main = code.Main;
                this.ContextRef = code.ContextRef;
                this.method = code.method;
                this.method_ref = code.method_ref;
                this.ContextMap = code.ContextMap;
                this.SchemaMap = code.SchemaMap;
                this.PortTypeMap = code.PortTypeMap;
                this.ClrInstanceFactoryMap = code.ClrInstanceFactoryMap;
                this.names = code.names;
            }

            //--- Properties ---
            internal CodeTypeReference MainRef {
                get {
                    return new CodeTypeReference(Main.Name);
                }
            }

            internal CodeMemberMethod Method {
                get {
                    return method;
                }
                set {
                    method = value;
                    Debug.Assert(method != null);
                    method_ref = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(MainRef), method.Name);
                }
            }

            internal CodeMethodReferenceExpression MethodRef {
                get {
                    return method_ref;
                }
            }

            //--- Methods ---
            internal void NewMethod() {
                CodeMemberMethod method = new CodeMemberMethod();
                method.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
                method.Name = NewName(METHOD);
                Method = method;
                Main.Members.Add(method);
            }

            internal CodeTypeDeclaration NewType(string name, string comment) {
                CodeTypeDeclaration type = new CodeTypeDeclaration(NewName(name));
                type.Attributes = MemberAttributes.Assembly;
                Namespace.Types.Add(type);
                if((comment != null) && (comment != "")) {
                    type.Comments.Add(new CodeCommentStatement(comment));
                }
                return type;
            }

            internal CodeExpression NewVariable(CodeTypeReference type, CodeExpression expr) {
                string var = NewName(VAR);
                Method.Statements.Add(new CodeVariableDeclarationStatement(type, var, expr));
                return new CodeVariableReferenceExpression(var);
            }

            internal CodeExpression NewGlobalVariable(CodeTypeReference type, CodeExpression expr) {
                string global_name = NewName(PORT_TYPE);
                CodeMemberField global_member = new CodeMemberField(type, global_name);
                global_member.InitExpression = expr;
                global_member.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
                Main.Members.Add(global_member);
                return MainFieldRef(global_name);
            }

            internal CodeExpression MainFieldRef(string field_name) {
                return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(MainRef), field_name);
            }

            internal string NewName(string name) {
                object count = names[name];
                if(count == null) {
                    names.Add(name, 1);
                    return name + 0;
                } else {
                    int i = (int)count;
                    names[name] = i + 1;
                    return name + i;
                }
            }
        }

        //--- Class Fields ---
        private static readonly CodeExpression ContextExpr = new CodeVariableReferenceExpression(CONTEXT);
        private static readonly CodeExpression UntypedContextExpr = new CodeVariableReferenceExpression(UNTYPED_CONTEXT);
        private static readonly CodeExpression MessageExpr = new CodeVariableReferenceExpression(MESSAGE);
        private static readonly CodeExpression UntypedMessageExpr = new CodeVariableReferenceExpression(UNTYPED_MESSAGE);
        private static readonly CodeExpression SchedulerExpr = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(Runtime.Global)), "Scheduler");
        private static readonly CodeTypeReference ReceiverDelegateRef = new CodeTypeReference(typeof(Runtime.ReceiverHandler));
        private static readonly CodeTypeReference ProcessDelegateRef = new CodeTypeReference(typeof(Runtime.ProcessHandler));

        //--- Class Methods ---
        public static void Generate(Declaration.Module module, string namespace_name) {
            Code current = new Code(module, namespace_name);
            CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            ICodeGenerator generator = provider.CreateGenerator();

            // generate code for types
            foreach(Schema.Base schema in module.Schemas.Values) {
                GenerateSchemaType(current, schema.Deref);
            }

            // generate contexts for processes
            foreach(Declaration.Context context in module.Contexts.Values) {
                GenerateContextType(current, context);
            }

            // generate code for processes
            foreach(Process.Base process in current.Module.Processes.Values) {
                GenerateProcess(current, process);
            }

            // output generated code
            generator.GenerateCodeFromCompileUnit(current.Unit, Console.Out, new CodeGeneratorOptions());
        }

        #region Processes
        private static void GenerateProcess(Code current, Process.Base process) {
            current.ContextRef = (CodeTypeReference)current.ContextMap[process.Context];
            if(process is Process.Sequence) {
                _GenerateProcess(current, (Process.Sequence)process);
            } else if(process is Process.Send) {
                _GenerateProcess(current, (Process.Send)process);
            } else {
                throw new NotSupportedException("unexpected");
            }
        }

        private static void _GenerateProcess(Code current, Process.Sequence process) {
            Code next = null;
            for(int i = process.Actions.Length-1; i >= 0; --i) {
                current = new Code(current);
                GenerateAction(current, process.Actions[i], next);
                next = current;
            }
        }

        private static void _GenerateProcess(Code current, Process.Send process) {
            _GenerateAction(current, new Action.Let(new Pattern.Local(process.Value.Schema, OUT_MESSAGE), process.Value), null);
            current.Method.Statements.Add(new CodeCommentStatement("send"));
            current.Method.Statements.Add(new CodeMethodInvokeExpression(FieldAt(current, process.PortIndex), CHANNEL_SEND_METHOD, new CodePrimitiveExpression(process.Selector), new CodeVariableReferenceExpression(OUT_MESSAGE)));
        }
        #endregion

        #region Actions
        private static void GenerateAction(Code current, Action.Base action, Code next) {
            if(action is Action.Fork) {
                _GenerateAction(current, (Action.Fork)action, next);
            } else if(action is Action.If) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(action is Action.Let) {
                _GenerateAction(current, (Action.Let)action, next);
            } else if(action is Action.Receive) {
                _GenerateAction(current, (Action.Receive)action, next);
            } else if(action is Action.Select) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(action is Action.Switch) {

                // TODO
                throw new NotImplementedException("missing code");
            } else {
                throw new NotSupportedException("unexpected");
            }
        }

        private static void _GenerateAction(Code current, Action.Receive action, Code next) {

            // create receiver method
            GenerateReceiverMethod(current, action.Pattern, next);
            CodeMethodReferenceExpression receiver = current.MethodRef;

            // create receiver delegate
            string delegate_name = DELEGATE + receiver.MethodName;
            CodeMemberField delegate_member = new CodeMemberField(ReceiverDelegateRef, delegate_name);
            delegate_member.InitExpression = new CodeDelegateCreateExpression(ReceiverDelegateRef, new CodeTypeReferenceExpression(current.MainRef), receiver.MethodName);
            delegate_member.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
            current.Main.Members.Add(delegate_member);

            // create action method
            GenerateActionMethod(current);
            current.Method.Statements.Add(new CodeCommentStatement("receive"));
            current.Method.Statements.Add(new CodeMethodInvokeExpression(FieldAt(current, action.PortIndex), CHANNEL_RECEIVE_METHOD, new CodePrimitiveExpression(action.Selector), ContextExpr, current.MainFieldRef(delegate_name)));
        }

        private static void _GenerateAction(Code current, Action.Fork action, Code next) {

            // create forked method
            Code fork_code = new Code(current);
            GenerateProcess(fork_code, action.Process);
            CodeMethodReferenceExpression fork = fork_code.MethodRef;

            // create fork delegate
            string delegate_name = DELEGATE + fork.MethodName;
            CodeMemberField delegate_member = new CodeMemberField(ProcessDelegateRef, delegate_name);
            delegate_member.InitExpression = new CodeDelegateCreateExpression(ProcessDelegateRef, new CodeTypeReferenceExpression(current.MainRef), fork.MethodName);
            delegate_member.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
            current.Main.Members.Add(delegate_member);

            // create action method
            GenerateActionMethod(current);
            current.Method.Statements.Add(new CodeCommentStatement("fork"));

            // TODO: create new context
            current.Method.Statements.Add(new CodeMethodInvokeExpression(SchedulerExpr, SCHEDULER_FORK_METHOD, new CodeObjectCreateExpression(fork_code.ContextRef, new CodeVariableReferenceExpression(CONTEXT)), current.MainFieldRef(delegate_name)));
            AddCallNext(current, next);
        }

        private static void _GenerateAction(Code current, Action.Let action, Code next) {
            GenerateActionMethod(current);
            current.Method.Statements.Add(new CodeCommentStatement("let"));
            CodeExpression var = current.NewVariable(GenerateSchemaType(new Code(current), action.Pattern.Schema), GenerateValue(new Code(current), action.Value));
            GeneratePattern(current, action.Pattern, var);
            AddCallNext(current, next);
        }

        private static void GenerateActionMethod(Code current) {
            current.NewMethod();
            current.Method.ReturnType = new CodeTypeReference(typeof(void));
            current.Method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), UNTYPED_CONTEXT));
            current.Method.Statements.Add(new CodeVariableDeclarationStatement(
                current.ContextRef, CONTEXT, new CodeCastExpression( current.ContextRef, UntypedContextExpr)
            ));
        }

        private static void GenerateReceiverMethod(Code current, Pattern.Base pattern, Code next) {
            current.NewMethod();
            current.Method.ReturnType = new CodeTypeReference(typeof(void));
            current.Method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), UNTYPED_CONTEXT));
            current.Method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), UNTYPED_MESSAGE));
            current.Method.Statements.Add(new CodeVariableDeclarationStatement(
                current.ContextRef, CONTEXT, new CodeCastExpression(current.ContextRef, UntypedContextExpr)
            ));

            // assign _msg to pattern
            if(!(pattern is Pattern.Void)) {
                CodeTypeReference pattern_type = GenerateSchemaType(new Code(current), pattern.Schema);
                current.Method.Statements.Add(new CodeVariableDeclarationStatement(
                    pattern_type, MESSAGE, new CodeCastExpression(pattern_type, UntypedMessageExpr)
                ));
                GeneratePattern(current, pattern, new CodeVariableReferenceExpression(MESSAGE));
            }

            // call continuation
            AddCallNext(current, next);
        }
        #endregion

        #region Helpers
        private static CodeExpression FieldAt(Code current, int[] index) {
            CodeExpression result = new CodeFieldReferenceExpression(ContextExpr, FIELD + index[0]);
            if(index.Length == 2) {
                result = new CodeFieldReferenceExpression(result, FIELD + index[1]);
            }
            return result;
        }

        private static CodeExpression ChannelAt(Code current, CodeExpression port, Sel selector) {
            return new CodeArrayIndexerExpression(port, new CodePrimitiveExpression(selector));
        }

        private static void AddCallNext(Code current, Code next) {
            if(next != null) {
                current.Method.Statements.Add(new CodeMethodInvokeExpression(next.MethodRef, ContextExpr));
            }
        }

        private static bool IsGroundType(Type type) {
            if(type == typeof(void)) {
                return true;
            } else if(type == typeof(bool)) {
                return true;
            } else if(type == typeof(int)) {
                return true;
            } else if(type == typeof(double)) {
                return true;
            } else if(type == typeof(char)) {
                return true;
            } else if(type == typeof(string)) {
                return true;
            }
            return false;
        }

        public static string TypeName(Type type, bool instance) {
            string result = type.FullName.Replace(".", "_").Replace("[]", "Array");
            return instance ? result : result + "_class";
        }

        public static string MethodName(MethodInfo method) {
            return TypeName(method.ReflectedType, !method.IsStatic) + "_" + method.Name;
        }
        #endregion

        #region Values & Patterns
        private static CodeExpression GenerateValue(Code current, Value.Base value) {
            if(value is Value.Boolean) {
                return new CodePrimitiveExpression(((Value.Boolean)value).Value);
            } else if(value is Value.Character) {
                return new CodePrimitiveExpression(((Value.Character)value).Value);
            } else if(value is Value.Element) {
                return GenerateValue(current, ((Value.Element)value).Value);
            } else if(value is Value.FloatingPoint) {
                return new CodePrimitiveExpression(((Value.FloatingPoint)value).Value);
            } else if(value is Value.Integer) {
                return new CodePrimitiveExpression(((Value.Integer)value).Value);
            } else if(value is Value.Local) {
                return new CodeVariableReferenceExpression(((Value.Local)value).Name);
            } else if(value is Value.New) {
                Schema.Base schema = ((Value.New)value).Schema.Deref;
                Debug.Assert(schema is Schema.Port);
                CodeExpression port_type = (CodeExpression)current.PortTypeMap[schema];
                Debug.Assert(port_type != null);
                return new CodeMethodInvokeExpression(port_type, PORT_CREATE_METHOD);
            } else if(value is Value.Ordered) {
                CodeTypeReference type = GenerateSchemaType(new Code(current), value.Schema);
                CodeExpression var = current.NewVariable(type, new CodeObjectCreateExpression(type));
                Value.Ordered ordered = (Value.Ordered)value;
                for(int i = 0; i < ordered.Count; ++i) {
                    current.Method.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(var, FIELD + i), GenerateValue(current, ordered[i])));
                }
                return var;
            } else if(value is Value.String) {
                return new CodePrimitiveExpression(((Value.String)value).Value);
            } else if(value is Value.Unordered) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(value is Value.Variable) {
                Value.Variable var = (Value.Variable)value;
                return FieldAt(current, var.Index);
            } else if(value is Value.Void) {
                throw new NotSupportedException("void value");
            } else {
                throw new NotSupportedException("unexpected");
            }
        }

        private static void GeneratePattern(Code current, Pattern.Base pattern, CodeExpression value) {
            if(pattern is Pattern.Discard) {

                // nothing to do
            } else if(pattern is Pattern.Element) {
                Pattern.Element p = (Pattern.Element)pattern;
                GeneratePattern(current, p.Pattern, value);
            } else if(pattern is Pattern.Local) {
                Pattern.Local p = (Pattern.Local)pattern;
                current.Method.Statements.Add(new CodeVariableDeclarationStatement(GenerateSchemaType(new Code(current), p.Schema), p.Name, value));
            } else if(pattern is Pattern.Ordered) {
                Pattern.Ordered p = (Pattern.Ordered)pattern;
                for(int i = 0; i < p.Count; ++i) {
                    GeneratePattern(current, p[i], new CodeFieldReferenceExpression(value, FIELD + i));
                }
            } else if(pattern is Pattern.Unordered) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(pattern is Pattern.Variable) {
                Pattern.Variable p = (Pattern.Variable)pattern;
                current.Method.Statements.Add(new CodeAssignStatement(FieldAt(current, p.Index), value));
            } else if(pattern is Pattern.Void) {

                // nothing to do
            } else {
                throw new NotSupportedException("unexpected");
            }
        }
        #endregion

        #region Type Generators
        private static CodeTypeReference GenerateSchemaType(Code current, Schema.Base schema) {
            CodeTypeReference result = (CodeTypeReference)current.SchemaMap[schema];
            if(result != null) {
                return result;
            }

            // check if map already contains a compatible schema
            foreach(Pair entry in current.SchemaMap) {
                Schema.Base key = (Schema.Base)entry.Key;
                if(key.Matches(schema)) {
                    return (CodeTypeReference)entry.Value;
                }
            }

            // generate type
            if(schema is Schema.Boolean) {

                // boolean type
                result = new CodeTypeReference(typeof(bool));
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.Character) {

                // char type
                result = new CodeTypeReference(typeof(char));
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.Choice) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(schema is Schema.Element) {
                return GenerateSchemaType(current, ((Schema.Element)schema).Schema);
            } else if(schema is Schema.FloatingPoint) {

                // double type
                result = new CodeTypeReference(typeof(double));
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.Integer) {

                // int type
                result = new CodeTypeReference(typeof(int));
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.Ordered) {

                // ordered anonymous type
                CodeTypeDeclaration type = current.NewType(TYPE, null);
                result = new CodeTypeReference(type.Name);
                current.SchemaMap.Add(schema, result);
                Schema.Ordered ordered = (Schema.Ordered)schema;
                for(int i = 0; i < ordered.Count; ++i) {
                    CodeMemberField field = new CodeMemberField(GenerateSchemaType(current, ordered[i]), FIELD + i);
                    field.Attributes = MemberAttributes.Assembly;
                    type.Members.Add(field);
                }
            } else if(schema is Schema.Port) {

                // port type
                result = new CodeTypeReference(((Schema.Port)schema).InstanceType);
                current.SchemaMap.Add(schema, result);

                // create port-type factory
                if(schema is Schema.GenericPort) {
                    GenerateGenericPortFactory(current, (Schema.GenericPort)schema);
                } else if(schema is Schema.ClrPort) {
                    GenerateClrPortFactory(current, (Schema.ClrPort)schema);
                } else {
                    throw new NotSupportedException("unexpected");
                }
            } else if(schema is Schema.Reference) {

                // named type
                result = GenerateSchemaType(current, schema.Deref);
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.String) {

                // string type
                result = new CodeTypeReference(typeof(string));
                current.SchemaMap.Add(schema, result);
            } else if(schema is Schema.Unordered) {

                // TODO
                throw new NotImplementedException("missing code");
            } else if(schema is Schema.Void) {

                // void type
                result = new CodeTypeReference(typeof(void));
                current.SchemaMap.Add(schema, result);
            } else {
                throw new NotSupportedException("unexpected");
            }
            return result;
        }

        private static void GenerateGenericPortFactory(Code current, Schema.GenericPort schema) {

            // create custom port-type
            CodeTypeDeclaration type = current.NewType(GENERIC_PORT_TYPE, "generic port-type");
            current.PortTypeMap.Add(schema, new CodeTypeReferenceExpression(type.Name));

            // create 'Create' method
            current.Method = new CodeMemberMethod();
            type.Members.Add(current.Method);
            current.Method.Name = PORT_CREATE_METHOD;
            current.Method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            current.Method.ReturnType = new CodeTypeReference(typeof(Runtime.GenericPort));
            int count = schema.Channels.Length;
            current.Method.Statements.Add(new CodeVariableDeclarationStatement(typeof(Runtime.Channel[]), CHANNELS, new CodeArrayCreateExpression(typeof(Runtime.Channel), count)));
            for(int i = 0; i < count; ++i) {
                current.Method.Statements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression(CHANNELS), new CodePrimitiveExpression(i)), new CodeObjectCreateExpression(typeof(Runtime.GenericChannel))));
            }
            current.Method.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(Runtime.GenericPort), new CodeVariableReferenceExpression(CHANNELS))));
        }

        private static void GenerateClrPortFactory(Code current, Schema.ClrPort schema) {
            bool instance = schema is Schema.ClrInstancePort;

            // create custom port-type
            CodeTypeDeclaration type = current.NewType(TypeName(schema.ClrType, instance), null);
            CodeTypeReferenceExpression entry = new CodeTypeReferenceExpression(type.Name);
            current.PortTypeMap.Add(schema, entry);
            if(instance) {
                current.ClrInstanceFactoryMap.Add(schema.ClrType, entry);
            }

            // create fields
            CodeMemberField member = new CodeMemberField(new CodeTypeReference(typeof(Runtime.Channel[])), CHANNELS);
            member.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            type.Members.Add(member);

            // create method wrappers for each channel
            CodeTypeReference[] channels = new CodeTypeReference[schema.Methods.Length];
            for(int i = 0; i < schema.Methods.Length; ++i) {
                channels[i] = CreateProxyMethod(new Code(current), schema.ClrType, schema.Channels[i], schema.Methods[i]);
            }

            // create constructor
            current.Method = new CodeTypeConstructor();
            type.Members.Add(current.Method);
            CodeExpression field = new CodeFieldReferenceExpression(entry, CHANNELS);
            current.Method.Statements.Add(new CodeAssignStatement(field, new CodeArrayCreateExpression(typeof(Runtime.Channel), schema.Methods.Length)));
            for(int i = 0; i < channels.Length; ++i) {
                current.Method.Statements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(field, new CodePrimitiveExpression(i)), new CodeObjectCreateExpression(channels[i])));
            }

            // create 'Create' method
            current.Method = new CodeMemberMethod();
            type.Members.Add(current.Method);
            current.Method.Name = PORT_CREATE_METHOD;
            current.Method.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            CodeExpression create;
            if(instance) {
                current.Method.ReturnType = new CodeTypeReference(typeof(Runtime.ClrInstancePort));
                current.Method.Parameters.Add(new CodeParameterDeclarationExpression(schema.ClrType, INSTANCE));
                create = new CodeObjectCreateExpression(typeof(Runtime.ClrInstancePort), new CodeFieldReferenceExpression(entry, CHANNELS), new CodeVariableReferenceExpression(INSTANCE));
            } else {
                current.Method.ReturnType = new CodeTypeReference(typeof(Runtime.ClrClassPort));
                create = new CodeObjectCreateExpression(typeof(Runtime.ClrClassPort), new CodeFieldReferenceExpression(entry, CHANNELS));
            }
            current.Method.Statements.Add(new CodeMethodReturnStatement(create));
        }

        private static CodeTypeReference CreateProxyMethod(Code current, Type base_type, Schema.Base schema, MethodInfo method) {
            CodeTypeDeclaration type = current.NewType(MethodName(method), null);
            type.BaseTypes.Add(typeof(Runtime.ClrMethodChannel));

            // create 'Send' method
            current.Method = new CodeMemberMethod();
            type.Members.Add(current.Method);
            current.Method.Name = CHANNEL_SEND_METHOD;
            current.Method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            current.Method.ReturnType = new CodeTypeReference(typeof(void));
            current.Method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Runtime.Port), PORT));
            current.Method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), UNTYPED_MESSAGE));
            CodeTypeReference schema_type = GenerateSchemaType(new Code(current), schema);
            current.Method.Statements.Add(new CodeVariableDeclarationStatement(
                schema_type, MESSAGE, new CodeCastExpression(schema_type, new CodeVariableReferenceExpression(UNTYPED_MESSAGE))
            ));
            Pattern.Base pattern = Util.CreateMethodPattern(current.Module, method);
            GeneratePattern(current, pattern, new CodeVariableReferenceExpression(MESSAGE));
            Pattern.Element element = (Pattern.Element)pattern;
            CodeExpression[] args;
            if(element.Pattern is Pattern.Ordered) {
                Pattern.Ordered actuals = (Pattern.Ordered)element.Pattern;
                args = new CodeExpression[actuals.Count-1];
                for(int i = 0; i < args.Length; ++i) {
                    if(actuals[i].Schema.Deref is Schema.ClrInstancePort) {
                        Schema.ClrInstancePort clr = (Schema.ClrInstancePort)actuals[i].Schema.Deref;
                        CodeExpression expr = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(ARG + i), TARGET_PROPERTY);
                        if(clr.ClrType != typeof(object)) {
                            expr = new CodeCastExpression(clr.ClrType , expr);
                        }
                        args[i] = expr;
                    } else {
                        args[i] = new CodeVariableReferenceExpression(ARG + i);
                    }
                }
            } else {
                args = new CodeExpression[0];
            }

            // check if we are invoking a class or instance method
            CodeExpression target;
            if(method.IsStatic) {
                target = new CodeTypeReferenceExpression(base_type);
            } else {
                target = new CodeVariableReferenceExpression(PORT);
                target = new CodeCastExpression(typeof(Runtime.ClrInstancePort), target);
                target = new CodePropertyReferenceExpression(target, TARGET_PROPERTY);
                target = new CodeCastExpression(base_type, target);
            }

            // invoke method and capture return value
            CodeExpression port = new CodeVariableReferenceExpression(REPLY);

            // TODO: we have to look-up the selector in the port-type
            Sel selector = 0;

            // generate call
            CodeExpression call;
            if(method.IsSpecialName) {
                object[] attr = method.GetCustomAttributes(false);
                if(method.Name.StartsWith("get_")) {

                    // check if this an indexed property
                    if(args.Length > 0) {
                        call = new CodeArrayIndexerExpression(target, args);
                    } else {
                        call = new CodePropertyReferenceExpression(target, method.Name.Substring(4));
                    }
                } else if(method.Name.StartsWith("set_")) {
                    call = null;

                    // check if this an indexed property
                    if(args.Length > 1) {
                        CodeExpression[] indices = new CodeExpression[args.Length-1];
                        Array.Copy(args, 0, indices, 0, indices.Length);
                        current.Method.Statements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(target, indices), args[0]));
                    } else {
                        current.Method.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(target, method.Name.Substring(4)), args[0]));
                    }
                } else if(method.Name.StartsWith("add_")) {
                    Debug.Assert(args.Length == 1);
                    call = null;
                    current.Method.Statements.Add(new CodeAttachEventStatement(target, method.Name.Substring(4), args[0]));
                } else if(method.Name.StartsWith("remove_")) {
                    Debug.Assert(args.Length == 1);
                    call = null;
                    current.Method.Statements.Add(new CodeRemoveEventStatement(target, method.Name.Substring(7), args[0]));
                } else {

                    // TODO: operators
                    throw new NotImplementedException("missing code");
                }
            } else {
                call = new CodeMethodInvokeExpression(target, method.Name, args);
            }

            // caputre return-value
            CodeExpression return_value;
            if(method.ReturnType == typeof(void)) {
                if(call != null) {
                    current.Method.Statements.Add(call);
                }
                return_value = new CodePrimitiveExpression(null);
            } else {
                Debug.Assert(call != null);
                current.Method.Statements.Add(new CodeVariableDeclarationStatement(method.ReturnType, RETURN, call));
                if(IsGroundType(method.ReturnType)) {
                    return_value = new CodeVariableReferenceExpression(RETURN);
                } else {
                    GenerateSchemaType(new Code(current), Util.ConvertClrType(current.Module, method.ReturnType, true));

                    CodeExpression clr_factory = (CodeExpression)current.ClrInstanceFactoryMap[method.ReturnType];
                    Debug.Assert(clr_factory != null);
                    return_value = new CodeMethodInvokeExpression(clr_factory, PORT_CREATE_METHOD, new CodeVariableReferenceExpression(RETURN));
                }
            }

            // add reply statement
            current.Method.Statements.Add(new CodeMethodInvokeExpression(port, Compiler.CodeBuilder.CHANNEL_SEND_METHOD, new CodePrimitiveExpression(selector), return_value));
            return new CodeTypeReference(type.Name);
        }

        private static CodeTypeReference GenerateContextType(Code current, Declaration.Context context) {
            CodeTypeReference result = (CodeTypeReference)current.ContextMap[context];
            if(result != null) {
                return result;
            }

            // create new type
            CodeTypeDeclaration type = current.NewType(TYPE, null);
            result = new CodeTypeReference(type.Name);
            current.ContextMap.Add(context, result);

            // define parent fields
            int i;
            for(i = 0; i < context.ParentCount; ++i) {
                CodeMemberField member = new CodeMemberField(GenerateContextType(new Code(current), context.ParentAt(i)), FIELD + i);
                member.Attributes = MemberAttributes.Assembly;
                type.Members.Add(member);
            }

            // define variable fields
            foreach(Pair var in context.Variables) {
                Schema.Base schema = (Schema.Base)var.Value;
                CodeMemberField member = new CodeMemberField(GenerateSchemaType(new Code(current), schema), FIELD + i);
                member.Attributes = MemberAttributes.Assembly;
                type.Members.Add(member);
                ++i;
            }

            // create constructor
            if(context.ParentCount > 0) {
                CodeConstructor constructor = new CodeConstructor();
                constructor.Attributes = MemberAttributes.Assembly;
                constructor.Parameters.Add(new CodeParameterDeclarationExpression(GenerateContextType(new Code(current), context.ParentAt(context.ParentCount-1)), CONTEXT));
                for(int j = 0; j < context.ParentCount-1; ++j) {
                    string field = FIELD + j;
                    constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field), new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(CONTEXT), field)));
                }
                constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), FIELD + (context.ParentCount-1)), new CodeVariableReferenceExpression(CONTEXT)));
                type.Members.Add(constructor);
            }
            return result;
        }
        #endregion

        //--- Constructors ---
        private CodeBuilder() {}
    }
}
