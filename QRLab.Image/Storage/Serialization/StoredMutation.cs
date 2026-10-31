using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Storage.Serialization;

// DTO одного кадра мутации для JSON: сами параметры (сериализуются полиморфно через атрибуты
// в Core) плюс имя файла PNG с результатом. internal — формат хранения не часть публичного API.
internal sealed record StoredMutation(MutationOptions Options, string Image);