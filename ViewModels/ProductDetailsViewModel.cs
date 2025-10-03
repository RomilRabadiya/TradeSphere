using System.Collections.Generic;
using TradeSphere3.Models;

namespace TradeSphere3.ViewModels
{
    public class ProductDetailsViewModel
    {
public Product Product { get; set; }
        public IEnumerable<Feedback> Feedbacks { get; set; }
        public int TopFeedbackCount { get; set; }
    }
}
