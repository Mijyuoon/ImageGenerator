using System;
using MoonSharp.Interpreter;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    class Vec {
        public float x { get; set; }
        public float y { get; set; }

        public Vec dup() => new Vec { x = x, y = y };

        public static implicit operator Point(Vec that) => new Point((int)that.x, (int)that.y);
        public static implicit operator Size(Vec that) => new Size((int)that.x, (int)that.y);

        public static implicit operator PointF(Vec that) => new PointF(that.x, that.y);
        public static implicit operator SizeF(Vec that) => new SizeF(that.x, that.y);

        [MoonSharpHidden]
        public Vec() { /* Default constructor */ }

        [MoonSharpHidden]
        public Vec(DynValue x, DynValue y) {
            this.x = (float)x
                     .CheckType(nameof(Vec), DataType.Number, 1)
                     .Number;

            this.y = (float)y
                     .CheckType(nameof(Vec), DataType.Number, 2)
                     .Number;
        }

        [MoonSharpHidden]
        public static Vec Create(DynValue x, DynValue y) => new Vec(x, y);
    }

    [MoonSharpUserData]
    class Blend {
        [MoonSharpHidden]
        public PixelBlenderMode itype { get; set; }

        public string type {
            get => itype.ToString().ToLower();
            set {
                if(!Enum.TryParse<PixelBlenderMode>(value, true, out var result)) {
                    throw new ScriptRuntimeException($"value '{value}' is not valid blend type");
                }

                itype = result;
            }
        }

        public float fraction { get; set; } = 1.0f;

        public Blend dup() => new Blend { itype = itype, fraction = fraction };

        [MoonSharpHidden]
        public Blend() { /* Default constructor */ }

        [MoonSharpHidden]
        public Blend(DynValue type, DynValue frac) {
            this.type = type
                        .CheckType(nameof(Blend), DataType.String, 1)
                        .String;

            if(frac.Type == DataType.Void) return;
            this.fraction = (float)frac
                            .CheckType(nameof(Blend), DataType.Number, 2)
                            .Number;
        }

        [MoonSharpHidden]
        public static Blend Create(DynValue type, DynValue frac) => new Blend(type, frac);
    }

    [MoonSharpUserData]
    class Brush {
        [MoonSharpHidden]
        public Rgba32 icolor { get; set; }

        public uint color {
            get => icolor.Rgba;
            set => icolor = new Rgba32(value);
        }

        public Brush dup() => new Brush { icolor = icolor };

        [MoonSharpHidden]
        public Brush() { /* Default constructor */ }

        [MoonSharpHidden]
        public Brush(DynValue color) {
            double fcol = color
                          .CheckType(nameof(Brush), DataType.Number, 1)
                          .Number;
            this.color = (uint)Math.Max(Math.Min(fcol, 0xFFFFFFFF), 0);
        }

        [MoonSharpHidden]
        public static Brush Create(DynValue color) => new Brush(color);
    }

    [MoonSharpUserData]
    class Pen {
        [MoonSharpHidden]
        public Rgba32 icolor { get; set; }

        public uint color {
            get => icolor.Rgba;
            set => icolor = new Rgba32(value);
        }

        public float width { get; set; } = 1f;

        public Pen dup() => new Pen { icolor = icolor, width = width };

        [MoonSharpHidden]
        public Pen() { /* Default constructor */ }

        [MoonSharpHidden]
        public Pen(DynValue color, DynValue width) {
            double fcol = color
                          .CheckType(nameof(Pen), DataType.Number, 1)
                          .Number;
            this.color = (uint)Math.Max(Math.Min(fcol, 0xFFFFFFFF), 0);

            if(width.Type == DataType.Void) return;
            this.width = (float)width
                         .CheckType(nameof(Pen), DataType.Number, 2)
                         .Number;
        }

        [MoonSharpHidden]
        public static Pen Create(DynValue color, DynValue width) => new Pen(color, width);
    }
}
