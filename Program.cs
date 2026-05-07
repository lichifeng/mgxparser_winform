using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace mgxparser
{
    internal static class Program
    {
        private const string NativeDllName = "MgxNative.dll";

        [STAThread]
        static void Main()
        {
            ExtractEmbeddedDll();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void ExtractEmbeddedDll()
        {
            string dllPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, NativeDllName);

            if (File.Exists(dllPath))
            {
                // Already extracted, verify it's loadable
                return;
            }

            string resourceName = "mgxparser." + NativeDllName;
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.WriteLine($"Embedded resource '{resourceName}' not found.");
                    return;
                }

                using (var fs = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }
            }
        }
    }
}
