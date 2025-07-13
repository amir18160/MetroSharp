namespace Infrastructure.ProwlarrWrapper.Models
{
    public class CapabilityCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SubCategory> SubCategories { get; set; }
    }
}