namespace QRLab.Core.Models.Recognition;

/// <summary>Итоговые состояния распознавания одного изображения.</summary>
public enum Status
{
    /// <summary>Код считан и совпадает с ожидаемым содержимым.</summary>
    Success,
    /// <summary>Код не удалось найти/декодировать.</summary>
    Failed,
    /// <summary>Декодирование не уложилось в тайм-аут.</summary>
    Timeout,
    /// <summary>Код считан, но текст не совпадает с ожидаемым.</summary>
    Mismatch
}
