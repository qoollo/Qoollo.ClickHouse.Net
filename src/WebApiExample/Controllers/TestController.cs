using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClickHouse.Ado;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.AggregatingQueueProcessor;
using Qoollo.ClickHouse.Net.Repository;
using WebApiExample.Dto;
using WebApiExample.Model;

namespace WebApiExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IClickHouseRepository _clickHouseRepository;
        private readonly IClickHouseAggregatingQueueProcessor<TestEntity> _processor;
        
        private readonly ILogger<TestController> _logger;

        private readonly string _selectAllQuery = $"SELECT * FROM {TestEntity.TableName};";
        private readonly string _selectByIdQuery = $"SELECT * FROM {TestEntity.TableName} WHERE userId = @serchId;";

        public TestController(IClickHouseRepository clickHouseRepository, IClickHouseAggregatingQueueProcessor<TestEntity> processor, ILogger<TestController> logger)
        {
            _clickHouseRepository = clickHouseRepository;
            _processor = processor;
            _logger = logger;
        }

        /// <summary>
        /// Return all entities
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<EntityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var mapper = new TestEntityMapper();
            try
            {
                var entities = await _clickHouseRepository.ExecuteQueryMappingAsync(_selectAllQuery, mapper);
                return Ok(entities.Select(e => new EntityDto(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Return entities by userId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<EntityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{userId}")]
        public async Task<IActionResult> Get([FromRoute] int userId)
        {
            var mapper = new TestEntityMapper();
            try
            {
                var parameters = new List<ClickHouseParameter>()
                {
                    new ClickHouseParameter()
                    {
                        ParameterName = "serchId",
                        Value = userId,
                        DbType = System.Data.DbType.Int32
                    }
                };

                var entities = await _clickHouseRepository.ExecuteQueryMappingAsync(_selectByIdQuery, mapper, parameters);
                return Ok(entities.Select(e => new EntityDto(e)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Add entity to queueProcessor
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(EntityDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost()]
        public IActionResult Post([FromBody] PostEntityDto dto)
        {
            try
            {
                var entity = new TestEntity(dto.UserId, _processor.TotalAddedItemsCount + 1, dto.TimeStamp, dto.TestArray, dto.Payload);
                _processor.Add(entity);
                _logger.LogInformation("Add entity with Id: {0}, userId {1}", entity.UserId, entity.Id);

                return Created($"{entity.Id}", new EntityDto(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
