﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI;

/// <summary>
/// The component model binding type converter.
/// </summary>
#if NET6_0_OR_GREATER
[RequiresDynamicCode("The method uses reflection and will not work in AOT environments.")]
[RequiresUnreferencedCode("The method uses reflection and will not work in AOT environments.")]
#endif
public class ComponentModelTypeConverter : IBindingTypeConverter
{
    private readonly MemoizingMRUCache<(Type fromType, Type toType), TypeConverter?> _typeConverterCache = new(
     static (types, _) =>
     {
         // NB: String is a Magical Type(tm) to TypeConverters. If we are
         // converting from string => int, we need the Int converter, not
         // the string converter :-/
         if (types.fromType == typeof(string))
         {
             types = (types.toType, types.fromType);
         }

         var converter = TypeDescriptor.GetConverter(types.fromType);
         return converter.CanConvertTo(types.toType) ? converter : null;
     },
     RxApp.SmallCacheLimit);

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type fromType, Type toType)
    {
        var converter = _typeConverterCache.Get((fromType, toType));
        return converter is not null ? 10 : 0;
    }

    /// <inheritdoc/>
    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        if (from is null)
        {
            result = null;
            return true;
        }

        var fromType = from.GetType();
        var converter = _typeConverterCache.Get((fromType, toType)) ?? throw new ArgumentException($"Can't convert {fromType} to {toType}. To fix this, register a IBindingTypeConverter");
        try
        {
            // TODO: This should use conversionHint to determine whether this is locale-aware or not
            result = (fromType == typeof(string)) ?
                         converter.ConvertFrom(from) : converter.ConvertTo(from, toType);

            return true;
        }
        catch (FormatException)
        {
            result = null;
            return false;
        }
        catch (Exception e)
        {
            // Errors from ConvertFrom end up here but wrapped in
            // outer exception. Add more types here as required.
            // IndexOutOfRangeException is given when trying to
            // convert empty strings with some/all? converters
            if (e.InnerException is IndexOutOfRangeException ||
                e.InnerException is FormatException)
            {
                result = null;
                return false;
            }

            throw new Exception($"Can't convert from {from.GetType()} to {toType}.", e);
        }
    }
}
