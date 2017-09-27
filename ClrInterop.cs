using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Id = System.String;
using Sel = System.Int32;

using Schema = XL.Declaration.Schema;
using Value = XL.Declaration.Value;
using Pattern = XL.Declaration.Pattern;
using Module = XL.Declaration.Module;

namespace XL {

    public sealed class UnsupportedTypeException : ApplicationException {}

    public sealed class Util {

        //--- Constants ---
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
        private const BindingFlags ClassFlags = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.InvokeMethod;

        //--- Class Methods ---
        public static void DeclareGroundPortSchemas(Module module) {

            // ground types
            module.Schemas.Declare(Schema.Void.Instance, "void");
            module.Schemas.Declare(Schema.Boolean.Instance, "bool");
            module.Schemas.Declare(Schema.Integer.Instance, "int");
            module.Schemas.Declare(Schema.FloatingPoint.Instance, "float");
            module.Schemas.Declare(Schema.Character.Instance, "char");
            module.Schemas.Declare(Schema.String.Instance, "string");

            // ground ports
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.Void.Instance }), "VoidPort");
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.Boolean.Instance }), "BoolPort");
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.Integer.Instance }), "IntPort");
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.FloatingPoint.Instance }), "FloatPort");
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.Character.Instance }), "CharPort");
            module.Schemas.Declare(new Schema.GenericPort(new Schema.Base[] { Schema.String.Instance }), "StringPort");
        }

        public static Id SchemaName(Type type, bool instance) {
            if(type == typeof(void)) {
                return "void";
            } else if(type == typeof(bool)) {
                return "bool";
            } else if(type == typeof(int)) {
                return "int";
            } else if(type == typeof(double)) {
                return "float";
            } else if(type == typeof(char)) {
                return "char";
            } else if(type == typeof(string)) {
                return "string";
            }
            string result = type.FullName.Replace(".", "$");
            return result;
        }

        public static Id PortSchemaName(Type type) {
            if(type == typeof(void)) {
                return "VoidPort";
            } else if(type == typeof(bool)) {
                return "BoolPort";
            } else if(type == typeof(int)) {
                return "IntPort";
            } else if(type == typeof(double)) {
                return "FloatPort";
            } else if(type == typeof(char)) {
                return "CharPort";
            } else if(type == typeof(string)) {
                return "StringPort";
            }
            return type.FullName.Replace(".", "$");
        }

        public static Schema.Base ConvertClrType(Module module, Type type, bool instance) {

            // check if we want the instance or class
            if(instance) {
                if(type == typeof(void)) {
                    return Schema.Void.Instance;
                } else if(type == typeof(bool)) {
                    return Schema.Boolean.Instance;
                } else if(type == typeof(int)) {
                    return Schema.Integer.Instance;
                } else if(type == typeof(double)) {
                    return Schema.FloatingPoint.Instance;
                } else if(type == typeof(char)) {
                    return Schema.Character.Instance;
                } else if(type == typeof(string)) {
                    return Schema.String.Instance;
                }
                Id name = type.FullName.Replace(".", "$").Replace("[]", "Array");
                return new Schema.Reference(GenerateClrType(module, name, type, instance), name);
            } else {
                Id name = type.FullName.Replace(".", "$").Replace("[]", "Array") + "_class";
                return new Schema.Reference(GenerateClrType(module, name, type, instance), name);
            }
        }

        public static Schema.Base CreatePortType(Module module, Schema.Base payload) {
            string name;
            if(payload == Schema.Void.Instance) {
                name = "VoidPort";
            } else if(payload == Schema.Boolean.Instance) {
                name = "BoolPort";
            } else if(payload == Schema.Integer.Instance) {
                name = "IntPort";
            } else if(payload == Schema.FloatingPoint.Instance) {
                name = "FloatPort";
            } else if(payload == Schema.Character.Instance) {
                name = "CharPort";
            } else if(payload == Schema.String.Instance) {
                name = "StringPort";
            } else {
                return new Schema.GenericPort(new Schema.Base[] { payload });
            }
            return new Schema.Reference(name);
        }

        private static Schema.Base GenerateClrType(Module module, Id name, Type type, bool instance) {

            // check if type is already generated
            Schema.Base found = module.Schemas[name];
            if(found != null) {
                return found;
            }

            // ensure it is not a value-type
            if(type.IsValueType || type.IsByRef) {
                throw new UnsupportedTypeException();
            }

            // determine if port-type corresponds to class or instance
            Schema.Port result;
            if(instance) {
                result = Schema.ClrInstancePort.CreatePlaceHolder();
            } else {
                result = Schema.ClrClassPort.CreatePlaceHolder();
            }

            // declare schema (to support recursion)
            module.Schemas.Declare(result, name);

            // collect methods that can be mapped
            Type used_type = type.IsArray ? typeof(System.Array) : type;
            MethodInfo[] all_methods = used_type.GetMethods(instance ? InstanceFlags : ClassFlags);
            ArrayList successful_schemas = new ArrayList(all_methods.Length);
            ArrayList successful_methods = new ArrayList(all_methods.Length);
            for(int i = 0; i < all_methods.Length; ++i) {
                try {
                    Schema.Base schema = CreateMethodSchema(module, all_methods[i]);
                    successful_schemas.Add(schema);
                    successful_methods.Add(all_methods[i]);
                } catch(UnsupportedTypeException) {

                    // ignore methods containing unsupported types
                }
            }

            // complete schema declaration
            if(instance) {
                Schema.ClrInstancePort.InitializePlaceHolder(
                    (Schema.ClrInstancePort)result, 
                    type,
                    (MethodInfo[])successful_methods.ToArray(typeof(MethodInfo)), 
                    (Schema.Base[])successful_schemas.ToArray(typeof(Schema.Base))
                );
            } else {
                Schema.ClrClassPort.InitializePlaceHolder(
                    (Schema.ClrClassPort)result, 
                    type,
                    (MethodInfo[])successful_methods.ToArray(typeof(MethodInfo)), 
                    (Schema.Base[])successful_schemas.ToArray(typeof(Schema.Base))
                );
            }
            return result;
        }

        private static Schema.Base CreateMethodSchema(Module module, MethodInfo method) {
            ParameterInfo[] args = method.GetParameters();
            ArrayList schemas = new ArrayList(args.Length);
            for(int i = 0; i < args.Length; ++i) {
                schemas.Add(ConvertClrType(module, args[i].ParameterType, true));
            }
            schemas.Add(CreatePortType(module, ConvertClrType(module, method.ReturnType, true)));

            // normalize result
            Schema.Base result;
            switch(schemas.Count) {
            case 0:
                result = new Schema.Element(method.Name, Declaration.Schema.Void.Instance);
                break;
            case 1:
                result = new Schema.Element(method.Name, (Schema.Base)schemas[0]);
                break;
            default:
                result = new Schema.Element(method.Name, new Schema.Ordered((Schema.Base[])schemas.ToArray(typeof(Schema.Base))));
                break;
            }
            return result;
        }

        public static Pattern.Base CreateMethodPattern(Module module, MethodInfo method) {
            ParameterInfo[] args = method.GetParameters();
            ArrayList patterns = new ArrayList(args.Length);
            for(int i = 0; i < args.Length; ++i) {
                patterns.Add(ClrPattern(module, "_arg" + i, args[i].ParameterType));
            }
            patterns.Add(new Pattern.Local(CreatePortType(module, ConvertClrType(module, method.ReturnType, true)), "_reply"));

            // normalize result
            Pattern.Base result;
            switch(patterns.Count) {
            case 0:
                result = new Pattern.Element(method.Name, Pattern.Void.Instance);
                break;
            case 1:
                result = new Pattern.Element(method.Name, (Pattern.Base)patterns[0]);
                break;
            default:
                result = new Pattern.Element(method.Name, new Pattern.Ordered((Pattern.Base[])patterns.ToArray(typeof(Pattern.Base))));
                break;
            }
            return result;
        }

        public static Pattern.Base ClrPattern(Module module, string arg, Type type) {
            if(type == typeof(void)) {
                return Pattern.Void.Instance;
            } else if(type == typeof(bool)) {
                return new Pattern.Local(Schema.Boolean.Instance, arg);
            } else if(type == typeof(int)) {
                return new Pattern.Local(Schema.Integer.Instance, arg);
            } else if(type == typeof(double)) {
                return new Pattern.Local(Schema.FloatingPoint.Instance, arg);
            } else if(type == typeof(char)) {
                return new Pattern.Local(Schema.Character.Instance, arg);
            } else if(type == typeof(string)) {
                return new Pattern.Local(Schema.String.Instance, arg);
            } else if(type == typeof(Schema.ClrInstancePort)) {
                // TODO: ???
                throw new NotImplementedException("missing code");
            } else if(type == typeof(Schema.ClrClassPort)) {
                // TODO: ???
                throw new NotImplementedException("missing code");
            }
            return new Pattern.Local(ConvertClrType(module, type, true), arg);
        }
    }

    namespace Declaration {
        namespace Schema {
            public abstract class ClrPort : Port {

                //--- Class Methods ---
                internal static void InitializePlaceHolder(ClrPort place_holder, Type clr_type, MethodInfo[] methods, Base[] channel_schemas) {
                    Debug.Assert(clr_type != null);
                    Debug.Assert(channel_schemas.Length > 0);
                    Debug.Assert(channel_schemas.Length == methods.Length);
                    place_holder.clr_type = clr_type;
                    place_holder.methods = methods;
                    place_holder.channel_schemas = channel_schemas;
                }

                //--- Fields ---
                private Type clr_type;
                private MethodInfo[] methods;
                private Base[] channel_schemas;

                //--- Constructors ---
                protected ClrPort() {}

                public ClrPort(Type clr_type, MethodInfo[] methods, Base[] channel_schemas) {
                    Debug.Assert(channel_schemas.Length > 0);
                    Debug.Assert(clr_type != null);
                    this.clr_type = clr_type;
                    this.methods = methods;
                    this.channel_schemas = channel_schemas;
                }

                //--- Properties ---
                public MethodInfo[] Methods {
                    get {
                        return methods;
                    }
                }

                public Base[] Channels {
                    get {
                        return channel_schemas;
                    }
                }

                public Type ClrType {
                    get {
                        return clr_type;
                    }
                }

                //--- Methods ---
                public override void Bind(Module module) {
                    for(int i = 0; i < channel_schemas.Length; ++i) {
                        channel_schemas[i].Bind(module);
                    }
                }

                public override Sel FindSelector(Base schema) {
                    for(int i = 0; i < channel_schemas.Length; ++i) {
                        if(channel_schemas[i].Matches(schema))  {
                            return i;
                        }
                    }
                    return -1;
                }
            }

            public sealed class ClrInstancePort : ClrPort {

                //--- Class Methods ---
                internal static ClrInstancePort CreatePlaceHolder() {
                    return new ClrInstancePort();
                }

                //--- Constructors ---
                private ClrInstancePort() {}
                public ClrInstancePort(Type clr_type, MethodInfo[] methods, Base[] channel_schemas) : base(clr_type, methods, channel_schemas) {}

                //--- Properties ---
                public override Type InstanceType {
                    get {
                        return typeof(Runtime.ClrInstancePort);
                    }
                }
            }

            public sealed class ClrClassPort : ClrPort {

                //--- Class Methods ---
                internal static ClrClassPort CreatePlaceHolder() {
                    return new ClrClassPort();
                }

                //--- Constructors ---
                private ClrClassPort() {}
                public ClrClassPort(Type clr_type, MethodInfo[] methods, Base[] channel_schemas) : base(clr_type, methods, channel_schemas) {}

                //--- Properties ---
                public override Type InstanceType {
                    get {
                        return typeof(Runtime.ClrClassPort);
                    }
                }
            }
        }
    }

    namespace Runtime {
    }
}
