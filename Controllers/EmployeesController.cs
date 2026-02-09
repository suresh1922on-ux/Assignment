using Assignment_Web_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Assignment_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
        {
            _config = config;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            List<Employee> employees = new();

            var sql = "SELECT * FROM Employees";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Department = reader["Department"] as string,
                    Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                    DateJoined = reader["DateJoined"] as DateTime?
                });
            }

            return Ok(employees);
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {
            var sql = "SELECT * FROM Employees WHERE Id = @Id";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return NotFound("Employee not found");

            var emp = new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Department = reader["Department"] as string,
                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                DateJoined = reader["DateJoined"] as DateTime?
            };

            return Ok(emp);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(Employee emp)
        {
            var sql = @"INSERT INTO Employees
                        (FirstName, LastName, Email, Department, Salary, DateJoined)
                        VALUES
                        (@FirstName, @LastName, @Email, @Department, @Salary, @DateJoined)";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
            cmd.Parameters.AddWithValue("@LastName", emp.LastName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@DateJoined", emp.DateJoined ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            return Ok("Employee created successfully");
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee emp)
        {
            var sql = @"UPDATE Employees SET
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        Department = @Department,
                        Salary = @Salary,
                        DateJoined = @DateJoined
                        WHERE Id = @Id";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@FirstName", emp.FirstName);
            cmd.Parameters.AddWithValue("@LastName", emp.LastName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@DateJoined", emp.DateJoined ?? (object)DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound("Employee not found");

            return Ok("Employee updated");
        }

        // ================= DELETE SINGLE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sql = "DELETE FROM Employees WHERE Id = @Id";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound("Employee not found");

            return Ok("Employee deleted");
        }

        // ================= DELETE MULTIPLE =================
        [HttpDelete("delete-multiple")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("No IDs provided");

            var sql = $"DELETE FROM Employees WHERE Id IN ({string.Join(",", ids)})";

            await using var con = new SqlConnection(_config.GetConnectionString("Con"));
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            int rows = await cmd.ExecuteNonQueryAsync();

            return Ok($"{rows} employees deleted");
        }
    }
}
