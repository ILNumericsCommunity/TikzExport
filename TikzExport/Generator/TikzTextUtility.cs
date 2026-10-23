using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AnyAscii;

namespace ILNumerics.Community.TikzExport.Generator;

/// <summary>
/// Provides helpers for escaping text in TikZ output.
/// </summary>
public static class TikzTextUtility
{
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

    /// <summary>
    /// Escapes the specified text for safe TikZ output.
    /// </summary>
    /// <param name="input">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeText(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var result = new StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (ch == '$')
            {
                var end = input.IndexOf('$', i + 1);
                if (end > i + 1)
                {
                    result.Append(input, i, end - i + 1);
                    i = end;
                    continue;
                }
            }

            var replacement = ReplacementMap.FirstOrDefault(p => p.Key == ch.ToString());
            if (replacement.Key != null)
            {
                result.Append(replacement.Value);
                continue;
            }

            if ((ch == '_' || ch == '^') && i > 0 && i + 1 < input.Length)
            {
                var match = Regex.Match(input.Substring(i + 1), @"^(?:\{([\w+\-]+)\}|([\w]))", RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    result.Append("${}").Append(ch).Append('{').Append(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Append("}$");
                    i += match.Length;
                    continue;
                }
            }
            result.Append(ch switch
            {
                '\\' => @"\textbackslash{}",
                '{' => @"\{",
                '}' => @"\}",
                '%' => @"\%",
                '&' => @"\&",
                '#' => @"\#",
                '$' => @"\$",
                '_' => @"\_",
                '^' => @"\textasciicircum{}",
                '~' => @"\textasciitilde{}",
                '\r' or '\n' => " ",
                _ => ch.ToString().Transliterate()
            });
        }

        return result.ToString();
    }
}
