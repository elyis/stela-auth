namespace STELA_AUTH.Shared.Entities
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
    }
}