﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    [Route("orders/outbound")]
    public class OutboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }


        [HttpPost("")]
        public TruckResponse Post([FromBody] OutboundOrderRequestModel request)
        {
            Log.Info(String.Format("Processing outbound order: {0}", request));

            var gtins = new List<String>();
            foreach (var orderLine in request.OrderLines)
            {
                if (gtins.Contains(orderLine.gtin))
                {
                    throw new ValidationException(String.Format("Outbound order request contains duplicate product gtin: {0}", orderLine.gtin));
                }
                gtins.Add(orderLine.gtin);
            }

            var productDataModels = _productRepository.GetProductsByGtin(gtins);
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            var lineItems = new List<StockAlteration>();
            var productIds = new List<int>();
            var errors = new List<string>();

            foreach (var orderLine in request.OrderLines)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.gtin));
                }
                else
                {
                    var product = products[orderLine.gtin];
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
                    productIds.Add(product.Id);
                }
            }

            if (errors.Count > 0)
            {
                throw new NoSuchEntityException(string.Join("; ", errors));
            }

            var stock = _stockRepository.GetStockByWarehouseAndProductIds(request.WarehouseId, productIds);

            var orderLines = request.OrderLines.ToList();
            errors = new List<string>();

            for (int i = 0; i < lineItems.Count; i++)
            {
                var lineItem = lineItems[i];
                var orderLine = orderLines[i];

                if (!stock.ContainsKey(lineItem.ProductId))
                {
                    errors.Add(string.Format("Product: {0}, no stock held", orderLine.gtin));
                    continue;
                }

                var item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.held)
                {
                    errors.Add(
                        string.Format("Product: {0}, stock held: {1}, stock to remove: {2}", orderLine.gtin, item.held,
                            lineItem.Quantity));
                }
            }

            if (errors.Count > 0)
            {
                throw new InsufficientStockException(string.Join("; ", errors));
            }

            _stockRepository.RemoveStock(request.WarehouseId, lineItems);


          double totalWeight=0;
            double maxWeight = 2000;
            // foreach (var lineItem in lineItems)
            // {var product = productDataModels.FirstOrDefault(p => p.Id.Equals(lineItem.ProductId));
            //     totalWeight += (product.Weight)*(lineItem.Quantity);
            // }
            // int truckCount=Convert.ToInt32(totalWeight/maxWeight)+1;



            //create allItems list of product and weight
            var allItems = new List<SingleItem>();

            foreach (var lineItem in lineItems)
            {
                var product = productDataModels.FirstOrDefault(p => p.Id.Equals(lineItem.ProductId));
                var singleItem = new SingleItem(product, product.Weight * lineItem.Quantity);
                allItems.Add(singleItem);
            }
            allItems.Sort();
            //a list of items in one truck
            List<SingleItem> truckItemList = new List<SingleItem>();
            List<Truck> truckList = new List<Truck>();


            //allocate items in List allItems to trucks
            while (allItems.Count > 0)
            {totalWeight=0;
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Weight + totalWeight <= maxWeight)
                    {
                        truckItemList.Add(allItems[i]);
                        allItems.Remove(allItems[i]);
                    }
                }
                //Truck has Id and a list of singleItems
                Truck newTruck = new Truck(truckItemList);
                truckList.Add(newTruck);
             }
             return new TruckResponse(truckList);

        }
    }
}