﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjWebsiteB.Models;

public partial class TPostAndTag
{
    public int FPostTagId { get; set; }

    public int? FPostId { get; set; }

    public int? FTagId { get; set; }

    public virtual TPost FPost { get; set; }

    public virtual TPostTag FTag { get; set; }
}