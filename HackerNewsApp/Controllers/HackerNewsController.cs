using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HackerNewsApp.Models;

namespace hackernews.Controllers
{
    public class HackerNewsController : Controller
    {
        private IMemoryCache cache;

        public HackerNewsController(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }

        public async Task<ActionResult> Index(string searchStory)
        {
            List<HackerStory> stories = new List<HackerStory>();
            stories = await BuildStoryList(searchStory, stories);
            return View(stories);
        }

        private async Task<List<HackerStory>> BuildStoryList(string searchString, List<HackerStory> stories)
        {
            string HackerApi = "https://hacker-news.firebaseio.com/v0/beststories.json";
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(HackerApi);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var bestIds = JsonConvert.DeserializeObject<List<int>>(result);

                    var tasks = bestIds.Select(GetStoryAsync);
                    stories = (await Task.WhenAll(tasks)).ToList();
                }
                else
                {
                    ViewData["Error"] = "true";
                }
            }
            return stories;
        }

        private async Task<HackerStory> GetStoryAsync(int storyId)
        {
            string StoryTemp = "https://hacker-news.firebaseio.com/v0/item/{0}.json";
            return await cache.GetOrCreateAsync<HackerStory>(storyId,
                async cacheEntry =>
                {
                    HackerStory HackerStory = new HackerStory();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var message = await httpClient.GetAsync(string.Format(StoryTemp, storyId));

                        if (message.IsSuccessStatusCode)
                        {
                            var story = message.Content.ReadAsStringAsync().Result;
                            HackerStory = JsonConvert.DeserializeObject<HackerStory>(story);
                        }
                        else
                        {
                            ViewData["Error"] = "true";
                        }
                    }
                    return HackerStory;
                });
        }
    }
}