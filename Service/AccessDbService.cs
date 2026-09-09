using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Configuration;
using PrescribingSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace PrescribingSystem.Service
{
    public class AccessDbService
    {
        private readonly string _connectionString;

        public AccessDbService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AccessConnection");
        }

    
    }
}
