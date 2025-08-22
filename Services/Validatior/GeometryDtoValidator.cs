using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Configuration;
using App.Services.Resources;
using NetTopologySuite.IO;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace App.Core.Validators
{
    public class GeometryValidator
    {
        private readonly WktValidationConfig _config;

        public GeometryValidator(IOptions<WktValidationConfig> config)
        {
            _config = config.Value;
        }

        public string? Validate(GeometryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ErrorMessages.NameRequierd;

            if (string.IsNullOrWhiteSpace(dto.Wkt))
                return ErrorMessages.WktRequired;

            if (!Enum.TryParse<App.Core.Entities.GeometryType>(dto.Type, true, out var geometryType))
                return ErrorMessages.TypeRequired;

            // Regex validation if enabled
            if (_config.EnableRegexValidation)
            {
                var regexValidationResult = ValidateWktWithRegex(dto.Wkt, geometryType);
                if (regexValidationResult != null)
                    return regexValidationResult;
            }

            // NetTopologySuite validation if enabled
            if (_config.EnableNetTopologyValidation)
            {
                var netTopologyValidationResult = ValidateWktWithNetTopology(dto.Wkt);
                if (netTopologyValidationResult != null)
                    return netTopologyValidationResult;
            }

            return null;
        }

        private string? ValidateWktWithRegex(string wkt, App.Core.Entities.GeometryType geometryType)
        {
            var geometryTypeString = geometryType.ToString();
            
            if (!_config.Patterns.ContainsKey(geometryTypeString))
                return $"Regex pattern not found for geometry type: {geometryTypeString}";

            var pattern = _config.Patterns[geometryTypeString];
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (!regex.IsMatch(wkt.Trim()))
                return $"WKT format does not match expected pattern for {geometryTypeString}";

            return null;
        }

        private string? ValidateWktWithNetTopology(string wkt)
        {
            try
            {
                var reader = new WKTReader();
                var geometry = reader.Read(wkt);
                
                if (geometry == null)
                    return ErrorMessages.InvalidWkt;
            }
            catch (Exception)
            {
                return ErrorMessages.InvalidWkt;
            }

            return null;
        }
    }
}
