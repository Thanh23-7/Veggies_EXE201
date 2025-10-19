using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;

namespace Veggies_EXE201.Services
{
    public class ActivityLogService
    {
        private readonly VeggiesDb2Context _context;

        public ActivityLogService(VeggiesDb2Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Ghi log hoạt động của người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng thực hiện hành động</param>
        /// <param name="action">Loại hành động (CREATE_PRODUCT, UPDATE_ORDER, etc.)</param>
        /// <param name="details">Chi tiết về hành động</param>
        /// <param name="ipAddress">Địa chỉ IP của người dùng</param>
        public async Task LogActivityAsync(int userId, string action, string details, string? ipAddress = null)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.Now
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để không ảnh hưởng đến flow chính
                Console.WriteLine($"Error logging activity: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi log hoạt động hệ thống (không có userId)
        /// </summary>
        /// <param name="action">Loại hành động hệ thống</param>
        /// <param name="details">Chi tiết về hành động</param>
        /// <param name="ipAddress">Địa chỉ IP</param>
        public async Task LogSystemActivityAsync(string action, string details, string? ipAddress = null)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserId = null, // Hành động hệ thống
                    Action = action,
                    Details = details,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.Now
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging system activity: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách log hoạt động với phân trang và lọc
        /// </summary>
        /// <param name="page">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng bản ghi mỗi trang</param>
        /// <param name="userId">Lọc theo userId (null = tất cả)</param>
        /// <param name="action">Lọc theo action (null = tất cả)</param>
        /// <param name="startDate">Ngày bắt đầu lọc</param>
        /// <param name="endDate">Ngày kết thúc lọc</param>
        /// <param name="searchTerm">Tìm kiếm trong details</param>
        public async Task<(List<ActivityLog> logs, int totalCount)> GetActivityLogsAsync(
            int page = 1,
            int pageSize = 20,
            int? userId = null,
            string? action = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null)
        {
            var query = _context.ActivityLogs
                .Include(log => log.User)
                .AsQueryable();

            // Lọc theo userId
            if (userId.HasValue)
            {
                query = query.Where(log => log.UserId == userId.Value);
            }

            // Lọc theo action
            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(log => log.Action == action);
            }

            // Lọc theo ngày
            if (startDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.CreatedAt <= endDate.Value);
            }

            // Tìm kiếm trong details
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(log => log.Details.Contains(searchTerm) || 
                                         (log.User != null && log.User.FullName.Contains(searchTerm)));
            }

            // Đếm tổng số bản ghi
            var totalCount = await query.CountAsync();

            // Phân trang và sắp xếp
            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        /// <summary>
        /// Lấy thống kê hoạt động theo ngày
        /// </summary>
        /// <param name="days">Số ngày gần nhất</param>
        public async Task<Dictionary<string, int>> GetActivityStatsAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);

            var stats = await _context.ActivityLogs
                .Where(log => log.CreatedAt >= startDate)
                .GroupBy(log => log.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var stat in stats)
            {
                result[stat.Date.ToString("yyyy-MM-dd")] = stat.Count;
            }

            return result;
        }

        /// <summary>
        /// Lấy danh sách các action phổ biến
        /// </summary>
        public async Task<List<string>> GetPopularActionsAsync()
        {
            return await _context.ActivityLogs
                .GroupBy(log => log.Action)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToListAsync();
        }

        /// <summary>
        /// Xóa log cũ (older than specified days)
        /// </summary>
        /// <param name="daysToKeep">Số ngày giữ lại log</param>
        public async Task<int> CleanupOldLogsAsync(int daysToKeep = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            
            var oldLogs = await _context.ActivityLogs
                .Where(log => log.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.ActivityLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
            }

            return oldLogs.Count;
        }
    }
}
