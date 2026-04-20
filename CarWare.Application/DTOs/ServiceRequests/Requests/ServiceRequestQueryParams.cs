public class ServiceRequestQueryParams
{
    public string? Status { get; set; } = "all";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? Search { get; set; }
}