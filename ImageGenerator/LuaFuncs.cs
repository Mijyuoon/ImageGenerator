using System;
using System.Collections.Generic;
using System.Text;
using MoonSharp.Interpreter;

namespace ImageGenerator.LuaLib {
    static class Globals {
        public static string input(string prompt, Script context) {
            return context.Options.DebugInput(prompt);
        }
    }
}
