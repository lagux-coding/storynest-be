using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class UsernameGenerateHelperHelper
    {
        private static readonly string[] Animals =
        {
            "lion", "tiger", "panda", "eagle", "wolf", "bear",
            "shark", "falcon", "dragon", "phoenix", "otter", "fox"
        };

        private static readonly string[] Adjectives =
        {
            "Dũng cảm", "Hài hước", "Nhanh nhẹn", "Thông minh", "Tinh nghịch",
            "Lười biếng", "Vui vẻ", "Trầm lặng", "Mạnh mẽ", "Nhỏ bé",
            "Hiền lành", "Ngốc nghếch", "Gan dạ", "Lanh lợi", "Ngạo nghễ",
            "Hồn nhiên", "Đáng yêu", "Kỳ lạ", "Bí ẩn", "Tinh anh",
            "Dịu dàng", "Hoang dã", "Cứng cỏi", "Điềm tĩnh", "Quỷ quyệt",
            "Thân thiện", "Ngầu lòi", "Khù khờ", "Bá đạo", "Khôn ngoan"
        };

        private static readonly string[] AnimalsDisplayName =
        {
            "Hổ", "Sư tử", "Gấu trúc", "Cáo", "Sói",
            "Chim ưng", "Cú mèo", "Cá mập", "Cá heo", "Rùa",
            "Thỏ", "Chuột túi", "Bò sát", "Khỉ", "Trâu",
            "Ngựa", "Chó sói đồng cỏ", "Báo đốm", "Lạc đà", "Voi",
            "Hải cẩu", "Cáo bạc", "Mèo rừng", "Diều hâu", "Chim sẻ",
            "Gấu nâu", "Chồn", "Nhím", "Cáo lửa", "Khủng long" 
        };

        public static string GenerateAnonymousName(int seed)
        {
            var rnd = new Random(seed);
            var adjective = Adjectives[rnd.Next(Adjectives.Length)];
            var animal = AnimalsDisplayName[rnd.Next(AnimalsDisplayName.Length)];
            return $"{animal} {adjective}";
        }

        public static string GenerateUsername(string? givenName)
        {
            string baseName;

            if (string.IsNullOrWhiteSpace(givenName))
            {
                // Chọn random con vật
                var random = new Random();
                baseName = Animals[random.Next(Animals.Length)];
            }
            else
            {
                // Chuẩn hóa givenName: lowercase + bỏ ký tự đặc biệt
                baseName = new string(givenName
                    .ToLower()
                    .Normalize(System.Text.NormalizationForm.FormD)
                    .Where(c => char.IsLetterOrDigit(c))
                    .ToArray());

                if (string.IsNullOrEmpty(baseName))
                {
                    // Nếu normalize xong vẫn rỗng → fallback animal
                    var random = new Random();
                    baseName = Animals[random.Next(Animals.Length)];
                }
            }

            // Random 6 số
            var suffix = new Random().Next(100000, 999999);

            return $"{baseName}{suffix}";
        }
    }
}
