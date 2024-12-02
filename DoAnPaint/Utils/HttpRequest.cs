using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DoAnPaint.Utils
{
    public class HttpRequest //Singleton
    {
        private static readonly Lazy<HttpRequest> _instance = new Lazy<HttpRequest>(() => new HttpRequest());
        private readonly HttpClient httpClient;

        // Constructor private để ngăn tạo instance bên ngoài
        private HttpRequest()
        {
            httpClient = new HttpClient();
        }

        // Instance được tạo lazily và thread-safe
        public static HttpRequest Instance => _instance.Value;
    }
}
