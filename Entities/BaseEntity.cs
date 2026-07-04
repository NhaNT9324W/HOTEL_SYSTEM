using System;

namespace Hotel_System.Entities
{
    /// <summary>
    /// Lớp cơ sở cho tất cả entity, chứa các thuộc tính chung.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}