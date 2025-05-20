using ScreentimeTracker.Core;

namespace ScreentimeTracker;

public static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MyApplicationContext());
    }

    
}