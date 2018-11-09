using MoonSharp.Interpreter;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    abstract class Drawable {
        public Vec pos { get; set; }

        public float ang { get; set; }

        public Blend blend { get; set; }

        [MoonSharpHidden]
        public Drawable() { /* Default constructor */ }

        [MoonSharpHidden]
        public Drawable(DynValue param) {
            var table = param
                        .CheckType(nameof(Drawable), DataType.Table, 1)
                        .Table;

            this.pos = table
                       .Get(nameof(pos))
                       .CheckUserDataType<Vec>(nameof(Drawable));

            this.ang = (float)table
                       .Get(nameof(ang))
                       .CheckType(nameof(Drawable), DataType.Number,
                           flags: TypeValidationFlags.AllowNil)
                       .Number;

            this.blend = table
                         .Get(nameof(blend))
                         .CheckUserDataType<Blend>(nameof(Drawable),
                             flags: TypeValidationFlags.AllowNil);
        }

        [MoonSharpHidden]
        public abstract void Draw(Processor.Context ctx);
    }
}
