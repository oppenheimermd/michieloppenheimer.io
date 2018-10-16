using System;
using System.Text;
using Blog.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Blog.Services
{
    public class BlogUserServices : IUserServices
    {
        private readonly UserConfig _userConfig;

        public BlogUserServices(UserConfig userConfig)
        {
            _userConfig = userConfig;
        }

        public bool ValidateUser(string username, string password)
        {
            return username == _userConfig.Username && VerifyHashedPassword(password, _userConfig);
        }

        private bool VerifyHashedPassword(string password, UserConfig config)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(config.Salt);

            byte[] hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );

            var hashText = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hashText == config.Password;
        }
    }
}
