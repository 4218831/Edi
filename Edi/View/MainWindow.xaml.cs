namespace Edi.View
{
  using System.IO;
  using System.Windows;
  using Edi.Events;
  using Edi.ViewModel;
  using Microsoft.Practices.Prism.ViewModel;
  using Xceed.Wpf.AvalonDock.Layout.Serialization;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region constructor
    public MainWindow()
    {
      this.InitializeComponent();

      this.DataContext = ApplicationViewModel.This;

      // Register this private methods to receive PRISM event notifications
      LoadLayoutEvent.Instance.Subscribe(OnLoadLayout);

      SynchronousEvent<SaveLayoutEventArgs>.Instance.Subscribe(OnSaveLayout);
    }
    #endregion constructor

    #region Workspace Layout Management
    /// <summary>
    /// Is executed when PRISM sends a <seealso cref="SynchronousEvent"/> notification
    /// that was initiallized by a third party (viewmodel).
    /// </summary>
    /// <param name="param">Can be used to return a result of this event</param>
    private void OnSaveLayout(SaveLayoutEventArgs param)
    {
      string xmlLayoutString = string.Empty;

      using (StringWriter fs = new StringWriter())
      {
        XmlLayoutSerializer xmlLayout = new XmlLayoutSerializer(this.dockManager);

        xmlLayout.Serialize(fs);

        xmlLayoutString = fs.ToString();
      }

      param.XmlLayout = xmlLayoutString;
    }

    /// <summary>
    /// Is executed when PRISM sends a Xml layout string notification
    /// via a sender which could be a viewmodel that wants to receive
    /// the load the <seealso cref="LoadLayoutEvent"/>.
    /// </summary>
    /// <param name="message"></param>
    private void OnLoadLayout(string message)
    {
      StringReader sr = new StringReader(message);

      var layoutSerializer = new XmlLayoutSerializer(dockManager);
      layoutSerializer.LayoutSerializationCallback += UpdateLayout;
      layoutSerializer.Deserialize(sr);
    }

    /// <summary>
    /// Convert a Avalondock ContentId into a viewmodel instance
    /// that represents a document or tool window. The re-load of
    /// this component is cancelled if the Id cannot be resolved.
    /// 
    /// The result is (viewmodel Id or Cancel) is returned in <paramref name="args"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void UpdateLayout(object sender, LayoutSerializationCallbackEventArgs args)
    {
      var resolver = this.DataContext as Edi.Interfaces.IViewModelResolver;

      if (resolver == null)
        return;

      // Get a matching viewmodel for that view via DataContext property of this view
      NotificationObject content_view_model = resolver.ContentViewModelFromID(args.Model.ContentId);

      if (content_view_model == null)
        args.Cancel = true;

      // found a match - return it
      args.Content = content_view_model;
    }
    #endregion Workspace Layout Management
  }
}
