namespace Varwin.Editor
{
    public static class DirectoryUtils
    {
        public static void OpenFolder(string path)
        {
#if UNITY_EDITOR_WIN
            var p = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe", 
                    WorkingDirectory = path.Replace(@"/", @"\"), 
                    UseShellExecute = false, 
                    Arguments = "/C start .",
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };
#else
            var p = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "xdg-open", 
                    WorkingDirectory = path, 
                    UseShellExecute = false, 
                    Arguments = ".",
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };
#endif
            p.Start();
        }
    }
}