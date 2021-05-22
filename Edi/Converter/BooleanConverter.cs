namespace Edi.Converter
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Windows.Data;

  /// <summary>
  /// Base class for conversion boolean values into other data typed values
  /// (visibility, NOT visibility etc).
  /// 
  /// Source: http://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter
  /// </summary>
  /// <typeparam name="T"></typeparam>
  
  public class BooleanConverter<T> : IValueConverter
  {
    public BooleanConverter(T trueValue, T falseValue)
    {
      True = trueValue;
      False = falseValue;
    }

    public T True { get; set; }
    public T False { get; set; }

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool && ((bool)value) ? True : False;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
    }
  }
}
