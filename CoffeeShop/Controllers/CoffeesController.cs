using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CoffeeShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoffeesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CoffeesController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public ActionResult<List<Coffee>> Get([FromQuery] string beanType, [FromQuery] string sortBy)
        {
            if (beanType == null)
            {
                beanType = "";
            }

            if(sortBy == null || sortBy.ToLower() != "beantype")
            {
                sortBy = "BeanType";
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Title, BeanType 
                        FROM Coffee
                        WHERE beanType LIKE '%' + @beanType + '%'";

                    cmd.Parameters.Add(new SqlParameter("@beanType", beanType));
                    cmd.Parameters.Add(new SqlParameter("@sorted", sortBy));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Coffee> coffees = new List<Coffee>();

                    while (reader.Read())
                    {
                        Coffee coffee = new Coffee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            BeanType = reader.GetString(reader.GetOrdinal("BeanType"))
                        };

                        coffees.Add(coffee);
                    }
                    reader.Close();

                    return Ok(coffees);
                }
            }
        }

        //[HttpGet("{id}", Name = "GetCoffee")]
        //public ActionResult<Coffee> Get([FromRoute] string id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT
        //                    Id, Title, BeanType
        //                FROM Coffee
        //                WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = cmd.ExecuteReader();

        //            Coffee coffee = null;

        //            if (reader.Read())
        //            {
        //                coffee = new Coffee
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    Title = reader.GetString(reader.GetOrdinal("Title")),
        //                    BeanType = reader.GetString(reader.GetOrdinal("BeanType"))
        //                };
        //            }

        //            if (coffee == null)
        //            {
        //                return NotFound($"Coffee with the id {id} is not found");
        //            }
        //            reader.Close();

        //            return Ok(coffee);
        //        }
        //    }
        //}
        [HttpPost]
        public ActionResult<Coffee> Post([FromBody] Coffee coffee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Coffee (Title, BeanType)
                                        OUTPUT INSERTED.Id
                                        VALUES (@title, @beanType)";
                    cmd.Parameters.Add(new SqlParameter("@title", coffee.Title));
                    cmd.Parameters.Add(new SqlParameter("@beanType", coffee.BeanType));

                    int newId = (int)cmd.ExecuteScalar();
                    coffee.Id = newId;
                    return CreatedAtRoute("GetCoffee", new { id = newId }, coffee);
                }
            }
        }
    }
}