using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Kerma.Infrastructure.Printing;

public interface IImpressoraService
{
    Task ImprimirTicketsAsync(IEnumerable<TicketImpressaoDto> tickets);
}

public record TicketImpressaoDto(
    string Codigo,
    string NomeItem,
    string EventoNome,
    int NumeroTicket,
    int TotalTickets
);

public class ImpressoraService(IConfiguration config) : IImpressoraService
{
    private static readonly byte[] CMD_INIT = [0x1B, 0x40];
    private static readonly byte[] CMD_ALIGN_CENTER = [0x1B, 0x61, 0x01];
    private static readonly byte[] CMD_ALIGN_LEFT = [0x1B, 0x61, 0x00];
    private static readonly byte[] CMD_BOLD_ON = [0x1B, 0x45, 0x01];
    private static readonly byte[] CMD_BOLD_OFF = [0x1B, 0x45, 0x00];
    private static readonly byte[] CMD_FONT_LARGE = [0x1B, 0x21, 0x30];
    private static readonly byte[] CMD_FONT_NORMAL = [0x1B, 0x21, 0x00];
    private static readonly byte[] CMD_LINE_FEED = [0x0A];
    private static readonly byte[] CMD_CUT = [0x1D, 0x56, 0x41, 0x10];

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int Level, [In] ref DOCINFO pDocInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPTStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPTStr)] public string pOutputFile;
        [MarshalAs(UnmanagedType.LPTStr)] public string pDataType;
    }

    public async Task ImprimirTicketsAsync(IEnumerable<TicketImpressaoDto> tickets)
    {
        var nomePrinter = config["Impressora:NomePrinter"] ?? "HPRT MPT-II";

        await Task.Run(() =>
        {
            foreach (var ticket in tickets)
            {
                var buffer = BuildTicketBuffer(ticket);
                SendRawToPrinter(nomePrinter, buffer);
                Thread.Sleep(300);
            }
        });
    }

    private static void SendRawToPrinter(string printerName, byte[] data)
    {
        if (!OpenPrinter(printerName, out var hPrinter, IntPtr.Zero))
            throw new InvalidOperationException($"Não foi possível abrir a impressora: {printerName}");

        var docInfo = new DOCINFO
        {
            pDocName = "Kerma Ticket",
            pDataType = "RAW",
            pOutputFile = null!
        };

        try
        {
            if (!StartDocPrinter(hPrinter, 1, ref docInfo))
                throw new InvalidOperationException("Erro ao iniciar documento.");

            if (!StartPagePrinter(hPrinter))
                throw new InvalidOperationException("Erro ao iniciar página.");

            var pBytes = Marshal.AllocCoTaskMem(data.Length);
            try
            {
                Marshal.Copy(data, 0, pBytes, data.Length);
                if (!WritePrinter(hPrinter, pBytes, data.Length, out _))
                    throw new InvalidOperationException("Erro ao escrever na impressora.");
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
            }

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    private byte[] BuildTicketBuffer(TicketImpressaoDto ticket)
    {
        var bytes = new List<byte>();

        void Push(params byte[][] cmds) => Array.ForEach(cmds, c => bytes.AddRange(c));
        void Text(string t) => bytes.AddRange(Encoding.ASCII.GetBytes(t));
        void Line(string t) { Text(t); Push(CMD_LINE_FEED); }
        void EmptyLine() => Push(CMD_LINE_FEED);
        void Dashed() => Line("- - - - - - - - - - - - - - - -");

        Push(CMD_INIT);

        // Cabeçalho: nome do evento em negrito
        Push(CMD_ALIGN_CENTER);
        Push(CMD_BOLD_ON);
        var evento = ticket.EventoNome.Length > 32
            ? ticket.EventoNome[..32]
            : ticket.EventoNome;
        Line(evento);
        Push(CMD_BOLD_OFF);
        Dashed();

        // Nome do item
        EmptyLine();
        Push(CMD_ALIGN_LEFT);
        Line($"Item: {ticket.NomeItem}");
        EmptyLine();

        // Código grande em negrito
        Push(CMD_ALIGN_CENTER);
        Push(CMD_BOLD_ON);
        Push(CMD_FONT_LARGE);
        Line(ticket.Codigo);
        Push(CMD_FONT_NORMAL);
        Push(CMD_BOLD_OFF);

        // Data e hora
        Line(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        EmptyLine();

        // Código de barras Code128
        bytes.AddRange([0x1D, 0x68, 0x50]); // altura
        bytes.AddRange([0x1D, 0x77, 0x02]); // largura
        bytes.AddRange([0x1D, 0x48, 0x02]); // HRI abaixo
        var barcodeData = Encoding.ASCII.GetBytes(ticket.Codigo);
        bytes.Add(0x1D);
        bytes.Add(0x6B);
        bytes.Add(0x49); // Code128
        bytes.Add((byte)barcodeData.Length);
        bytes.AddRange(barcodeData);

        EmptyLine();
        EmptyLine();

        // Rodapé
        Dashed();
        Push(CMD_ALIGN_CENTER);
        Push(CMD_BOLD_ON);
        Line("* KERMA EVENTOS *");
        Push(CMD_BOLD_OFF);
        Line("By Dotlin Systems");
        EmptyLine();

        // Linha tracejada final
        Dashed();
        EmptyLine();
        EmptyLine();

        // Cortar papel
        Push(CMD_CUT);

        return [.. bytes];
    }
}