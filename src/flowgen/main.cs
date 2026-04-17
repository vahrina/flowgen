using Flow.Launcher.Plugin.PassGen.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.PassGen;

public class Main : IPlugin, ISettingProvider
{
    private const int MinLen = 4;
    private const int MaxLen = 100;
    private const int MaxResultCount = 8;

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

        var request = ParseRequest(requested);
        if (request.ShowHelp)
            return BuildHelpResults();

        var effective = BuildEffectiveSettings(request);
        var cfg = BuildAlphabet(effective, out var requiredGroups);

        if (cfg.AllChars.Length == 0)
        {
            return new()
                {
                    new Result
                    {
                        Title = "no allowed characters",
                        SubTitle = "enable letters, digits, or symbols in the plugin settings",
                        IcoPath = "icon.png",
                        Action = _ => true
                    }
                };
        }

        int length = 0;

        if (request.LengthOverride.HasValue)
        {
            length = Math.Clamp(request.LengthOverride.Value, MinLen, MaxLen);
        }
        else
        {
            if (effective.LengthMode == PasswordLengthMode.Random)
            {
                var min = Math.Max(effective.RandomMinLength, requiredGroups);
                var max = effective.RandomMaxLength;

                if (max < requiredGroups)
                {
                    return new()
                        {
                            new Result
                            {
                                Title = $"length too small: max {max}",
                                SubTitle = $"minimum required is {requiredGroups} for your selected options",
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
                length = Math.Clamp(effective.DefaultLength, MinLen, MaxLen);
            }
        }

        if (length < requiredGroups)
        {
            return new()
                {
                    new Result
                    {
                        Title = $"length too small: {length}",
                        SubTitle = $"minimum required is {requiredGroups} for your selected options",
                        IcoPath = "icon.png",
                        Action = _ => true
                    }
                };
        }

        if (request.Count <= 1)
        {
            var pwd = GeneratePassword(length, cfg);
            return BuildSinglePasswordResult(pwd, length);
        }

        return BuildMultiPasswordResults(length, cfg, request.Count);
    }

    private List<Result> BuildSinglePasswordResult(string password, int length)
    {
        return new()
            {
                new Result
                {
                    Title = password,
                    SubTitle = $"enter to copy ({length} chars)",
                    IcoPath = "icon.png",
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(password);
                        return true;
                    }
                }
            };
    }

    private List<Result> BuildMultiPasswordResults(int length, (string AllChars, string? Upper, string? Lower, string? Digits, string? Symbols) cfg, int count)
    {
        var results = new List<Result>(count);
        var safeCount = Math.Clamp(count, 1, MaxResultCount);

        for (var i = 1; i <= safeCount; i++)
        {
            var password = GeneratePassword(length, cfg);
            results.Add(new Result
            {
                Title = password,
                SubTitle = $"enter to copy {i}/{safeCount} ({length} chars)",
                IcoPath = "icon.png",
                Action = _ =>
                {
                    _context.API.CopyToClipboard(password);
                    return true;
                }
            });
        }

        return results;
    }

    private List<Result> BuildHelpResults()
    {
        return new()
            {
                new Result
                {
                    Title = "-c n",
                    SubTitle = "show n passwords (max 8)",
                    IcoPath = "icon.png",
                    Action = _ => true
                },
                new Result
                {
                    Title = "-l | -u | -a",
                    SubTitle = "lower | upper | both",
                    IcoPath = "icon.png",
                    Action = _ => true
                },
                new Result
                {
                    Title = "-d | --no-num",
                    SubTitle = "force numbers on/off",
                    IcoPath = "icon.png",
                    Action = _ => true
                },
                new Result
                {
                    Title = "-s | --no-sym",
                    SubTitle = "force symbols on/off",
                    IcoPath = "icon.png",
                    Action = _ => true
                }
            };
    }

    private Settings BuildEffectiveSettings(ParsedRequest request)
    {
        var copy = new Settings
        {
            DefaultLength = _settings.DefaultLength,
            LengthMode = _settings.LengthMode,
            RandomMinLength = _settings.RandomMinLength,
            RandomMaxLength = _settings.RandomMaxLength,
            IncludeDigits = _settings.IncludeDigits,
            LetterMode = _settings.LetterMode,
            SymExclam = _settings.SymExclam,
            SymAt = _settings.SymAt,
            SymHash = _settings.SymHash,
            SymDollar = _settings.SymDollar,
            SymPercent = _settings.SymPercent,
            SymUnderscore = _settings.SymUnderscore,
            SymDash = _settings.SymDash,
            SymDot = _settings.SymDot,
            SymPlus = _settings.SymPlus,
            SymEquals = _settings.SymEquals,
            SymAmpersand = _settings.SymAmpersand,
            SymAsterisk = _settings.SymAsterisk,
            SymCaret = _settings.SymCaret,
            SymComma = _settings.SymComma,
            SymQuestion = _settings.SymQuestion,
            SymOpenParen = _settings.SymOpenParen,
            SymCloseParen = _settings.SymCloseParen,
            SymSlash = _settings.SymSlash,
            SymOpenBracket = _settings.SymOpenBracket,
            SymCloseBracket = _settings.SymCloseBracket,
            SymOpenBrace = _settings.SymOpenBrace,
            SymCloseBrace = _settings.SymCloseBrace,
            SymTilde = _settings.SymTilde,
            SymBacktick = _settings.SymBacktick,
            SymBackslash = _settings.SymBackslash,
            SymPipe = _settings.SymPipe,
            SymLessThan = _settings.SymLessThan,
            SymGreaterThan = _settings.SymGreaterThan,
            SymApostrophe = _settings.SymApostrophe,
            SymQuote = _settings.SymQuote,
            SymColon = _settings.SymColon,
            SymSemicolon = _settings.SymSemicolon
        };

        if (request.LetterModeOverride.HasValue)
            copy.LetterMode = request.LetterModeOverride.Value;

        if (request.IncludeDigitsOverride.HasValue)
            copy.IncludeDigits = request.IncludeDigitsOverride.Value;

        if (request.IncludeSymbolsOverride.HasValue)
            SetAllSymbols(copy, request.IncludeSymbolsOverride.Value);

        return copy;
    }

    private static ParsedRequest ParseRequest(string raw)
    {
        var parsed = new ParsedRequest();
        if (string.IsNullOrWhiteSpace(raw))
            return parsed;

        var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var token = parts[i].ToLowerInvariant();
            switch (token)
            {
                case "-h":
                    parsed.ShowHelp = true;
                    break;
                case "-l":
                    parsed.LetterModeOverride = LetterMode.LowerOnly;
                    break;
                case "-u":
                    parsed.LetterModeOverride = LetterMode.UpperOnly;
                    break;
                case "-a":
                    parsed.LetterModeOverride = LetterMode.LowerAndUpper;
                    break;
                case "-d":
                    parsed.IncludeDigitsOverride = true;
                    break;
                case "--no-num":
                    parsed.IncludeDigitsOverride = false;
                    break;
                case "-s":
                    parsed.IncludeSymbolsOverride = true;
                    break;
                case "--no-sym":
                    parsed.IncludeSymbolsOverride = false;
                    break;
                case "-c":
                    if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out var count))
                    {
                        parsed.Count = Math.Clamp(count, 1, MaxResultCount);
                        i++;
                    }
                    break;
                default:
                    if (parsed.LengthOverride is null && int.TryParse(token, out var n))
                        parsed.LengthOverride = n;
                    break;
            }
        }

        return parsed;
    }

    private static void SetAllSymbols(Settings settings, bool enabled)
    {
        settings.SymExclam = enabled;
        settings.SymAt = enabled;
        settings.SymHash = enabled;
        settings.SymDollar = enabled;
        settings.SymPercent = enabled;
        settings.SymUnderscore = enabled;
        settings.SymDash = enabled;
        settings.SymDot = enabled;
        settings.SymPlus = enabled;
        settings.SymEquals = enabled;
        settings.SymAmpersand = enabled;
        settings.SymAsterisk = enabled;
        settings.SymCaret = enabled;
        settings.SymComma = enabled;
        settings.SymQuestion = enabled;
        settings.SymOpenParen = enabled;
        settings.SymCloseParen = enabled;
        settings.SymSlash = enabled;
        settings.SymOpenBracket = enabled;
        settings.SymCloseBracket = enabled;
        settings.SymOpenBrace = enabled;
        settings.SymCloseBrace = enabled;
        settings.SymTilde = enabled;
        settings.SymBacktick = enabled;
        settings.SymBackslash = enabled;
        settings.SymPipe = enabled;
        settings.SymLessThan = enabled;
        settings.SymGreaterThan = enabled;
        settings.SymApostrophe = enabled;
        settings.SymQuote = enabled;
        settings.SymColon = enabled;
        settings.SymSemicolon = enabled;
    }

    private sealed class ParsedRequest
    {
        public int? LengthOverride { get; set; }
        public int Count { get; set; } = 1;
        public bool ShowHelp { get; set; }
        public LetterMode? LetterModeOverride { get; set; }
        public bool? IncludeDigitsOverride { get; set; }
        public bool? IncludeSymbolsOverride { get; set; }
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
}
