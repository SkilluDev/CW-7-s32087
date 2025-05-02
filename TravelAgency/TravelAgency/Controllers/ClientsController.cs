using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Models.DTOs;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    [Route("{id}/trips")]
    public async Task<IActionResult> GetAllTripsById([FromRoute] int id)
    {
        return Ok(await dbService.GetTripsByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientPostDTO client)
    {
        try
        {
            var result = await dbService.AddClientAsync(client);
            return Created($"clients/{result.Id}", result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut]
    [Route("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> AddTripToClient([FromRoute] int clientId, [FromRoute] int tripId)
    {
        try
        {
            await dbService.AddTripToClientAsync(clientId, tripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }

    [HttpDelete]
    [Route("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClient([FromRoute] int clientId, [FromRoute] int tripId)
    {
        try
        {
            await dbService.RemoveTripFromClientAsync(clientId, tripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
}