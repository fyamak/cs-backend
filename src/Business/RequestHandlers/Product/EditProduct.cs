﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Business.Services.Security.Auth.Jwt.Models;
using Business.Services.Security.Auth.Jwt;
using Infrastructure.Data.Postgres.Entities;
using Shared.Extensions;
using Infrastructure.Data.Postgres.Repositories.Interface;


namespace Business.RequestHandlers.Product
{
    public class EditProduct
    {
        public class EditProductRequest : IRequest<DataResult<EditProductResponse>>
        {
            public int Id { get; internal set; }
            public string Name { get; set; }
        }

        public class EditProductResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class EditProductRequestValidator : AbstractValidator<EditProductRequest>
        {
            public EditProductRequestValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        public class EditProductRequestHandler : IRequestHandler<EditProductRequest, DataResult<EditProductResponse>>
        {
            private const string InvalidProductId = "Invalid product id.";
            private const string ProductWithSameNameAlreadyExists = "Product with same name already exists";
            private const string ProductCouldNotUpdatedOnDatabase = "Product could not update on database.";
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;
            //private readonly IProductRepository _productRepository;

            public EditProductRequestHandler(IUnitOfWork unitOfWork, ILogger logger)
            {
                _unitOfWork = unitOfWork;
                _logger = logger;
            }

            public async Task<DataResult<EditProductResponse>> Handle(EditProductRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    //what should i do if it is deleted?
                   var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted);

                    if (product == null)
                    {
                        return DataResult<EditProductResponse>.Invalid(InvalidProductId);
                    }

                    if (await _unitOfWork.Products.CountAsync(p => p.Name == request.Name) > 0)
                    {
                        return DataResult<EditProductResponse>.Invalid(ProductWithSameNameAlreadyExists);
                    }

                    product.Name = request.Name;
                    product.UpdatedAt = DateTime.UtcNow;
                    // is result check? is PostgresContext throw error if it is return 0
                    int result = await _unitOfWork.Products.Update(product);
                    if (result > 0)
                    {
                        return DataResult<EditProductResponse>.Success(new EditProductResponse
                        {
                            Id = product.Id,
                            Name = product.Name,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.UpdatedAt
                        });
                    }

                    return DataResult<EditProductResponse>.Invalid(ProductCouldNotUpdatedOnDatabase);
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<EditProductResponse>.Error(ex.Message);
                }
            }
        }
    }
}
