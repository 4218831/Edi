namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Threading;
  using Edi.Interfaces;
  using Edi.ViewModel.Base;
  using ICSharpCode.AvalonEdit.Document;
  using Microsoft.Practices.Prism.Commands;
  using Microsoft.Practices.Prism.ViewModel;
  using Microsoft.Win32;
  using Xceed.Wpf.AvalonDock.Layout.Serialization;

  public enum ToggleEditorOption
  {
    WordWrap = 0,
    ShowLineNumber = 1,
    ShowEndOfLine = 2,
    ShowSpaces = 3,
    ShowTabs = 4
  }

  /// <summary>
  /// This class manages the viewmodel properties and methods for the entire application.
  /// </summary>
  public class ApplicationViewModel : NotificationObject, IViewModelResolver, ILayoutViewModelParent
  {
    #region fields
    private static ApplicationViewModel _this = new ApplicationViewModel();

    private bool mIsBusy = true;

    ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
    ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
    ToolViewModel[] _tools = null;
    FileStatsViewModel _fileStats = null;
    #endregion fields

    #region constructor
    /// <summary>
    /// Class constructor
    /// </summary>
    protected ApplicationViewModel()
    {
      this.ADLayout = new AvalonDockLayoutViewModel(this);
    }
    #endregion constructor

    #region properties
    public static ApplicationViewModel This
    {
      get { return _this; }
    }

    /// <summary>
    /// List all file documents managed by this viewmodel.
    /// </summary>
    public ReadOnlyObservableCollection<FileViewModel> Files
    {
      get
      {
        if (_readonyFiles == null)
          _readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

        return _readonyFiles;
      }
    }

    /// <summary>
    /// List all tool windows managed by this viewmodel.
    /// </summary>
    public IEnumerable<ToolViewModel> Tools
    {
      get
      {
        if (_tools == null)
          _tools = new ToolViewModel[] { FileStats };
        return _tools;
      }
    }

    public FileStatsViewModel FileStats
    {
      get
      {
        if (_fileStats == null)
          _fileStats = new FileStatsViewModel();

        return _fileStats;
      }
    }

    /// <summary>
    /// Get/set property to signal whether application is busy or not.
    /// 
    /// The property set method invokes a CommandManager.InvalidateRequerySuggested()
    /// if the new value is different from the old one. The mouse cursor is set to Wait
    /// if the application is busy.
    /// </summary>
    public bool IsBusy
    {
      get
      {
        return this.mIsBusy;
      }

      set
      {
        if (this.mIsBusy != value)
        {
          this.mIsBusy = value;
          this.RaisePropertyChanged(() => this.IsBusy);

          CommandManager.InvalidateRequerySuggested();

          Application.Current.Dispatcher.BeginInvoke(new Action(() =>
          {
            if (value == false)
              Mouse.OverrideCursor = null;
            else
              Mouse.OverrideCursor = Cursors.Wait;
          }),
            DispatcherPriority.Background);
        }
      }
    }

    #region ActiveDocument

    private FileViewModel _activeDocument = null;
    public FileViewModel ActiveDocument
    {
      get { return _activeDocument; }
      set
      {
        if (_activeDocument != value)
        {
          _activeDocument = value;
          RaisePropertyChanged("ActiveDocument");
          if (ActiveDocumentChanged != null)
            ActiveDocumentChanged(this, EventArgs.Empty);
        }
      }
    }

    public event EventHandler ActiveDocumentChanged;

    #endregion

    #region Application Properties
    /// <summary>
    /// Get a path to the directory where the application
    /// can persist/load user data on session exit and re-start.
    /// </summary>
    public string DirAppData
    {
      get
      {
        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         System.IO.Path.DirectorySeparatorChar + this.Company;

        try
        {
          if (System.IO.Directory.Exists(dirPath) == false)
            System.IO.Directory.CreateDirectory(dirPath);
        }
        catch
        {
        }

        return dirPath;
      }
    }

    public string Company
    {
      get
      {
        return "EdiDemo";
      }
    }
    #endregion Application Properties

    #region commands
    #region OpenCommand
    DelegateCommand<object> _openCommand = null;
    public ICommand OpenCommand
    {
      get
      {
        if (_openCommand == null)
        {
          _openCommand = new DelegateCommand<object>((p) => OnOpen(p), (p) => CanOpen(p));
        }

        return _openCommand;
      }
    }

    private bool CanOpen(object parameter)
    {
      return true;
    }

    private void OnOpen(object parameter)
    {
      var dlg = new OpenFileDialog();
      if (dlg.ShowDialog().GetValueOrDefault())
      {
        var fileViewModel = Open(dlg.FileName);
        ActiveDocument = fileViewModel;
      }
    }

    public FileViewModel Open(string filepath)
    {
      var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
      if (fileViewModel != null)
        return fileViewModel;

      if (System.IO.File.Exists(filepath) == false)
        return null;

      fileViewModel = new FileViewModel(filepath);
      _files.Add(fileViewModel);

      return fileViewModel;
    }

    #endregion

    #region NewCommand
    DelegateCommand<object> _newCommand = null;
    public ICommand NewCommand
    {
      get
      {
        if (_newCommand == null)
        {
          _newCommand = new DelegateCommand<object>((p) => OnNew(p), (p) => CanNew(p));
        }

        return _newCommand;
      }
    }

    private bool CanNew(object parameter)
    {
      return true;
    }

    private void OnNew(object parameter)
    {
      _files.Add(new FileViewModel() { Document = new TextDocument() });
      ActiveDocument = _files.Last();
    }

    #endregion

    #region Workspace Managment
    /// <summary>
    /// Expose command to load/save AvalonDock layout on application startup and shut-down.
    /// </summary>
    public AvalonDockLayoutViewModel ADLayout  { get; private set; }
    #endregion Workspace Managment
    #endregion commands
    #endregion properties

    #region close save file handling methods
    internal void Close(FileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
        if (res == MessageBoxResult.Cancel)
          return;
        if (res == MessageBoxResult.Yes)
        {
          Save(fileToClose);
        }
      }

      _files.Remove(fileToClose);
    }

    internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave.FilePath == null || saveAsFlag)
      {
        var dlg = new SaveFileDialog();
        if (dlg.ShowDialog().GetValueOrDefault())
          fileToSave.FilePath = dlg.SafeFileName;
      }

      File.WriteAllText(fileToSave.FilePath, fileToSave.Document.Text);
      ActiveDocument.IsDirty = false;
    }
    #endregion close save file handling methods

    #region ToggleEditorOptionCommand
    DelegateCommand<object> _toggleEditorOptionCommand = null;
    public ICommand ToggleEditorOptionCommand
    {
      get
      {
        if (this._toggleEditorOptionCommand == null)
        {
          this._toggleEditorOptionCommand = new DelegateCommand<object>((p) => OnToggleEditorOption(p),
                                                                     (p) => CanToggleEditorOption(p));
        }

        return this._toggleEditorOptionCommand;
      }
    }

    private bool CanToggleEditorOption(object parameter)
    {
      if (this.ActiveDocument != null)
        return true;

      return false;
    }

    private void OnToggleEditorOption(object parameter)
    {
      FileViewModel f = this.ActiveDocument;

      if (parameter == null)
        return;

      if ((parameter is ToggleEditorOption) == false)
        return;

      ToggleEditorOption t = (ToggleEditorOption)parameter;

      if (f != null)
      {
        switch (t)
        {
          case ToggleEditorOption.WordWrap:
              f.WordWrap = !f.WordWrap;
            break;

          case ToggleEditorOption.ShowLineNumber:
              f.ShowLineNumbers = !f.ShowLineNumbers;
            break;

          case ToggleEditorOption.ShowSpaces:
            f.TextOptions.ShowSpaces = !f.TextOptions.ShowSpaces;
            break;

          case ToggleEditorOption.ShowTabs:
            f.TextOptions.ShowTabs = !f.TextOptions.ShowTabs;
            break;

          case ToggleEditorOption.ShowEndOfLine:
            f.TextOptions.ShowEndOfLine = !f.TextOptions.ShowEndOfLine;
            break;

          default:
            break;
        }
      }
    }
    #endregion ToggleEditorOptionCommand

    #region Workspace Managment Methods
    /// <summary>
    /// Translate a string ID into a matching viewmodel instance (tool window or document)
    /// that is managed by this class.
    /// </summary>
    /// <param name="content_id"></param>
    /// <returns>viewmodel instance or null (if match was not succesful)</returns>
    NotificationObject Edi.Interfaces.IViewModelResolver.ContentViewModelFromID(string content_id)
    {
      // Query for a tool window and return it
      var anchorable_vm = this.Tools.FirstOrDefault(d => d.ContentId == content_id);
      if (anchorable_vm != null)
        return anchorable_vm;

      // Query for a matching document and return it
      return this.Open(content_id);
    }

    /// <summary>
    /// Method is called via interface from <seealso cref="AvalonDockLayoutViewModel"/>
    /// when the application loads layout.
    /// </summary>
    /// <param name="args"></param>
    public void ReloadContentOnStartUp(LayoutSerializationCallbackEventArgs args)
    {
      string sId = args.Model.ContentId;

      // Empty Ids are invalid but possible if aaplication is closed with File>New without edits.
      if (string.IsNullOrWhiteSpace(sId) == true)
      {
        args.Cancel = true;
        return;
      }

      if (args.Model.ContentId == FileStatsViewModel.ToolContentId)
        args.Content = this.FileStats;
      else
      {
        args.Content = this.ReloadDocument(args.Model.ContentId);

        if (args.Content == null)
          args.Cancel = true;
      }
    }

    private object ReloadDocument(string path)
    {
      object ret = null;

      if (!string.IsNullOrWhiteSpace(path))
      {
        switch (path)
        {
          /***
                    case StartPageViewModel.StartPageContentId: // Re-create start page content
                      if (Workspace.This.GetStartPage(false) == null)
                      {
                        ret = Workspace.This.GetStartPage(true);
                      }
                      break;
          ***/
          default:
            // Re-create text document
            ret = this.Open(path);
            break;
        }
      }

      return ret;
    }
    #endregion Workspace Managment Methods
  }
}
