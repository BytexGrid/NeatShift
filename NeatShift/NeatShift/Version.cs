namespace NeatShift
{
    public static class Version
    {
        public static string Current
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "2.0.0";
            }
        }
        public const string Copyright = "© 2024 BytexGrid";
        public const string Description = "A modern file organization tool with symbolic link support.";
    }
} 