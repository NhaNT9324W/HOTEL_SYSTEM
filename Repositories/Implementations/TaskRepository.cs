using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<HousekeepingTask>> GetAllAsync() =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<HousekeepingTask?> GetByIdAsync(int id) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<HousekeepingTask>> SearchAsync(string keyword) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.Room.RoomNumber.Contains(keyword) ||
                            t.AssignedTo.FullName.Contains(keyword) ||
                            t.Description.Contains(keyword))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<HousekeepingTask>> GetByStaffIdAsync(int staffId) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == staffId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task AddAsync(HousekeepingTask task)
        {
            await _context.HousekeepingTasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HousekeepingTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.HousekeepingTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task != null)
            {
                _context.HousekeepingTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}