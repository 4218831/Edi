namespace Edi.Interfaces
{
  using Microsoft.Practices.Prism.ViewModel;

  /// <summary>
  /// Interface to resolve string id into a
  /// matching viewmodel that represents a tool window or document.
  /// </summary>
  public interface IViewModelResolver
  {
    NotificationObject ContentViewModelFromID(string content_id);
  }
}
