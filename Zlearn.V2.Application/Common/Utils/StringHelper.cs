using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Zlearn.V2.Application.Common.Utils
{
    public class StringHelper
    {
        public static string IndexToChar(int i) => ((char)('A' + i)).ToString();

        public static string GetDefaultImageUrl()
            => "https://res.cloudinary.com/dvk5yt0oi/image/upload/v1751492653/b2e0meuozqt0ti4r7her_qhbicx.jpg";

        public static string GetRandomUserName(string prefix = "user")
        {
            var suffix = DateTime.UtcNow.ToString("yyMMddHHmmss");
            return $"{prefix}{suffix}";
        }

        public static string GetRandomString(int length, string source = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var random = new Random();
            return new string(Enumerable.Repeat(source, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ObjectToJsonString(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T? JsonStringToObject<T>(string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonString) ?? default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static string AppendParamsToUrl(string url, Dictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.Any())
                return url;
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var param in parameters)
            {
                query[param.Key] = param.Value;
            }
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        public static string GetRandomNickName()
        {
            var defaultNames = new HashSet<string>
            {
                "Gà", "Chó", "Mèo", "Cá", "Rùa", "Thỏ", "Hươu", "Sư Tử", "Cáo", "Gấu",
                "Cá Mập", "Cá Heo", "Voi", "Khỉ", "Ngựa", "Bò", "Cừu", "Lợn", "Gà Tây", "Chim Cánh Cụt",
                "Cá Voi", "Cá Sấu", "Bướm", "Chuồn Chuồn", "Kiến", "Nhện", "Bọ Cạp", "Gấu Trúc", "Hổ", "Sói",
                "Vịt", "Ếch", "Bò Sát", "Chim Sẻ"
            };

            var defaultChars = new HashSet<string>
            {
                "Mập", "Gầy", "Lùn", "Cao", "Xinh", "Đẹp", "Dễ Thương", "Ngốc Nghếch",
                "Thông Minh", "Hài Hước", "Dũng Cảm", "Nhanh Nhẹn", "Lười Biếng", "Hòa Đồng", "Tò Mò"
            };

            var defaultFoods = new HashSet<string>
            {
                "Bánh Mì", "Phở", "Bún", "Cơm", "Mì Quảng", "Bánh Xèo", "Bánh Bao", "Bánh Canh",
                "Pizza", "Hamburger", "Hotdog", "Sushi", "Ramen", "Tacos", "Gỏi Cuốn", "Nem Rán",
                "Chè", "Kem", "Bánh Flan", "Bánh Tiramisu", "Bánh Trung Thu", "Bánh Pía", "Bánh Kem",
                "Sinh Tố Bơ", "Sinh Tố Dâu", "Sinh Tố Xoài", "Trái Cây Dầm", "Sữa Chua Trái Cây",
                "Xoài", "Dưa Hấu", "Ổi", "Mận", "Chôm Chôm", "Sầu Riêng", "Dừa", "Nho", "Táo",
                "Trà Sữa", "Trà Đào", "Trà", "Nước Mía", "Cà Phê", "Nước Cam", "Nước Ép Cà Rốt", "Nước Ép Ổi"
            };

            var random = new Random();
            var name = defaultNames.ElementAt(random.Next(defaultNames.Count));
            var charSuffix = defaultChars.ElementAt(random.Next(defaultChars.Count));
            var foodSuffix = defaultFoods.ElementAt(random.Next(defaultFoods.Count));
            return $"{name} {charSuffix} Thích {foodSuffix}";
        }

        public static string GenerateSlug(string source)
        {
            // create en string from vi string
            char[] vi = { 'à', 'á', 'ạ', 'ả', 'ã', 'â', 'ầ', 'ấ', 'ậ', 'ẩ', 'ẫ', 'ă', 'ằ', 'ắ', 'ặ', 'ẳ', 'ẵ', 'è', 'é', 'ẹ', 'ẻ', 'ẽ', 'ê', 'ề', 'ế', 'ệ', 'ể', 'ễ', 'ì', 'í', 'ị', 'ỉ', 'ĩ', 'ò', 'ó', 'ọ', 'ỏ', 'õ', 'ô', 'ồ', 'ố', 'ộ', 'ổ', 'ỗ', 'ơ', 'ờ', 'ớ', 'ợ', 'ở', 'ỡ', 'ù', 'ú', 'ụ', 'ủ', 'ũ', 'ư', 'ừ', 'ứ', 'ự', 'ử', 'ữ', 'ỳ', 'ý', 'ỵ', 'ỷ', 'ỹ', 'đ' };
            char[] en = { 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'a', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'i', 'i', 'i', 'i', 'i', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'o', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'y', 'y', 'y', 'y', 'y', 'd' };
            var map = new Dictionary<char, char>();
            for (int i = 0; i < vi.Length; i++)
            {
                map.Add(vi[i], en[i]);
            }
            string seo = "";
            source = source.ToLower();
            source = source.Replace("-", " ");
            for (int i = 0; i < source.Length; i++)
            {
                if (map.TryGetValue(source[i], out char val))
                {
                    seo += val;
                }
                else
                {
                    seo += source[i];
                }
            }

            seo = seo.Trim();
            seo = Regex.Replace(seo, @"[^a-z0-9\s-]", ""); //remove special characters
            seo = Regex.Replace(seo, @"\s+", "-");
            return seo.Length > 100 ? seo[..100] : seo;
        }

        public static string GenerateUniqueSlug(string source, int maxLength = 100)
        {
            var baseSlug = GenerateSlug(source);
            var suffix = $"-{Guid.NewGuid().ToString("N")[..8]}";
            if (baseSlug.Length + suffix.Length > maxLength)
            {
                baseSlug = baseSlug[..(maxLength - suffix.Length)];
            }
            return $"{baseSlug}{suffix}";
        }

        public static string GetJobId(string prefix, string typeName) => $"{prefix}_{typeName}";
    }
}
