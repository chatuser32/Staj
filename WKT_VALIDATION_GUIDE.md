# WKT Validation Implementation Guide

## Overview
Bu proje, kullanıcıdan name ve WKT (Well-Known Text) bilgilerini alır, WKT'yi geometry'ye çevirir ve veritabanında geometry olarak saklar. Ayrıca WKT tiplerinin doğru formatda olup olmadığını regex pattern'ları ile kontrol eder.

## Configuration

### appsettings.json
WKT validation ayarları `appsettings.json` dosyasında tanımlanmıştır:

```json
{
  "WktValidation": {
    "Patterns": {
      "Point": "^POINT\\s*\\(\\s*-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*\\)$",
      "LineString": "^LINESTRING\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)$",
      "Polygon": "^POLYGON\\s*\\(\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)(\\s*,\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\))*\\s*\\)$"
    },
    "EnableRegexValidation": true,
    "EnableNetTopologyValidation": true
  }
}
```

### WKT Pattern Açıklamaları

#### Point Pattern
```regex
^POINT\\s*\\(\\s*-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*\\)$
```
- `POINT(10.5 20.3)` ✅ Geçerli
- `POINT(10 20)` ✅ Geçerli
- `POINT(-10.5 -20.3)` ✅ Geçerli (negatif koordinatlar)
- `POINT(10.5)` ❌ Geçersiz (eksik Y koordinatı)

#### LineString Pattern
```regex
^LINESTRING\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)$
```
- `LINESTRING(0 0, 1 1, 2 2)` ✅ Geçerli
- `LINESTRING(0.5 0.5, 1.5 1.5)` ✅ Geçerli
- `LINESTRING(0 0)` ❌ Geçersiz (tek nokta)

#### Polygon Pattern
```regex
^POLYGON\\s*\\(\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)(\\s*,\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\))*\\s*\\)$
```
- `POLYGON((0 0, 10 0, 10 10, 0 10, 0 0))` ✅ Geçerli
- `POLYGON((0 0, 10 0, 10 10, 0 10, 0 0), (2 2, 8 2, 8 8, 2 8, 2 2))` ✅ Geçerli (delikli polygon)

## Implementation

### 1. Configuration Class
```csharp
// Core/Configuration/WktValidationConfig.cs
public class WktValidationConfig
{
    public Dictionary<string, string> Patterns { get; set; } = new Dictionary<string, string>();
    public bool EnableRegexValidation { get; set; } = true;
    public bool EnableNetTopologyValidation { get; set; } = true;
}
```

### 2. Enhanced Validator
```csharp
// Services/Validatior/GeometryDtoValidator.cs
public class GeometryValidator
{
    private readonly WktValidationConfig _config;

    public GeometryValidator(IOptions<WktValidationConfig> config)
    {
        _config = config.Value;
    }

    public string? Validate(GeometryDto dto)
    {
        // Name validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ErrorMessages.NameRequierd;

        // WKT validation
        if (string.IsNullOrWhiteSpace(dto.Wkt))
            return ErrorMessages.WktRequired;

        // Type validation
        if (!Enum.TryParse<App.Core.Entities.GeometryType>(dto.Type, true, out var geometryType))
            return ErrorMessages.TypeRequired;

        // Regex validation (if enabled)
        if (_config.EnableRegexValidation)
        {
            var regexValidationResult = ValidateWktWithRegex(dto.Wkt, geometryType);
            if (regexValidationResult != null)
                return regexValidationResult;
        }

        // NetTopologySuite validation (if enabled)
        if (_config.EnableNetTopologyValidation)
        {
            var netTopologyValidationResult = ValidateWktWithNetTopology(dto.Wkt);
            if (netTopologyValidationResult != null)
                return netTopologyValidationResult;
        }

        return null;
    }
}
```

### 3. Service Registration
```csharp
// Services/ServiceExtentions/ServiceExtention.cs
public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
{
    // Configure WKT validation settings
    services.Configure<WktValidationConfig>(options => 
        configuration.GetSection("WktValidation").Bind(options));
    
    // Register validators
    services.AddScoped<GeometryValidator>();
    
    // Register services
    services.AddScoped<IGeometryService, GeometryService>();
    return services;
}
```

## API Usage

### POST /api/geometry
Yeni geometry oluşturur:

```json
{
  "name": "Test Point",
  "wkt": "POINT(10.5 20.3)",
  "type": "Point"
}
```

### Validation Examples

#### Valid Requests
```json
// Point
{
  "name": "Istanbul",
  "wkt": "POINT(28.9784 41.0082)",
  "type": "Point"
}

// LineString
{
  "name": "Road",
  "wkt": "LINESTRING(0 0, 1 1, 2 2)",
  "type": "LineString"
}

// Polygon
{
  "name": "Area",
  "wkt": "POLYGON((0 0, 10 0, 10 10, 0 10, 0 0))",
  "type": "Polygon"
}
```

#### Invalid Requests
```json
// Invalid Point format
{
  "name": "Bad Point",
  "wkt": "POINT(10.5)",
  "type": "Point"
}
// Error: "WKT format does not match expected pattern for Point"

// Empty name
{
  "name": "",
  "wkt": "POINT(10.5 20.3)",
  "type": "Point"
}
// Error: "Name is required"

// Invalid type
{
  "name": "Test",
  "wkt": "POINT(10.5 20.3)",
  "type": "InvalidType"
}
// Error: "Type is required"
```

## Features

### Dual Validation
1. **Regex Validation**: Hızlı format kontrolü
2. **NetTopologySuite Validation**: Detaylı geometri doğrulama

### Configurable
- Validation türleri açılıp kapatılabilir
- Regex pattern'ları configuration'dan değiştirilebilir
- Yeni geometri tipleri kolayca eklenebilir

### Database Storage
- WKT string olarak saklanır
- Geometry NetTopologySuite ile parse edilir ve Geometry kolonda saklanır
- Spatial queries için optimize edilmiş

## Testing

Projeyi test etmek için:

```bash
# Build project
dotnet build

# Run API
dotnet run --project BasarSoftProject

# Test with curl
curl -X POST "https://localhost:7000/api/geometry" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Point",
    "wkt": "POINT(10.5 20.3)",
    "type": "Point"
  }'
```

## Configuration Flexibility

Regex pattern'ları ihtiyaçlara göre özelleştirilebilir:

```json
{
  "WktValidation": {
    "Patterns": {
      "Point": "^POINT\\s*\\(\\s*-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*\\)$",
      "LineString": "^LINESTRING\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)$",
      "Polygon": "^POLYGON\\s*\\(\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\)(\\s*,\\s*\\(\\s*(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?\\s*,\\s*)+(-?\\d+(\\.\\d+)?\\s+-?\\d+(\\.\\d+)?)\\s*\\))*\\s*\\)$"
    },
    "EnableRegexValidation": true,
    "EnableNetTopologyValidation": true
  }
}
```

Bu implementasyon ile kullanıcıdan name ve WKT bilgileri alınır, regex ile format kontrolü yapılır, NetTopologySuite ile geometry'ye çevrilir ve veritabanında hem WKT string hem de Geometry olarak saklanır.