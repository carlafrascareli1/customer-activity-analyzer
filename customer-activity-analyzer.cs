using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace customer_activity_analyzer
{
       
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    [Route("api/customer-activity-analyzer")]
    [ApiController]
    public class customer_activity_analyzer : ControllerBase
    {
        HttpClient _httpClient;
        const string _baseUrl = "https://jsonplaceholder.typicode.com";

        public customer_activity_analyzer()
        {
            _httpClient = new HttpClient();
        }

        private async Task<IEnumerable<User>> GetUsers(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve users. Status code: {response.StatusCode}");
            }
            
            var users = await JsonSerializer.DeserializeAsync<IEnumerable<User>>
                (await response.Content.ReadAsStreamAsync(cancellationToken), 
                cancellationToken: cancellationToken);

            return users ?? Enumerable.Empty<User>();
        }

         private async Task<IEnumerable<Post>> GetPosts(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/posts", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve posts. Status code: {response.StatusCode}");
            }
            
            var jsonResponse = await response.Content
                .ReadFromJsonAsAsyncEnumerable<Post>(cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);
            
            return jsonResponse.OfType<Post>();
        }

        

        [Route("getactivity")]
        public async Task<IActionResult> GetCustomerActivity(int minimumPosts = 0,
            int top = 5,
            CancellationToken cancellationToken = default)
        {
           var users = await GetUsers(cancellationToken);
           var posts = await GetPosts(cancellationToken);

           var userActivity = users.Select(user => new
            {
                User = user,
                UserPosts = posts.Where(p=> p.userId == user.id).ToList()
            });

            var filteredActivity = userActivity
                .Where(ua => ua.UserPosts.Count >= minimumPosts)
                .OrderByDescending(ua => ua.UserPosts.Count)
                .Take(top)
                .Select(ua => new
                {
                    UserId = ua.User.id,
                    UserName = ua.User.name,
                    PostCount = ua.UserPosts.Count,
                    Posts = ua.UserPosts.Select(p => new { p.id, p.title })
                });
            
            return Ok(filteredActivity);
        }
    }
}
