using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebAPI.Models;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Context;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CryptocurrencyController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;
        private readonly APIDbContext _context;

        public CryptocurrencyController(IConnectionMultiplexer redis, APIDbContext context)
        {
            this._redis = redis;
            this._context = context;
        }

        /// <summary>
        /// Get a cryptocurrency's metrics with optional filters
        /// </summary>
        /// <remarks>
        /// 
        /// Sample requests:
        /// 
        ///     GET api/cryptocurrency/btc
        ///     Authorization: Basic YW5nZWxfYW5nZWxvdjphbmdlbA==
        ///     
        ///     GET api/cryptocurrency/btc?startDate=2021/10/01
        ///     Authorization: Basic YW5nZWxfYW5nZWxvdjphbmdlbA==
        ///     
        ///     GET api/cryptocurrency/btc?endDate=2009/01/05     
        ///     Authorization: Basic YW5nZWxfYW5nZWxvdjphbmdlbA==
        /// </remarks>
        /// <param name="key"> - an abbreviation of the cryptocurrency, 
        /// Possible ones: btc (Bitcoin), bch (Bitcoin Cach), bnb (Binance Coin), ltc (Litecoin), bsv (BitcoinSV), eth (Ethereum) , xrp (Ripple)
        /// </param>
        /// <returns> cryptocurrency metrics data for the last day in JSON format</returns>
        /// <response code="200">OK: Returns data in JSON format</response>
        /// <response code="204">No content: There is no content in the given period</response>
        /// <response code="400">Bad request: Wrong cryptocurrency abbreviation/ Query parameters are incorrect/ Start date must be before the end date 
        /// </response>  
        /// GET /api/cryptocurrency/{key}?startDate=dd/MM/yyyy
        [Authorize]
        [HttpGet]
        [Route("{key}")]
        public async Task<IActionResult> GetCryptocurrencyData([FromRoute] string key, [FromQuery]SearchFilter filter)
        {
            IDatabase db = this._redis.GetDatabase();
            string cryptocurrencyData = await db.StringGetAsync(key);
            if (cryptocurrencyData == null)
            {
                return (IActionResult)this.BadRequest(new { errorMessage = "Cryptocurrency not found" });
            }
            else
            {
                if (!string.IsNullOrEmpty(filter.startDate) || !String.IsNullOrEmpty(filter.endDate))
                {
                    List<Cryptocurrency> cryptocurrencyList = new List<Cryptocurrency>();
                    DateTime startDate, endDate = new DateTime();
                    bool isStartDateValid = DateTime.TryParse(filter.startDate, out startDate);
                    bool isEndDateValid = DateTime.TryParse(filter.endDate, out endDate);
                    // deserialize cached data in object    
                    cryptocurrencyList = JsonConvert.DeserializeObject<List<Cryptocurrency>>(cryptocurrencyData);
                    // get cryptocurrency metrics in period of time
                    if (!String.IsNullOrEmpty(filter.startDate) && !String.IsNullOrEmpty(filter.endDate))
                    {
                        // check if start date and end data are in correct format
                        if (!isStartDateValid || !isEndDateValid)
                        {
                            return (IActionResult)this.BadRequest("Parameters format not correct");
                        }
                        // check is start date is before end date
                        if (startDate > endDate)
                        {
                            return (IActionResult)this.BadRequest("Start date must be before the end date");
                        }
                        cryptocurrencyList = cryptocurrencyList.Where(c => c.date >= DateTime.Parse(filter.startDate) && c.date <= DateTime.Parse(filter.endDate)).ToList();
                    }
                    else if (!String.IsNullOrEmpty(filter.startDate) && String.IsNullOrEmpty(filter.endDate))
                    {
                        // check if start date is in correct format
                        if (!isStartDateValid)
                        {
                            return (IActionResult)this.BadRequest("Parameter format not correct");
                        }

                        cryptocurrencyList = cryptocurrencyList.Where(c => c.date >= DateTime.Parse(filter.startDate)).ToList();
                    }
                    else if (String.IsNullOrEmpty(filter.startDate) && !String.IsNullOrEmpty(filter.endDate))
                    {
                        // check if end date is in correct format
                        if (!isEndDateValid)
                        {
                            return (IActionResult)this.BadRequest("Parameter format not correct");
                        }
                        cryptocurrencyList = cryptocurrencyList.Where(c => c.date <= DateTime.Parse(filter.endDate)).ToList();
                    }
                    cryptocurrencyData = (cryptocurrencyList.Count!=0)?JsonConvert.SerializeObject(cryptocurrencyList, Formatting.Indented):"";
                }

                return string.IsNullOrEmpty(cryptocurrencyData) ? (IActionResult)this.NoContent() : this.Ok(cryptocurrencyData);
            }
        }


    }
}
