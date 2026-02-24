using LoanSystem.WinForms.Forms;

namespace LoanSystem.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var services = new AppServices();
        Application.Run(new LoginForm(services));
    }
}
