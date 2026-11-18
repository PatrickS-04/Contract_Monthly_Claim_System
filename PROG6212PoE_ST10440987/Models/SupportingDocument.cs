namespace PROG6212PoE_ST10440987.Models
{
    public class SupportingDocument
    {
        public int SupportingDocumentId { get; set; }
        public long maxFileLimit { get; set; }
        public string fileType { get; set; }
        public byte[] dataFromFile { get; set; }
        public int ClaimId { get; set; }
        public Claim Claim { get; set; }
    }
}
