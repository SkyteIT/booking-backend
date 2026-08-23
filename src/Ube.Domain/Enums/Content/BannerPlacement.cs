namespace Ube.Domain.Enums.Content;

public enum BannerPlacement
{
    LandingPage = 1,
    ExplorePage = 2,
    SearchResults = 3,
    CategoryPage = 4,
    EventPage = 5,

    // Backward-compatible aliases for older clients and existing references.
    Homepage = LandingPage,
    HomepageHero = LandingPage,
    HomepageBanner = LandingPage,
    LandingPageHero = LandingPage,
    LandingPageBanner = LandingPage
}
