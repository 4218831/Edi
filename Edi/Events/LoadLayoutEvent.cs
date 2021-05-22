namespace Edi.Events
{
  using Microsoft.Practices.Composite.Events;
  using Microsoft.Practices.Composite.Presentation.Events;

  /// <summary>
  /// Class implements a simple PRISM LoadLayout string event
  /// </summary>
  public class LoadLayoutEvent : CompositePresentationEvent<string>
  {
    private static readonly EventAggregator _eventAggregator;
    private static readonly LoadLayoutEvent _event;

    static LoadLayoutEvent()
    {
      _eventAggregator = new EventAggregator();
      _event = _eventAggregator.GetEvent<LoadLayoutEvent>();
    }

    public static LoadLayoutEvent Instance
    {
      get { return _event; }
    }
  }
}
