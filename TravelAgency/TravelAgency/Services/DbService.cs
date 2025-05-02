using Microsoft.Data.SqlClient;
using TravelAgency.Exceptions;
using TravelAgency.Models;
using TravelAgency.Models.DTOs;

namespace TravelAgency.Services;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync();
    public Task<IEnumerable<TripGetDTO>> GetTripsByIdAsync(int id);
    public Task<Client> AddClientAsync(ClientPostDTO client);
    public Task AddTripToClientAsync(int clientId, int tripId);
    Task RemoveTripFromClientAsync(int clientId, int tripId);
}

public class DbService(IConfiguration config) : IDbService
{
    
    private readonly string? _connectionString = config.GetConnectionString("Default");
    public async Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync()
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        const string sql = "select Trip.IdTrip, Trip.Name, Description, DateFrom, DateTo, MaxPeople, Country.IdCountry, Country.Name from Trip join Country_Trip ON Country_Trip.IdTrip = Trip.IdTrip JOIN Country ON Country_Trip.IdCountry = Country.IdCountry";
        
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var found = result.FirstOrDefault((a) => a.Id == reader.GetInt32(0));
            CountryGetDTO country = new CountryGetDTO()
            {
                Id = reader.GetInt32(6),
                Name = reader.GetString(7),
            };
            if (found != null)
            {
                found.CountryList.Add(country);
                continue;
            }
            result.Add(new TripGetDTO()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                CountryList = [country]
            });
        }

        return result;
    }

    public async Task<IEnumerable<TripGetDTO>> GetTripsByIdAsync(int id)
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        if (!await CheckIfClientExistsAsync(id))
        {
            return new List<TripGetDTO>();
        }
        
        const string sql = "select Trip.IdTrip, Client.IdClient from Trip JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip JOIN Client ON Client_Trip.IdClient = Client.IdClient WHERE Client.IdClient = @ClientId";
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ClientId", id);
        
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var trips = await GetTripsDetailsAsync();
        while (await reader.ReadAsync())
        {
            result.Add(trips.First((t) => t.Id == reader.GetInt32(0)));
        }
        
        return result;
    }

    public async Task<Client> AddClientAsync(ClientPostDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        const string sql = "insert into Client (FirstName, LastName, Email, Telephone, Pesel) values (@FirstName, @LastName, @Email, @Telephone, @Pesel); select SCOPE_IDENTITY();";
        
        var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new Client()
        {
            Id = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel,
        };



    }

    public async Task AddTripToClientAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        const string sql = "insert into Client_Trip (IdClient, IdTrip, RegisteredAt) values (@IdClient, @IdTrip, @CurrentTime)";
        
        await using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@IdClient", clientId);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        command.Parameters.AddWithValue("@CurrentTime", Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")));
        
        await connection.OpenAsync();
        
        var lines = await command.ExecuteNonQueryAsync();

        if (lines == 0)
        {
            throw new NotFoundException($"No trip with id {tripId} or client with id {clientId} found");
        }
    }

    public async Task RemoveTripFromClientAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        const string sql = "delete from Client_Trip where IdClient = @IdClient and IdTrip = @IdTrip";
        
        await using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@IdClient", clientId);
        command.Parameters.AddWithValue("@IdTrip", tripId);
        
        await connection.OpenAsync();
        var lines = await command.ExecuteNonQueryAsync();

        if (lines == 0)
        {
            throw new NotFoundException($"No trip with id {tripId} and client with id {clientId} found");
        }
    }

    public async Task<bool> CheckIfClientExistsAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        const string sql = "select count(1) from Client WHERE IdClient = @IdClient";
        
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", id);
        
        await connection.OpenAsync();
        
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            return true;
        }

        return false;

    }
}