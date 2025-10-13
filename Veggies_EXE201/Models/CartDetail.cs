using System;
using System.Collections.Generic;

namespace Veggies_EXE201.Models;

public partial class CartDetail
{
    public int Id { get; set; }

    public int ShoppingCartId { get; set; }

    public int ProductId { get; set; }

    public int Count { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ShoppingCart ShoppingCart { get; set; } = null!;
}
