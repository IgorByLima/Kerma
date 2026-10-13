using System.Text;

namespace Kerma.Application.Utils;

public static class GeradorCodigo
{
    private static readonly char[] Caracteres =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    private static readonly Random _random = new();

    /// <summary>
    /// Gera um código no formato PREFIX-XXXX (ex: US-A1B2, EV-C3D4, TK-X9K2)
    /// </summary>
    public static string Gerar(string prefixo)
    {
        var sb = new StringBuilder(prefixo.Length + 5);
        sb.Append(prefixo);
        sb.Append('-');

        for (int i = 0; i < 4; i++)
            sb.Append(Caracteres[_random.Next(Caracteres.Length)]);

        return sb.ToString();
    }

    public static string GerarUsuario() => Gerar("US");
    public static string GerarEvento() => Gerar("EV");
    public static string GerarTicket() => Gerar("TK");
}