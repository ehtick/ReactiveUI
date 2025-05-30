﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Tests;

/// <summary>
/// Tests for activating views.
/// </summary>
public class ActivatingViewTests
{
    /// <summary>
    /// Tests to make sure that views generally activate.
    /// </summary>
    [Fact]
    public void ActivatingViewSmokeTest()
    {
        var locator = new ModernDependencyResolver();
        locator.InitializeSplat();
        locator.InitializeReactiveUI();
        locator.Register(() => new ActivatingViewFetcher(), typeof(IActivationForViewFetcher));

        using (locator.WithResolver())
        {
            var vm = new ActivatingViewModel();
            var fixture = new ActivatingView
            {
                ViewModel = vm
            };
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, vm.IsActiveCount);
            Assert.Equal(1, fixture.IsActiveCount);

            fixture.Unloaded.OnNext(Unit.Default);
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);
        }
    }

    /// <summary>
    /// Tests for making sure nulling the view model deactivate it.
    /// </summary>
    [Fact]
    public void NullingViewModelDeactivateIt()
    {
        var locator = new ModernDependencyResolver();
        locator.InitializeSplat();
        locator.InitializeReactiveUI();
        locator.Register(() => new ActivatingViewFetcher(), typeof(IActivationForViewFetcher));

        using (locator.WithResolver())
        {
            var vm = new ActivatingViewModel();
            var fixture = new ActivatingView
            {
                ViewModel = vm
            };
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, vm.IsActiveCount);
            Assert.Equal(1, fixture.IsActiveCount);

            fixture.ViewModel = null;
            Assert.Equal(0, vm.IsActiveCount);
        }
    }

    /// <summary>
    /// Tests switching the view model deactivates it.
    /// </summary>
    [Fact]
    public void SwitchingViewModelDeactivatesIt()
    {
        var locator = new ModernDependencyResolver();
        locator.InitializeSplat();
        locator.InitializeReactiveUI();
        locator.Register(() => new ActivatingViewFetcher(), typeof(IActivationForViewFetcher));

        using (locator.WithResolver())
        {
            var vm = new ActivatingViewModel();
            var fixture = new ActivatingView
            {
                ViewModel = vm
            };
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, vm.IsActiveCount);
            Assert.Equal(1, fixture.IsActiveCount);

            var newVm = new ActivatingViewModel();
            Assert.Equal(0, newVm.IsActiveCount);

            fixture.ViewModel = newVm;
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(1, newVm.IsActiveCount);
        }
    }

    /// <summary>
    /// Tests setting the view model after loaded loads it.
    /// </summary>
    [Fact]
    public void SettingViewModelAfterLoadedLoadsIt()
    {
        var locator = new ModernDependencyResolver();
        locator.InitializeSplat();
        locator.InitializeReactiveUI();
        locator.Register(() => new ActivatingViewFetcher(), typeof(IActivationForViewFetcher));

        using (locator.WithResolver())
        {
            var vm = new ActivatingViewModel();
            var fixture = new ActivatingView();

            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, fixture.IsActiveCount);

            fixture.ViewModel = vm;
            Assert.Equal(1, fixture.IsActiveCount);
            Assert.Equal(1, vm.IsActiveCount);

            fixture.Unloaded.OnNext(Unit.Default);
            Assert.Equal(0, fixture.IsActiveCount);
            Assert.Equal(0, vm.IsActiveCount);
        }
    }

    /// <summary>
    /// Tests the can unload and load view again.
    /// </summary>
    [Fact]
    public void CanUnloadAndLoadViewAgain()
    {
        var locator = new ModernDependencyResolver();
        locator.InitializeSplat();
        locator.InitializeReactiveUI();
        locator.Register(() => new ActivatingViewFetcher(), typeof(IActivationForViewFetcher));

        using (locator.WithResolver())
        {
            var vm = new ActivatingViewModel();
            var fixture = new ActivatingView
            {
                ViewModel = vm
            };
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, vm.IsActiveCount);
            Assert.Equal(1, fixture.IsActiveCount);

            fixture.Unloaded.OnNext(Unit.Default);
            Assert.Equal(0, vm.IsActiveCount);
            Assert.Equal(0, fixture.IsActiveCount);

            fixture.Loaded.OnNext(Unit.Default);
            Assert.Equal(1, vm.IsActiveCount);
            Assert.Equal(1, fixture.IsActiveCount);
        }
    }
}
