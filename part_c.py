import os, sys

ROOT = "/workspace"

def annotate(relpath, items):
    path = os.path.join(ROOT, relpath)
    with open(path, encoding="utf-8") as f:
        text = f.read()
    for anchor, comment in items:
        count = text.count(anchor)
        if count != 1:
            print(f"ERROR {relpath}: anchor '{anchor[:60]}' found {count} times"); sys.exit(1)
        idx = text.index(anchor)
        line_start = text.rfind("\n", 0, idx) + 1
        indent = text[line_start:idx]
        assert indent.strip() == "", f"anchor not at line start: {anchor!r}"
        commented = "\n".join(indent + c if c else "" for c in comment.splitlines())
        text = text.replace(anchor, commented + "\n" + anchor, 1)
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print("annotated:", relpath)

annotate("QRLab.Image/Generation/CodeImageRenderer.cs", [
("public sealed class CodeImageRenderer",
 "// Растеризатор логической битовой матрицы кода в PNG-изображение заданного размера.\n"
 "// Работает через SkiaSharp: каждый «чёрный» модуль матрицы превращается в квадрат\n"
 "// из scale×scale пикселей (для линейных штрихкодов высота масштабируется отдельно).\n"
 "// Антиалиасинг выключен намеренно — код должен состоять из резких чёрно-белых границ."),
("    public RasterImage Render(BitMatrix matrix, GenerationOptions settings)",
 "    // Главный метод: принимает матрицу модулей и параметры генерации, возвращает готовый PNG.\n"
 "    // Результат оборачивается в неизменяемый RasterImage — дальше по приложению ходят только байты."),
("        var scale = Math.Min(settings.Width / matrix.Width, settings.Height / matrix.Height);",
 "        // Целочисленный масштаб: подбирается минимальный из двух, чтобы код гарантированно\n"
 "        // поместился в кадр и сохранил пропорции модулей (квадраты, а не прямоугольники)."),
("        if (scale < 1) throw new ArgumentException(\"Изображение слишком мало для кода.\");",
 "        // Если целый масштаб меньше единицы — физически невозможно нарисовать код без усечения."),
("        var scaleY = settings.IsLinear ? settings.Height / matrix.Height : scale;",
 "        // У линейных штрихкодов (EAN-13, Code128) матрица низкая и широкая,\n"
 "        // поэтому по высоте её растягивают на весь кадр отдельным масштабом."),
("        var left = (settings.Width - matrix.Width * scale) / 2;",
 "        // Отступы слева и сверху центрируют код внутри изображения."),
("        using var surface = SKSurface.Create(new SKImageInfo(settings.Width, settings.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));",
 "        // Создаём растровую поверхность нужного размера; Bgra8888 + Opaque — быстрый формат\n"
 "        // без альфа-канала, потому что код всегда непрозрачный (фон — белая подложка)."),
("        paint.IsAntialias = false;",
 "        // Отключаем сглаживание: пиксельная точность критична для распознаваемости кода."),
("        surface.Canvas.Clear(SKColors.White);",
 "        // Белый фон заполняет весь кадр, включая поля (quiet zone) вокруг кода."),
("        for (var y = 0; y < matrix.Height; y++)",
 "        // Проходим по всем модулям матрицы и рисуем только «чёрные» (true) клетки.\n"
 "        // Индексация matrix[x, y] соответствует координатам модуля, а не пикселя."),
("        using var image = surface.Snapshot();",
 "        // Снимок поверхности -> изображение -> PNG в памяти -> неизменяемый RasterImage.\n"
 "        // Качество 100 и отсутствие фильтра дают детерминированный результат для одинаковых входных данных."),
])

annotate("QRLab.Image/Generation/Encoders/CodeEncoder.cs", [
("public abstract class CodeEncoder",
 "// Базовый класс стратегии кодирования: инкапсулирует работу с ZXing MultiFormatWriter.\n"
 "// Потомки отвечают за конкретное семейство форматов (QR или линейные/PDF417 штрихкоды),\n"
 "// описывая, какие опции они поддерживают и какой BarcodeFormat использовать."),
("    protected const int LinearMatrixHeight = 64;",
 "    // Фиксированная высота матрицы линейного штрихкода: у таких кодов ширина определяется\n"
 "    // содержимым, а высота произвольна — renderer потом растянет её на весь кадр."),
("    public abstract bool Supports(GenerationOptions options);",
 "    // Признак «этот кодировщик умеет работать с данными опциями» — используется генератором\n"
 "    // для выбора стратегии без проверки типов снаружи."),
("    protected abstract BarcodeFormat GetFormat(GenerationOptions options);",
 "    // Возвращает формат ZXing, соответствующий настройкам пользователя."),
("    protected virtual IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options) =>",
 "    // Подсказки ZXing по умолчанию: только размер защитного поля (quiet zone).\n"
 "    // Для линейных кодов поле удваивается, т.к. EAN/Code128 требуют широких полей для сканеров."),
("    public BitMatrix Encode(string content, GenerationOptions options) => new MultiFormatWriter().encode(",
 "    // Кодирование текста в битовую матрицу. Ноль в качестве желаемого размера означает\n"
 "    // «подобрать минимально возможный автоматом» — реальный пиксельный размер задаст renderer."),
])

annotate("QRLab.Image/Generation/Encoders/QrCodeEncoder.cs", [
("public sealed class QrCodeEncoder : CodeEncoder",
 "// Кодировщик QR-кодов: единственный источник подсказок про коррекцию ошибок и версию символа."),
("    public override bool Supports(GenerationOptions options) => options is QrGenerationOptions;",
 "    // Активируется, когда UI прислал именно QrGenerationOptions (полиморфная модель из Core)."),
("    protected override BarcodeFormat GetFormat(GenerationOptions options) => BarcodeFormat.QR_CODE;",
 "    // Жёстко фиксируем формат ZXing для QR."),
("    protected override IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options)",
 "    // Расширяем базовые подсказки спецификой QR: кодировка UTF-8, уровень коррекции ошибок\n"
 "    // и (опционально) явная версия символа вместо автоматического подбора."),
("        hints[EncodeHintType.CHARACTER_SET] = \"UTF-8\";",
 "        // Явно указываем UTF-8, чтобы кириллица и эмодзи кодировались предсказуемо."),
("        hints[EncodeHintType.ERROR_CORRECTION] = qr.Correction switch",
 "        // Уровень коррекции L/M/Q/H переводится из модели приложения в enum ZXing:\n"
 "        // чем выше уровень, тем больше избыточных данных и тем устойчивее код к мутациям."),
("        if (qr.Version != GenerationDefaults.AutomaticQrVersion) hints[EncodeHintType.QR_VERSION] = qr.Version;",
 "        // Специальная версия-«авто» не передаётся в подсказки — тогда размер выбирает сам ZXing."),
])

annotate("QRLab.Image/Generation/Encoders/BarcodeEncoder.cs", [
("public sealed class BarcodeEncoder : CodeEncoder",
 "// Кодировщик семейства штрихкодов (EAN-13, Code128, PDF417). Один класс на все подтипы:\n"
 "// различие сводится к сопоставлению BarcodeType -> BarcodeFormat через таблицу Formats."),
("    private static readonly Dictionary<BarcodeType, BarcodeFormat> Formats = new()",
 "    // Статическая таблица соответствия: модель приложения -> формат ZXing.\n"
 "    // При добавлении нового типа достаточно дописать сюда пару и класс опций в Core."),
("    public override bool Supports(GenerationOptions options) =>",
 "    // Поддержка = «это BarcodeGenerationOptions И тип есть в таблице» —\n"
 "    // неизвестный подтип приведёт к понятной ошибке «кодировщик не зарегистрирован»."),
("    protected override BarcodeFormat GetFormat(GenerationOptions options)",
 "    // Простое отображение по таблице; вызывается только после успешного Supports."),
("    protected override IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options)",
 "    // Из специфичных подсказок — только UTF-8 для PDF417 (он поддерживает произвольный текст);\n"
 "    // EAN-13 и Code128 работают с фиксированными алфавитами, где кодировка не важна."),
])
print("PART C OK")
