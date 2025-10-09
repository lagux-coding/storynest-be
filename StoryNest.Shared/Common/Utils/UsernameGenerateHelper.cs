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
            "dũng cảm", "hài hước", "nhanh nhẹn", "thông minh", "tinh nghịch",
            "lười biếng", "tui vẻ", "trầm lặng", "mạnh mẽ", "nhỏ bé",
            "hiền lành", "ngốc nghếch", "gan dạ", "lanh lợi", "ngạo nghễ",
            "hồn nhiên", "đáng yêu", "kỳ lạ", "bí ẩn", "tinh anh",
            "dịu dàng", "hoang dã", "cứng cỏi", "điềm tĩnh", "quỷ quyệt",
            "thân thiện", "ngầu lòi", "khù khờ", "bá đạo", "khôn ngoan"
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

        private static readonly string[] Prefixes =
        {
            "Người", "Kẻ", "Ai đó", "Bóng", "Cơn gió", "Giấc mộng",
            "Dòng chữ", "Ánh trăng", "Mảnh hồn", "Tiếng nói", "Cú đêm"
        };

        private static readonly string[] Modifiers =
        {
            "", "đang", "từng", "vẫn", "chợt", "khẽ", "đã", "vô tình", "nhẹ nhàng", "một mình"
        };

        private static readonly string[] Actions =
        {
            "viết", "kể", "mơ", "lang thang", "giấu mình", "lặng im",
            "đợi", "cười", "nhớ", "trôi", "tìm lại", "ngủ quên", "nghe", "gọi thầm", "mỉm cười"
        };

        private static readonly string[] Connectors =
        {
            "trong", "giữa", "bên", "trên", "dưới", "nơi", "về", "cùng", "phía sau", "bên kia"
        };

        private static readonly string[] Objects =
        {
            "mưa", "đêm", "chiều thu", "khung cửa", "ký ức", "trang giấy",
            "giấc ngủ", "dòng sông", "ánh trăng", "bầu trời", "biển", "khoảng lặng",
            "mùa cũ", "nỗi nhớ", "tiếng đàn", "kỷ niệm", "bức thư", "tán lá", "con phố",
            "bình minh", "bóng tối", "hoàng hôn", "ánh nắng", "cơn mộng", "sương mai"
        };

        private static readonly string[] AdjectivesCom =
        {
            "", "xưa", "nhỏ", "xa", "cũ", "lạ", "cuối cùng", "ngắn ngủi", "dài lâu", "nhẹ tênh", "vội vàng"
        };

        private static readonly string[] Suffixes =
        {
            "– còn dang dở", "– chưa kể hết", "– trong tổ",
            "– của ngày xưa", "– vừa tỉnh giấc", "– chưa đặt tên", "– như cơn mơ"
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

        public static string GeneratePoeticAnonymousName(int userId)
        {
            var rnd = new Random(userId);

            string prefix = Prefixes[rnd.Next(Prefixes.Length)];
            string modifier = Modifiers[rnd.Next(Modifiers.Length)];
            string action = Actions[rnd.Next(Actions.Length)];

            string name = $"{prefix} {(string.IsNullOrEmpty(modifier) ? "" : modifier + " ")}{action}".Trim();

            if (rnd.NextDouble() > 0.3)
            {
                string connector = Connectors[rnd.Next(Connectors.Length)];
                string obj = Objects[rnd.Next(Objects.Length)];
                string adj = AdjectivesCom[rnd.Next(AdjectivesCom.Length)];
                name += $" {connector} {obj}{(string.IsNullOrEmpty(adj) ? "" : " " + adj)}";
            }

            if (rnd.NextDouble() > 0.7)
                name += $" {Suffixes[rnd.Next(Suffixes.Length)]}";

            int salt = Math.Abs(userId.GetHashCode()) % 1000;
            name += $" #{salt}";

            return name;
        }
    }
}
