using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly TelecomBillingDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(TelecomBillingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            //This is an asynchronous LINQ
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
            {
                throw new InvalidOperationException("User already exists");
            }

            // Validate role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                throw new ArgumentException("Invalid role");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User) 
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && 
                                         !rt.IsRevoked && 
                                         rt.ExpiresAt > DateTime.UtcNow);

            if (refreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = refreshToken.User;
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User is not active");
            }

            // Revoke the old refresh token
            refreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<UserInfo?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
                
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                    return null;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                return user != null ? new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    PlanType = user.PlanType,
                    Country = user.Country,
                    IsRoaming = user.IsRoaming,
                    IsActive = user.IsActive
                } : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            var token = GenerateJwtToken(user);
            var refreshTokenString = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    PlanType = user.PlanType,
                    Country = user.Country,
                    IsRoaming = user.IsRoaming,
                    IsActive = user.IsActive
                }
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Note: User and Subscriber are now the same entity, so no separate SubscriberId claim needed

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public async Task<SubscriberResponse?> GetSubscriberAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return MapToSubscriberResponse(user);
        }

        public async Task<SubscriberListResponse> GetSubscribersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();
            var totalCount = await query.CountAsync();
            
            var users = await query
                .OrderBy(u => u.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new SubscriberListResponse
            {
                Subscribers = users.Select(MapToSubscriberResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<SubscriberResponse?> UpdateSubscriberAsync(int id, UpdateSubscriberRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.PlanType))
                user.PlanType = request.PlanType;
            if (!string.IsNullOrEmpty(request.Country))
                user.Country = request.Country;
            if (request.IsRoaming.HasValue)
                user.IsRoaming = request.IsRoaming.Value;
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToSubscriberResponse(user);
        }

        public async Task<SubscriberResponse?> UpdateSubscriberPlanAsync(int id, UpdatePlanRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.PlanType = request.PlanType;
            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToSubscriberResponse(user);
        }

        public async Task<bool> DeactivateSubscriberAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static SubscriberResponse MapToSubscriberResponse(User user)
        {
            var yearsActive = (DateTime.UtcNow - user.CreatedAt).Days / 365;
            var isLoyaltyEligible = yearsActive >= 2;

            return new SubscriberResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                PlanType = user.PlanType,
                Country = user.Country,
                IsRoaming = user.IsRoaming,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastUpdated = user.LastUpdated,
                YearsActive = yearsActive,
                IsLoyaltyEligible = isLoyaltyEligible
            };
        }
    }
}
