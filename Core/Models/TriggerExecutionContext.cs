using System.Collections.Concurrent;
using PixelMacroEngine.Core.Abstractions;

namespace PixelMacroEngine.Core.Models;

public class TriggerExecutionContext
{
    public FrameBuffer Frame { get; set; } = new();
    public ITriggerEvaluator? CurrentlyActiveTrigger { get; set; }
    public int ActiveTriggerCurrentWeight { get; set; }

    /// <summary>Имя комбо, к которому относится текущий контекст выполнения.</summary>
    public string ComboName { get; set; } = string.Empty;

    /// <summary>Потокобезопасный словарь переменных текущей сессии выполнения.</summary>
    private readonly ConcurrentDictionary<string, object> _sessionVariables = new();

    /// <summary>Записать переменную сессии.</summary>
    public void SetVariable<T>(string key, T value) where T : notnull => _sessionVariables[key] = value;

    /// <summary>Попытаться прочитать переменную сессии указанного типа.</summary>
    public bool TryGetVariable<T>(string key, out T? value)
    {
        if (_sessionVariables.TryGetValue(key, out var raw) && raw is T casted)
        {
            value = casted;
            return true;
        }
        value = default;
        return false;
    }
}
