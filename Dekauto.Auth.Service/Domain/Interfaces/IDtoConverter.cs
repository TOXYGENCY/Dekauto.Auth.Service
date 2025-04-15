namespace Dekauto.Auth.Service.Domain.Interfaces
{
    public interface IDtoConverter<Full, Dto>
    {
        Task<Full> FromDtoAsync(Dto dto);

        Dto ToDto(Full full);
        IEnumerable<Dto> ToDtos(IEnumerable<Full> full);
    }
}
