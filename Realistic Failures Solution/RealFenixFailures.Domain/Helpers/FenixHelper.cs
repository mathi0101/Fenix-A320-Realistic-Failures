using System.Text.RegularExpressions;

namespace RealFenixFailures.Domain.Helpers;

public static class FenixHelper {
    private readonly static Random _random = Random.Shared;
    public static class Intervalos {
        private static readonly Regex IntervaloRegex = new(
            @"^\s*([\(\[])\s*(-?\d+)\s*,\s*(-?\d+)\s*([\)\]])\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
        public static string CerradoEntre(int v1, int v2) {
            return $"({v1},{v2})";
        }
        public static string AbiertoEntre(int v1, int v2) {
            return $"[{v1},{v2}]";
        }

        public static int? GetValorRandomIntervalo(string? intervalo) {
            if (string.IsNullOrWhiteSpace(intervalo))
                return null;

            var match = IntervaloRegex.Match(intervalo);
            if (!match.Success)
                return null;

            if (!int.TryParse(match.Groups[2].Value, out var v1))
                return null;

            if (!int.TryParse(match.Groups[3].Value, out var v2))
                return null;

            var izquierda = match.Groups[1].Value[0]; // '(' o '['
            var derecha = match.Groups[4].Value[0];   // ')' o ']'

            int minimo = v1;
            int maximo = v2;

            // Lado izquierdo
            if (izquierda == '(')
                minimo++;
            else if (izquierda != '[')
                return null;

            // Lado derecho
            if (derecha == ')')
                maximo--;
            else if (derecha != ']')
                return null;

            // Intervalo vacío o inválido
            if (minimo > maximo)
                return null;

            // NextInt64 usa límite superior exclusivo
            var valor = _random.Next(minimo, maximo + 1);
            return valor;
        }
    }
}
