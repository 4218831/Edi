namespace Edi.ViewModel
{
  using System;
  using System.IO;
  using System.Windows.Media.Imaging;

  public class FileStatsViewModel : Base.ToolViewModel
  {
    public FileStatsViewModel()
      : base("File Stats")
    {
      ApplicationViewModel.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
      ContentId = ToolContentId;

      BitmapImage bi = new BitmapImage();
      bi.BeginInit();
      bi.UriSource = new Uri("pack://application:,,/Images/property-blue.png");
      bi.EndInit();
      IconSource = bi;
    }

    public const string ToolContentId = "FileStatsTool";

    void OnActiveDocumentChanged(object sender, EventArgs e)
    {
      if (ApplicationViewModel.This.ActiveDocument != null &&
          ApplicationViewModel.This.ActiveDocument.FilePath != null &&
          File.Exists(ApplicationViewModel.This.ActiveDocument.FilePath))
      {
        var fi = new FileInfo(ApplicationViewModel.This.ActiveDocument.FilePath);
        FileSize = fi.Length;
        LastModified = fi.LastWriteTime;
      }
      else
      {
        FileSize = 0;
        LastModified = DateTime.MinValue;
      }
    }

    #region FileSize

    private long _fileSize;
    public long FileSize
    {
      get { return _fileSize; }
      set
      {
        if (_fileSize != value)
        {
          _fileSize = value;
          RaisePropertyChanged("FileSize");
        }
      }
    }

    #endregion

    #region LastModified

    private DateTime _lastModified;
    public DateTime LastModified
    {
      get { return _lastModified; }
      set
      {
        if (_lastModified != value)
        {
          _lastModified = value;
          RaisePropertyChanged("LastModified");
        }
      }
    }

    #endregion




  }
}
