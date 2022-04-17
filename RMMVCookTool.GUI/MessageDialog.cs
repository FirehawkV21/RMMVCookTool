namespace RMMVCookTool.GUI;

public static class MessageDialog
{
    public static void ThrowErrorMessage(Exception e)
    {
        using (TaskDialog errorDialog = new())
        {
            errorDialog.WindowTitle = Resources.ErrorText;
            errorDialog.MainIcon = TaskDialogIcon.Error;
            errorDialog.MainInstruction = Resources.ErrorOccuredTitle;
            errorDialog.Content = e.Message;
            errorDialog.ExpandedInformation = Resources.StackTraceLine + e.StackTrace;
            errorDialog.Footer = Resources.ErrorDetailsMessage;
            errorDialog.FooterIcon = TaskDialogIcon.Information;
            TaskDialogButton okButton = new(ButtonType.Ok);
            errorDialog.Buttons.Add(okButton);
            errorDialog.ShowDialog();

        }
    }

    public static void ThrowErrorMessage(string title, string message)
    {
        using (TaskDialog errorDialog = new())
        {
            errorDialog.WindowTitle = title;
            errorDialog.MainIcon = TaskDialogIcon.Error;
            errorDialog.MainInstruction = Resources.ErrorOccuredTitle;
            errorDialog.Content = message;
            errorDialog.Footer = Resources.ErrorDetailsMessage;
            errorDialog.FooterIcon = TaskDialogIcon.Information;
            TaskDialogButton okButton = new(ButtonType.Ok);
            errorDialog.Buttons.Add(okButton);
            errorDialog.ShowDialog();

        }
    }

    public static void ThrowWarningMessage(string title, string message, string extramessage)
    {
        using TaskDialog warningDialog = new();
        warningDialog.WindowTitle = title;
        warningDialog.MainInstruction = message;
        warningDialog.Content = extramessage;
        warningDialog.MainIcon = TaskDialogIcon.Warning;
        TaskDialogButton confirmMessage = new(ButtonType.Ok);
        warningDialog.Buttons.Add(confirmMessage);
        warningDialog.ShowDialog();
    }

    public static void ThrowCompleteMessage(string message)
    {
        using (TaskDialog completeDialog = new())
        {
            completeDialog.WindowTitle = Resources.CompleteText;
            completeDialog.MainIcon = TaskDialogIcon.Information;
            completeDialog.MainInstruction = message;
            TaskDialogButton okButton = new(ButtonType.Ok);
            completeDialog.Buttons.Add(okButton);
            completeDialog.ShowDialog();
        }
    }

    public static void ThrowCompleteMessage(string message, string extramessage)
    {
        using (TaskDialog completeDialog = new())
        {
            completeDialog.WindowTitle = Resources.CompleteText;
            completeDialog.MainIcon = TaskDialogIcon.Information;
            completeDialog.MainInstruction = message;
            completeDialog.Content = extramessage;
            TaskDialogButton okButton = new(ButtonType.Ok);
            completeDialog.Buttons.Add(okButton);
            completeDialog.ShowDialog();
        }
    }
}
