using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace RedisProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        const string memoryCacheKey = "MemoryCacheExample";
        const string redisCacheKey = "RedisKeyExample";

        public HomeController(IDistributedCache distributedCache, IMemoryCache memoryCache)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return Content("Merhaba .Net Core");
        }

        [HttpGet("/GetRedisCache")]
        public async Task<string> GetRedisCache()
        {
            var result = await _distributedCache.GetStringAsync(redisCacheKey);

            if (!string.IsNullOrEmpty(result))
            {
                return "Redis Cache'den geldi : " + result;
            }
            else
            {
                result = DateTime.Now.ToString();
                var cacheExpirationOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };

                _distributedCache.SetString(redisCacheKey, result, cacheExpirationOptions);
                return "Redis Cache'e ekledi : " + result;
            }
        }

        [HttpGet("/RemoveRedisCache")]
        public async Task<string> RemoveRedisCache()
        {
            await _distributedCache.RemoveAsync(redisCacheKey);
            return "Redis cache verisi temizlendi";
        }

        [HttpGet("/GetMemoryCache")]
        public string GetMemoryCache()
        {
            if (!_memoryCache.TryGetValue(memoryCacheKey, out string result))
            {
                result = DateTime.Now.ToString("dd MMMM yyyy hh:mm");

                var cacheExpirationOptions = new MemoryCacheEntryOptions
                {
                    // Belirli bir tarihte cache verisinin yenilenmesini işlemini yapar
                    //AbsoluteExpiration = Convert.ToDateTime("22.03.2020 14:40"),
                    //Priority = CacheItemPriority.Low
                    
                    // Belirtilen dakika sonrasında cache verisinin yenilenmesi yapılır
                    //AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                    //Priority = CacheItemPriority.Normal

                    // Belirtilen dakika boyunca sayfaya istek gelmezse cache verisi otomatik silinecek
                    SlidingExpiration = TimeSpan.FromMinutes(2),
                    Priority = CacheItemPriority.High //Cache verisinin önem düzeyini belirttik
                };

                _memoryCache.Set(memoryCacheKey, result, cacheExpirationOptions);
            }

            return result;
        }

        [HttpGet("/RemoveMemoryCache")]
        public string RemoveMemoryCache()
        {
            _memoryCache.Remove(memoryCacheKey);
            return "Memory cache temizlendi";
        }
    }
}