using System;
using System.Collections.Generic;

namespace Eventify.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
}
