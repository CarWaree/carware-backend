using System;

public class RejectResponseDto
{
    public int Id { get; set; } 
    public ServiceRequestStatus Status { get; set; }
    public DateTime RejectedAt { get; set; }
}