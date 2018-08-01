using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using Params = ImageGenerator.Params;
using LuaLib = ImageGenerator.LuaLib;
using System.Linq;
using System.Reflection;

namespace ImageGenerator {
    [MoonSharpUserData]
    class LuaContextParams {
        public string TemplatePath { get; set; }
    }

    class LuaContext {
        static LuaContext() {
            UserData.RegisterAssembly(Assembly.GetAssembly(typeof(LuaContext)));
        }

        public string FilePath { get; }

        public Script Lua { get; }

        private bool hasExecuted = false;

        public LuaContext(string filePath) {
            FilePath = filePath;

            Lua = new Script(CoreModules.Basic |
                             CoreModules.Bit32 |
                             CoreModules.ErrorHandling |
                             // CoreModules.IO |
                             CoreModules.Math |
                             CoreModules.Metatables |
                             CoreModules.OS_Time |
                             CoreModules.String |
                             CoreModules.Table |
                             CoreModules.TableIterators);

            // Object constructors
            Lua.Globals["Vec"] = (Func<DynValue, DynValue, Params.Vec>)Params.Vec.Create;
            Lua.Globals["Blend"] = (Func<DynValue, DynValue, Params.Blend>)Params.Blend.Create;
            Lua.Globals["Image"] = (Func<DynValue, Params.Image>)Params.Image.Create;
            Lua.Globals["Brush"] = (Func<DynValue, Params.Brush>)Params.Brush.Create;
            Lua.Globals["Pen"] = (Func<DynValue, DynValue, Params.Pen>)Params.Pen.Create;
            Lua.Globals["Font"] = (Func<DynValue, DynValue, DynValue, Params.Font>)Params.Font.Create;
            Lua.Globals["Label"] = (Func<DynValue, Params.Label>)Params.Label.Create;

            // Utility functions
            Lua.Globals["input"] = (Func<string, Script, string>)LuaLib.Globals.input;

            // Constants
            Lua.Globals["TEMPLATEPATH"] = Path.GetFullPath(FilePath);
            Lua.Globals["TEMPLATE"] = Path.GetFileNameWithoutExtension(FilePath);
        }

        public void Execute() {
            CheckHasExecuted(false);

            using(var stream = File.OpenRead(FilePath)) {
                Lua.DoStream(stream, codeFriendlyName: Path.GetFileNameWithoutExtension(FilePath));
            }

            hasExecuted = true;
        }

        public void SetArgsTable(IEnumerable<string> args) {
            CheckHasExecuted(false);

            var elems = args.Select(x => DynValue.NewString(x));
            Lua.Globals["args"] = DynValue.NewTable(Lua, elems.ToArray());
        }

        public Params.Main GetProcessorParams() {
            CheckHasExecuted(true);

            var table = DynValue.NewTable(Lua.Globals);
            return Params.Main.Create(table);
        }

        private void CheckHasExecuted(bool desired) {
            if(!desired && hasExecuted) {
                throw new InvalidOperationException("Cannot perform this operation after Execute() call");
            } else if(desired && !hasExecuted) {
                throw new InvalidOperationException("Execute() must be called prior to this operation");
            }
        }
    }
}
