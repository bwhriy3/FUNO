namespace Funo.Web.Localization;

/// <summary>
/// Holds the language chosen by this browser connection and notifies
/// components when it changes. Scoped, so every player picks their own.
///
/// Bu tarayici baglantisinin sectigi dili tutar ve degistiginde
/// bilesenleri haberdar eder. Scoped oldugu icin her oyuncu kendi dilini secer.
/// </summary>
public sealed class LanguageState
{
    public Lang Current { get; private set; } = Lang.Tr;

    public event Action? Changed;

    public void Set(Lang lang)
    {
        if (Current == lang)
            return;

        Current = lang;
        Changed?.Invoke();
    }

    public void Toggle() => Set(Current == Lang.Tr ? Lang.En : Lang.Tr);

    /// <summary>Shorthand for translating a key in the current language.</summary>
    public string T(string key) => Strings.Get(key, Current);

    public string T(string key, params object[] args) => Strings.Get(key, Current, args);

    public string LogText(LogEntry entry) => Strings.Log(entry, Current);
}
