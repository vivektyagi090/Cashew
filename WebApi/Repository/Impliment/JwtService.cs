using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class JwtService: IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, IUserRepository userRepository, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _logger = logger;
        }
        public string GenerateToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username ?? ""),
                new Claim("email", user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("isVerified", user.IsEmailVerified.ToString()),
                new Claim("isActive", user.IsActive.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpiryInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "")),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // We want to get claims from expired tokens as well
            };
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        public async Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = expiry;

                return await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token for user ID: {UserId}", userId);
                return false;
            }
        }
        //public async Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        //{
        //    try
        //    {
        //        var user = await _userRepository.GetUserByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return false;
        //            user.RefreshToken = refreshToken;
        //            user.RefreshTokenExpiryTime = expiry;
        //            return await _userRepository.UpdateUserAsync(userId);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving refresh token for user ID: {UserId}", userId);
        //        return false;
        //    }
        //}

        public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token for user ID: {UserId}", userId);
                return false;

            }
        }

        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                
                    return false;
                    user.RefreshToken = null;
                 user.RefreshTokenExpiry = null;
                  return await _userRepository.UpdateUserAsync(user);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token for user ID: {UserId}", userId);
                throw;
            }
        }
    }
}
