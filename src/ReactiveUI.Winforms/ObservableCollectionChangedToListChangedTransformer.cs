﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Specialized;

namespace ReactiveUI.Winforms;

internal static class ObservableCollectionChangedToListChangedTransformer
{
    /// <summary>
    ///     Transforms a NotifyCollectionChangedEventArgs into zero or more ListChangedEventArgs.
    /// </summary>
    /// <param name="ea">The event args.</param>
    /// <returns>An enumerable of <see cref="ListChangedEventArgs"/>.</returns>
    internal static IEnumerable<ListChangedEventArgs> AsListChangedEventArgs(this NotifyCollectionChangedEventArgs ea)
    {
        switch (ea.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                yield return new(ListChangedType.Reset, -1);
                break;

            case NotifyCollectionChangedAction.Replace:
                yield return new(ListChangedType.ItemChanged, ea.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove when ea.OldItems is not null:
                {
                    foreach (var index in Enumerable.Range(ea.OldStartingIndex, ea.OldItems.Count))
                    {
                        yield return new(ListChangedType.ItemDeleted, index);
                    }

                    break;
                }

            case NotifyCollectionChangedAction.Add when ea.NewItems is not null:
                {
                    foreach (var index in Enumerable.Range(ea.NewStartingIndex, ea.NewItems.Count))
                    {
                        yield return new(ListChangedType.ItemAdded, index);
                    }

                    break;
                }

            case NotifyCollectionChangedAction.Move:
                // https://msdn.microsoft.com/en-us/library/acskc6xz(v=vs.110).aspx
                yield return new(
                                ListChangedType.ItemMoved,
                                ea.NewStartingIndex,
                                ea.OldStartingIndex);
                break;
        }
    }
}
