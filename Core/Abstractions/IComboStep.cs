using System.Threading;
using System.Threading.Tasks;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Abstractions;

/// <summary>
/// Базовый контракт шага комбо: выполняет асинхронное действие.
/// </summary>
public interface IComboStep
{
    Task ExecuteAsync(TriggerExecutionContext context, CancellationToken ct);
}
