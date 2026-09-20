using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Infrastructure.Data
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException ex)
        {
            return ex.InnerException is Microsoft.Data.SqlClient.SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
