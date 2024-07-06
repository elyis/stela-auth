using System.Text.Json.Serialization;

namespace STELA_AUTH.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountRole
    {
        User,
        Admin
    }
}