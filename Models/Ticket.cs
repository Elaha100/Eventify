using System;
using System.Collections.Generic;

namespace Eventify.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int OrderId { get; set; }

    public int EventId { get; set; }

    public string TicketType { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
