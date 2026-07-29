using System.Text.Encodings.Web;
using System.Text.Json;

namespace AigioLTemplate.Constants;

static partial class SerializerConstants
{
    public static readonly JsonWriterOptions DefaultJsonWriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static readonly JsonDocumentOptions DefaultJsonDocumentOptions = new()
    {
        AllowDuplicateProperties = true,
        AllowTrailingCommas = true,
    };
}
