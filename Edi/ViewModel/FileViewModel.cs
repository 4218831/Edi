﻿namespace Edi.ViewModel
{
  using System.IO;
  using System.Text;
  using System.Windows.Input;
  using System.Windows.Media;
  using GalaSoft.MvvmLight.Command;
  using ICSharpCode.AvalonEdit.Document;
  using ICSharpCode.AvalonEdit.Highlighting;
  using ICSharpCode.AvalonEdit.Utils;

  /// <summary>
  /// Class to manage viewmodel properties and methods for a text (file) editor.
  /// </summary>
  public class FileViewModel : PaneViewModel
  {
    #region fields
    static ImageSourceConverter ISC = new ImageSourceConverter();
    #endregion fields

    #region constructor
    public FileViewModel(string filePath)
    {
      FilePath = filePath;
      Title = FileName;

      //Set the icon only for open documents (just a test)
      IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;
    }

    public FileViewModel()
    {
      IsDirty = true;
      Title = FileName;
    }
    #endregion constructor

    #region FilePath
    private string _filePath = null;
    public string FilePath
    {
      get { return _filePath; }
      set
      {
        if (_filePath != value)
        {
          _filePath = value;
          RaisePropertyChanged("FilePath");
          RaisePropertyChanged("FileName");
          RaisePropertyChanged("Title");

          if (File.Exists(this._filePath))
          {
            this._document = new TextDocument();
            this.HighlightDef = HighlightingManager.Instance.GetDefinition("XML");
            this._isDirty = false;
            this.IsReadOnly = false;
            this.ShowLineNumbers = false;
            this.WordWrap = false;

            // Check file attributes and set to read-only if file attributes indicate that
            if ((System.IO.File.GetAttributes(this._filePath) & FileAttributes.ReadOnly) != 0)
            {
              this.IsReadOnly = true;
              this.IsReadOnlyReason = "This file cannot be edit because another process is currently writting to it.\n" +
                                      "Change the file access permissions or save the file in a different location if you want to edit it.";
            }

            using (FileStream fs = new FileStream(this._filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
              using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
              {
                this._document = new TextDocument(reader.ReadToEnd());
              }
            }

            ContentId = _filePath;
          }
        }
      }
    }
    #endregion

    #region FileName
    public string FileName
    {
      get
      {
        if (FilePath == null)
          return "Noname" + (IsDirty ? "*" : "");

        return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
      }
    }
    #endregion FileName

    #region TextContent

    private TextDocument _document = null;
    public TextDocument Document
    {
      get { return this._document; }
      set
      {
        if (this._document != value)
        {
          this._document = value;
          RaisePropertyChanged("Document");
          IsDirty = true;
        }
      }
    }

    #endregion

    #region HighlightingDefinition

    private IHighlightingDefinition _highlightdef = null;
    public IHighlightingDefinition HighlightDef
    {
      get { return this._highlightdef; }
      set
      {
        if (this._highlightdef != value)
        {
          this._highlightdef = value;
          RaisePropertyChanged("HighlightDef");
          IsDirty = true;
        }
      }
    }

    #endregion

    #region Title
    /// <summary>
    /// Title is the string that is usually displayed - with or without dirty mark '*' - in the docking environment
    /// </summary>
    new public string Title
    {
      get
      {
        return System.IO.Path.GetFileName(this.FilePath) + (this.IsDirty == true ? "*" : string.Empty);
      }

      set
      {
        base.Title = value;
      }
    }
    #endregion

    #region IsDirty

    private bool _isDirty = false;
    public bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        if (_isDirty != value)
        {
          _isDirty = value;
          RaisePropertyChanged("IsDirty");
          RaisePropertyChanged("Title");
          RaisePropertyChanged("FileName");
        }
      }
    }

    #endregion

    #region IsReadOnly
    private bool mIsReadOnly = false;
    public bool IsReadOnly
    {
      get
      {
        return this.mIsReadOnly;
      }

      protected set
      {
        if (this.mIsReadOnly != value)
        {
          this.mIsReadOnly = value;
          this.RaisePropertyChanged("IsReadOnly");
        }
      }
    }

    private string mIsReadOnlyReason = string.Empty;
    public string IsReadOnlyReason
    {
      get
      {
        return this.mIsReadOnlyReason;
      }

      protected set
      {
        if (this.mIsReadOnlyReason != value)
        {
          this.mIsReadOnlyReason = value;
          this.RaisePropertyChanged("IsReadOnlyReason");
        }
      }
    }
    #endregion IsReadOnly

    #region WordWrap
    // Toggle state WordWrap
    private bool mWordWrap = false;
    public bool WordWrap
    {
      get
      {
        return this.mWordWrap;
      }

      set
      {
        if (this.mWordWrap != value)
        {
          this.mWordWrap = value;
          this.RaisePropertyChanged("WordWrap");
        }
      }
    }
    #endregion WordWrap

    #region ShowLineNumbers
    // Toggle state ShowLineNumbers
    private bool mShowLineNumbers = false;
    public bool ShowLineNumbers
    {
      get
      {
        return this.mShowLineNumbers;
      }

      set
      {
        if (this.mShowLineNumbers != value)
        {
          this.mShowLineNumbers = value;
          this.RaisePropertyChanged("ShowLineNumbers");
        }
      }
    }
    #endregion ShowLineNumbers

    #region TextEditorOptions
    private ICSharpCode.AvalonEdit.TextEditorOptions mTextOptions
      = new ICSharpCode.AvalonEdit.TextEditorOptions()
      {
        ConvertTabsToSpaces= false,
        IndentationSize = 2
      };

    public ICSharpCode.AvalonEdit.TextEditorOptions TextOptions
    {
      get
      {
        return this.mTextOptions;
      }

      set
      {
        if (this.mTextOptions != value)
        {
          this.mTextOptions = value;
          this.RaisePropertyChanged("TextOptions");
        }
      }
    }
    #endregion TextEditorOptions

    #region SaveCommand
    RelayCommand<object> _saveCommand = null;
    public ICommand SaveCommand
    {
      get
      {
        if (_saveCommand == null)
        {
          _saveCommand = new RelayCommand<object>((p) => OnSave(p), (p) => CanSave(p));
        }

        return _saveCommand;
      }
    }

    private bool CanSave(object parameter)
    {
      return IsDirty;
    }

    private void OnSave(object parameter)
    {
      ApplicationViewModel.This.Save(this, false);
    }

    #endregion

    #region SaveAsCommand
    RelayCommand<object> _saveAsCommand = null;
    public ICommand SaveAsCommand
    {
      get
      {
        if (_saveAsCommand == null)
        {
          _saveAsCommand = new RelayCommand<object>((p) => OnSaveAs(p), (p) => CanSaveAs(p));
        }

        return _saveAsCommand;
      }
    }

    private bool CanSaveAs(object parameter)
    {
      return IsDirty;
    }

    private void OnSaveAs(object parameter)
    {
      ApplicationViewModel.This.Save(this, true);
    }

    #endregion

    #region CloseCommand
    RelayCommand<object> _closeCommand = null;
    public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new RelayCommand<object>((p) => OnClose(), (p) => CanClose());
        }

        return _closeCommand;
      }
    }

    private bool CanClose()
    {
      return true;
    }

    private void OnClose()
    {
      ApplicationViewModel.This.Close(this);
    }
    #endregion
  }
}
