using System.Collections.Generic;

public class ServiceRequestListResponse
{
    public int Total { get; set; }
    public CountsDto Counts { get; set; } = new();
    public List<ServiceRequestDto> Data { get; set; } = new();
}