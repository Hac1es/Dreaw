using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using System.Runtime.InteropServices;
using System.Text;
namespace Server.Models
{
    public class JDT
    {
        // Cấu hình thuật toán và serializer
        private readonly HMACSHA256Algorithm algorithm = new HMACSHA256Algorithm(); // Thuật toán ký
        private readonly JsonNetSerializer serializer = new JsonNetSerializer();  // JSON serializer
        private readonly JwtBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder(); // Base64 URL encoder
        private JwtValidator? validator;
        public (int, object?) Decoder(string jwt, string key)
        {
            validator = new JwtValidator(serializer, new UtcDateTimeProvider());
            JwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            // Giải mã và kiểm tra token
            try
            {
                var payload = decoder.DecodeToObject<IDictionary<string, object>>(jwt, key, verify: true);
                return (200, payload);
            }
            catch(TokenExpiredException)
            {
                return (401, "Expired");
            }
            catch(SignatureVerificationException)
            {
                return (401, "Unauthorized");
            }
            catch(Exception ex)
            {
                return (500, ex.Message);
            } 
        }
    }
}
