﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjWebsiteB.Models;

public partial class TOrdersDetail
{
    public int FOrderDetailsId { get; set; }

    public int? FOrderId { get; set; }

    public int? FProductId { get; set; }

    public int? FOrderQty { get; set; }

    public decimal? FUnitPrice { get; set; }

    public string FPurchaseCategoryName { get; set; }

    public int? FEventRegistrationFormId { get; set; }

    public int? FAttractionTicketId { get; set; }

    public virtual TEventRegistrationForm FEventRegistrationForm { get; set; }

    public virtual TOrder FOrder { get; set; }

    public virtual TProduct FProduct { get; set; }
}