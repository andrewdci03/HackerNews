using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace ExternalServices
{
    public class HackerNewsService
    {
        const string BestStoriesApi = "https://hacker-news.firebaseio.com/v0/beststories.json";
        const string StoryApiTemplate = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

        //public async List<HackerStory> GetStories(string searchString)
        //{
        //    List<HackerStory> stories = new List<HackerStory>();
        //    stories = await NewMethod(searchString, stories);
        //    return View(stories);
        //}

        public static async Task<List<HackerStory>> NewMethod(string searchString, List<HackerStory> stories)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(BestStoriesApi);
                if (response.IsSuccessStatusCode)
                {
                    var storiesResponse = response.Content.ReadAsStringAsync().Result;
                    var bestIds = JsonConvert.DeserializeObject<List<int>>(storiesResponse);

                    var tasks = bestIds.Select(GetStoryAsync);
                    stories = (await Task.WhenAll(tasks)).ToList();

                    if (!String.IsNullOrEmpty(searchString))
                    {
                        var search = searchString.ToLower();
                        // Provide feedback to View
                        ViewData["Filter"] = searchString;
                        stories = stories.Where(s =>
                                            s.Title.ToLower().IndexOf(search) > -1 || s.By.ToLower().IndexOf(search) > -1)
                                            .ToList();
                    }
                }
                else
                {
                    // TODO Just indicate failed attempt. Could add more specific errors. 
                    ViewData["FailedConnection"] = "true";
                }
            }

            return stories;
        }
    }
}
