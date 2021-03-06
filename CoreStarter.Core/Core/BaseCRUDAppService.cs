﻿using AutoMapper;
using CoreStarter.Core.EntityUtlities;
using CoreStarter.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreStarter.Core.Core
{
    public class BaseCRUDAppService<TEntity, TEntityPrimaryKey, TEntityDto, TCreateEntityDto, TEditEntityDto>
      : IBaseCRUDAppService<TEntityPrimaryKey, TEntityDto, TCreateEntityDto, TEditEntityDto>

        where TEntity : class
        where TEntityDto : new()
        where TCreateEntityDto : class
        where TEditEntityDto : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;

        protected internal BaseCRUDAppService(IServiceBaseParameter serviceBaseParameter)
        {
            _unitOfWork = serviceBaseParameter.UnitOfWork;
            _mapper = serviceBaseParameter.Mapper;
        }

        public virtual async Task<IReadOnlyList<TEntityDto>> GetAllAsync()
        {
            var query = await _unitOfWork.Repository<TEntity, TEntityPrimaryKey>().ListAllAsync();

            return _mapper.Map<IReadOnlyList<TEntityDto>>(query);
        }

        public virtual async Task<TEntityDto> InsertAsync(TCreateEntityDto createEntityDto)
        {
            TEntity entity = _mapper.Map<TEntity>(createEntityDto);

            PropertyInfo prop = entity.GetType().GetProperty(nameof(AuditedEntity<TEntityPrimaryKey>.CreationTime));

            if (prop != null) prop.SetValue(entity, DateTime.Now);

            _unitOfWork.Repository<TEntity, TEntityPrimaryKey>().Add(entity);

            int result = await _unitOfWork.Complete();

            return result > 0 ? _mapper.Map<TEntityDto>(entity) : new TEntityDto();
        }

        public virtual async Task<TEntityDto> UpdateAsync(TEditEntityDto editEntityDto)
        {
            TEntityPrimaryKey entityPrimaryKey = (TEntityPrimaryKey)editEntityDto.GetType().GetProperty(nameof(EntityPK<TEntityPrimaryKey>.Id)).GetValue(editEntityDto, null);

            TEntity entityToUpdate = _mapper.Map<TEntity>(await GetByIdAsync(entityPrimaryKey));

            PropertyInfo prop = entityToUpdate.GetType().GetProperty(nameof(AuditedEntity<TEntityPrimaryKey>.UpdateTime));

            if (prop != null) prop.SetValue(entityToUpdate, DateTime.Now);

            _ = _mapper.Map(editEntityDto, entityToUpdate);

            _unitOfWork.Repository<TEntity, TEntityPrimaryKey>().Update(entityToUpdate);

            int result = await _unitOfWork.Complete();

            return result > 0 ? _mapper.Map<TEntityDto>(entityToUpdate) : new TEntityDto();
        }

        public virtual async Task<bool> DeleteAsync(TEntityPrimaryKey entityPrimaryKey)
        {
            TEntity entityToDelete = _mapper.Map<TEntity>(await GetByIdAsync(entityPrimaryKey));

            _unitOfWork.Repository<TEntity, TEntityPrimaryKey>().Delete(entityToDelete);

            int result = await _unitOfWork.Complete();

            bool isSuccessed = result > 0;

            return isSuccessed;
        }

        public virtual async Task<TEntityDto> GetByIdAsync(TEntityPrimaryKey entityPrimaryKey)
        {
            TEntity entity = await _unitOfWork.Repository<TEntity, TEntityPrimaryKey>().GetByIdAsync(entityPrimaryKey);

            TEntityDto entityDto = _mapper.Map<TEntityDto>(entity);

            return entityDto;
        }
    }
}
