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

annotate("QRLab.Image/Recognition/ZxingCodeRecognizer.cs", [
("public sealed class ZxingCodeRecognizer : ICodeRecognizer",
 "// Реализация распознавания на ZXing BarcodeReaderGeneric. Ключевые аспекты:\n"
 "// 1) последовательная обработка образцов с общей кнопкой отмены;\n"
 "// 2) персональный тайм-аут на каждый образец (linked CancellationTokenSource);\n"
 "// 3) ограничение числа одновременных декодеров семафором — защита от зависших нативных вызовов;\n"
 "// 4) замер времени декодирования Stopwatch — попадает в CSV-отчёт."),
("    private readonly SemaphoreSlim _workers = new(2);",
 "    // Декодирование — CPU-тяжёлая и частично нативная операция; две параллельные попытки —\n"
 "    // компромисс между скоростью и отзывчивостью UI. Тайм-аут не убивает поток, поэтому число\n"
 "    // фоновых задач жёстко ограничено."),
("            var elapsed = Stopwatch.StartNew();",
 "            // Засекаем время на весь проход по образцу: подготовка растра + декодирование."),
("            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);",
 "            // Linked CTS объединяет внешнюю отмену и локальный дедлайн: сработает любая из причин."),
("            deadline.CancelAfter(options.TimeoutMilliseconds);",
 "            // Индивидуальный тайм-аут — плохой код не должен блокировать обработку всего набора."),
("                await _workers.WaitAsync(deadline.Token);",
 "                // Захват слота может не состояться до истечения дедлайна — тогда это тоже Timeout."),
("                var work = Task.Run(() =>",
 "                // Само декодирование уходит в ThreadPool, чтобы не блокировать UI-поток Avalonia.\n"
 "                // CancellationToken.None внутри: отменить нативный вызов ZXing невозможно,\n"
 "                // поэтому мы просто перестаём ждать результат, а слот освобождается в finally."),
("                var decoded = await work.WaitAsync(deadline.Token);",
 "                // WaitAsync бросает OperationCanceledException по тайм-ауту — задача продолжает\n"
 "                // работать «в фоне» и корректно освобождает семафор, но её результат игнорируется."),
("                var status = decoded.Error is not null || decoded.Content is null ? Status.Failed",
 "                // Трёхзначная логика итога: ошибка/пусто -> Failed; совпало с ожидаемым текстом\n"
 "                // образца -> Success; прочитали другое -> Mismatch (например, EAN исказил данные)."),
("            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)",
 "            // Фильтр исключает случай «отменил пользователь»: тогда исключение уходит выше\n"
 "            // и прерывает весь процесс. Сюда попадаем только по индивидуальному тайм-ауту."),
("    private static (string? Content, string? Error) Decode(SamplePreview sample, RecognitionOptions options)",
 "    // Ядро: превращает PNG образца в массив BGRA и вызывает ZXing. Возвращает кортеж,\n"
 "    // чтобы отличить «код не найден» (Content=null) от исключения декодера (Error)."),
("            var source = sample.Frames.LastOrDefault()?.Image ?? sample.Image;",
 "            // Распознаём всегда последний кадр цепочки мутаций; если мутаций нет — исходное изображение."),
("            var bytes = new byte[source.Width * source.Height * 4];",
 "            // Плоский массив без выравнивания строк: ZXing ожидает ровно width*height*4 байта."),
("                data.Slice(y * pixels.RowBytes, source.Width * 4).CopyTo(bytes.AsSpan(y * source.Width * 4));",
 "                // Построчное копирование убирает возможное выравнивание RowBytes из Skia-буфера."),
("            var reader = new BarcodeReaderGeneric",
 "            // Generic-ридер работает с произвольным LuminanceSource (в отличие от Bitmap-версии).\n"
 "            // AutoRotate/TryHarder/TryInverted включаются только в «Enhanced» режиме:\n"
 "            // он медленнее, но ловит перевёрнутые и повёрнутые коды после перспективы."),
("                PossibleFormats = [BarcodeFormat.QR_CODE, BarcodeFormat.EAN_13, BarcodeFormat.CODE_128, BarcodeFormat.PDF_417]",
 "                // Белый список форматов = ровно те, что умеет генерировать приложение;\n"
 "                // сужение поиска ускоряет декодирование и снижает ложные срабатывания."),
("            var luminance = new RGBLuminanceSource(bytes, source.Width, source.Height, RGBLuminanceSource.BitmapFormat.BGRA32);",
 "            // ZXing оперирует яркостью (grayscale): конвертация BGRA->яркость происходит внутри источника."),
("            return result is null ? (null, \"Код не найден.\") : (result.Text, null);",
 "            // null результата — штатная ситуация (мутация уничтожила код), а не исключение."),
("        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)",
 "        // Повреждённые данные или ошибки растра фиксируются как Error конкретного образца —\n"
 "        // весь батч распознавания не должен падать из-за одного битого файла."),
])

annotate("QRLab.Image/Recognition/Reports/CsvColumn.cs", [
("internal sealed record CsvColumn(string Name, Func<RecognitionReportRow, string?> Value);",
 "// Описание одной колонки CSV: заголовок + функция извлечения значения из строки отчёта.\n"
 "// Такой табличный подход позволяет менять состав/порядок колонок в одном месте (см. Columns)."),
])

annotate("QRLab.Image/Recognition/Reports/CsvRecognitionReportExporter.cs", [
("public sealed class CsvRecognitionReportExporter : IRecognitionReportExporter",
 "// Экспорт результатов распознавания в CSV (разделитель «;» — дружит с русской локали Excel).\n"
 "// Все значения экранируются кавычками, файл пишется через временный «черновик» и атомарно\n"
 "// перемещается на место, чтобы при сбое пользователь не получил полузаписанный отчёт."),
("    private static readonly CsvColumn[] Columns =",
 "    // Декларативное описание отчёта: номер, образец, формат, ожидания/факт, статус,\n"
 "    // время в мс (F3 — три знака), цепочка мутаций и текст ошибки. Числа форматируются\n"
 "    // через InvariantCulture, чтобы десятичный разделитель был стабильным."),
("        var temporary = Path.Combine(directory, $\".qrlab-csv-{Guid.NewGuid():N}\");",
 "        // Скрытый временный файл в той же папке: File.Move поверх целевого пути атомарен\n"
 "        // в пределах одного тома, поэтому готовый CSV никогда не бывает «неполным»."),
("            await using (var writer = new StreamWriter(temporary, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))",
 "            // UTF-8 с BOM — требование Excel: без маркера кириллические заголовки отображаются кракозябрами."),
("            File.Move(temporary, path, overwrite: true);",
 "            // Финальная публикация файла; отмена после этой точки уже не повредит записи."),
("        finally { if (File.Exists(temporary)) File.Delete(temporary); }",
 "        // Гарантированная очистка черновика при любом исходе (включая отмену и исключение)."),
])

annotate("QRLab.Image/Storage/SampleSetStorage.cs", [
("public sealed class SampleSetStorage : ISampleSetStorage",
 "// Хранилище наборов образцов на диске: JSON-манифест qrlab-set.json + PNG-файлы рядом.\n"
 "// Принципы: (1) версионирование формата (CurrentVersion=3) — наборы другой версии отвергаются\n"
 "// явно; (2) лимиты размеров против повреждённых файлов; (3) атомарность сохранения\n"
 "// через staging-папку и Directory.Move; (4) при загрузке — полная повторная валидация данных."),
("    public const string ManifestName = \"qrlab-set.json\";",
 "    // Имя манифеста — контракт формата; Load ищет именно его в выбранной папке."),
("    private const int CurrentVersion = 3;",
 "    // Версия схемы сериализации. Меняется при любом несовместимом изменении Stored*-записей."),
("    private const int MaximumManifestBytes = 64 * 1024 * 1024;",
 "    // Потолок размера манифеста защищает от чтения гигантского/повреждённого JSON в память."),
("    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };",
 "    // Отступы делают файл читаемым для человека — набор можно отредактировать руками."),
("    public SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options)",
 "    // Сохранение всей цепочки: оригиналы и все кадры мутаций каждого образца."),
("        if (samples.Count is < GenerationLimits.MinimumCount or > GenerationLimits.MaximumCount)",
 "        // Количество сверяется с теми же границами Core, что и при генерации — единый источник правды."),
("        if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())",
 "        // Разрешаем писать только в новую или пустую папку: нельзя случайно перезаписать чужие файлы."),
("        var staging = Path.Combine(parent, $\".qrlab-{Guid.NewGuid():N}\");",
 "        // Пишем во временную папку-«черновик» рядом с целевой; если что-то упадёт посередине,\n"
 "        // конечная папка останется нетронутой (staging удаляется в finally)."),
("            var images = new Dictionary<RasterImage, string>(ReferenceEqualityComparer.Instance);",
 "            // Дедупликация по ссылке: несколько кадров с одинаковым объектом RasterImage\n"
 "            // (например, «нулевые» мутации возвращают source как есть) пишутся на диск один раз."),
("            string WriteImage(RasterImage image, string name)",
 "            // Локальная функция записи PNG: проверили кэш -> записали -> запомнили имя файла."),
("                var prefix = $\"sample-{index + 1:0000}\";",
 "                // Человекочитаемые имена: sample-0001.png, sample-0001-frame-0001.png —\n"
 "                // порядок нумерации виден прямо в проводнике."),
("            File.WriteAllText(Path.Combine(staging, ManifestName),",
 "            // Манифест пишется последним: он ссылается на уже существующие файлы изображений."),
("            Directory.Move(staging, directory);",
 "            // Атомарная публикация всего набора одним переименованием в пределах тома."),
("    public SampleSet Load(string directory)",
 "    // Загрузка: прочитать манифест -> проверить версию и лимиты -> подтянуть PNG ->\n"
 "    // восстановить полиморфные опции -> повторно валидировать всё и собрать SampleSet."),
("        if (new FileInfo(manifest).Length > MaximumManifestBytes) throw new InvalidDataException(\"Описание набора слишком велико.\");",
 "        // Размер проверяем ДО ReadAllText — иначе можно получить OutOfMemory на битом файле."),
("        catch (JsonException exception) { throw new InvalidDataException(\"Описание набора повреждено.\", exception); }",
 "        // Любая ошибка парсинга приводится к единому типу InvalidDataException для UI-обработчика."),
("        if (document.Version != CurrentVersion) throw new InvalidDataException(\"Версия формата набора не поддерживается.\");",
 "        // Строгая политика совместимости вместо эвристик «попробуем прочитать как получится»."),
("        var images = new Dictionary<string, RasterImage>(StringComparer.Ordinal);",
 "        // Кэш по имени файла: один PNG читается один раз даже на десятки ссылок из манифеста."),
("            if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)",
 "            // Защита от path traversal: имя обязано быть простым именем файла .png без подпапок —\n"
 "            // вручную отредактированный манифест не должен дать прочитать произвольный файл диска."),
("            if (new FileInfo(path).Length > MaximumImageBytes) throw new InvalidDataException(\"Изображение слишком велико.\");",
 "            // Отдельный (больший) лимит на растровые данные."),
("                stored.Settings.Validate();",
 "                // Параметры генерации из файла проходят ту же валидацию, что и созданные в UI:\n"
 "                // набор, загруженный с диска, не имеет права содержать невозможные значения."),
("                if (original.Width != stored.Settings.Width || original.Height != stored.Settings.Height)",
 "                // Кросс-проверка целостности: реальный размер PNG обязан совпадать с заявленным."),
("                    frame.Options.Validate();",
 "                    // Опции мутаций тоже полиморфны и валидируются; битый кадр = ошибка всего набора."),
("            if (samples.Select(s => s.Number).Distinct().Count() != samples.Length)",
 "            // Номера образцов уникальны — иначе ссылки в отчётах и UI стали бы неоднозначными."),
("        catch (ArgumentException exception) { throw new InvalidDataException(\"Некорректные данные набора.\", exception); }",
 "        // Ошибки валидации моделей конвертируем в тот же тип, что ожидает слой представления."),
])

annotate("QRLab.Image/Storage/Serialization/StoredMutation.cs", [
("internal sealed record StoredMutation(MutationOptions Options, string Image);",
 "// DTO одного кадра мутации для JSON: сами параметры (сериализуются полиморфно через атрибуты\n"
 "// в Core) плюс имя файла PNG с результатом. internal — формат хранения не часть публичного API."),
])
annotate("QRLab.Image/Storage/Serialization/StoredSample.cs", [
("internal sealed record StoredSample(int Number, string Name, string Content, GenerationOptions Settings,",
 "// DTO образца в манифесте: метаданные, полиморфные настройки генерации, имя файла оригинала\n"
 "// и массив кадров мутаций. Изображения в JSON не вкладываются — только ссылки на файлы."),
])
annotate("QRLab.Image/Storage/Serialization/StoredSetDocument.cs", [
("internal sealed record StoredSetDocument(int Version, StoredSample[] Samples);",
 "// Корень JSON-манифеста: версия формата + все образцы набора."),
])
print("PART B OK")
