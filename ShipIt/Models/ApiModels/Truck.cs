using System;
using System.Collections.Generic;
using System.Text;
using ShipIt.Models.DataModels;

namespace ShipIt.Models.ApiModels
{
    public class Truck
    {
        public int Id { get; set; }

        public List <SingleItem> Items{get;set;}
        

        public Truck (List<SingleItem> items)
        {
            Items=items;
        }

        //Empty constructor needed for Xml serialization
        public Truck()
        {
        }
    }
}