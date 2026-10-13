using Microsoft.Extensions.Configuration;
using Resend;

namespace Kerma.Application.Services;

public interface IEmailService
{
    Task EnviarCredenciaisAsync(string destinatario, string nome, string codigoLogin, string senha, CancellationToken ct = default);
    Task EnviarNovaSenhaAsync(string destinatario, string nome, string novaSenha, CancellationToken ct = default);
    Task EnviarExtratoAsync(string destinatario, string nomeEvento, string htmlExtrato, CancellationToken ct = default);
}

public class EmailService(IConfiguration config) : IEmailService
{
    private readonly string _apiKey = config["Email:ApiKey"] ?? "";
    private readonly string _de = config["Email:De"] ?? "noreply@dotlim.com.br";
    private readonly string _nomeRemetente = config["Email:NomeRemetente"] ?? "Kerma Eventos";

    public async Task EnviarCredenciaisAsync(string destinatario, string nome, string codigoLogin, string senha, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 500px; margin: 0 auto; padding: 24px;">
              <h2 style="color: #1a0a2e;">🎟️ Bem-vindo ao Kerma!</h2>
              <p>Olá, <strong>{nome}</strong>!</p>
              <div style="background: #f5f0ff; border-radius: 8px; padding: 16px; margin: 16px 0;">
                <p style="margin: 0 0 8px;"><strong>Código de login:</strong> <code>{codigoLogin}</code></p>
                <p style="margin: 0;"><strong>Senha:</strong> <code>{senha}</code></p>
              </div>
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;
        await EnviarAsync(destinatario, "Suas credenciais — Kerma", html, ct);
    }

    public async Task EnviarNovaSenhaAsync(string destinatario, string nome, string novaSenha, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 500px; margin: 0 auto; padding: 24px;">
              <h2 style="color: #1a0a2e;">🔑 Senha redefinida</h2>
              <p>Olá, <strong>{nome}</strong>! Sua nova senha: <code>{novaSenha}</code></p>
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;
        await EnviarAsync(destinatario, "Senha redefinida — Kerma", html, ct);
    }

    public async Task EnviarExtratoAsync(string destinatario, string nomeEvento, string htmlExtrato, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 24px;">
              <h2>📊 Extrato — {nomeEvento}</h2>
              {htmlExtrato}
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;
        await EnviarAsync(destinatario, $"Extrato — {nomeEvento}", html, ct);
    }

    private async Task EnviarAsync(string destinatario, string assunto, string html, CancellationToken ct)
    {
        // Usar HttpClient diretamente via API REST do Resend
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            from = $"{_nomeRemetente} <{_de}>",
            to = new[] { destinatario },
            subject = assunto,
            html = html
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

        await httpClient.PostAsync("https://api.resend.com/emails", content, ct);
    }
}