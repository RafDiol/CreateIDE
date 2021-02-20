using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using Path = System.IO.Path;
using System.Runtime.InteropServices;
using System.Threading;
using Settings = YourIDE.Properties.Settings;
using System.Diagnostics;

namespace YourIDE
{
    class Compiler
    {
        static string exePath = null;
        public void CompileToExe(string outputPath, string projectPath ,string[] references,
            string outfilename, string sourceFile, int WarningLevel, bool TreatWarningsAsErrors, string compilerOptions, bool IncludeDebugInfo)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            // Generate an executable instead of
            // a class library.
            cp.GenerateExecutable = true;

            // Set the assembly file name to generate.
            exePath = cp.OutputAssembly = Path.Combine(outputPath, outfilename+".exe");

            // Generate debug information.
            cp.IncludeDebugInformation = IncludeDebugInfo;

            // Add all the assembly references.
            foreach (string item in references)
            {
                cp.ReferencedAssemblies.Add(item + ".dll");
            }

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            // Set the level at which the compiler
            // should start displaying warnings.
            cp.WarningLevel = WarningLevel;

            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = TreatWarningsAsErrors;

            // Set compiler argument to optimize output.
            cp.CompilerOptions = compilerOptions;

            // Set a temporary files collection.
            // The TempFileCollection stores the temporary files
            // generated during a build in the current directory,
            // and does not delete them after compilation.
            cp.TempFiles = new TempFileCollection(".", false);


            if (Directory.Exists("Resources"))
            {
                if (provider.Supports(GeneratorSupport.Resources))
                {
                    // Set the embedded resource file of the assembly.
                    // This is useful for culture-neutral resources,
                    // or default (fallback) resources.
                    cp.EmbeddedResources.Add("Resources\\Default.resources");

                    // Set the linked resource reference files of the assembly.
                    // These resources are included in separate assembly files,
                    // typically localized for a specific language and culture.
                    cp.LinkedResources.Add("Resources\\nb-no.resources");
                }
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Invoke compilation.
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

            ShowErrors(cr.Errors.Count, cr);
        }

        public void CompileToDLL(string outputPath, string projectPath, string[] references,
            string outfilename, string sourceFile, int WarningLevel, bool TreatWarningsAsErrors, string compilerOptions, bool IncludeDebugInfo)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            // Generate a class library or more commonly known as DLL.
            cp.GenerateExecutable = false;

            // Set the assembly file name to generate.
            cp.OutputAssembly = Path.Combine(outputPath, outfilename+".dll");

            // Generate debug information.
            cp.IncludeDebugInformation = IncludeDebugInfo;

            // Add all the assembly references.
            foreach (string item in references)
            {
                cp.ReferencedAssemblies.Add(item + ".dll");
            }

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            // Set the level at which the compiler
            // should start displaying warnings.
            cp.WarningLevel = WarningLevel;

            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = TreatWarningsAsErrors;

            // Set compiler argument to optimize output.
            cp.CompilerOptions = compilerOptions;

            // Set a temporary files collection.
            // The TempFileCollection stores the temporary files
            // generated during a build in the current directory,
            // and does not delete them after compilation.
            cp.TempFiles = new TempFileCollection(".", false);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Invoke compilation.
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

            ShowErrors(cr.Errors.Count, cr);
        }
        
        private static void ShowErrors(int count, CompilerResults cr)
        {
            Log log = new Log();
            log.Show();
            if (count > 0)
            {
                foreach (CompilerError ce in cr.Errors)
                {
                    log.Error(ce.ToString());
                }
            }
            else
            {
                log.Success("Project Compiled Successfully...");
                if (Settings.Default.autoRun == true && Settings.Default.compileOption == 0)
                {
                    try
                    {
                        log.Success("\nStarting App...");
                        Process.Start(exePath);
                        log.Success("\nApp Started...");
                    }
                    catch
                    {
                        log.Error("Failed To Launch App...");
                    }
                }
            }
        }
    }
}
