using System;
public class ServiceRequestDto
{
    public int Id { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public CarDto Car { get; set; } = new();
    public ClientDto Client { get; set; } = new();
}