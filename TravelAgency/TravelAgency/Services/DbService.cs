using Microsoft.Data.SqlClient;
using TravelAgency.Models.DTOs;

namespace TravelAgency.Services;

public class DbService(IConfiguration config)
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    public async Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync()
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        const string sql = "select IdTrip, Name, Description, DateFrom, DateTo, MaxPeople from Trip";
        
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TripGetDTO()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5)
            });
        }

        return result;
    }
}