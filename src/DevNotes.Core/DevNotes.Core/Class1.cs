using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevNotes.Core;

/// <summary>
/// 提供实现 <see cref="INotifyPropertyChanged"/> 的基础类，
/// 供 ViewModel 或需要属性变更通知的类型继承使用。
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// 当某个可绑定属性的值发生变化时触发，用于通知 WPF 界面更新。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发 <see cref="PropertyChanged"/> 事件。
    /// 一般在属性的 setter 中调用。
    /// </summary>
    /// <param name="propertyName">属性名称，默认使用调用方成员名。</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 设置字段的新值，并在发生实际变化时触发属性变更通知。
    /// </summary>
    /// <typeparam name="T">字段类型。</typeparam>
    /// <param name="field">要更新的后备字段引用。</param>
    /// <param name="value">新值。</param>
    /// <param name="propertyName">属性名称，默认使用调用方成员名。</param>
    /// <returns>如果字段值发生了变化并触发通知，则返回 true；否则返回 false。</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
