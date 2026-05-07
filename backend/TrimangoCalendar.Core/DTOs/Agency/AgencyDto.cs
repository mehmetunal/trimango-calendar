public class AgencyDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; }
    public string TaxNumber { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string ContactPerson { get; set; }
    public string Type { get; set; }
    public string TypeDescription { get; set; }
    public decimal DefaultCommissionRate { get; set; }
    public int AuthorizedPropertyCount { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

