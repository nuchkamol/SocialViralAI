using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SocialViralAI.Models;

namespace SocialViralAI.Services
{
    public class YouTubeService
    {
  

        private readonly HttpClient _http;
        private readonly string _apiKey;

        public YouTubeService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiKeys:YouTube"];
        }


        public async Task<List<Video>> GetVideos(string keyword, int maxResults)
        {
            var apiKey = _apiKey;
            string url;

            var videos = new List<Video>();

            if (string.IsNullOrEmpty(keyword))
            {
                // 🔥 trending
                url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&chart=mostPopular&regionCode=TH&maxResults={maxResults}&key={apiKey}";
            }
            else
            {
                // 🔍 search
                var search_url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={keyword}&maxResults={maxResults}&type=video&key={apiKey}";

                var searchJson = await _http.GetStringAsync(search_url); // ✅ แก้ตรงนี้
                var searchDoc = JsonDocument.Parse(searchJson);

                var videoIds = searchDoc.RootElement
                    .GetProperty("items")
                    .EnumerateArray()
                    .Select(x => x.GetProperty("id").TryGetProperty("videoId", out var vid) ? vid.GetString() : null)
                    .Where(x => x != null)
                    .ToList();

                url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={string.Join(",", videoIds)}&key={apiKey}";
            }

            var json = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(json);

            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                videos.Add(new Video
                {
                    Title = item.GetProperty("snippet").GetProperty("title").GetString(),
                    Views = long.Parse(item.GetProperty("statistics").GetProperty("viewCount").GetString()),
                    Likes = item.GetProperty("statistics").TryGetProperty("likeCount", out var like)
                        ? long.Parse(like.GetString())
                        : 0,
                    VideoId = item.GetProperty("id").GetString()
                });
            }

            // 🔥 STEP 3: sort (สำคัญสุด)
            return videos
                .OrderByDescending(v => v.Likes)
                .ToList();
        }


        public async Task<List<Video>> GetShortVideos(string keyword, int maxResults)
        {
            var apiKey = _apiKey;
            string url;

            var videos = new List<Video>();



            if (string.IsNullOrEmpty(keyword))
            {
                // 🔥 trending
                keyword = "shorts"; // 🔥 บังคับเลย

            }
                // 🔍 search
                var search_url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={keyword}&maxResults={maxResults}&type=video&key={apiKey}";

                var searchJson = await _http.GetStringAsync(search_url); // ✅ แก้ตรงนี้
                var searchDoc = JsonDocument.Parse(searchJson);

                var videoIds = searchDoc.RootElement
                    .GetProperty("items")
                    .EnumerateArray()
                    .Select(x => x.GetProperty("id").TryGetProperty("videoId", out var vid) ? vid.GetString() : null)
                    .Where(x => x != null)
                    .ToList();

                url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={string.Join(",", videoIds)}&key={apiKey}";
            

            var json = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(json);

            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                bool isShort = false;

                // 🔹 1. เช็ค duration (หลัก)
                if (item.TryGetProperty("contentDetails", out var content) &&
                    content.TryGetProperty("duration", out var durationProp))
                {
                    var duration = durationProp.GetString();

                    try
                    {
                        TimeSpan durationTime = XmlConvert.ToTimeSpan(duration);

                        if (durationTime.TotalSeconds <= 90) // 🔥 ผ่อนเป็น 90 วิ
                            isShort = true;
                    }
                    catch
                    {
                        // กัน parse พัง
                    }
                }

                // 🔹 2. เช็ค title (fallback โคตรสำคัญ)
                var title = item.GetProperty("snippet").GetProperty("title").GetString()?.ToLower() ?? "";

                if (title.Contains("short"))
                    isShort = true;

                // 🔹 3. เช็คแนวตั้ง (fallback)
                if (item.GetProperty("snippet").GetProperty("thumbnails").TryGetProperty("high", out var thumb))
                {
                    int width = thumb.GetProperty("width").GetInt32();
                    int height = thumb.GetProperty("height").GetInt32();

                    if (height > width)
                        isShort = true;
                }

                // ❗ filter ออก
                if (!isShort) continue;

                // 🔹 4. views
                var views = item.GetProperty("statistics").TryGetProperty("viewCount", out var v)
                    ? long.Parse(v.GetString())
                    : 0;

                // 🔹 5. likes
                var likes = item.GetProperty("statistics").TryGetProperty("likeCount", out var l)
                    ? long.Parse(l.GetString())
                    : 0;

                // 🔹 6. videoId
                string videoId = "";

                if (item.GetProperty("id").ValueKind == JsonValueKind.Object)
                    videoId = item.GetProperty("id").GetProperty("videoId").GetString();
                else
                    videoId = item.GetProperty("id").GetString();

                // 🔹 7. สร้าง object
                var video = new Video
                {
                    Title = title,
                    Views = views,
                    Likes = likes,
                    VideoId = videoId,
                    IsShort = true,
                    ViralScore = (int)((likes * 5) + (views / 2000))
                };

                videos.Add(video);
            }

            // 🔥 STEP 3: sort (สำคัญสุด)
            return videos
                .OrderByDescending(v => v.ViralScore)
                .ToList();
        }


    
        //public async Task<List<Video>> GetVideos()
        //{
        //    var apiKey = _apiKey;

        //    var searchUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q=ข่าว&maxResults=5&type=video&key={apiKey}";

        //    var searchJson = await _http.GetStringAsync(searchUrl);
        //    var searchDoc = JsonDocument.Parse(searchJson);

        //    var videoIds = searchDoc.RootElement
        //        .GetProperty("items")
        //        .EnumerateArray()
        //        .Select(x => x.GetProperty("id").GetProperty("videoId").GetString())
        //        .ToList();

        //    var statsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={string.Join(",", videoIds)}&key={apiKey}";

        //    var statsJson = await _http.GetStringAsync(statsUrl);
        //    var statsDoc = JsonDocument.Parse(statsJson);

        //    var videos = new List<Video>();

        //    foreach (var item in statsDoc.RootElement.GetProperty("items").EnumerateArray())
        //    {
        //        videos.Add(new Video
        //        {
        //            Title = item.GetProperty("snippet").GetProperty("title").GetString(),
        //            Views = long.Parse(item.GetProperty("statistics").GetProperty("viewCount").GetString()),
        //            Likes = item.GetProperty("statistics").TryGetProperty("likeCount", out var like)
        //                ? long.Parse(like.GetString())
        //                : 0,
        //            VideoId = item.GetProperty("id").GetString()
        //        });
        //    }

        //    return videos;
        //}
    }
}