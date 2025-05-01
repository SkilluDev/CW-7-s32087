namespace TravelAgency.Models;

public class Trip
{
    public int Id { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
}