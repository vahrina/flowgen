using Flow.Launcher.Plugin.PassGen.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flow.Launcher.Plugin.PassGen;

public partial class SettingsView : UserControl
{
    private const int MinLen = 4;
    private const int MaxLen = 100;

    private readonly PluginInitContext _context;
    private readonly Settings _settings;

    public SettingsView(PluginInitContext context, Settings settings)
    {
        InitializeComponent();

        _context = context;
        _settings = settings;

        LoadToUI();
        WireEvents();
        UpdateToggleAllButton();

        Loaded += (_, __) => ApplySymbolsGridColumns();
        SizeChanged += (_, __) => ApplySymbolsGridColumns();
    }

    private void ApplySymbolsGridColumns()
    {
        var w = SymbolsGroup.ActualWidth;
        if (double.IsNaN(w) || w <= 0) return;

        var inner = w - 40;
        if (inner <= 0) return;

        var minCell = 95.0;
        var cols = (int)Math.Floor(inner / minCell);
        cols = Math.Clamp(cols, 3, 7);

        if (SymbolsGrid.Columns != cols)
            SymbolsGrid.Columns = cols;
    }

    private void UpdateLenModeToggle()
    {
        var isRandom = _settings.LengthMode == PasswordLengthMode.Random;
        LenModeToggleBtn.Content = isRandom ? "random" : "fixed";
        FixedLengthPanel.Visibility = isRandom ? Visibility.Collapsed : Visibility.Visible;
        RandomLengthPanel.Visibility = isRandom ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateToggleAllButton()
    {
        var all = AllSymbolBoxes();
        var allChecked = all.All(b => b.IsChecked == true);
        SymbolsToggleAllBtn.Content = allChecked ? "clear all" : "select all";
    }

    private IReadOnlyList<CheckBox> AllSymbolBoxes() => new[]
    {
            SymExclam, SymAt, SymHash, SymDollar, SymPercent,
            SymUnderscore, SymDash, SymDot, SymComma, SymColon, SymSemicolon, SymPlus,
            SymEquals, SymAmpersand, SymAsterisk, SymCaret, SymQuestion,
            SymOpenParen, SymCloseParen,
            SymSlash, SymOpenBracket, SymCloseBracket, SymOpenBrace, SymCloseBrace,
            SymTilde, SymBacktick, SymBackslash, SymPipe, SymLessThan, SymGreaterThan, SymApostrophe, SymQuote
        };

    private IReadOnlyList<CheckBox> RecommendedSymbolBoxes() => new[]
    {
            SymExclam, SymAt, SymHash, SymDollar, SymPercent, SymUnderscore, SymDash, SymDot
        };

    private void SetSymbols(IEnumerable<CheckBox> boxes, bool value)
    {
        foreach (var b in boxes)
            b.IsChecked = value;
    }

    private void WireEvents()
    {
        LenModeToggleBtn.Click += (_, __) =>
        {
            _settings.LengthMode = _settings.LengthMode == PasswordLengthMode.Random
                ? PasswordLengthMode.Fixed
                : PasswordLengthMode.Random;
            UpdateLenModeToggle();
            Save();
        };

        DefaultLengthBox.LostFocus += (_, __) => Save();
        DefaultLengthBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Save();
                e.Handled = true;
            }
        };

        MinLengthBox.LostFocus += (_, __) => Save();
        MaxLengthBox.LostFocus += (_, __) => Save();

        MinLengthBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Save();
                e.Handled = true;
            }
        };

        MaxLengthBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Save();
                e.Handled = true;
            }
        };

        IncludeDigitsBox.Click += (_, __) => Save();

        LowerOnlyRadio.Click += (_, __) => Save();
        UpperOnlyRadio.Click += (_, __) => Save();
        BothRadio.Click += (_, __) => Save();

        foreach (var b in AllSymbolBoxes())
        {
            b.Click += (_, __) =>
            {
                UpdateToggleAllButton();
                Save();
            };
        }

        SymbolsToggleAllBtn.Click += (_, __) =>
        {
            var all = AllSymbolBoxes();
            var allChecked = all.All(b => b.IsChecked == true);
            SetSymbols(all, !allChecked);
            UpdateToggleAllButton();
            Save();
        };

        SymbolsRecommendedBtn.Click += (_, __) =>
        {
            SetSymbols(AllSymbolBoxes(), false);
            SetSymbols(RecommendedSymbolBoxes(), true);
            UpdateToggleAllButton();
            Save();
        };
    }

    private void LoadToUI()
    {
        DefaultLengthBox.Text = Math.Clamp(_settings.DefaultLength, MinLen, MaxLen).ToString();
        MinLengthBox.Text = Math.Clamp(_settings.RandomMinLength, MinLen, MaxLen).ToString();
        MaxLengthBox.Text = Math.Clamp(_settings.RandomMaxLength, MinLen, MaxLen).ToString();

        IncludeDigitsBox.IsChecked = _settings.IncludeDigits;

        LowerOnlyRadio.IsChecked = _settings.LetterMode == LetterMode.LowerOnly;
        UpperOnlyRadio.IsChecked = _settings.LetterMode == LetterMode.UpperOnly;
        BothRadio.IsChecked = _settings.LetterMode == LetterMode.LowerAndUpper;

        SymExclam.IsChecked = _settings.SymExclam;
        SymAt.IsChecked = _settings.SymAt;
        SymHash.IsChecked = _settings.SymHash;
        SymDollar.IsChecked = _settings.SymDollar;
        SymPercent.IsChecked = _settings.SymPercent;

        SymUnderscore.IsChecked = _settings.SymUnderscore;
        SymDash.IsChecked = _settings.SymDash;
        SymDot.IsChecked = _settings.SymDot;
        SymComma.IsChecked = _settings.SymComma;
        SymColon.IsChecked = _settings.SymColon;
        SymSemicolon.IsChecked = _settings.SymSemicolon;
        SymPlus.IsChecked = _settings.SymPlus;

        SymEquals.IsChecked = _settings.SymEquals;
        SymAmpersand.IsChecked = _settings.SymAmpersand;
        SymAsterisk.IsChecked = _settings.SymAsterisk;
        SymCaret.IsChecked = _settings.SymCaret;
        SymQuestion.IsChecked = _settings.SymQuestion;

        SymOpenParen.IsChecked = _settings.SymOpenParen;
        SymCloseParen.IsChecked = _settings.SymCloseParen;

        SymSlash.IsChecked = _settings.SymSlash;
        SymOpenBracket.IsChecked = _settings.SymOpenBracket;
        SymCloseBracket.IsChecked = _settings.SymCloseBracket;
        SymOpenBrace.IsChecked = _settings.SymOpenBrace;
        SymCloseBrace.IsChecked = _settings.SymCloseBrace;

        SymTilde.IsChecked = _settings.SymTilde;
        SymBacktick.IsChecked = _settings.SymBacktick;
        SymBackslash.IsChecked = _settings.SymBackslash;
        SymPipe.IsChecked = _settings.SymPipe;
        SymLessThan.IsChecked = _settings.SymLessThan;
        SymGreaterThan.IsChecked = _settings.SymGreaterThan;
        SymApostrophe.IsChecked = _settings.SymApostrophe;
        SymQuote.IsChecked = _settings.SymQuote;

        UpdateLenModeToggle();
    }

    private void Save()
    {
        _settings.DefaultLength = ClampText(DefaultLengthBox.Text, MinLen, MaxLen, _settings.DefaultLength);
        DefaultLengthBox.Text = _settings.DefaultLength.ToString();

        var min = ClampText(MinLengthBox.Text, MinLen, MaxLen, _settings.RandomMinLength);
        var max = ClampText(MaxLengthBox.Text, MinLen, MaxLen, _settings.RandomMaxLength);

        if (max < min) max = min;

        _settings.RandomMinLength = min;
        _settings.RandomMaxLength = max;

        MinLengthBox.Text = _settings.RandomMinLength.ToString();
        MaxLengthBox.Text = _settings.RandomMaxLength.ToString();

        _settings.IncludeDigits = IncludeDigitsBox.IsChecked == true;

        _settings.LetterMode =
            LowerOnlyRadio.IsChecked == true ? LetterMode.LowerOnly :
            UpperOnlyRadio.IsChecked == true ? LetterMode.UpperOnly :
            LetterMode.LowerAndUpper;

        _settings.SymExclam = SymExclam.IsChecked == true;
        _settings.SymAt = SymAt.IsChecked == true;
        _settings.SymHash = SymHash.IsChecked == true;
        _settings.SymDollar = SymDollar.IsChecked == true;
        _settings.SymPercent = SymPercent.IsChecked == true;

        _settings.SymUnderscore = SymUnderscore.IsChecked == true;
        _settings.SymDash = SymDash.IsChecked == true;
        _settings.SymDot = SymDot.IsChecked == true;
        _settings.SymComma = SymComma.IsChecked == true;
        _settings.SymColon = SymColon.IsChecked == true;
        _settings.SymSemicolon = SymSemicolon.IsChecked == true;
        _settings.SymPlus = SymPlus.IsChecked == true;

        _settings.SymEquals = SymEquals.IsChecked == true;
        _settings.SymAmpersand = SymAmpersand.IsChecked == true;
        _settings.SymAsterisk = SymAsterisk.IsChecked == true;
        _settings.SymCaret = SymCaret.IsChecked == true;
        _settings.SymQuestion = SymQuestion.IsChecked == true;

        _settings.SymOpenParen = SymOpenParen.IsChecked == true;
        _settings.SymCloseParen = SymCloseParen.IsChecked == true;

        _settings.SymSlash = SymSlash.IsChecked == true;
        _settings.SymOpenBracket = SymOpenBracket.IsChecked == true;
        _settings.SymCloseBracket = SymCloseBracket.IsChecked == true;
        _settings.SymOpenBrace = SymOpenBrace.IsChecked == true;
        _settings.SymCloseBrace = SymCloseBrace.IsChecked == true;

        _settings.SymTilde = SymTilde.IsChecked == true;
        _settings.SymBacktick = SymBacktick.IsChecked == true;
        _settings.SymBackslash = SymBackslash.IsChecked == true;
        _settings.SymPipe = SymPipe.IsChecked == true;
        _settings.SymLessThan = SymLessThan.IsChecked == true;
        _settings.SymGreaterThan = SymGreaterThan.IsChecked == true;
        _settings.SymApostrophe = SymApostrophe.IsChecked == true;
        _settings.SymQuote = SymQuote.IsChecked == true;

        _context.API.SaveSettingJsonStorage<Settings>();
    }

    private static int ClampText(string? text, int min, int max, int fallback)
    {
        if (!int.TryParse((text ?? "").Trim(), out var v))
            v = fallback;
        return Math.Clamp(v, min, max);
    }

    public void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        if (Parent is not UIElement parent) return;

        var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = this
        };

        parent.RaiseEvent(args);
    }

    public void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }
}
