using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfEmbedWinForm
{
	class AvalonPadTemplateSelector : DataTemplateSelector
	{
		public AvalonPadTemplateSelector()
		{

		}
		//public AvalonPadTemplateSelector(IList<PadDescriptor> padDescriptors)
		//{
		//	_padCollection = padDescriptors;
		//}
		public DataTemplate StringTemplate { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is string)
			{
				//return StringTemplate;
				//DataTemplate template = new DataTemplate { DataType = typeof(string) };

				//FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
				//stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

				//FrameworkElementFactory info = new FrameworkElementFactory(typeof(TextBlock));
				//info.SetValue(TextBlock.TextProperty, "Hello!");
				//stackPanelFactory.AppendChild(info);

				//FrameworkElementFactory title = new FrameworkElementFactory(typeof(TextBlock));
				//title.SetBinding(TextBlock.TextProperty, new Binding("."));
				//stackPanelFactory.AppendChild(title);

				//template.VisualTree = stackPanelFactory;
				//return template;

				var obj = new CustomWindowsFormsHost();
				obj.Width = 200;

				DataTemplate template = new DataTemplate { DataType = typeof(string) };

				FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
				stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

				//FrameworkElementFactory info = new FrameworkElementFactory(typeof(CustomWindowsFormsHost));
				//info.SetValue(CustomWindowsFormsHost.ChildProperty, new Project.ProjectBrowserControl());
				//info.SetValue(FrameworkElement.WidthProperty, 100.0);
				//info.SetValue(FrameworkElement.HeightProperty, 100.0);
				//stackPanelFactory.AppendChild(info);

				FrameworkElementFactory control = new FrameworkElementFactory(typeof(UserControl1));
				stackPanelFactory.AppendChild(control);

				FrameworkElementFactory title = new FrameworkElementFactory(typeof(TextBlock));
				title.SetBinding(TextBlock.TextProperty, new Binding("."));
				stackPanelFactory.AppendChild(title);

				template.VisualTree = stackPanelFactory;
				return template;
			}
			//return new DataTemplate { Template = new TemplateContent() { Content = new TextBox { Text = item as string } } };
			else
			{
				//foreach (var pad in _padCollection)
				{
					//if (item.GetType() == pad.ViewModelType)
					{
						//return pad.PadContent;
					}
				}
			}
			return base.SelectTemplate(item, container);
		}
	}
}
