namespace RealFenixFailures.Domain;

internal static class Helpers {
    internal static class Intervalos {
        public static string CerradoEntre(int v1, int v2) {
            return $"({v1},{v2})";
        }
        public static string AbiertoEntre(int v1, int v2) {
            return $"[{v1},{v2}]";
        }
    }
}
