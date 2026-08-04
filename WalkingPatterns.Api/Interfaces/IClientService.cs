using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Interfaces
{
    public interface IClientService
    {
        Task<List<Client>> GetAllClientsAsync();
        Task<Client?> GetClientByIdAsync(int id);
        Task<Client> AddClientAsync(Client client);
        Task<Client?> UpdateClientAsync(int id, Client client);
        Task<bool> DeleteClientAsync(int id);
    }
}