﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjWebsiteB.Models;

public partial class TProductCategory
{
    public int FProductCategoryId { get; set; }

    public string FCategoryName { get; set; }

    public virtual ICollection<TProduct> TProducts { get; set; } = new List<TProduct>();
}