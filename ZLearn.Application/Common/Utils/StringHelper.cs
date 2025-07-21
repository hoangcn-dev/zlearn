using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ZLearn.Application.Common.Utils
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
    }
}
