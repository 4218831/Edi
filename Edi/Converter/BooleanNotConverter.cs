namespace Edi.Converter
{
  using System.Windows;

  /// <summary>
  /// WPF Converter class to convert boolean values into visibility values.
  /// </summary>
  public sealed class BooleanNotConverter : BooleanConverter<bool>
  {
    public BooleanNotConverter() :
      base(false, true) { }
  }
}
