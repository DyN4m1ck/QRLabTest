namespace QRLab.Core.Models.Generation;

/// <summary>Источник содержимого кодов.</summary>
public enum ContentSource
{
    /// <summary>Шаблон строки с плейсхолдером {num}, который заменяется номером образца.</summary>
    NumberedTemplate,
    /// <summary>Произвольный текст без подстановки номера.</summary>
    Text
}
