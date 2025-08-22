using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Validators;
using App.Repositories.Interfaces;
using App.Services.Responses;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services.Interfaces;
using App.Services.Resources;

namespace App.Services.Services
{
    public class GeometryService : IGeometryService
    {
        private readonly IGeometryRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly GeometryValidator validator;

        public GeometryService(IGeometryRepository repository, IUnitOfWork unitOfWork, GeometryValidator validator)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.validator = validator;
        }

        public async Task<Response<GeometryDto>> AddGeometryAsync(GeometryDto dto)
        {
            var validationMessage = validator.Validate(dto);
            if (validationMessage != null)
                return Response<GeometryDto>.Fail(validationMessage);

            var entity = new GeometryEntity
            {
                Name = dto.Name,
                Wkt = dto.Wkt,
                Type = Enum.Parse<App.Core.Entities.GeometryType>(dto.Type, true),
                Geoloc = new WKTReader().Read(dto.Wkt)
            };

            await repository.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();

            return Response<GeometryDto>.Ok(dto, ErrorMessages.AddSuccess);
        }

        public async Task<Response<GeometryDto>> GetByIdAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
                return Response<GeometryDto>.Fail(ErrorMessages.NotFound);

            var dto = new GeometryDto
            {
                Name = entity.Name,
                Wkt = entity.Wkt,
                Type = entity.Type.ToString()
            };

            return Response<GeometryDto>.Ok(dto);
        }

        public async Task<Response<List<GeometryDto>>> GetAllAsync()
        {
            var entities = await repository.GetAllAsync();
            if (!entities.Any())
                return Response<List<GeometryDto>>.Fail(ErrorMessages.EmptyList);

            var list = entities.Select(e => new GeometryDto
            {
                Name = e.Name,
                Wkt = e.Wkt,
                Type = e.Type.ToString()
            }).ToList();

            return Response<List<GeometryDto>>.Ok(list);
        }

        public async Task<Response<GeometryDto>> UpdateGeometryAsync(int id, GeometryDto dto)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
                return Response<GeometryDto>.Fail(ErrorMessages.NotFound);

            var validationMessage = validator.Validate(dto);
            if (validationMessage != null)
                return Response<GeometryDto>.Fail(validationMessage);

            entity.Name = dto.Name;
            entity.Wkt = dto.Wkt;
            entity.Type = Enum.Parse<Core.Entities.GeometryType>(dto.Type, true);
            entity.Geoloc = new WKTReader().Read(dto.Wkt);

            repository.Update(entity);
            await unitOfWork.SaveChangesAsync();

            return Response<GeometryDto>.Ok(dto, ErrorMessages.UpdateSuccess);
        }

        public async Task<Response<bool>> DeleteGeometryAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
                return Response<bool>.Fail(ErrorMessages.NotFound);

            repository.Delete(entity);
            await unitOfWork.SaveChangesAsync();

            return Response<bool>.Ok(true, ErrorMessages.DeleteSuccess);
        }
    }
}
