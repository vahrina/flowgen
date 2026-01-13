using Flow.Launcher.Plugin.PassGen.Enums;

namespace Flow.Launcher.Plugin.PassGen;

public class Settings
{
    public int DefaultLength { get; set; } = 15;

    public PasswordLengthMode LengthMode { get; set; } = PasswordLengthMode.Fixed;
    public int RandomMinLength { get; set; } = 12;
    public int RandomMaxLength { get; set; } = 15;

    public bool IncludeDigits { get; set; } = true;
    public LetterMode LetterMode { get; set; } = LetterMode.LowerAndUpper;

    public EnterActionMode EnterActionMode { get; set; } = EnterActionMode.CopyAndPaste;

    public bool SymExclam { get; set; } = true;
    public bool SymAt { get; set; } = true;
    public bool SymHash { get; set; } = true;
    public bool SymDollar { get; set; } = true;
    public bool SymPercent { get; set; } = true;
    public bool SymUnderscore { get; set; } = true;
    public bool SymDash { get; set; } = true;
    public bool SymDot { get; set; } = true;

    public bool SymPlus { get; set; } = false;
    public bool SymEquals { get; set; } = false;
    public bool SymAmpersand { get; set; } = false;
    public bool SymAsterisk { get; set; } = false;
    public bool SymCaret { get; set; } = false;
    public bool SymComma { get; set; } = false;
    public bool SymQuestion { get; set; } = false;

    public bool SymOpenParen { get; set; } = false;
    public bool SymCloseParen { get; set; } = false;

    public bool SymSlash { get; set; } = false;
    public bool SymOpenBracket { get; set; } = false;
    public bool SymCloseBracket { get; set; } = false;
    public bool SymOpenBrace { get; set; } = false;
    public bool SymCloseBrace { get; set; } = false;

    public bool SymTilde { get; set; } = false;
    public bool SymBacktick { get; set; } = false;
    public bool SymBackslash { get; set; } = false;
    public bool SymPipe { get; set; } = false;
    public bool SymLessThan { get; set; } = false;
    public bool SymGreaterThan { get; set; } = false;
    public bool SymApostrophe { get; set; } = false;
    public bool SymQuote { get; set; } = false;
    public bool SymColon { get; set; } = false;
    public bool SymSemicolon { get; set; } = false;
}