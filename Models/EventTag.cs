using System;
using System.Collections.Generic;

namespace Eventify.Models;

public partial class EventTag
{
    public int EventId { get; set; }

    public int TagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
