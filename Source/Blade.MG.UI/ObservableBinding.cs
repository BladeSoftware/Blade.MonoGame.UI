#nullable enable

using Blade.MG.UI;
using System.ComponentModel;

public class ObservableBinding<T> : Binding<T>, IDisposable
{
    private readonly INotifyPropertyChanged _source;
    private readonly string _propertyName;
    private T _cachedValue = default!;
    private bool _isDirty = true;
    private bool _disposed;

    public ObservableBinding(INotifyPropertyChanged source, string propertyName, Func<T> getter, Action<T>? setter = null)
        : base(getter, setter)
    {
        _source = source;
        _propertyName = propertyName;
        _source.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName || string.IsNullOrEmpty(e.PropertyName))
            _isDirty = true;
    }

    protected override T GetValue()
    {
        if (_isDirty)
        {
            _cachedValue = base.GetValue();
            _isDirty = false;
        }
        return _cachedValue;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _source.PropertyChanged -= OnPropertyChanged;
        _disposed = true;
    }
}