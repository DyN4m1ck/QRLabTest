using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

// Шаблонный базовый класс всех мутаций (паттерн «шаблонный метод»).
// Берёт на себя рутину: проверку типа параметров и их валидацию, а потомкам оставляет
// только чистую математику искажения в защищённом перегруженном Apply.
public abstract class ImageMutation<TOptions> : IImageMutation where TOptions : MutationOptions
{
    // Вид мутации — ключ реестра в MutationProcessor; каждая реализация возвращает свою константу.
    public abstract MutationKind Kind { get; }

    // Публичный вход из интерфейса IImageMutation: принимает базовые параметры и приводит их
    // к конкретному типу TOptions. Чужой тип опций — ошибка программиста/конфигурации, не данные.
    public RasterImage Apply(RasterImage image, MutationOptions options)
    {
        if (options is not TOptions typed)
            throw new ArgumentException("Тип параметров не соответствует мутации.", nameof(options));
        // Валидируем уже типизированные параметры (диапазоны силы эффекта, seed и т.п.)
        // до обращения к графическому стеку — дешёвая проверка перед дорогой операцией.
        typed.Validate();
        return Apply(image, typed);
    }

    protected abstract RasterImage Apply(RasterImage image, TOptions options);
}