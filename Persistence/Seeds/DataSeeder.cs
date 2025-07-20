using Bogus;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;


namespace Persistence.Seeds
{

    public class DataSeeder
    {
        public static async Task SeedAsync(DataContext context, UserManager<User> userManager)
        {
            var faker = new Faker("en");

            var roles = new[] { "Admin", "Owner", "User" };

            var users = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Bio, f => f.Lorem.Paragraph())
                .RuleFor(u => u.TelegramProfileImage, f => f.Internet.Avatar())
                .RuleFor(u => u.TelegramId, f => f.Random.Number(100000, 999999).ToString())
                .RuleFor(u => u.Image, f => f.Internet.Avatar())
                .RuleFor(u => u.IsActive, f => true)
                .RuleFor(u => u.IsConfirmed, f => true)
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.CreatedAt, f => f.Date.Past())
                .RuleFor(u => u.UpdatedAt, f => f.Date.Recent())
                .Generate(20);

            foreach (var user in users)
            {
                await userManager.CreateAsync(user, "Pass1234");
                await userManager.AddToRoleAsync(user, faker.PickRandom(roles));
            }

            // Plans
            var plans = new Faker<Plan>()
                .RuleFor(p => p.Id, _ => Guid.NewGuid())
                .RuleFor(p => p.Title, f => $"{f.Commerce.ProductAdjective()} Plan")
                .RuleFor(p => p.Code, f => f.Random.AlphaNumeric(6).ToUpper())
                .RuleFor(p => p.Length, f => f.PickRandom(7, 30, 90))
                .RuleFor(p => p.Price, f => f.Random.Int(10, 100))
                .RuleFor(p => p.Discount, f => f.Random.Int(0, 30))
                .RuleFor(p => p.DiscountReason, f => f.Commerce.ProductName())
                .RuleFor(p => p.NumOfDownloads, f => f.Random.Int(10, 100))
                .RuleFor(p => p.NumOfRequest, f => f.Random.Int(5, 50))
                .Generate(5);

            await context.Plans.AddRangeAsync(plans);

            // Subscriptions
            var subscriptions = users.SelectMany(u => new Faker<Subscription>()
                .RuleFor(s => s.Id, _ => Guid.NewGuid())
                .RuleFor(s => s.UserId, _ => u.Id)
                .RuleFor(s => s.PlanId, f => f.PickRandom(plans).Id)
                .RuleFor(s => s.StartedAt, f => f.Date.Past(1))
                .RuleFor(s => s.EndAt, (f, s) => s.StartedAt.AddDays(f.Random.Int(7, 90)))
                .RuleFor(s => s.PaidPrice, f => f.Random.Int(10, 100))
                .RuleFor(s => s.Status, f => f.PickRandom<SubscriptionStatus>())
                .Generate(faker.Random.Int(1, 2)))
                .ToList();

            await context.Subscriptions.AddRangeAsync(subscriptions);

            // Banned Users
            var banned = users.Take(5).Select(u => new BannedUser
            {
                UserId = u.Id,
                Reason = faker.Lorem.Sentence(),
                BannedAt = faker.Date.Past(),
                FreeAt = faker.Random.Bool(0.5f) ? faker.Date.Future() : null
            });

            await context.BannedUsers.AddRangeAsync(banned);

            // OmdbItems
            var omdbItems = new Faker<OmdbItem>()
                .RuleFor(o => o.Id, _ => Guid.NewGuid())
                .RuleFor(o => o.ImdbId, f => f.Random.Guid().ToString())
                .RuleFor(o => o.Title, f => f.Lorem.Sentence(3))
                .RuleFor(o => o.Rated, _ => "PG-13")
                .RuleFor(o => o.Released, f => f.Date.Past().ToShortDateString())
                .RuleFor(o => o.Runtime, f => $"{f.Random.Int(60, 120)} min")
                .RuleFor(o => o.Genres, f => f.Make(2, () => f.Commerce.Categories(1)[0]))
                .RuleFor(o => o.Actors, f => f.Make(3, () => f.Name.FullName()))
                .RuleFor(o => o.Plot, f => f.Lorem.Paragraph())
                .RuleFor(o => o.PlotFa, f => f.Lorem.Paragraph())
                .RuleFor(o => o.Languages, _ => new[] { "English", "Spanish" })
                .RuleFor(o => o.Countries, _ => new[] { "USA" })
                .RuleFor(o => o.Awards, f => f.Commerce.ProductMaterial())
                .RuleFor(o => o.Poster, f => f.Image.PicsumUrl())
                .RuleFor(o => o.Metascore, f => f.Random.Int(1, 100))
                .RuleFor(o => o.ImdbRating, f => f.Random.Double(1, 10))
                .RuleFor(o => o.ImdbVotes, f => f.Random.Int(1000, 100000))
                .RuleFor(o => o.Type, f => f.PickRandom<OmdbItemType>())
                .RuleFor(o => o.BoxOffice, f => $"${f.Random.Int(100000, 10000000)}")
                .RuleFor(o => o.TotalSeasons, f => f.Random.Int(1, 10))
                .RuleFor(o => o.Directors, f => f.Make(2, () => f.Name.FullName()))
                .RuleFor(o => o.Writers, f => f.Make(2, () => f.Name.FullName()))
                .RuleFor(o => o.Year, f => f.Date.Past().Year)
                .RuleFor(o => o.RottenTomatoesScore, f => f.Random.Int(1, 100))
                .Generate(10);

            await context.OmdbItems.AddRangeAsync(omdbItems);

            // Related Seasons, Episodes, Documents
            foreach (var item in omdbItems)
            {
                var seasons = new List<Season>();
                for (int s = 1; s <= item.TotalSeasons; s++)
                {
                    var season = new Season
                    {
                        Id = Guid.NewGuid(),
                        SeasonNumber = s,
                        OmdbItemId = item.Id
                    };

                    var episodes = new List<Episode>();
                    for (int e = 1; e <= faker.Random.Int(5, 10); e++)
                    {
                        var episode = new Episode
                        {
                            Id = Guid.NewGuid(),
                            SeasonId = season.Id,
                            EpisodeNumber = e
                        };

                        var documents = new Faker<Document>()
                            .RuleFor(d => d.Id, _ => Guid.NewGuid())
                            .RuleFor(d => d.FileName, f => f.System.FileName())
                            .RuleFor(d => d.MimeType, f => f.System.MimeType())
                            .RuleFor(d => d.FileId, f => f.Random.Guid().ToString())
                            .RuleFor(d => d.UniqueFileId, f => f.Random.Guid().ToString())
                            .RuleFor(d => d.FileSize, f => f.Random.Int(1000, 1000000))
                            .RuleFor(d => d.ChatName, f => f.Internet.UserName())
                            .RuleFor(d => d.MessageId, f => f.Random.Int(1, 10000))
                            .RuleFor(d => d.ChatId, f => f.Random.Int(1, 100000))
                            .RuleFor(d => d.IsSubbed, f => f.Random.Bool())
                            .RuleFor(d => d.Resolution, _ => "1080p")
                            .RuleFor(d => d.Codec, _ => "H.264")
                            .RuleFor(d => d.Encoder, _ => "x264")
                            .RuleFor(d => d.Type, f => f.PickRandom<DocumentType>())
                            .RuleFor(d => d.CreatedAt, f => f.Date.Past())
                            .RuleFor(d => d.UpdatedAt, f => f.Date.Recent())
                            .Generate(faker.Random.Int(1, 4));

                        episode.Documents = documents;
                        episodes.Add(episode);
                    }

                    season.Episodes = episodes;
                    seasons.Add(season);
                }

                item.Seasons = seasons;
            }

            await context.SaveChangesAsync();
        }
    }
}