using Device.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.HttpAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceDataController:ControllerBase
    {
        private readonly IMySQLDataService _mySQLDataService;
        private readonly ILogger<DeviceDataController> _logger;
         public DeviceDataController(ILogger<DeviceDataController> logger, IMySQLDataService mySQLDataService)
        {
            _logger = logger;
            _mySQLDataService = mySQLDataService;
        }

        [HttpGet]
        public async Task<IEnumerable<DeviceDataInfos>> Get(int page = 1, int size = 10)
        {
            return await _mySQLDataService.GetDeviceDataPageAsync(page, size);
        }
        //post
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DeviceDataInfos deviceDataInfos)
        {
            if (deviceDataInfos == null)
            {
                return BadRequest();
            }
            await _mySQLDataService.InsertDeviceData(deviceDataInfos);
            return Ok();
        }

        // GET api/devicedata/latest
        [HttpGet("latest")]
        public async Task< IEnumerable<DeviceDataInfos>> GetLatest()
        {
            return await _mySQLDataService.GetDeviceDataHistory();
        }



    }
}
