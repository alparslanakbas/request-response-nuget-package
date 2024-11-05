global using System.Text;

// middlewares
global using Microsoft.AspNetCore.Http;
global using Microsoft.IO;
global using RRM_Library.Interfaces;
global using RRM_Library.Models;
global using System.Diagnostics;

// log writers
global using Microsoft.Extensions.Logging;

// models
global using System.Text.Json.Serialization;

// log writers
global using RRM_Library.MessageCreators;

// extensions
global using Microsoft.AspNetCore.Builder;
global using RRM_Library.LogWriters;
global using RRM_Library.Middlewares;