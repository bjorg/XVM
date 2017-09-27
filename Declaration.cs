using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

using Id = System.String;
using Sel = System.Int32;
using Pair = System.Collections.DictionaryEntry;

namespace XL.Declaration {
    internal sealed class Util {

        //--- Class Methods ---
        internal static string UnquoteString(string value) {
            System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length);
            for(int i = 1; i < value.Length-1; ++i) {
                if(value[i] == '\\') {
                    ++i;
                    switch(value[i]) {
                    case 'n':
                        result.Append("\n");
                        break;
                    case 'r':
                        result.Append("\r");
                        break;
                    case 't':
                        result.Append("\t");
                        break;
                    case 'b':
                        result.Append("\b");
                        break;
                    case 'f':
                        result.Append("\f");
                        break;
                    case '"':
                        result.Append("\"");
                        break;
                    case '\\':
                        result.Append("\\");
                        break;
                    case 'u':
                        // TODO: decode hexadecimal digits
                        throw new NotImplementedException("missing code");
                    }
                } else {
                    result.Append(value[i]);
                }
            }
            return result.ToString();
        }

        //--- Constructors ---
        private Util() {}
    }

    public sealed class Module {

        //--- Fields ---
        public SchemaCollection Schemas = new SchemaCollection();
        public ContextCollection Contexts = new ContextCollection();
        public ProcessCollection Processes = new ProcessCollection();
        private Declaration.Context root;

        //--- Properties ---
        public Declaration.Context Root {
            get {
                if(root == null) {
                    root = new Declaration.Context(this);
                }
                return root;
            }
        }

        //--- Methods ---
        public void Bind() {
            foreach(Schema.Base schema in Schemas.Values) {
                schema.Bind(this);
            }
            foreach(Process.Base process in Processes.Values) {
                process.Bind(Root);
            }
        }
    }

    public sealed class SchemaCollection {

        //--- Fields ---
        private Hashtable types = new Hashtable();

        //--- Properties ---
        public Schema.Base this[Id name] {
            get {
                return (Schema.Base)types[name];
            }
        }

        public Schema.Base[] Values {
            get {
                return (Schema.Base[])(new ArrayList(types.Values)).ToArray(typeof(Schema.Base));
            }
        }

        public int Count {
            get {
                return types.Count;
            }
        }

        //--- Methods ---
        public bool Contains(Id name) {
            return types.Contains(name);
        }

        public void Declare(Schema.Base schema, Id name) {
            types.Add(name, schema);
        }

        public string Declare(Schema.Base schema) {
            foreach(Pair p in types) {
                Schema.Base found = (Schema.Base)p.Value;
                if(found.Matches(schema)) {
                    return (string)p.Key;
                }
            }
            string name = "_schema" + types.Count;
            Declare(schema, name);
            return name;
        }
    }

    public sealed class ContextCollection {

        //--- Fields ---
        private ArrayList contexts = new ArrayList();

        //--- Properties ---
        public Context this[int index] {
            get {
                return (Context)contexts[index];
            }
        }

        public Context[] Values {
            get {
                return (Context[])contexts.ToArray(typeof(Context));
            }
        }

        public int Count {
            get {
                return contexts.Count;
            }
        }

        //--- Methods ---
        public void Add(Context context) {
            contexts.Add(context);
        }
    }

    public sealed class ProcessCollection {

        //--- Fields ---
        private ArrayList processes = new ArrayList();

        //--- Properties ---
        public Process.Base this[int index] {
            get {
                return (Process.Base)processes[index];
            }
        }

        public Process.Base[] Values {
            get {
                return (Process.Base[])processes.ToArray(typeof(Process.Base));
            }
        }

        public int Count {
            get {
                return processes.Count;
            }
        }

        //--- Methods ---
        public void Add(Process.Base process) {
            processes.Add(process);
        }
    }

    public sealed class Context {

        //--- Fields ---
        private Module module;
        private Hashtable variables = new Hashtable();
        private ArrayList parents;

        //--- Constructors ---
        public Context(Module module) {
            this.module = module;
            parents = new ArrayList();
            module.Contexts.Add(this);
        }

        public Context(Context parent) {
            module = parent.module;
            parents = new ArrayList(parent.parents.Count+1);
            parents.AddRange(parent.parents);
            parents.Add(parent);
            module.Contexts.Add(this);
        }

        //--- Properties ---
        public Schema.Base this[Id name] {
            get {
                object pair = variables[name];
                if((pair == null) && (parents.Count > 0)) {
                    return ((Context)parents[parents.Count-1])[name];
                }
                Debug.Assert(pair != null);
                return (Schema.Base)((Pair)pair).Value;
            }
        }

        public int Count {
            get {
                return variables.Count + parents.Count;
            }
        }

        public int ParentCount {
            get {
                return parents.Count;
            }
        }

        public Module Module {
            get {
                return module;
            }
        }

        public Pair[] Variables {
            get {
                Pair[] result = new Pair[variables.Count];
                foreach(Pair entry in variables) {
                    Pair p = (Pair)entry.Value;
                    int index = (int)p.Key;
                    result[index-ParentCount] = new Pair(entry.Key, p.Value);
                }
                return result;
            }
        }

        //--- Methods ---
        public int[] Declare(Schema.Base schema, Id name) {
            int[] result = new int[] { variables.Count + ParentCount };
            variables.Add(name, new Pair(result[0], schema));
            return result;
        }

        public int[] FindIndex(Id name) {
            int index = FindIndexInternal(name);
            if(index != -1) {
                return new int[] { index };
            }
            for(int i = parents.Count-1; i >= 0; --i) {
                index = ((Context)parents[i]).FindIndexInternal(name);
                if(index != -1) {
                    return new int[] { i, index };
                }
            }
            Debug.Fail("variable not found");
            return null;
        }

        private int FindIndexInternal(Id name) {
            object pair = variables[name];
            if(pair != null) {
                return (int)((Pair)pair).Key;
            }
            return -1;
        }

        public Context ParentAt(int index) {
            return (Context)parents[index];
        }
    }


    namespace Process {
        public abstract class Base {

            //--- Properties ---
            public abstract Context Context { get; }

            //--- Methods ---
            public abstract void Bind(Context decl);
        }

        public class Sequence : Base {

            //--- Fields ----
            private Action.Base[] actions;
            private Context decl;

            //--- Constructors ---
            public Sequence(Action.Base[] actions) {
                this.actions = actions;
            }

            //--- Properties ---
            public Action.Base this[int index] {
                get {
                    return actions[index];
                }
            }

            public Action.Base[] Actions {
                get {
                    return actions;
                }
            }

            public override Context Context {
                get {
                    return decl;
                }
            }

            public int Count {
                get {
                    return actions.Length;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                this.decl = new Context(decl);
                for(int i = 0; i < actions.Length; ++i) {
                    actions[i].Bind(Context);
                }
            }
        }

        public class Send : Base {

            //--- Fields ---
            private Id name;
            private int[] index;
            private Value.Base value;
            private Sel selector = -1;
            private Context decl;

            //--- Constructors ---
            public Send(Id name, Value.Base value) {
                this.name = name;
                this.value = value;
            }

            //--- Properties ---
            public override Context Context {
                get {
                    return decl;
                }
            }

            public int[] PortIndex {
                get {
                    return index;
                }
            }

            public Sel Selector {
                get {
                    return selector;
                }
            }

            public Value.Base Value {
                get {
                    return value;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                this.decl = new Context(decl);
                value.Bind(Context);
                index = Context.FindIndex(name);
                Debug.Assert(index != null);
                Schema.Port port_type = (Schema.Port)Context[name].Deref;
                selector = port_type.FindSelector(value.Schema);
                Debug.Assert(selector != -1);
            }
        }

    }

    namespace Action {
        public abstract class Base {

            //--- Methods ---
            public abstract void Bind(Context decl);
        }

        public class Receive : Base {

            //--- Fields ---
            private Id name;
            private int[] index;
            private Pattern.Base pattern;
            private Sel selector = -1;
            private Schema.Port port_type;

            //--- Constructors ---
            public Receive(Id name, Pattern.Base pattern) {
                Debug.Assert(name != null);
                Debug.Assert(pattern != null);
                this.name = name;
                this.pattern = pattern;
            }

            //--- Properties ---
            public Id PortName {
                get {
                    return name;
                }
            }

            public int[] PortIndex {
                get {
                    return index;
                }
            }

            public Schema.Port PortType {
                get {
                    return port_type;
                }
            }

            public Pattern.Base Pattern {
                get {
                    return pattern;
                }
            }

            public Sel Selector {
                get {
                    return selector;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                pattern.Bind(decl);
                index = decl.FindIndex(name);
                Debug.Assert(index != null);
                port_type = (Schema.Port)decl[name].Deref;
                selector = port_type.FindSelector(pattern.Schema);
            }
        }

        public class Let : Base {

            //--- Fields ---
            private Pattern.Base pattern;
            private Value.Base value;

            //--- Constructors ---
            public Let(Pattern.Base pattern, Value.Base value) {
                this.pattern = pattern;
                this.value = value;
            }

            //--- Properties ---
            public Pattern.Base Pattern {
                get {
                    return pattern;
                }
            }

            public Value.Base Value {
                get {
                    return value;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                value.Bind(decl);
                pattern.Bind(decl);
                Debug.Assert(pattern.Schema.Matches(value.Schema));
            }
        }

        public class Select : Base {

            //--- Fields ---
            private Receive[] receivers;

            //--- Constructors ---
            public Select(Action.Receive[] receivers) {
                this.receivers = receivers;
            }

            //--- Methods ---
            public override void Bind(Context decl) {

                // TODO:
                throw new NotImplementedException("missing code");
            }
        }

        public class If : Base {

            //--- Fields ---
            private Value.Base value;
            private Process.Base on_true;
            private Process.Base on_false;

            //--- Constructors ---
            public If(Value.Base value, Process.Base on_true, Process.Base on_false) {
                this.value = value;
                this.on_true = on_true;
                this.on_false = on_false;
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                value.Bind(decl);
                Debug.Assert(value.Schema is Schema.Boolean);
                on_true.Bind(decl);
                on_false.Bind(decl);
            }
        }

        public class Switch : Base {

            //--- Types ---
            public class Case {

                //--- Fields ---
                private Pattern.Base pattern;
                private Base process;

                //--- Contructors ---
                public Case(Pattern.Base pattern, Base process) {
                    this.pattern = pattern;
                    this.process = process;
                }
            }

            //--- Fields ---
            private Value.Base value;
            private Case[] cases;

            //--- Constructors ---
            public Switch(Value.Base value, Case[] cases) {
                this.value = value;
                this.cases = cases;
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                value.Bind(decl);

                // TODO: ensure value-type is covered by all cases (and vice-versa)
                throw new NotImplementedException("missing code");
            }
        }

        public class Fork : Base {

            //--- Fields ---
            private Process.Base process;

            //--- Constructors ---
            public Fork(Process.Base process) {
                this.process = process;
            }

            //--- Properties ---
            public Process.Base Process {
                get {
                    return process;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                process.Bind(decl);
            }
        }
    }

    namespace Value {
        public abstract class Base {

            //--- Properties ---
            public abstract Schema.Base Schema { get; }

            //--- Methods ---
            public abstract void Bind(Context decl);
        }

        public class Unordered : Base {

            //--- Fields ---
            private Base[] values;

            //--- Constructors ---
            public Unordered(Base[] values) {
                this.values = values;
            }

            //--- Properties ---
            public Base this[int index] {
                get {
                    return values[index];
                }
            }

            public int Count {
                get {
                    return values.Length;
                }
            }

            public Base[] Values {
                get {
                    return values;
                }
            }

            public override Schema.Base Schema {
                get {
                    Declaration.Schema.Base[] schemas = new Declaration.Schema.Base[Count];
                    for(int i = 0; i < Count; ++i) {
                        schemas[i] = values[i].Schema;
                    }
                    return new Declaration.Schema.Unordered(schemas);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                for(int i = 0; i < Count; ++i) {
                    values[i].Bind(decl);
                }
            }
        }

        public class Ordered : Base {

            //--- Fields ---
            private Base[] values;

            //--- Constructors ---
            public Ordered(Base[] values) {
                this.values = values;
            }

            //--- Properties ---
            public Base this[int index] {
                get {
                    return values[index];
                }
            }

            public int Count {
                get {
                    return values.Length;
                }
            }

            public Base[] Values {
                get {
                    return values;
                }
            }

            public override Schema.Base Schema {
                get {
                    Declaration.Schema.Base[] schemas = new Declaration.Schema.Base[Count];
                    for(int i = 0; i < Count; ++i) {
                        schemas[i] = values[i].Schema;
                    }
                    return new Declaration.Schema.Ordered(schemas);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                for(int i = 0; i < Count; ++i) {
                    values[i].Bind(decl);
                }
            }
        }

        public class Element : Base {

            //--- Fields ---
            private string tag;
            private Base value;

            //--- Constructors ---
            public Element(string tag, Base value) {
                this.tag = tag;
                this.value = value;
            }

            //--- Properties ---
            public string Tag {
                get {
                    return tag;
                }
            }

            public Base Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return new Declaration.Schema.Element(tag, value.Schema);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                value.Bind(decl);
            }
        }

        public class New : Base {

            //--- Fields ---
            private Schema.Base schema;

            //--- Constructors ---
            public New(Schema.Base schema) {
                this.schema = schema;
            }

            //--- Properties ---
            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                schema.Bind(decl.Module);
                Debug.Assert(schema.Deref is Schema.Port);
            }
        }

        public class Variable : Base {

            //--- Fields ---
            private Id name;
            private int[] index;
            private Schema.Base schema;

            //--- Constructors ---
            public Variable(Id name) {
                this.name = name;
            }

            //--- Properties ---
            public int[] Index {
                get {
                    return index;
                }
            }

            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                index = decl.FindIndex(name);
                schema = decl[name];
            }
        }

        public class Local : Base {

            //--- Fields ---
            private Id name;
            private Schema.Base schema;

            //--- Constructors ---
            public Local(Schema.Base schema, Id name) {
                this.schema = schema;
                this.name = name;
            }

            //--- Properties ---
            public Id Name {
                get {
                    return name;
                }
            }

            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                throw new InvalidOperationException();
            }
        }

        public abstract class Literal : Base {

            //--- Methods ---
            public override void Bind(Context decl) {}
        }

        public class Void : Literal {

            //--- Class Fields ---
            public static Void Instance = new Void();

            //--- Constructors ---
            private Void() {}

            //--- Properties ---
            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.Void.Instance;
                }
            }
        }

        public class Boolean : Literal {

            //--- Fields ---
            private bool value;

            //--- Constructors ---
            public Boolean(bool value) {
                this.value = value;
            }

            //--- Properties ---
            public bool Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.Boolean.Instance;
                }
            }
        }

        public class Integer : Literal {

            //--- Fields ---
            private int value;

            //--- Constructors ---
            public Integer(int value) {
                this.value = value;
            }

            //--- Properties ---
            public int Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.Integer.Instance;
                }
            }
        }

        public class FloatingPoint : Literal {

            //--- Fields ---
            private double value;

            //--- Constructors ---
            public FloatingPoint(double value) {
                this.value = value;
            }

            //--- Properties ---
            public double Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.FloatingPoint.Instance;
                }
            }
        }

        public class Character : Literal {

            //--- Fields ---
            private char value;

            //--- Constructors ---
            public Character(char value) {
                this.value = value;
            }

            //--- Properties ---
            public char Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.Character.Instance;
                }
            }
        }

        public class String : Literal {

            //--- Fields ---
            private string value;

            //--- Constructors ---
            public String(string value) {
                this.value = value;
            }

            //--- Properties ---
            public string Value {
                get {
                    return value;
                }
            }

            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.String.Instance;
                }
            }
        }

//        public class Port : Literal {
//
//            //--- Fields ---
//            private Schema.Port port_type;
//            private Runtime.Channel[] channels;
//
//            //--- Constructors ---
//            public Port(Schema.Port port_type, Runtime.Channel[] channels) {
//                this.port_type = port_type;
//                this.channels = channels;
//            }
//
//            //--- Properties ---
//            public Runtime.Channel[] Channels {
//                get {
//                    return channels;
//                }
//            }
//
//            public override Schema.Base Schema {
//                get {
//                    return port_type;
//                }
//            }
//
//            //--- Properties ---
//            public Runtime.Channel this[Sel selector] {
//                get {
//                    return channels[selector];
//                }
//            }
//        }
    }

    namespace Pattern {
        public abstract class Base {

            //--- Properties ---
            public abstract Schema.Base Schema { get; }

            //--- Methods ---
            public abstract void Bind(Context decl);
        }

        public class Unordered : Base {

            //--- Fields ---
            private Base[] patterns;

            //--- Constructors ---
            public Unordered(Base[] patterns) {
                this.patterns = patterns;
            }

            //--- Properties ---
            public Base this[int index] {
                get {
                    return patterns[index];
                }
            }

            public int Count {
                get {
                    return patterns.Length;
                }
            }

            public override Schema.Base Schema {
                get {
                    Declaration.Schema.Base[] schemas = new Declaration.Schema.Base[Count];
                    for(int i = 0; i < Count; ++i) {
                        schemas[i] = patterns[i].Schema;
                    }
                    return new Declaration.Schema.Unordered(schemas);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                for(int i = 0; i < Count; ++i) {
                    patterns[i].Bind(decl);
                }
            }
        }

        public class Ordered : Base {

            //--- Fields ---
            private Base[] patterns;

            //--- Constructors ---
            public Ordered(Base[] patterns) {
                this.patterns = patterns;
            }

            //--- Properties ---
            public Base this[int index] {
                get {
                    return patterns[index];
                }
            }

            public int Count {
                get {
                    return patterns.Length;
                }
            }

            public override Schema.Base Schema {
                get {
                    Declaration.Schema.Base[] schemas = new Declaration.Schema.Base[Count];
                    for(int i = 0; i < Count; ++i) {
                        schemas[i] = patterns[i].Schema;
                    }
                    return new Declaration.Schema.Ordered(schemas);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                for(int i = 0; i < Count; ++i) {
                    patterns[i].Bind(decl);
                }
            }
        }

        public class Element : Base {

            //--- Fields ---
            private string tag;
            private Base pattern;

            //--- Constructors ---
            public Element(string tag, Base pattern) {
                this.tag = tag;
                this.pattern = pattern;
            }

            //--- Properties ---
            public string Tag {
                get {
                    return tag;
                }
            }

            public Base Pattern {
                get {
                    return pattern;
                }
            }

            public override Schema.Base Schema {
                get {
                    return new Declaration.Schema.Element(tag, pattern.Schema);
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                pattern.Bind(decl);
            }
        }

        public class Variable : Base {

            //--- Fields ---
            private Schema.Base schema;
            private int[] index;
            private Id name;

            //--- Constructors ---
            public Variable(Schema.Base schema, Id name) {
                this.schema = schema;
                this.name = name;
            }

            //--- Properties ---
            public int[] Index {
                get {
                    return index;
                }
            }

            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                schema.Bind(decl.Module);
                index = decl.Declare(schema, name);
            }
        }

        public class Local : Base {

            //--- Fields ---
            private Schema.Base schema;
            private Id name;

            //--- Constructors ---
            public Local(Schema.Base schema, Id name) {
                this.schema = schema;
                this.name = name;
            }

            //--- Properties ---
            public Id Name {
                get {
                    return name;
                }
            }

            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                throw new InvalidOperationException();
            }
        }

        public class Discard : Base {

            //--- Fields ---
            private Schema.Base schema;

            //--- Constructors ---
            public Discard(Schema.Base schema) {
                this.schema = schema;
            }

            //--- Properties ---
            public override Schema.Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {
                schema.Bind(decl.Module);
            }
        }

        public class Void : Base {

            //--- Class Fields ---
            public readonly static Void Instance = new Void();

            //--- Constructors ---
            private Void() {}

            //--- Properties ---
            public override Schema.Base Schema {
                get {
                    return Declaration.Schema.Void.Instance;
                }
            }

            //--- Methods ---
            public override void Bind(Context decl) {}
        }
    }

    namespace Schema {
        public abstract class Base {

            //--- Properties ---
            public Base Deref {
                get {
                    Base schema = this;
                    while(schema is Reference) {
                        schema = ((Reference)schema).Schema;
                    }
                    return schema;
                }
            }

            //--- Methods ---
            public abstract void Bind(Module module);
            public abstract bool Matches(Value.Base value);
            public abstract bool Matches(Schema.Base schema);
        }

        public class Unordered : Base {

            //--- Fields ---
            private Base[] schemas;

            //--- Constructors ---
            public Unordered(Base[] schemas) {
                this.schemas = schemas;
            }

            //--- Properties ---
            public int Count {
                get {
                    return schemas.Length;
                }
            }

            //--- Methods ---
            public override void Bind(Module module) {
                for(int i = 0; i < Count; ++i) {
                    schemas[i].Bind(module);
                }
            }

            public override bool Matches(Value.Base value) {

                // TODO:
                throw new NotImplementedException("missing code");
            }

            public override bool Matches(Schema.Base schema) {

                // TODO:
                throw new NotImplementedException("missing code");
            }
        }

        public class Ordered : Base {

            //--- Fields ---
            private Base[] schemas;

            //--- Constructors ---
            public Ordered(Base[] schemas) {
                this.schemas = schemas;
            }

            //--- Properties ---
            public int Count {
                get {
                    return schemas.Length;
                }
            }

            public Base this[int index] {
                get {
                    return schemas[index];
                }
            }

            //--- Methods ---
            public override void Bind(Module module) {
                for(int i = 0; i < Count; ++i) {
                    schemas[i].Bind(module);
                }
            }

            public override bool Matches(Value.Base value) {
                Value.Ordered ordered = value as Value.Ordered;
                if((ordered == null) || (ordered.Count != Count)) {
                    return false;
                }
                for(int i = 0; i < Count; ++i) {
                    if(!schemas[i].Matches(ordered[i])) {
                        return false;
                    }
                }
                return true;
            }

            public override bool Matches(Schema.Base schema) {
                Ordered ordered = schema as Ordered;
                if((ordered == null) || (ordered.Count != Count)) {
                    return false;
                }
                for(int i = 0; i < Count; ++i) {
                    if(!schemas[i].Matches(ordered[i])) {
                        return false;
                    }
                }
                return true;
            }
        }

        public class Choice : Base {

            //--- Fields ---
            private Base[] schemas;

            //--- Constructors ---
            public Choice(Base[] schemas) {
                this.schemas = schemas;
            }

            //--- Properties ---
            public Base this[int index] {
                get {
                    return schemas[index];
                }
            }

            public int Count {
                get {
                    return schemas.Length;
                }
            }

            //--- Methods ---
            public override void Bind(Module module) {
                for(int i = 0; i < Count; ++i) {
                    schemas[i].Bind(module);
                }
            }

            public override bool Matches(Value.Base value) {
                for(int i = 0; i < Count; ++i) {
                    if(schemas[i].Matches(value)) {
                        return true;
                    }
                }
                return false;
            }

            public override bool Matches(Schema.Base schema) {
                Choice choice = schema as Choice;
                if((choice == null) || (choice.Count != Count)) {
                    return false;
                }
                for(int i = 0; i < Count; ++i) {
                    if(!schemas[i].Matches(choice[i])) {
                        return false;
                    }
                }
                return true;
            }
        }

        public class Element : Base {

            //--- Fields ---
            private string tag;
            private Base schema;

            //--- Constructors ---
            public Element(string tag, Base schema) {
                this.tag = tag;
                this.schema = schema;
            }

            //--- Properties ---
            public string Tag {
                get {
                    return tag;
                }
            }

            public Base Schema {
                get {
                    return schema;
                }
            }
    
            //--- Methods ---
            public override void Bind(Module module) {
                schema.Bind(module);
            }

            public override bool Matches(Value.Base value) {
                Value.Element element = value as Value.Element;
                if(element == null) {
                    return false;
                }
                return (element.Tag == Tag) && Schema.Matches(element.Value);
            }

            public override bool Matches(Schema.Base schema) {
                Element element = schema as Element;
                if(element == null) {
                    return false;
                }
                return (element.Tag == Tag) && Schema.Matches(element.Schema);
            }
        }

        public class Reference : Base {

            //--- Fields ---
            private Id name;
            private Base schema;

            //--- Constructors ---
            public Reference(Id name) {
                this.name = name;
            }

            public Reference(Base schema, Id name) {
                this.schema = schema;
                this.name = name;
            }

            //--- Properties ---
            public string Name {
                get {
                    return name;
                }
            }

            public Base Schema {
                get {
                    return schema;
                }
            }

            //--- Methods ---
            public override void Bind(Module module) {
                if(schema == null) {
                    schema = module.Schemas[name];
                    Debug.Assert(schema != null);
                }
            }

            public override bool Matches(Value.Base value) {
                return schema.Deref.Matches(value);
            }

            public override bool Matches(Schema.Base schema) {
                Reference reference = schema as Reference;
                if(reference == null) {
                    return false;
                }
                return (reference.Name == Name);
            }
        }

        public abstract class Port : Base {

            //--- Properties ---
            public abstract Type InstanceType { get; }

            //--- Methods ---
            public abstract Sel FindSelector(Schema.Base schema);

            public override bool Matches(Value.Base value) {

                // we don't support port literals
                throw new NotSupportedException("unexpected");
            }

            public override bool Matches(Schema.Base schema) {
                return Object.ReferenceEquals(schema, this);
            }
        }

        public class GenericPort : Port {

            //--- Fields ---
            private Base[] channel_schemas;

            //--- Constructors ---
            public GenericPort(Base[] channel_schemas) {
                Debug.Assert(channel_schemas.Length > 0);
                this.channel_schemas = channel_schemas;
            }

            //--- Properties ---
            public Base[] Channels {
                get {
                    return channel_schemas;
                }
            }

            public override Type InstanceType {
                get {
                    return typeof(Runtime.Port);
                }
            }

            //--- Methods ---
            public override Sel FindSelector(Schema.Base schema) {
                for(int i = 0; i < channel_schemas.Length; ++i) {
                    if(channel_schemas[i].Matches(schema))  {
                        return i;
                    }
                }
                return -1;
            }

            public override void Bind(Module module) {
                for(int i = 0; i < channel_schemas.Length; ++i) {
                    channel_schemas[i].Bind(module);
                }
            }
        }

        public abstract class Ground : Base {

            //--- Methods ---
            public override void Bind(Module module) {}
        }

        public class Void : Ground {

            //--- Class Fields ---
            public static readonly Void Instance = new Void();

            //--- Constructors ---
            private Void() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.Void;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is Void;
            }
        }

        public class Boolean : Ground {

            //--- Class Fields ---
            public static readonly Boolean Instance = new Boolean();

            //--- Constructors ---
            private Boolean() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.Boolean;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is Boolean;
            }
        }

        public class Integer : Ground {

            //--- Class Fields ---
            public static readonly Integer Instance = new Integer();

            //--- Constructors ---
            private Integer() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.Integer;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is Integer;
            }
        }

        public class FloatingPoint : Ground {

            //--- Class Fields ---
            public static readonly FloatingPoint Instance = new FloatingPoint();

            //--- Constructors ---
            private FloatingPoint() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.FloatingPoint;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is FloatingPoint;
            }
        }

        public class Character : Ground {

            //--- Class Fields ---
            public static readonly Character Instance = new Character();

            //--- Constructors ---
            private Character() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.Character;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is Character;
            }
        }

        public class String : Ground {

            //--- Class Fields ---
            public static readonly String Instance = new String();

            //--- Constructors ---
            private String() {}
    
            //--- Methods ---
            public override bool Matches(Value.Base value) {
                return value is Value.String;
            }

            public override bool Matches(Schema.Base schema) {
                return schema is String;
            }
        }
    }
}
