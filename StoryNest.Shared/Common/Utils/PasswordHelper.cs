using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Shared.Common.Utils
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 310000; // Number of iterations for PBKDF2

        private const string Letters = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";

        public static string GenerateBasicPassword(int length = 6)
        {
            if (length < 6) length = 6; // enforce tối thiểu 6 ký tự

            var allChars = Letters + Digits;
            var random = new Random();

            // đảm bảo có ít nhất 1 số
            var passwordChars = new[]
            {
                Letters[random.Next(Letters.Length)],
                Digits[random.Next(Digits.Length)]
            }.ToList();

            // fill phần còn lại
            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // shuffle cho random
            return new string(passwordChars.OrderBy(_ => random.Next()).ToArray());
        }

        public static string HashPassword(string password)
        {
            // Tạo salt random
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Hash với PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);

            // Gộp salt + key + iterations lại thành 1 chuỗi base64
            var hashBytes = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
            Buffer.BlockCopy(key, 0, hashBytes, SaltSize, KeySize);

            return $"pbkdf2-sha256.{Iterations}.{Convert.ToBase64String(hashBytes)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Tách ra: iterations + hash
            var parts = hashedPassword.Split('.', 3);
            var iterations = int.Parse(parts[1]);
            var hashBytes = Convert.FromBase64String(parts[2]);

            // Lấy salt từ hash
            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            // Lấy key gốc
            var key = new byte[KeySize];
            Buffer.BlockCopy(hashBytes, SaltSize, key, 0, KeySize);

            // Hash lại password user nhập
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var keyToCheck = pbkdf2.GetBytes(KeySize);

            // So sánh an toàn (không bị timing attack)
            return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
        }
    }
}
