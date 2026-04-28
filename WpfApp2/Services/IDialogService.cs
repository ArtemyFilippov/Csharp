namespace WpfApp2.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Внимание");
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}