﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShoppingApi.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoppingApi.Models.Catalog;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using ShoppingApi.Hubs;

namespace ShoppingApi.Controllers
{
    public class CatalogController : ControllerBase
    {
        private readonly ShoppingDataContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _mapperConfig;
        private readonly ILogger<CatalogController> _logger;
        private readonly IHubContext<CurbsideOrdersHub> _hub;

        public CatalogController(ShoppingDataContext context, IConfiguration config, IMapper mapper, MapperConfiguration mapperConfig, ILogger<CatalogController> logger, IHubContext<CurbsideOrdersHub> hub)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _mapperConfig = mapperConfig;
            _logger = logger;
            _hub = hub;
        }

        [HttpPost("catalog")]
        public async Task<ActionResult> AddItem([FromBody] PostCatalogRequest newItem)
        {
            if(!ModelState.IsValid)
            {
                _logger.LogInformation("Got a bad request, looked like this @{newItem}", newItem);
                return BadRequest(ModelState);
            } else
            {
                // what am I missing?
                var item = _mapper.Map<ShoppingItem>(newItem);
                _context.ShoppingItems.Add(item);
                await _context.SaveChangesAsync();
                var response = _mapper.Map<GetCatalogResponseSummaryItem>(item);
                await _hub.Clients.All.SendAsync("ShoppingItemAdded", response);
                return StatusCode(201, response);
            }
        }

        [HttpGet("catalog")]
        public async Task<ActionResult> GetFullCatalog()
        {
            var data = await _context
                .ShoppingItems
                .AsNoTracking()
                .TagWith("catalog#getfullcatalog")
                .Where(item => item.InInventory)
                .ProjectTo<GetCatalogResponseSummaryItem>(_mapperConfig)
                .ToListAsync();

            var response = new GetCatalogResponse
            {
                Data = data
            };


            return Ok(response);
        }
    }
}
