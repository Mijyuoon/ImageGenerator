using System;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageGenerator {
    class ProcessorException : Exception {
        public ProcessorException(string message) : base(message) { }
    }

    class Processor {
        public class Context {
            private Processor processor;

            public IImageProcessingContext<Rgba32> Target { get; }

            public Context(Processor proc, IImageProcessingContext<Rgba32> target) {
                this.processor = proc;
                this.Target = target;
            }

            public FontFamily GetFont(string name) => processor.GetFont(name);

            public string ExpandPath(string path) => processor.ExpandPath(path);
        }

        public string TemplateDir { get; set; }

        public Image<Rgba32> CurrentImage { get; private set; }

        public FontCollection FontCache { get; }

        public Processor(string filePath) {
            TemplateDir = Path.GetDirectoryName(Path.GetFullPath(filePath));
            FontCache = new FontCollection();
        }

        public void ProcessParams(Params.Main data) {
            CurrentImage?.Dispose();
            CurrentImage = Image.Load<Rgba32>(ExpandPath(data.background));

            if(data.fonts != null) {
                // Load fonts
                foreach(var font in data.fonts) {
                    FontCache.Install(ExpandPath(font));
                }
            }

            CurrentImage.Mutate(ctx => {
                // Draw all drawable objects
                var imageCtx = new Context(this, ctx);

                foreach(var dobj in data.objects) {
                    dobj.Draw(imageCtx);
                }
            });
        }

        public void SaveImagePng(string filePath) {
            var encoder = new PngEncoder {
                PngColorType = PngColorType.RgbWithAlpha,
            };
            CurrentImage?.Save(filePath, encoder);
        }

        private FontFamily GetFont(string name) {
            FontFamily family = null;

            if(FontCache.TryFind(name, out family)) return family;
            if(SystemFonts.TryFind(name, out family)) return family;

            throw new ProcessorException($"Font family '{name}' not found");
        }

        private string ExpandPath(string path) {
            if(path.StartsWith('@')) {
                return Path.Combine(TemplateDir, path.Substring(1));
            }

            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
