using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using Hangfire;
using Hangfire.SqlServer;
using System.Text;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Core.Services;
using TrimangoCalendar.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

