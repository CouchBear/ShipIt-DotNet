using System.Collections.Generic;

namespace ShipIt.Models.ApiModels
{
    public class TruckResponse: Response
    {
        // public Truck Truck { get; set; }
        
        public List<Truck> Trucks{get;set;}
        
        public TruckResponse(List<Truck> trucks) 
        {Trucks=trucks; }
    }
}