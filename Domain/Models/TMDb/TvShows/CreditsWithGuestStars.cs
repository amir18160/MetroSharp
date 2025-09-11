namespace Domain.Models.TMDb.TvShows;

public class CreditsWithGuestStars : Credits
{
    public List<Cast> GuestStars { get; set; }
}