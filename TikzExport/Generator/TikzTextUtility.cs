using System.Collections.Generic;
using System.Text.RegularExpressions;
using AnyAscii;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Provides helpers for escaping text in TikZ output.
/// </summary>
public static class TikzTextUtility
{
    private static readonly Regex EscapeBackslashRegex = new(@"\\(?=[^\{\}_\^])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EscapeOutsideMathRegex = new(@"(?!\B\$[^\$]*)([\{\}_\^])(?![^\$]*\$\B)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex JoinMathEnvironmentsRegex = new(@"\$([\^_\ \+\-\*\/]*)\$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MathSequenceSubscriptBraceRegex = new(@"([\w\d]+_\{[\w\d]+\})", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MathSequenceSubscriptRegex = new(@"(.*)\b([\w\d]+_[\w\d]{1})\b(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MathSequenceSuperscriptBraceRegex = new(@"([\w\d]+\^\{[\w\d]+\})", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MathSequenceSuperscriptRegex = new(@"(.*)\b([\w\d]+\^[\w\d]{1})\b(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyList<KeyValuePair<string, string>> ReplacementMap =
    [
        new("α", @"${\alpha}$"), new("β", @"${\beta}$"), new("γ", @"${\gamma}$"), new("Γ", @"${\Gamma}$"), new("δ", @"${\delta}$"), new("Δ", @"${\Delta}$"),
        new("ϵ", @"${\epsilon}$"), new("ε", @"${\varepsilon}$"), new("ζ", @"${\zeta}$"), new("η", @"${\eta}$"), new("θ", @"${\theta}$"), new("ϑ", @"${\vartheta}$"),
        new("Θ", @"${\Theta}$"), new("ι", @"${\iota}$"), new("κ", @"${\kappa}$"), new("ϰ", @"${\kappa}$"), new("λ", @"${\lambda}$"), new("Λ", @"${\Lambda}$"),
        new("μ", @"${\mu}$"), new("µ", @"${\mu}$"), new("ν", @"${\nu}$"), new("ξ", @"${\xi}$"), new("π", @"${\pi}$"), new("Π", @"${\Pi}$"), new("ρ", @"${\rho}$"),
        new("ϱ", @"${\varrho}$"), new("σ", @"${\sigma}$"), new("ς", @"${\sigma}$"), new("Σ", @"${\Sigma}$"), new("τ", @"${\tau}$"), new("υ", @"${\upsilon}$"),
        new("Υ", @"${\Upsilon}$"), new("ϕ", @"${\phi}$"), new("φ", @"${\varphi}$"), new("Φ", @"${\Phi}$"), new("χ", @"${\chi}$"), new("ψ", @"${\psi}$"), new("Ψ", @"${\Psi}$"),
        new("ω", @"${\omega}$"), new("Ω", @"${\Omega}$"), new("±", @"${\pm}$"), new("∓", @"${\mp}$"), new("≈", @"${\approx}$"), new("∼", @"${\sim}$"), new("≅", @"${\cong}$"),
        new("≠", @"${\neq}$"), new("⊕", @"${\oplus}$"), new("×", @"${\times}$"), new("∇", @"${\nabla}$"), new("→", @"${\rightarrow}$"), new("←", @"${\leftarrow}$"),
        new("⇒", @"${\Rightarrow}$"), new("⇐", @"${\Leftarrow}$"), new("↔", @"${\leftrightarrow}$"), new("⇔", @"${\Leftrightarrow}$"), new("↦", @"${\mapsto}$")
    ];

    private static readonly Regex ScientificNotationRegex = new(@"([\d]+)\^([\{\}\d\+\-]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UnescapeMathBracesRegex = new(@"(?!\b\$[^\$]*)\\([\{\}]+)(?![^\$]*\$\b)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Escapes the specified text for safe TikZ output.
    /// </summary>
    /// <param name="input">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeText(string input)
    {
        foreach (var replacement in ReplacementMap)
            input = input.Replace(replacement.Key, replacement.Value);

        // Scientific notation
        input = ScientificNotationRegex.Replace(input, @"$${$1^{$2}}$$");

        // Detect basic math sequences "a_{a}" and "a^{b}"
        input = MathSequenceSubscriptRegex.Replace(input, @"$1$$$2$$$3"); // "a_a"
        input = MathSequenceSuperscriptRegex.Replace(input, @"$1$$$2$$$3"); // "a^a"
        input = MathSequenceSubscriptBraceRegex.Replace(input, @"$$$1$$"); // "a_{abc}"
        input = MathSequenceSuperscriptBraceRegex.Replace(input, @"$$$1$$"); // "a^{abc}"

        // Replace '{', '}', '_', '^' (if not inside math environment)
        input = EscapeOutsideMathRegex.Replace(input, @"\$1"); // '{}_^'

        // Replace '\' (if not used for escaping above)
        input = EscapeBackslashRegex.Replace(input, @"\\"); // '\'

        // Revert double escaping of greek letters and math characters
        input = input.Replace(@"{\\", @"{\");

        // Join consecutive math environments
        input = JoinMathEnvironmentsRegex.Replace(input, @"$1");

        // Un-escape '\{' in math environment
        input = UnescapeMathBracesRegex.Replace(input, @"$1"); // '{}'

        // ASCII transliteration (replace all remaining non-ascii characters)
        input = input.Transliterate();

        return input;
    }
}
