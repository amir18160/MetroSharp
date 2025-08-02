

namespace Domain.Models.TMDb.General
{
    public class TranslationData
    {

        public string Name { get; set; }



        private string Title
        {
            set => Name = value;
        }

        public string Overview { get; set; }

        private string Biography
        {
            set => Overview = value;
        }


        public string HomePage { get; set; }


        public string Tagline { get; set; }


        public int Runtime { get; set; }
    }
}
