namespace Edi.View
{
  using System.IO;
  using System.Windows;
  using Edi.ViewModel;
  using GalaSoft.MvvmLight;
  using GalaSoft.MvvmLight.Messaging;
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

      // Register this private method to receive all MVVM Light notifications of the NotificationMessageAction<string> type
      Messenger.Default.Register<NotificationMessageAction<string>>(this, notication_message_action_recieved);

      // Register this private method to receive all MVVM Light notifications of the NotificationMessage<string> type
      Messenger.Default.Register<NotificationMessage<string>>(this, notification_message_string_received);
    }
    #endregion constructor

    #region Workspace Layout Management
    /// <summary>
    /// Is executed when MVVM Light sends a <seealso cref="NotificationMessageAction"/> notification
    /// that was initiallized by a third party (viewmodel).
    /// </summary>
    /// <param name="message"></param>
    private void notication_message_action_recieved(NotificationMessageAction<string> message)
    {
      // Is this a save workspace layout notification?
      if (message.Notification == Notifications.GetWorkspaceLayout)
      {
        string xmlLayoutString = string.Empty;

        using (StringWriter fs = new StringWriter())
        {
          XmlLayoutSerializer xmlLayout = new XmlLayoutSerializer(this.dockManager);

          xmlLayout.Serialize(fs);

          xmlLayoutString = fs.ToString();
        }

        message.Execute(xmlLayoutString);
      }
    }

    /// <summary>
    /// Is executed when MVVM Light sends a <seealso cref="NotificationMessage"/> notification
    /// via a sender which could be a viewmodel that wants to receive the load the WorkspaceLayout.
    /// </summary>
    /// <param name="message"></param>
    private void notification_message_string_received(NotificationMessage<string> message)
    {
      // Is this a load workspace layout notification?
      if (message.Notification == Notifications.LoadWorkspaceLayout)
      {
        StringReader sr = new StringReader(message.Content);

        var layoutSerializer = new XmlLayoutSerializer(dockManager);
        layoutSerializer.LayoutSerializationCallback += UpdateLayout;
        layoutSerializer.Deserialize(sr);
      }
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
      ViewModelBase content_view_model = resolver.ContentViewModelFromID(args.Model.ContentId);

      if (content_view_model == null)
        args.Cancel = true;

      // found a match - return it
      args.Content = content_view_model;
    }
    #endregion Workspace Layout Management
  }
}
