namespace DoucmentManagmentSys.Models
{
    public class ArchivedVersion
    {

        public int Id { get; set; }

        public byte[]? Content { get; set; }
        public string? Version { get; set; }

    public ArchivedDocument? Document { get; set; }
    }
}
