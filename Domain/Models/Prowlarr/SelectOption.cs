namespace Domain.Models.Prowlarr
{
    public class SelectOption
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string Hint { get; set; }
        public int ParentValue { get; set; }
    }

}