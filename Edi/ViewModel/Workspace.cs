namespace Edi.ViewModel
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Collections.ObjectModel;
  using System.Windows.Input;
  using Microsoft.Win32;
  using System.IO;
  using System.Windows;
  using AvalonDock.Layout;
  using Edi.ViewModel.Base;
  using Edi.Command;
  using SimpleControls.MRU.ViewModel;

  class Workspace : Base.ViewModelBase
  {
    protected Workspace()
    {

    }

    static Workspace _this = new Workspace();

    public static Workspace This
    {
      get { return _this; }
    }

    ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
    ReadOnlyObservableCollection<FileViewModel> _readonyFiles = null;
    public ReadOnlyObservableCollection<FileViewModel> Files
    {
      get
      {
        if (_readonyFiles == null)
          _readonyFiles = new ReadOnlyObservableCollection<FileViewModel>(_files);

        return _readonyFiles;
      }
    }

    ToolViewModel[] _tools = null;

    public IEnumerable<ToolViewModel> Tools
    {
      get
      {
        if (_tools == null)
          _tools = new ToolViewModel[] { this.RecentFiles, this.FileStats };
        return _tools;
      }
    }

    FileStatsViewModel _fileStats = null;
    public FileStatsViewModel FileStats
    {
      get
      {
        if (_fileStats == null)
          _fileStats = new FileStatsViewModel();

        return _fileStats;
      }
    }

    #region OpenCommand
    RelayCommand _openCommand = null;
    public ICommand OpenCommand
    {
      get
      {
        if (_openCommand == null)
        {
          _openCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
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
      // Verify whether file is already open in editor, and if so, show it
      var fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);

      if (fileViewModel != null)
      {
        this.ActiveDocument = fileViewModel; // File is already open so shiw it

        return fileViewModel;
      }

      fileViewModel = _files.FirstOrDefault(fm => fm.FilePath == filepath);
      if (fileViewModel != null)
        return fileViewModel;

      fileViewModel = new FileViewModel(filepath);
      _files.Add(fileViewModel);
      this.RecentFiles.AddNewEntryIntoMRU(filepath);

      return fileViewModel;
    }

    #endregion

    #region NewCommand
    RelayCommand _newCommand = null;
    public ICommand NewCommand
    {
      get
      {
        if (_newCommand == null)
        {
          _newCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
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
      _files.Add(new FileViewModel());
      ActiveDocument = _files.Last();
    }

    #endregion

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

      File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
      ActiveDocument.IsDirty = false;
    }

    #region Recent File List Pin Unpin Commands
    /// <summary>
    /// This property manages the data visible in the Recent Files ViewModel.
    /// </summary>
    private RecentFilesViewModel _recentFiles = null;
    public RecentFilesViewModel RecentFiles
    {
      get
      {
        if (_recentFiles == null)
          _recentFiles = new RecentFilesViewModel();

        return _recentFiles;
      }
    }

    private void PinCommand_Executed(object o, ExecutedRoutedEventArgs e)
    {
      MRUEntryVM cmdParam = o as MRUEntryVM;

      if (cmdParam == null)
        return;

      if (e != null)
        e.Handled = true;

      this.RecentFiles.MruList.PinUnpinEntry(!cmdParam.IsPinned, cmdParam);
    }

    private void AddMRUEntry_Executed(object o, ExecutedRoutedEventArgs e)
    {
      MRUEntryVM cmdParam = o as MRUEntryVM;

      if (cmdParam == null)
        return;

      if (e != null)
        e.Handled = true;

      this.RecentFiles.MruList.AddMRUEntry(cmdParam);
    }

    private void RemoveMRUEntry_Executed(object o, ExecutedRoutedEventArgs e)
    {
      MRUEntryVM cmdParam = o as MRUEntryVM;

      if (cmdParam == null)
        return;

      if (e != null)
        e.Handled = true;

      this.RecentFiles.MruList.RemovePinEntry(cmdParam);
    }
    #endregion Recent File List Pin Unpin Commands

    /// <summary>
    /// Bind a window to some commands to be executed by the viewmodel.
    /// </summary>
    /// <param name="win"></param>
    public void InitCommandBinding(Window win)
    {
      win.CommandBindings.Add(new CommandBinding(AppCommand.LoadFile,
      (s, e) =>
      {
        if (e == null)
          return;

        string filename = e.Parameter as string;

        if (filename == null)
          return;

        this.Open(filename);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.PinUnpin,
      (s, e) =>
      {
        this.PinCommand_Executed(e.Parameter, e);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.RemoveMruEntry,
      (s, e) =>
      {
        this.RemoveMRUEntry_Executed(e.Parameter, e);
      }));

      win.CommandBindings.Add(new CommandBinding(AppCommand.AddMruEntry,
      (s, e) =>
      {
        this.AddMRUEntry_Executed(e.Parameter, e);
      }));
    }
  }
}
