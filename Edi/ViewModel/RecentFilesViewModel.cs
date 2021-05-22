namespace Edi.ViewModel
{
  using System;
  using System.IO;

  using SimpleControls.MRU.Model;
  using SimpleControls.MRU.ViewModel;

  internal class RecentFilesViewModel : Base.ToolViewModel
  {
    private MRUListVM mMruList;

    public const string ToolContentId = "RecentFilesTool";

    public RecentFilesViewModel()
      : base("Recent Files")
    {
      ////Workspace.This.ActiveDocumentChanged += new EventHandler(OnActiveDocumentChanged);
      ContentId = ToolContentId;

      this.mMruList = new MRUListVM();
    }

    public override Uri IconSource
    {
      get
      {
        return new Uri("pack://application:,,,/SimpleControls;component/MRU/Images/NoPin16.png", UriKind.RelativeOrAbsolute);
      }
    }

    public MRUListVM MruList
    {
      get
      {
        return this.mMruList;
      }

      private set
      {
        if (this.mMruList != value)
        {
          this.mMruList = value;
          this.NotifyPropertyChanged(() => this.MruList);
        }
      }
    }

    public void AddNewEntryIntoMRU(string filePath)
    {
      if (this.MruList.FindMRUEntry(filePath) == null)
      {
        MRUEntryVM e = new MRUEntryVM() { IsPinned = false, PathFileName = filePath };

        this.MruList.AddMRUEntry(e);

        this.NotifyPropertyChanged(() => this.MruList);
      }
    }
  }
}
