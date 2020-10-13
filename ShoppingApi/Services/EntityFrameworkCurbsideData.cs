﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoppingApi.Controllers;
using ShoppingApi.Domain;
using ShoppingApi.Models.Curbside;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingApi.Services
{
    public class EntityFrameworkCurbsideData : IDoCurbsideQueries, IDoCurbsideCommands
    {
        private readonly ShoppingDataContext _context;
        private readonly IMapper _mapper;

        public EntityFrameworkCurbsideData(ShoppingDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        async Task<CurbsideOrder> IDoCurbsideCommands.AddOrder(PostCurbsideOrderRequest orderToPlace)
        {
            await Task.Delay(3000);

            var order = _mapper.Map<CurbsideOrder>(orderToPlace);
            _context.CurbsideOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        async Task<GetCurbsideOrdersResponse> IDoCurbsideQueries.GetAll()
        {
            var response = new GetCurbsideOrdersResponse();
            var data = await _context.CurbsideOrders.ToListAsync();
            response.Data = data;
            response.NumberOfApprovedOrders = response.Data.Count(o => o.Status == CurbsideOrderStatus.Approved);
            response.NumberOfDeniedOrders = response.Data.Count(o => o.Status == CurbsideOrderStatus.Denied);
            response.NumberOfFulfilledOrders = response.Data.Count(o => o.Status == CurbsideOrderStatus.Fulfilled);
            response.NumberOfPendingOrders = response.Data.Count(o => o.Status == CurbsideOrderStatus.Pending);

            return response;
        }

        async Task<CurbsideOrder> IDoCurbsideQueries.GetById(int orderId)
        {
            var response = await _context.CurbsideOrders.SingleOrDefaultAsync(c => c.Id == orderId);
            return response;
        }
    }
}
