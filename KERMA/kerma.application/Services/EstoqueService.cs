using Kerma.Domain.Interfaces;
using System.Collections.Concurrent;

namespace Kerma.Application.Services;

public interface IEstoqueService
{
    Task<bool> TentarReservarAsync(int itemId, int quantidade, CancellationToken ct = default);
    Task LiberarReservaAsync(int itemId);
}

public class EstoqueService(IItemRepositorio itemRepo) : IEstoqueService
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _semaforos = new();

    private static SemaphoreSlim ObterSemaforo(int itemId)
        => _semaforos.GetOrAdd(itemId, _ => new SemaphoreSlim(1, 1));

    public async Task<bool> TentarReservarAsync(int itemId, int quantidade, CancellationToken ct = default)
    {
        var semaforo = ObterSemaforo(itemId);

        var entrou = await semaforo.WaitAsync(TimeSpan.FromSeconds(5), ct);
        if (!entrou)
            return false;

        try
        {
            var item = await itemRepo.ObterPorIdAsync(itemId, ct);
            if (item is null || item.EstoqueAtual < quantidade)
            {
                semaforo.Release();
                return false;
            }
            return true;
        }
        catch
        {
            semaforo.Release();
            throw;
        }
    }

    public Task LiberarReservaAsync(int itemId)
    {
        ObterSemaforo(itemId).Release();
        return Task.CompletedTask;
    }
}