using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Params = ImageGenerator.Params;

namespace ImageGenerator {
    class ProcessorException : Exception {
        public ProcessorException(string message) : base(message) { }
    }

    class Processor {
        public string TemplateDir { get; set; }

        public Image<Rgba32> CurrentImage { get; private set; }

        public FontCollection FontCache { get; }

        public Processor(string filePath) {
            TemplateDir = Path.GetDirectoryName(Path.GetFullPath(filePath));
            FontCache = new FontCollection();
        }

        public void ProcessParams(Params.Main data) {
            CurrentImage?.Dispose();
            CurrentImage = Image.Load<Rgba32>(GetActualPath(data.background));

            if(data.fonts != null) {
                // Load fonts
                foreach(var font in data.fonts) {
                    FontCache.Install(GetActualPath(font));
                }
            }

            CurrentImage.Mutate(ctx => {
                foreach(var dobj in data.objects) {

                    // Draw images
                    if(dobj is Params.Image dimg) {
                        using(var image = Image.Load<Rgba32>(GetActualPath(dimg.file))) {
                            if(dimg.size != null) {
                                image.Mutate(im => im.Resize(dimg.size, KnownResamplers.Bicubic, false));
                            }

                            var options = new GraphicsOptions(true);
                            if(dimg.blend != null) {
                                options.BlenderMode = dimg.blend.itype;
                                options.BlendPercentage = dimg.blend.fraction;
                            }

                            ctx.DrawImage(options, image, dimg.pos);
                        }
                    }

                    // Draw labels
                    if(dobj is Params.Label dlbl) {
                        var font = new Font(GetFont(dlbl.font.name), dlbl.font.size, dlbl.font.istyle);
                        var brush = (dlbl.brush != null) ? Brushes.Solid(dlbl.brush.icolor) : null;
                        var pen = (dlbl.pen != null) ? Pens.Solid(dlbl.pen.icolor, dlbl.pen.width) : null;

                        var options = new TextGraphicsOptions(true) {
                            ApplyKerning = true,
                            WrapTextWidth = dlbl.wrap,
                            HorizontalAlignment = dlbl.ihalign,
                            VerticalAlignment = dlbl.ivalign,
                        };
                        if(dlbl.blend != null) {
                            options.BlenderMode = dlbl.blend.itype;
                            options.BlendPercentage = dlbl.blend.fraction;
                        }

                        ctx.DrawText(options, dlbl.text, font, brush, pen, dlbl.pos);
                    }
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

        private string GetActualPath(string path) {
            if(path.StartsWith('@')) {
                return Path.Combine(TemplateDir, path.Substring(1));
            }

            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
