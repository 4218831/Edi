namespace Edi.Converter
{
  using System.Windows;

  /// <summary>
  /// WPF Converter class to convert boolean values into visibility values.
  /// </summary>
  public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
  {
    public BooleanToVisibilityConverter() :
      base(Visibility.Visible, Visibility.Collapsed) { }
  }
}
