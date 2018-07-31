﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    class Vec {
        public float x { get; set; }
        public float y { get; set; }

        public Vec dup() => new Vec { x = this.x, y = this.y };

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
    class Image {
        public string file { get; set; }

        public Vec pos { get; set; }

        public Vec size { get; set; }

        public Blend blend { get; set; }

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
    }

    [MoonSharpUserData]
    class Brush {
        [MoonSharpHidden]
        public Rgba32 icolor { get; set; }

        public uint color {
            get => icolor.Rgba;
            set => icolor = new Rgba32(value);
        }
        
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

        public float width { get; set; }

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

    [MoonSharpUserData]
    class Font {
        public string name { get; set; }

        public float size { get; set; }

        [MoonSharpHidden]
        public Font() { /* Default constructor */ }

        [MoonSharpHidden]
        public Font(DynValue name, DynValue size) {
            this.name = name
                        .CheckType(nameof(Font), DataType.String, 1)
                        .String;

            this.size = (float)size
                        .CheckType(nameof(Font), DataType.Number, 2)
                        .Number;
        }

        [MoonSharpHidden]
        public static Font Create(DynValue name, DynValue size) => new Font(name, size);
    }

    [MoonSharpUserData]
    class Label {
        public Vec pos { get; set; }

        public string text { get; set; }

        public Font font { get; set; }

        public Brush brush { get; set; }

        public Pen pen { get; set; }

        public Blend blend { get; set; }

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

        [MoonSharpHidden]
        public Label() { /* Default constructor */ }

        [MoonSharpHidden]
        public Label(DynValue param) {
            var table = param
                        .CheckType(nameof(Label), DataType.Table, 1)
                        .Table;

            this.pos = table
                       .Get(nameof(pos))
                       .CheckUserDataType<Vec>(nameof(Label));

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

            this.blend = table
                         .Get(nameof(blend))
                         .CheckUserDataType<Blend>(nameof(Image),
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
    }

    [MoonSharpUserData]
    class Main {
        public string background { get; set; }

        public List<Image> images { get; set; }

        public List<Label> labels { get; set; }

        public List<string> fonts { get; set; }

        [MoonSharpHidden]
        public Main() { /* Default constructor */ }

        [MoonSharpHidden]
        public Main(DynValue param) {
            var table = param
                        .CheckType(nameof(Main), DataType.Table, 1)
                        .Table;

            this.background = table
                              .Get(nameof(background))
                              .CheckType(nameof(Main), DataType.String)
                              .String;

            this.images = table
                          .Get(nameof(images))
                          .CheckType(nameof(Main), DataType.Table,
                              flags: TypeValidationFlags.AllowNil)
                          .Table
                          ?.GetArrayUserData<Image>(nameof(Main))
                          ?.ToList();

            this.labels = table
                          .Get(nameof(labels))
                          .CheckType(nameof(Main), DataType.Table,
                              flags: TypeValidationFlags.AllowNil)
                          .Table
                          ?.GetArrayUserData<Label>(nameof(Main))
                          ?.ToList();

            this.fonts = table
                         .Get(nameof(fonts))
                         .CheckType(nameof(Main), DataType.Table,
                             flags: TypeValidationFlags.AllowNil)
                         .Table
                         ?.GetArrayString(nameof(Main))
                         ?.ToList();
        }

        [MoonSharpHidden]
        public static Main Create(DynValue param) => new Main(param);
    }
}