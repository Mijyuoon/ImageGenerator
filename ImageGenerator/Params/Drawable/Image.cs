using MoonSharp.Interpreter;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using IS = SixLabors.ImageSharp;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    class Image : Drawable {
        public string file { get; set; }

        public Vec size { get; set; }

        public Blend blend { get; set; }

        public Image dup() => new Image { file = file, pos = pos, size = size, blend = blend };

        [MoonSharpHidden]
        public Image() { /* Default constructor */ }

        [MoonSharpHidden]
        public Image(DynValue param) {
            var table = param
                        .CheckType(nameof(Image), DataType.Table, 1)
                        .Table;

            this.file = table
                        .Get(nameof(file))
                        .CheckType(nameof(Image), DataType.String)
                        .String;

            this.pos = table
                       .Get(nameof(pos))
                       .CheckUserDataType<Vec>(nameof(Image));

            this.size = table
                        .Get(nameof(size))
                        .CheckUserDataType<Vec>(nameof(Image),
                            flags: TypeValidationFlags.AllowNil);

            this.blend = table
                         .Get(nameof(blend))
                         .CheckUserDataType<Blend>(nameof(Image),
                             flags: TypeValidationFlags.AllowNil);
        }

        [MoonSharpHidden]
        public static Image Create(DynValue param) => new Image(param);

        public override void Draw(Processor.Context ctx) {
            using(var image = IS.Image.Load<Rgba32>(ctx.ExpandPath(this.file))) {
                image.Mutate(im => {
                    if(this.size != null) {
                        im.Resize(this.size, KnownResamplers.Bicubic, false);
                    }
                });

                var options = new GraphicsOptions(true);
                if(this.blend != null) {
                    options.BlenderMode = this.blend.itype;
                    options.BlendPercentage = this.blend.fraction;
                }

                ctx.Target.DrawImage(options, image, this.pos);
            }
        }
    }
}
