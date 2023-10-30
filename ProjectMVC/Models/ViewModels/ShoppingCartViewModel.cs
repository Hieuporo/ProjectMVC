using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProjectMVC.Models.ViewModels
{
    public class ShoppingCartViewModel
    {
       public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
       public OrderHeader OrderHeader { get; set; }
    }
}