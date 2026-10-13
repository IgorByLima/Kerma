using Microsoft.AspNetCore.SignalR;

namespace Kerma.API.Hubs;

public class FilaCozinhaHub : Hub
{
    public async Task EntrarNoEvento(int eventoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"evento-{eventoId}");
    }

    public async Task SairDoEvento(int eventoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"evento-{eventoId}");
    }
}