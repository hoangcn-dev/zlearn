using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Utils
{
    public class StringHelper
    {
        public static string IndexToChar(int i) => ((char)('A' + i)).ToString();
        public static string GetDefaultImageUrl()
            => "https://res.cloudinary.com/dvk5yt0oi/image/upload/v1751492653/b2e0meuozqt0ti4r7her_qhbicx.jpg";
    }
}
