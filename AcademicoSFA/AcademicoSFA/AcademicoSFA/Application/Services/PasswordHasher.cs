using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace AcademicoSFA.Application.Services
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Generar una sal criptográficamente aleatoria
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derivar un hash de 256 bits de la contraseña con PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Combinar el salt y el hash para almacenarlos juntos
            return Convert.ToBase64String(salt) + ":" + hashed;
        }

        // Método para verificar una contraseña contra un hash almacenado
        public static bool VerifyPassword(string storedHash, string password)
        {
            // Extraer el salt y el hash almacenados
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            // Derivar un hash de la contraseña proporcionada con el mismo salt
            string newHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Comparar el hash derivado con el hash almacenado
            return newHash == hash;
        }
    }
}
