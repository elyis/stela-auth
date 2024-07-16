namespace STELA_AUTH.Core.Entities.Request
{
    public class UpdateProfileBody
    {
        public Guid AccountId { get; set; }
        public string FileName { get; set; }
    }
}