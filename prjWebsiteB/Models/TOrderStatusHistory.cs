﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjWebsiteB.Models;

public partial class TOrderStatusHistory
{
    public int FStatusId { get; set; }

    public string FStatusName { get; set; }

    public DateTime? FTimestamp { get; set; }

    public virtual ICollection<TOrder> TOrders { get; set; } = new List<TOrder>();
}