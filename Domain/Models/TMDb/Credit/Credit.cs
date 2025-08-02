using Domain.Models.TMDb;
using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Credit
{
    public class Credit
    {
        public CreditType CreditType { get; set; }     
        public string Department { get; set; }    
        public string Id { get; set; }
        public string Job { get; set; }
        public CreditMedia Media { get; set; }   
        public MediaType MediaType { get; set; }
        public CreditPerson Person { get; set; }
    }
}