using System;

public class CompleteResponseDto
{
    public int Id { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public DateTime CompletedAt { get; set; }
}