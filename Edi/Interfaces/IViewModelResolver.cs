namespace Edi.Interfaces
{
  using GalaSoft.MvvmLight;

  /// <summary>
  /// Interface to resolve string id into a
  /// matching viewmodel that represents a tool window or document.
  /// </summary>
  public interface IViewModelResolver
  {
    ViewModelBase ContentViewModelFromID(string content_id);
  }
}
