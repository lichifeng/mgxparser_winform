using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace mgxparser
{
    internal static class Program
    {
        private const string DllName = "MgxNative.dll";
        private static string _nativeDllDir;

        [STAThread]
        static void Main()
        {
            _nativeDllDir = Path.Combine(Path.GetTempPath(), "mgxparser");
            ExtractEmbeddedDll();

            // Hook assembly resolution so the CLR can find the mixed-mode DLL
            // in the temp folder when MgxNative types are first used.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveNativeDll;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void ExtractEmbeddedDll()
        {
            Directory.CreateDirectory(_nativeDllDir);
            string dllPath = Path.Combine(_nativeDllDir, DllName);
            string resourceName = "mgxparser." + DllName;

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.WriteLine($"Embedded resource '{resourceName}' not found.");
                    return;
                }

                if (File.Exists(dllPath) && stream.Length == new FileInfo(dllPath).Length)
                    return; // already extracted and up-to-date

                stream.Seek(0, SeekOrigin.Begin);
                using (var fs = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
                    stream.CopyTo(fs);
            }
        }

        private static Assembly ResolveNativeDll(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("MgxNative"))
            {
                string path = Path.Combine(_nativeDllDir, DllName);
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }
            return null;
        }
    }
}
