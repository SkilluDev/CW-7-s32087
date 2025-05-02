namespace TravelAgency.Models.DTOs;

public class TripGetDTO
{
    public int Id { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    
    public List<CountryGetDTO> CountryList { get; set; }
}