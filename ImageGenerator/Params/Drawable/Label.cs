using System;
using MoonSharp.Interpreter;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.ImageSharp.Processing.Text;
using F = SixLabors.Fonts;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    class Font {
        public string name { get; set; }

        public float size { get; set; }

        [MoonSharpHidden]
        public FontStyle istyle { get; set; }

        public string style {
            get => istyle.ToString().ToLower();
            set {
                value = value ?? default(FontStyle).ToString();

                if(!Enum.TryParse<FontStyle>(value, true, out var result))
                    throw new ScriptRuntimeException($"Value '{value}' is not valid font style");

                istyle = result;
            }
        }

        public Font dup() => new Font { name = name, size = size, istyle = istyle };

        [MoonSharpHidden]
        public Font() { /* Default constructor */ }

        [MoonSharpHidden]
        public Font(DynValue name, DynValue size, DynValue style) {
            this.name = name
                        .CheckType(nameof(Font), DataType.String, 1)
                        .String;

            this.size = (float)size
                        .CheckType(nameof(Font), DataType.Number, 2)
                        .Number;

            if(style.Type == DataType.Void) return;
            this.style = style
                         .CheckType(nameof(Font), DataType.String, 3)
                         .String;
        }

        [MoonSharpHidden]
        public static Font Create(DynValue name, DynValue size, DynValue style) => new Font(name, size, style);
    }

    [MoonSharpUserData]
    class Label : Drawable {
        public string text { get; set; }

        public Font font { get; set; }

        public Brush brush { get; set; }

        public Pen pen { get; set; }

        public float wrap { get; set; }

        [MoonSharpHidden]
        public HorizontalAlignment ihalign { get; set; }

        public string halign {
            get => ihalign.ToString().ToLower();
            set {
                value = value ?? default(HorizontalAlignment).ToString();

                if(!Enum.TryParse<HorizontalAlignment>(value, true, out var result))
                    throw new ScriptRuntimeException($"Value '{value}' is not valid horizontal alignment");

                ihalign = result;
            }
        }

        [MoonSharpHidden]
        public VerticalAlignment ivalign { get; set; }

        public string valign {
            get => ivalign.ToString().ToLower();
            set {
                value = value ?? default(VerticalAlignment).ToString();

                if(!Enum.TryParse<VerticalAlignment>(value, true, out var result))
                    throw new ScriptRuntimeException($"Value '{value}' is not valid vertical alignment");

                ivalign = result;
            }
        }

        public Label dup() => new Label {
            pos = pos, ang = ang, blend = blend,
            text = text, font = font, brush = brush, pen = pen,
            wrap = wrap, ihalign = ihalign, ivalign = ivalign,
        };

        [MoonSharpHidden]
        public Label() { /* Default constructor */ }

        [MoonSharpHidden]
        public Label(DynValue param) : base(param) {
            var table = param
                        .CheckType(nameof(Label), DataType.Table, 1)
                        .Table;

            this.text = table
                        .Get(nameof(text))
                        .CheckType(nameof(Label), DataType.String)
                        .String;

            this.font = table
                        .Get(nameof(font))
                        .CheckUserDataType<Font>(nameof(Label));

            this.brush = table
                         .Get(nameof(brush))
                         .CheckUserDataType<Brush>(nameof(Label),
                             flags: TypeValidationFlags.AllowNil);

            this.pen = table
                       .Get(nameof(pen))
                       .CheckUserDataType<Pen>(nameof(Label),
                           flags: TypeValidationFlags.AllowNil);

            this.wrap = (float)table
                        .Get(nameof(wrap))
                        .CheckType(nameof(Label), DataType.Number,
                            flags: TypeValidationFlags.AllowNil | TypeValidationFlags.AutoConvert)
                        .Number;

            this.halign = table
                          .Get(nameof(halign))
                          .CheckType(nameof(Label), DataType.String,
                              flags: TypeValidationFlags.AllowNil | TypeValidationFlags.AutoConvert)
                          .String;

            this.valign = table
                          .Get(nameof(valign))
                          .CheckType(nameof(Label), DataType.String,
                              flags: TypeValidationFlags.AllowNil | TypeValidationFlags.AutoConvert)
                          .String;
        }

        [MoonSharpHidden]
        public static Label Create(DynValue param) => new Label(param);

        public override void Draw(Processor.Context ctx) {
            var font = new F.Font(ctx.GetFont(this.font.name), this.font.size, this.font.istyle);
            var brush = (this.brush != null) ? Brushes.Solid(this.brush.icolor) : null;
            var pen = (this.pen != null) ? Pens.Solid(this.pen.icolor, this.pen.width) : null;

            var options = new TextGraphicsOptions(true) {
                ApplyKerning = true,
                WrapTextWidth = this.wrap,
                HorizontalAlignment = this.ihalign,
                VerticalAlignment = this.ivalign,
            };

            if(this.blend != null) {
                options.BlenderMode = this.blend.itype;
                options.BlendPercentage = this.blend.fraction;
            }

            ctx.Target.DrawText(options, this.text, font, brush, pen, this.pos);
        }
    }
}
