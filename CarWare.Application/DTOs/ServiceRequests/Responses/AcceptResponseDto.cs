using System;

public class AcceptResponseDto
{
    public int Id { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public DateTime AcceptedAt { get; set; }
}