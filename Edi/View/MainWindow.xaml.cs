namespace Edi.View
{
  using System.IO;
  using System.Windows;
  using System.Windows.Input;
  using AvalonDock.Layout.Serialization;
  using Edi.Command;
  using Edi.ViewModel;

  /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = Workspace.This;
        }
    }
}
