using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace ImageGenerator.Params {
    [MoonSharpUserData]
    class Main {
        public string background { get; set; }

        public List<Drawable> objects { get; set; }

        public List<string> fonts { get; set; }

        public List<string> images { get; set; }

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

            this.objects = table
                           .Get(nameof(objects))
                           .CheckType(nameof(Main), DataType.Table,
                               flags: TypeValidationFlags.AllowNil)
                           .Table
                           ?.GetArrayUserData<Drawable>(nameof(Main))
                           ?.ToList();

            this.fonts = table
                         .Get(nameof(fonts))
                         .CheckType(nameof(Main), DataType.Table,
                             flags: TypeValidationFlags.AllowNil)
                         .Table
                         ?.GetArrayString(nameof(Main))
                         ?.ToList();

            this.images = table
                          .Get(nameof(images))
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
