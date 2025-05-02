using Microsoft.AspNetCore.Mvc;
using TravelAgency.Services;

namespace TravelAgency.Controllers;


[ApiController]
[Route("[controller]")]
public class TripsController(IDbService dbService) : ControllerBase
{
    //Get all trips available including the countries that they are happening in
    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        return Ok(await dbService.GetTripsDetailsAsync());
    }
}