using System;
using System.Windows.Input;

namespace DevNotes.Core;

/// <summary>
/// 一个通用的命令实现，用于在 WPF 中将按钮等控件与 ViewModel 中的委托逻辑绑定。
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// 当命令的可执行状态发生变化时触发。
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// 初始化 <see cref="RelayCommand"/> 实例。
    /// </summary>
    /// <param name="execute">执行命令时调用的委托，不可为 null。</param>
    /// <param name="canExecute">用于判断命令当前是否可执行的委托，可为 null 表示始终可执行。</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 判断给定参数下命令是否可以执行。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    /// <returns>当命令允许执行时返回 true，否则返回 false。</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// 执行命令绑定的逻辑。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// 主动触发 <see cref="CanExecuteChanged"/> 事件，
    /// 用于通知界面重新评估命令是否可用，从而刷新按钮的启用状态。
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

