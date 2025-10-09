using System.Collections.Generic;
using System.Linq;

namespace Veggies_EXE201.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal GrandTotal => CartItems.Sum(x => x.Total);
    }
}