﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Tests.Winforms;

/// <summary>
/// A fake view model.
/// </summary>
public class FakeViewModel : ReactiveObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeViewModel"/> class.
    /// </summary>
    public FakeViewModel() => Cmd = ReactiveCommand.Create(() => { });

    /// <summary>
    /// Gets or sets the command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> Cmd { get; protected set; }
}
