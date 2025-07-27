// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Skill;

internal sealed class CultureSwitcher : IDisposable
{
    private static readonly bool _invariant = IsGlobalizationInvariant();
    private readonly CultureInfo? _previous;

    private CultureSwitcher(string name)
    {
        _previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        }
        catch (ArgumentException)
        {
            // Ignore invalid/unknown cultures
            _previous = null;
        }
    }

    public static IDisposable UseCulture(string name)
        => _invariant ? NullDisposable.Instance : new CultureSwitcher(name);

    public void Dispose()
    {
        if (_previous is not null)
        {
            CultureInfo.CurrentCulture = _previous;
        }
    }

    private static bool IsGlobalizationInvariant()
    {
        // Based on https://www.meziantou.net/detect-globalization-invariant-mode-in-dotnet.htm
        if (AppContext.TryGetSwitch("System.Globalization.Invariant", out bool isEnabled) && isEnabled)
        {
            return true;
        }

        if (Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT") is { Length: > 0 } value &&
            (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) || value is "1"))
        {
            return true;
        }

        return false;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private sealed class NullDisposable : IDisposable
    {
        internal static readonly NullDisposable Instance = new();

        public void Dispose()
        {
            // No-op
        }
    }
}
