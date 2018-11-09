using MoonSharp.Interpreter;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    abstract class Drawable {
        public Vec pos { get; set; }

        [MoonSharpHidden]
        public abstract void Draw(Processor.Context ctx);
    }
}
