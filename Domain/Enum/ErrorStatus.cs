using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Domain.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorStatus
    {
        Expired = 1,
        Invalid = 2,
        Empty = 3
    }
}
