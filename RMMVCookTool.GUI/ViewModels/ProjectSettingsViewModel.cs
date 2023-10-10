using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;

namespace RMMVCookTool.GUI.ViewModels;
internal sealed class ProjectSettingsViewModel : BindableBase, IDialogAware
{
    private string fileExtension;
    private bool removeSource;
    private bool compressFiles;
    private bool removeAfterCompression;
    private int compressionLevel;

    public string Title { get; private set; }
    public string FileExtension { get => fileExtension; set => SetProperty(ref fileExtension, value); }
    public bool RemoveSource { get => removeSource; set => SetProperty(ref removeSource, value); }
    public bool CompressFiles { get => compressFiles; set => SetProperty(ref compressFiles, value); }
    public bool RemoveAfterCompression { get => removeAfterCompression; set => SetProperty(ref removeAfterCompression, value); }
    public int CompressionLevel { get => compressionLevel; set => SetProperty(ref compressionLevel, value); }
    
    public DelegateCommand SaveCommand { get; private set; }
    public DelegateCommand CancelCommand { get; private set; }

    public DialogCloseListener RequestClose { get; }

    public bool CanCloseDialog() => true;

    public ProjectSettingsViewModel() {
        SaveCommand = new(SaveSettings);
        CancelCommand = new(CancelSettings);
    }
    public void OnDialogClosed()
    {
        //Method intentionally left blank;
    }
    public void OnDialogOpened(IDialogParameters parameters)
    {
        Title = parameters.GetValue<string>("title");
        FileExtension = parameters.GetValue<string>("fileExtension");
        RemoveSource = parameters.GetValue<bool>("removeSource");
        CompressFiles = parameters.GetValue<bool>("compressFiles");
        RemoveAfterCompression = parameters.GetValue<bool>("removeAfterCompression");
        CompressionLevel = parameters.GetValue<int>("compressionLevel");
    }

    private void SaveSettings()
    {
        ButtonResult button = ButtonResult.OK;
        DialogResult result = new DialogResult(button);
        result.Parameters.Add("fileExtension", FileExtension);
        result.Parameters.Add("removeSource", RemoveSource);
        result.Parameters.Add("compressFiles", CompressFiles);
        result.Parameters.Add("removeAfterCompression", RemoveAfterCompression);
        result.Parameters.Add("compressionLevel", CompressionLevel);
        RequestClose.Invoke(result);
    }

    private void CancelSettings()
    {
        ButtonResult button = ButtonResult.Cancel;
        DialogResult result = new DialogResult(button);
        RequestClose.Invoke(result);
    }
}
