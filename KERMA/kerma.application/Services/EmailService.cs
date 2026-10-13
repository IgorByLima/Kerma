using Microsoft.Extensions.Configuration;
using Resend;
using System.Net.Mail;

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
    private readonly string _de = config["Email:De"] ?? "noreply@kerma.app";
    private readonly string _nomeRemetente = config["Email:NomeRemetente"] ?? "Kerma Eventos";

    public async Task EnviarCredenciaisAsync(string destinatario, string nome, string codigoLogin, string senha, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 500px; margin: 0 auto; padding: 24px;">
              <h2 style="color: #1a0a2e;">🎟️ Bem-vindo ao Kerma!</h2>
              <p>Olá, <strong>{nome}</strong>!</p>
              <p>Suas credenciais de acesso ao sistema foram criadas:</p>
              <div style="background: #f5f0ff; border-radius: 8px; padding: 16px; margin: 16px 0;">
                <p style="margin: 0 0 8px;"><strong>Código de login:</strong> <code style="background: #e8d5ff; padding: 2px 8px; border-radius: 4px;">{codigoLogin}</code></p>
                <p style="margin: 0;"><strong>Senha:</strong> <code style="background: #e8d5ff; padding: 2px 8px; border-radius: 4px;">{senha}</code></p>
              </div>
              <p style="color: #666; font-size: 13px;">Por segurança, recomendamos redefinir sua senha no primeiro acesso.</p>
              <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;

        await EnviarAsync(destinatario, "Suas credenciais de acesso — Kerma", html, ct);
    }

    public async Task EnviarNovaSenhaAsync(string destinatario, string nome, string novaSenha, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 500px; margin: 0 auto; padding: 24px;">
              <h2 style="color: #1a0a2e;">🔑 Senha redefinida</h2>
              <p>Olá, <strong>{nome}</strong>!</p>
              <p>Sua senha foi redefinida por um administrador:</p>
              <div style="background: #f5f0ff; border-radius: 8px; padding: 16px; margin: 16px 0;">
                <p style="margin: 0;"><strong>Nova senha:</strong> <code style="background: #e8d5ff; padding: 2px 8px; border-radius: 4px;">{novaSenha}</code></p>
              </div>
              <p style="color: #666; font-size: 13px;">Acesse o sistema e redefina sua senha assim que possível.</p>
              <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;

        await EnviarAsync(destinatario, "Sua senha foi redefinida — Kerma", html, ct);
    }

    public async Task EnviarExtratoAsync(string destinatario, string nomeEvento, string htmlExtrato, CancellationToken ct = default)
    {
        var html = $"""
            <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 24px;">
              <h2 style="color: #1a0a2e;">📊 Extrato do evento — {nomeEvento}</h2>
              {htmlExtrato}
              <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
              <p style="color: #999; font-size: 12px;">Kerma Eventos — By Dotlin Systems</p>
            </div>
            """;

        await EnviarAsync(destinatario, $"Extrato — {nomeEvento}", html, ct);
    }

    private async Task EnviarAsync(string destinatario, string assunto, string html, CancellationToken ct)
    {
        var options = new ResendClientOptions { ApiToken = _apiKey };
        var client = new ResendClient(options);

        var message = new EmailMessage
        {
            From = $"{_nomeRemetente} <{_de}>",
            To = { destinatario },
            Subject = assunto,
            HtmlBody = html,
        };

        await client.EmailSendAsync(message, ct);
    }
}