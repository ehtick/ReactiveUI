﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI;

/// <summary>
/// Encapsulates a composite user interaction.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the bulk of the actual implementation for combined reactive commands. You should not
/// create instances of this class directly, but rather via the static creation methods on the non-generic
/// <see cref="ReactiveCommand"/> class.
/// </para>
/// <para>
/// A <c>CombinedReactiveCommand</c> combines multiple reactive commands into a single command. Executing
/// the combined command executes all child commands. Since all child commands will receive the same execution
/// parameter, all child commands must accept a parameter of the same type.
/// </para>
/// <para>
/// In order for the combined command to be executable, all child commands must themselves be executable.
/// In addition, any <c>canExecute</c> observable passed in during construction must also yield <c>true</c>.
/// </para>
/// </remarks>
/// <typeparam name="TParam">
/// The type of parameter values passed in during command execution.
/// </typeparam>
/// <typeparam name="TResult">
/// The type of the values that are the result of command execution.
/// </typeparam>
#if NET6_0_OR_GREATER
[RequiresDynamicCode("The method uses reflection and will not work in AOT environments.")]
[RequiresUnreferencedCode("The method uses reflection and will not work in AOT environments.")]
#endif
public class CombinedReactiveCommand<TParam, TResult> : ReactiveCommandBase<TParam, IList<TResult>>
{
    private readonly ReactiveCommand<TParam, IList<TResult>> _innerCommand;
    private readonly ScheduledSubject<Exception> _exceptions;
    private readonly IDisposable _exceptionsSubscription;
    private readonly IScheduler _outputScheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinedReactiveCommand{TParam, TResult}"/> class.
    /// </summary>
    /// <param name="childCommands">The child commands which will be executed.</param>
    /// <param name="canExecute">A observable when the command can be executed.</param>
    /// <param name="outputScheduler">The scheduler where to dispatch the output from the command.</param>
    /// <exception cref="ArgumentNullException">Fires when required arguments are null.</exception>
    /// <exception cref="ArgumentException">Fires if the child commands container is empty.</exception>
    protected internal CombinedReactiveCommand(
        IEnumerable<ReactiveCommandBase<TParam, TResult>> childCommands,
        IObservable<bool>? canExecute,
        IScheduler? outputScheduler = null)
    {
        childCommands.ArgumentNullExceptionThrowIfNull(nameof(childCommands));

        _outputScheduler = outputScheduler ?? RxApp.MainThreadScheduler;

        var childCommandsArray = childCommands.ToArray();

        if (childCommandsArray.Length == 0)
        {
            throw new ArgumentException("No child commands provided.", nameof(childCommands));
        }

        _exceptions = new ScheduledSubject<Exception>(_outputScheduler, RxApp.DefaultExceptionHandler);

        var canChildrenExecute = childCommandsArray.Select(x => x.CanExecute)
                                                   .CombineLatest()
                                                   .Select(x => x.All(y => y));
        var combinedCanExecute = (canExecute ?? Observables.True)
                                 .Catch<bool, Exception>(ex =>
                                 {
                                     _exceptions.OnNext(ex);
                                     return Observables.False;
                                 })
                                 .StartWith(false)
                                 .CombineLatest(canChildrenExecute, (ce, cce) => ce && cce)
                                 .DistinctUntilChanged()
                                 .Replay(1)
                                 .RefCount();

        _exceptionsSubscription = childCommandsArray.Select(x => x.ThrownExceptions)
                                                    .Merge()
                                                    .Subscribe(ex => _exceptions.OnNext(ex));

        _innerCommand = new ReactiveCommand<TParam, IList<TResult>>(
                                                                    param =>
                                                                        childCommandsArray
                                                                            .Select(x => x.Execute(param))
                                                                            .CombineLatest(),
                                                                    combinedCanExecute,
                                                                    _outputScheduler);

        // we already handle exceptions on individual child commands above, but the same exception
        // will tick through innerCommand. Therefore, we need to ensure we ignore it or the default
        // handler will execute and the process will be torn down
        _innerCommand
            .ThrownExceptions
            .Subscribe();

        CanExecute.Subscribe(OnCanExecuteChanged);
    }

    /// <inheritdoc/>
    public override IObservable<bool> CanExecute => _innerCommand.CanExecute;

    /// <inheritdoc/>
    public override IObservable<bool> IsExecuting => _innerCommand.IsExecuting;

    /// <inheritdoc/>
    public override IObservable<Exception> ThrownExceptions => _exceptions;

    /// <inheritdoc/>
    public override IDisposable Subscribe(IObserver<IList<TResult>> observer) => _innerCommand.Subscribe(observer);

    /// <inheritdoc/>
    public override IObservable<IList<TResult>> Execute(TParam parameter) => _innerCommand.Execute(parameter);

    /// <inheritdoc/>
    public override IObservable<IList<TResult>> Execute() => _innerCommand.Execute();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerCommand.Dispose();
            _exceptions.Dispose();
            _exceptionsSubscription.Dispose();
        }
    }
}
