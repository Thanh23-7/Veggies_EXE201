using System;
using System.Collections.Generic;

namespace Veggies_EXE201.ViewModels
{
    public class OrderHistoryViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new List<OrderSummaryViewModel>();
    }

    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public int ItemCount { get; set; }
    }
}