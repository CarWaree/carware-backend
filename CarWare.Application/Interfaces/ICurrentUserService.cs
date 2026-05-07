public interface ICurrentUserService
{
    string UserId { get; }
    int? ServiceCenterId { get; }
}