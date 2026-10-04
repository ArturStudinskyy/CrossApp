namespace Core.Dto;

public sealed record WarehouseDto(
    string Id,
    string Name,
    string City,
    string? Address = null
) : ImportEntityDto;