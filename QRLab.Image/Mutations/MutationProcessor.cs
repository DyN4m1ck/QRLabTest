using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

// Оркестратор цепочки мутаций. Получает список всех зарегистрированных IImageMutation
// (через внедрение зависимостей из App) и строит из них словарь по Kind — поиск нужной
// реализации становится O(1) и не требует ветвлений по типу в рантайме.
public sealed class MutationProcessor(IEnumerable<IImageMutation> mutations) : IMutationProcessor
{
    // Дубликат Kind у двух реализаций упадёт здесь же, на старте приложения, а не в момент мутации.
    private readonly IReadOnlyDictionary<MutationKind, IImageMutation> _mutations = mutations.ToDictionary(m => m.Kind);

    // Применяет операции последовательно: выход i-й мутации становится входом i+1-й.
    // Возвращает НЕ только финальный кадр, а всю историю (MutationFrame = параметры + результат) —
    // UI показывает каждый промежуточный снимок, а распознавание берёт последний кадр.
    public IReadOnlyList<MutationFrame> Apply(RasterImage original, IReadOnlyList<MutationOptions> operations)
    {
        var frames = new List<MutationFrame>(operations.Count);
        var image = original;
        foreach (var options in operations)
        {
            // Неизвестный вид операции = сбой конфигурации DI; молча пропускать нельзя,
            // иначе пользователь получил бы «неполную» цепочку и неверный результат.
            if (!_mutations.TryGetValue(options.Kind, out var mutation))
                throw new ArgumentException("Реализация мутации не зарегистрирована.");
            image = mutation.Apply(image, options);
            // В каждый кадр кладётся ссылка на неизменяемый RasterImage — никакого дублирования байтов.
            frames.Add(new(options, image));
        }
        return frames;
    }
}