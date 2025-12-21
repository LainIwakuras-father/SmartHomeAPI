using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;
using SmartHomeAPI.Core.Entities;

namespace SmartHome.Infra.Repositories
{
    public class UserRepository(IndustrialDbContext context, ILogger<UserRepository> logger) : IUserRepository, IDisposable
    {
        private bool _disposed;
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    context?.Dispose();
                    
                }
                _disposed = true;
            }
        } 
        
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
               return await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id); 
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"На найден пользователь с айди {id}",id);
                throw;
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
               return await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username); 
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"На найден пользователь с именем {username}",username);
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
               return await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email); 
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"На найден пользователь с email {email}",email);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
               return await context.Users
                    .AsNoTracking()
                    .ToListAsync(); 
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"Ошибка получения пользователей");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            try
            {
               return await context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == role)
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"На найдены пользователи с ролью {role}",role);
                throw;
            }
        }
        public async Task<User> AddAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка создания пользователя: {Username}", user.Username);
                throw;
            }
        }

        public Task UpdateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var user = await GetByIdAsync(id);
                if (user != null)
                {
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ошибка удаленяи пользователя с айди: {Id}", id);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return await context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Username.ToLower() == username.ToLower());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка операции по имени: {Username}", username);
                throw;
            }
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка операции по почте: {Email}", email);
                throw;
            }
        }
    }
}
