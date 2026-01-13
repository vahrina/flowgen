using Flow.Launcher.Plugin.PassGen.Enums;
using Flow.Launcher.Plugin.PassGen.Input;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.PassGen;

public class Main : IPlugin, ISettingProvider
{
    private const int PasteInitialDelayMs = 220;
    private const int MinLen = 4;
    private const int MaxLen = 100;

    private PluginInitContext _context = null!;
    private Settings _settings = new();

    public void Init(PluginInitContext context)
    {
        _context = context;
        _settings = _context.API.LoadSettingJsonStorage<Settings>();

        _settings.DefaultLength = Math.Clamp(_settings.DefaultLength, MinLen, MaxLen);

        _settings.RandomMinLength = Math.Clamp(_settings.RandomMinLength, MinLen, MaxLen);
        _settings.RandomMaxLength = Math.Clamp(_settings.RandomMaxLength, MinLen, MaxLen);
        if (_settings.RandomMaxLength < _settings.RandomMinLength)
            _settings.RandomMaxLength = _settings.RandomMinLength;
    }

    public Control CreateSettingPanel() => new SettingsView(_context, _settings);

    public List<Result> Query(Query query)
    {
        var requested = (query.Search ?? "").Trim();

        var cfg = BuildAlphabet(_settings, out var requiredGroups);

        if (cfg.AllChars.Length == 0)
        {
            return new()
                {
                    new Result
                    {
                        Title = "No allowed characters",
                        SubTitle = "Enable letters, digits, or symbols in the plugin settings.",
                        IcoPath = "icon.png",
                        Action = _ => true
                    }
                };
        }

        int length;

        if (int.TryParse(requested, out var n))
        {
            length = Math.Clamp(n, MinLen, MaxLen);
        }
        else
        {
            if (_settings.LengthMode == PasswordLengthMode.Random)
            {
                var min = Math.Max(_settings.RandomMinLength, requiredGroups);
                var max = _settings.RandomMaxLength;

                if (max < requiredGroups)
                {
                    return new()
                        {
                            new Result
                            {
                                Title = $"Length too small: max {max}",
                                SubTitle = $"Minimum required is {requiredGroups} for your selected options. Increase Max length.",
                                IcoPath = "icon.png",
                                Action = _ => true
                            }
                        };
                }

                if (max < min) max = min;
                length = RandomNumberGenerator.GetInt32(min, max + 1);
            }
            else
            {
                length = Math.Clamp(_settings.DefaultLength, MinLen, MaxLen);
            }
        }

        if (length < requiredGroups)
        {
            return new()
                {
                    new Result
                    {
                        Title = $"Length too small: {length}",
                        SubTitle = $"Minimum required is {requiredGroups} for your selected options. Increase the length.",
                        IcoPath = "icon.png",
                        Action = _ => true
                    }
                };
        }

        var pwd = GeneratePassword(length, cfg);

        return new()
            {
                new Result
                {
                    Title = pwd,
                    SubTitle = $"{BuildEnterSubtitle()} password ({length} chars)",
                    IcoPath = "icon.png",
                    Action = _ =>
                    {
                        ExecuteEnterAction(pwd);
                        return true;
                    }
                }
            };
    }

    private static (string AllChars, string? Upper, string? Lower, string? Digits, string? Symbols)
        BuildAlphabet(Settings s, out int requiredGroups)
    {
        const string U = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string L = "abcdefghijklmnopqrstuvwxyz";
        const string D = "0123456789";

        var symbols = string.Concat(
            s.SymExclam ? "!" : "",
            s.SymAt ? "@" : "",
            s.SymHash ? "#" : "",
            s.SymDollar ? "$" : "",
            s.SymPercent ? "%" : "",

            s.SymUnderscore ? "_" : "",
            s.SymDash ? "-" : "",
            s.SymDot ? "." : "",
            s.SymComma ? "," : "",
            s.SymColon ? ":" : "",
            s.SymSemicolon ? ";" : "",
            s.SymPlus ? "+" : "",
            s.SymEquals ? "=" : "",
            s.SymAmpersand ? "&" : "",
            s.SymAsterisk ? "*" : "",
            s.SymCaret ? "^" : "",
            s.SymQuestion ? "?" : "",

            s.SymOpenParen ? "(" : "",
            s.SymCloseParen ? ")" : "",

            s.SymSlash ? "/" : "",
            s.SymOpenBracket ? "[" : "",
            s.SymCloseBracket ? "]" : "",
            s.SymOpenBrace ? "{" : "",
            s.SymCloseBrace ? "}" : "",

            s.SymTilde ? "~" : "",
            s.SymBacktick ? "`" : "",
            s.SymBackslash ? "\\" : "",
            s.SymPipe ? "|" : "",
            s.SymLessThan ? "<" : "",
            s.SymGreaterThan ? ">" : "",
            s.SymApostrophe ? "'" : "",
            s.SymQuote ? "\"" : ""
        );

        string? upper = null, lower = null, digits = null, sym = null;

        switch (s.LetterMode)
        {
            case LetterMode.LowerOnly: lower = L; break;
            case LetterMode.UpperOnly: upper = U; break;
            case LetterMode.LowerAndUpper: upper = U; lower = L; break;
        }

        if (s.IncludeDigits) digits = D;
        if (!string.IsNullOrEmpty(symbols)) sym = symbols;

        var all = string.Concat(upper ?? "", lower ?? "", digits ?? "", sym ?? "");

        requiredGroups = 0;
        if (s.LetterMode == LetterMode.LowerAndUpper) requiredGroups += 2;
        else if (s.LetterMode is LetterMode.LowerOnly or LetterMode.UpperOnly) requiredGroups += 1;
        if (digits != null) requiredGroups += 1;
        if (sym != null) requiredGroups += 1;

        return (all, upper, lower, digits, sym);
    }

    private static string GeneratePassword(int length, (string AllChars, string? Upper, string? Lower, string? Digits, string? Symbols) cfg)
    {
        var chars = new List<char>(length);

        if (cfg.Upper != null) chars.Add(cfg.Upper[RandomNumberGenerator.GetInt32(cfg.Upper.Length)]);
        if (cfg.Lower != null) chars.Add(cfg.Lower[RandomNumberGenerator.GetInt32(cfg.Lower.Length)]);
        if (cfg.Digits != null) chars.Add(cfg.Digits[RandomNumberGenerator.GetInt32(cfg.Digits.Length)]);
        if (cfg.Symbols != null) chars.Add(cfg.Symbols[RandomNumberGenerator.GetInt32(cfg.Symbols.Length)]);

        while (chars.Count < length)
            chars.Add(cfg.AllChars[RandomNumberGenerator.GetInt32(cfg.AllChars.Length)]);

        for (int i = chars.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private string BuildEnterSubtitle()
    {
        return _settings.EnterActionMode == EnterActionMode.CopyAndPaste
            ? "Copy and Paste"
            : "Copy";
    }

    private void ExecuteEnterAction(string text)
    {
        if (_settings.EnterActionMode != EnterActionMode.CopyAndPaste)
        {
            _context.API.CopyToClipboard(text);
            return;
        }

        var blocked = PasteHelper.GetForegroundWindowHandle();

        _context.API.CopyToClipboard(text);
        _context.API.HideMainWindow();
        PasteHelper.PasteFromClipboard(PasteInitialDelayMs, blocked);
    }
}