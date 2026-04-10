using Microsoft.AspNetCore.Mvc;
using SocialViralAI.Models;
using SocialViralAI.Services;
public class HomeController : Controller
{
    private readonly YouTubeService _yt;
    private readonly AiService _ai;

    public HomeController(YouTubeService yt, AiService ai)
    {
        _yt = yt;
        _ai = ai;
    }

    //public async Task<IActionResult> Index()
    //{
    //    var videos = await _yt.GetVideos();
    //    var insight = await _ai.Analyze(videos);

    //    ViewBag.Insight = insight;
    //    var maxViews = videos.Max(v => v.Views);
    //    ViewBag.MaxViews = maxViews;
    //    return View(videos);
    //}

    public async Task<IActionResult> Index(string keyword = "ข่าว", int maxResults = 5)
    {
        ViewBag.Keyword = keyword;
        ViewBag.MaxResults = maxResults;
  
        var videos = await _yt.GetVideos(keyword, maxResults);
        var maxViews = videos.Max(v => v.Views);
        ViewBag.MaxViews = maxViews;
        var insight = await _ai.Analyze(videos);
 
        ViewBag.Insight = insight;
        return View(videos);
    }

    public async Task<IActionResult> Short(string keyword = "", int maxResults = 5)
    {
        var videos = await _yt.GetShortVideos(keyword, maxResults);

        var shorts = videos.Where(v => v.IsShort).ToList();

        // 🔥 เรียงจากปังสุด
        shorts = shorts.OrderByDescending(v => v.ViralScore).ToList();

        // 🏆 ตัวแรก = top
        if (shorts.Any())
        {
            shorts[0].IsTop = true;
        }

        //var insight = await _ai.AnalyzeShorts(shorts);

        //ViewBag.Insight = insight;

        return View(shorts);
    }
}