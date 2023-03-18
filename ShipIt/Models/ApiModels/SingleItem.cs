using System;
using System.Text;
using ShipIt.Models.DataModels;
using static ShipIt.Models.ApiModels.Product;

namespace ShipIt.Models.ApiModels
{
    public class SingleItem
    {public ProductDataModel Product{set;get;}
    public double Weight{set;get;}

    public SingleItem(ProductDataModel product,double weight)
    {Product=product;
    Weight=weight;
    }
    }
}