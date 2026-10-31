namespace QRLab.Core.Models.Generation;

/// Источник содержимого кодов.
public enum ContentSource
{
    /// Шаблон строки с плейсхолдером {num}, который заменяется номером образца.
    NumberedTemplate,
    /// Произвольный текст без подстановки номера.
    Text
}
