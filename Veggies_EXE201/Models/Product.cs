using System;
using System.Collections.Generic;

namespace Veggies_EXE201.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public int? CategoryId { get; set; }

    public string? VietGapCertificateUrl { get; set; }

    public string? CultivationVideoUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ProductImage { get; set; }

    public int? SellerId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User? Seller { get; set; }
}
