using System;
using System.Collections.Generic;

namespace Veggies_EXE201.Models;

public partial class ActivityLog
{
    public int LogId { get; set; }

    public string? Action { get; set; }

    public int? UserId { get; set; }

    public string? Details { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Thêm các thuộc tính bị thiếu
    public DateTime? Timestamp 
    { 
        get => CreatedAt; 
        set => CreatedAt = value; 
    }

    public string? IpAddress { get; set; }

    // Navigation property
    public virtual User? User { get; set; }
}
