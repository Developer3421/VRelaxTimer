# Localization Resources

This project uses C# Dictionary-based localization through the `LocalizationHelper` class.

## Supported Languages

- English (en)
- Ukrainian (uk) - Українська
- German (de) - Deutsch
- Turkish (tr) - Türkçe
- Russian (ru) - Русский

## What Gets Translated

The following UI elements are translated based on the selected language:

### Main Window
- "TIME FOR MYSELF" (timer subtitle)
- Timer control buttons (START, PAUSE, RESET)
- Send button (✉️ emoji)
- Preset timer buttons (20:00, 30:00, 40:00, Custom)
- Language and Agreement buttons
- Chat messages (User, AI labels)

### Custom Timer Dialog
- Window title
- "Set Time:" label
- "Infinity mode" checkbox
- Format hint (HH:MM:SS)
- OK and Cancel buttons
- Invalid format error messages

### Language Selection Window
- "SELECT LANGUAGE" title
- All language button labels

### User Agreement Window
- Window title
- Agreement text content
- ACCEPT and DECLINE buttons

## What Stays Untranslated

- **"RELAX TIMER"** - Main application title (intentionally kept in English across all languages)
- Timer display numbers
- Preset timer values (20:00, 30:00, 40:00)

## Usage

All UI strings are bound to `LocalizationHelper.Instance` using the indexer pattern:

```xaml
<TextBlock Text="{Binding Source={x:Static local:LocalizationHelper.Instance}, Path=[TimeForMyself]}" />
<Button Content="{Binding Source={x:Static local:LocalizationHelper.Instance}, Path=[Start]}" />
```

For dynamically created dialogs (like Custom Timer), use direct property access:

```csharp
Title = LocalizationHelper.Instance["CustomTimerTitle"]
```

## Language Switching

Users can switch languages via the Language Selection window, which updates all bindings automatically through `INotifyPropertyChanged`. The selected language persists for the current session.

## Adding New Translations

To add a new language or translation key:

1. Add the language code and translations to the `_translations` dictionary in `LocalizationHelper.cs`
2. Add a button to `LanguageSelectionWindow.xaml` with the language name
3. Add a click handler in `LanguageSelectionWindow.xaml.cs` that sets `LocalizationHelper.Instance.CurrentLanguage`

## Technical Details

- All translations are stored in-memory as nested dictionaries (`Dictionary<string, Dictionary<string, string>>`)
- Language changes trigger `INotifyPropertyChanged.PropertyChanged` event
- WPF bindings automatically update when the language changes
- Fallback to English if a translation key is missing in the selected language


