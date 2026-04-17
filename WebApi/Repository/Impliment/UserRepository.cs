using Microsoft.AspNetCore.Connections;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private readonly Helper _dbHelper;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(Helper dbHelper, ILogger<UserRepository> logger, DbConnectionFactory connectionFactory)
        {
            _dbHelper = dbHelper;
            _logger = logger;
            _connectionFactory = connectionFactory;
        }



        public async Task<int> CreateUserAsync(UserModel user)
        {
            //RegisterResponse response = new RegisterResponse();
            try
            {

                // string message = string.Empty;

                //SqlParameter[] sqlParameters = new SqlParameter[5];

                SqlParameter[] sqlParameters = new SqlParameter[5];

                sqlParameters[0] = new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = (user.Username ?? "").Trim() };
                sqlParameters[1] = new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (user.Email ?? "").Trim().ToLower() };
                sqlParameters[2] = new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = (user.PasswordHash ?? "").Trim() };
                sqlParameters[3] = new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = (object?)user.FullName?.Trim() ?? DBNull.Value };
                sqlParameters[4] = new SqlParameter("@PhoneNumber", SqlDbType.NVarChar, 20) { Value = (object?)user.PhoneNumber?.Trim() ?? DBNull.Value };

                //sqlParameters[5] = new SqlParameter("@CreatedAt", SqlDbType.DateTime);
                //sqlParameters[5].Value = DateTime.UtcNow;

                using (DataTable dt = await _dbHelper.ExecuteStoredProcedureAsync("sp_CreateUser", CommandType.StoredProcedure, CancellationToken.None, sqlParameters))
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["Result"]?.ToString() == "Success")
                        {
                            int userId = Convert.ToInt32(row["Id"]);

                            _logger.LogInformation("User registered successfully: {Username} (ID: {UserId})", row["Username"]?.ToString(), userId);

                            return userId;
                        }
                        throw new Exception(row["Message"]?.ToString() ?? "Registration failed");
                    }
                }
                // ✅ REQUIRED: ensures method always returns or throws
                throw new Exception("No response received from registration procedure");
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                _logger.LogWarning(ex, "Duplicate user registration attempt: {Username}, {Email}",
                    user.Username, user.Email);
                throw new Exception("Username or email already exists", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                throw;
            }
        }
        public async Task<UserModel?> GetUserByIdAsync(int id)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", id)
                };

                var dataTable = await _dbHelper.FillDataTableAsync("SELECT * FROM Users WHERE Id = @Id", CommandType.Text, CancellationToken.None, parameters);

                if (dataTable.Rows.Count == 0)
                    return null;

                return MapToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                throw;
            }
        }
        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Username", username)
                };

                var dataTable = await _dbHelper.FillDataTableAsync(
                    "SELECT * FROM Users WHERE Username = @Username",
                    CommandType.Text,
                    CancellationToken.None,
                    parameters
                );

                if (dataTable.Rows.Count == 0)
                    return null;

                return MapToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Email", email)
                };

                var dataTable = await _dbHelper.FillDataTableAsync(
                    "SELECT * FROM Users WHERE Email = @Email",
                    CommandType.Text,
                    CancellationToken.None,
                    parameters
                );

                if (dataTable.Rows.Count == 0)
                    return null;

                return MapToUser(dataTable.Rows[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }


        public async Task<bool> UserExistsAsync(string username, string email)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Email", email)
                };

                var query = "SELECT COUNT(1) FROM Users WHERE Username = @Username OR Email = @Email";

                var result = await _dbHelper.ExecuteScalarAsync(
                    query,

                    CancellationToken.None,
                    parameters
                );

                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists: {Username}, {Email}", username, email);
                throw;
            }
        }




        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", user.Id),
                    new SqlParameter("@Username", user.Username ?? ""),
                    new SqlParameter("@Email", user.Email ?? ""),
                    new SqlParameter("@FullName", (object?)user.FullName ?? DBNull.Value),
                    new SqlParameter("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value),
                    new SqlParameter("@UpdatedAt", DateTime.UtcNow),
                    new SqlParameter("@IsActive", user.IsActive),
                    new SqlParameter("@IsEmailVerified", user.IsEmailVerified)
                };

                var query = @"
                    UPDATE Users 
                    SET Username = @Username, 
                        Email = @Email, 
                        FullName = @FullName, 
                        PhoneNumber = @PhoneNumber, 
                        UpdatedAt = @UpdatedAt,
                        IsActive = @IsActive,
                        IsEmailVerified = @IsEmailVerified
                    WHERE Id = @Id";

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    query,
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {Id}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", id)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    "DELETE FROM Users WHERE Id = @Id",
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", userId),
                    new SqlParameter("@PasswordHash", newPasswordHash),
                    new SqlParameter("@UpdatedAt", DateTime.UtcNow)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = @UpdatedAt WHERE Id = @Id",
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user ID: {Id}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateEmailVerificationStatusAsync(int userId, bool isVerified)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", userId),
                    new SqlParameter("@IsEmailVerified", isVerified),
                    new SqlParameter("@UpdatedAt", DateTime.UtcNow)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET IsEmailVerified = @IsEmailVerified, UpdatedAt = @UpdatedAt WHERE Id = @Id",
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email verification status for user ID: {Id}", userId);
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", userId),
                    new SqlParameter("@IsActive", true),
                    new SqlParameter("@UpdatedAt", DateTime.UtcNow)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id",
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user ID: {Id}", userId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Id", userId),
                    new SqlParameter("@IsActive", false),
                    new SqlParameter("@UpdatedAt", DateTime.UtcNow)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    "UPDATE Users SET IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id",
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user ID: {Id}", userId);
                throw;
            }
        }


        // Helper method to map DataRow to User
        private UserModel MapToUser(DataRow row)
        {
            return new UserModel
            {
                Id = Convert.ToInt32(row["Id"]),
                Username = row["Username"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty,
                FullName = row["FullName"] != DBNull.Value ? row["FullName"]?.ToString() : null,
                PhoneNumber = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"]?.ToString() : null,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                IsEmailVerified = Convert.ToBoolean(row["IsEmailVerified"])
            };
        }


        public async Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)


                    return false;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = expiry;
                return await UpdateUserAsync(user);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token for user ID: {Id}", userId);
                throw;
            }
        }


        public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token for user ID: {Id}", userId);
                throw;
            }
        }


        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                    return false;
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                return await UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token for user ID: {Id}", userId);
                throw;
            }
        }

        public async Task<(IEnumerable<UserModel> Users, int TotalCount)> GetAllUsersAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchTerm = null)
        {
            const string query = @"
        SELECT 
            Id,
            Username,
            Email,
            FullName,
            PhoneNumber,
            IsEmailVerified,
            IsActive,
            CreatedAt,
            LastLoginAt
        FROM Users 
        WHERE (@SearchTerm IS NULL OR 
               Username LIKE '%' + @SearchTerm + '%' OR
               Email LIKE '%' + @SearchTerm + '%' OR
               FullName LIKE '%' + @SearchTerm + '%')
        ORDER BY CreatedAt DESC
        OFFSET @Offset ROWS 
        FETCH NEXT @PageSize ROWS ONLY;
        
        SELECT COUNT(*) FROM Users 
        WHERE (@SearchTerm IS NULL OR 
               Username LIKE '%' + @SearchTerm + '%' OR
               Email LIKE '%' + @SearchTerm + '%' OR
               FullName LIKE '%' + @SearchTerm + '%')";

            try
            {
                var parameters = new[]
                {
            new SqlParameter("@SearchTerm",
                string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : (object)searchTerm.Trim()),
            new SqlParameter("@Offset", (pageNumber - 1) * pageSize),
            new SqlParameter("@PageSize", pageSize)
        };

                var dataSet = await _dbHelper.ExecuteQueryAsync(
                    query,
                    CommandType.Text,
                    CancellationToken.None,
                    parameters
                );

                if (dataSet == null || dataSet.Tables.Count < 2)
                {
                    return (Enumerable.Empty<UserModel>(), 0);
                }

                var users = new List<UserModel>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    users.Add(new UserModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Username = row["Username"]?.ToString() ?? string.Empty,
                        Email = row["Email"]?.ToString() ?? string.Empty,
                        FullName = row["FullName"]?.ToString() ?? string.Empty,
                        PhoneNumber = row["PhoneNumber"]?.ToString() ?? string.Empty,
                        IsEmailVerified = Convert.ToBoolean(row["IsEmailVerified"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"])

                    });
                }

                var totalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0]);

                return (users, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated users");
                throw;
            }
        }

        public async Task<UserModel?> GetByEmailVerificationTokenAsync(string token)
        {
            try
            {
                var parameters = new[]
                {
            new SqlParameter("@Token", token)
        };

                using var reader = await _dbHelper.ExecuteReaderAsync(
                    @"SELECT Id, IsEmailVerified, EmailVerificationTokenExpiry
              FROM Users
              WHERE EmailVerificationToken = @Token",

                    CancellationToken.None,
                    parameters
                );

                if (!reader.Read())
                    return null;

                return new UserModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                    EmailVerificationTokenExpiry = reader.IsDBNull(reader.GetOrdinal("EmailVerificationTokenExpiry"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("EmailVerificationTokenExpiry"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email verification token");
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(int userId)
        {
            var parameters = new[]
            {
        new SqlParameter("@Id", userId),
        new SqlParameter("@UpdatedAt", DateTime.UtcNow)
    };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                @"UPDATE Users
          SET IsEmailVerified = 1,
              EmailVerificationToken = NULL,
              EmailVerificationTokenExpiry = NULL,
              UpdatedAt = @UpdatedAt
          WHERE Id = @Id",
                null,
                CancellationToken.None,
                parameters
            );

            return rowsAffected > 0;
        }
        public async Task SavePasswordResetTokenAsync(
    int userId,
    string token,
    DateTime expiry)
        {
            var parameters = new[]
            {
        new SqlParameter("@Id", userId),
        new SqlParameter("@Token", token),
        new SqlParameter("@Expiry", expiry),
        new SqlParameter("@UpdatedAt", DateTime.UtcNow)
    };

            await _dbHelper.ExecuteNonQueryAsync(
                @"UPDATE Users
          SET PasswordResetToken = @Token,
              PasswordResetTokenExpiry = @Expiry,
              UpdatedAt = @UpdatedAt
          WHERE Id = @Id",
                null,
                CancellationToken.None,
                parameters
            );
        }
        public async Task<UserModel?> GetUserByPasswordResetTokenAsync(string token)
        {
            try
            {
                var parameters = new[]
                {
            new SqlParameter("@Token", token)
        };

                using var reader = await _dbHelper.ExecuteReaderAsync(
                    @"SELECT Id, PasswordResetTokenExpiry
              FROM Users
              WHERE PasswordResetToken = @Token",
                    CancellationToken.None,
                    parameters
                );

                if (!reader.Read())
                    return null;

                return new UserModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    PasswordResetTokenExpiry = reader.IsDBNull(
                        reader.GetOrdinal("PasswordResetTokenExpiry"))
                        ? null
                        : reader.GetDateTime(
                            reader.GetOrdinal("PasswordResetTokenExpiry"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error fetching user by password reset token");
                throw;
            }
        }

        //public async Task UpdateUserTokenAsync(UserModel user)
        //{
        //    const string sql = @"
        //UPDATE Users 
        //SET RefreshToken = @RefreshToken, 
        //    RefreshTokenExpiry = @RefreshTokenExpiry,
        //    IsActive = @IsActive,
        //    FullName = @FullName
        //WHERE Id = @Id";

        //    using (var connection = _dbHelper.ExecuteNonQueryAsync())
        //    {
        //        await connection.ExecuteAsync(sql, new
        //        {
        //            user.RefreshToken,
        //            user.RefreshTokenExpiry,
        //            user.IsActive,
        //            user.FullName,
        //            user.Id
        //        });
        //    }
        //}
        public async Task<bool> UpdateRefreshTokenAsync(UserModel user)
        {
            try
            {
                var parameters = new[]
                {
            new SqlParameter("@Id", user.Id),
            new SqlParameter("@RefreshToken", (object?)user.RefreshToken ?? DBNull.Value),
            new SqlParameter("@RefreshTokenExpiry", (object?)user.RefreshTokenExpiry ?? DBNull.Value),
            new SqlParameter("@UpdatedAt", DateTime.UtcNow)
        };

                // This query is optimized to touch only session-related columns
                var query = @"
            UPDATE Users 
            SET RefreshToken = @RefreshToken, 
                RefreshTokenExpiry = @RefreshTokenExpiry,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(
                    query,
                    null,
                    CancellationToken.None,
                    parameters
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RefreshToken for user ID: {Id}", user.Id);
                throw;
            }
        }
        // Helper method to map DataRow to User
        //private UserModel MapToUser(DataRow row)
        //{
        //    return new UserModel
        //    {
        //        Id = Convert.ToInt32(row["Id"]),
        //        Username = row["Username"].ToString(),
        //        Email = row["Email"].ToString(),
        //        PasswordHash = row["PasswordHash"].ToString(),
        //        FullName = row["FullName"] != DBNull.Value ? row["FullName"].ToString() : null,
        //        PhoneNumber = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"].ToString() : null,
        //        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
        //        UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null,
        //        IsActive = Convert.ToBoolean(row["IsActive"]),
        //        IsEmailVerified = Convert.ToBoolean(row["IsEmailVerified"])
        //    };
        //}

    }
}
