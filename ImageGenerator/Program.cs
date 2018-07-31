using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using MoonSharp.Interpreter;

namespace ImageGenerator {
    class ProgramOpts {
        [Option('i', Required = true, HelpText = "Template file to be processed")]
        public string InputFile { get; set; }

        [Option('o', Required = true, HelpText = "Resulting image file")]
        public string OutputFile { get; set; }

        [Value(0, MetaName = "ARGS")]
        public IEnumerable<string> Arguments { get; set; }
    }

    class Program {
        static void Main(string[] args) {
            var parser = new Parser(cfg => {
                cfg.EnableDashDash = true;
            });

            var result = parser.ParseArguments<ProgramOpts>(args);

            result.WithNotParsed(errs => {
                Console.WriteLine("Invalid command line options");
                Environment.Exit(-1);
            });

            result.WithParsed(opts => {
                try {
                    var luaContext = new LuaContext(opts.InputFile);
                    var processor = new Processor(luaContext.FilePath);

                    luaContext.SetArgsTable(opts.Arguments);
                    luaContext.Execute();

                    var param = luaContext.GetProcessorParams();
                    processor.ProcessParams(param);

                    processor.SaveImagePng(opts.OutputFile);
                } catch(InterpreterException ex) {
                    Console.Error.WriteLine($"Script error: {ex.DecoratedMessage ?? ex.Message}");
                    Environment.Exit(10);
                } catch(ProcessorException ex) {
                    Console.Error.WriteLine($"Processing error: {ex.Message}");
                    Environment.Exit(20);
                } catch(IOException ex) {
                    Console.WriteLine($"I/O error: {ex.Message}");
                    Environment.Exit(30);
                }
            });


        }
    }
}
