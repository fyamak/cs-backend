﻿using Autofac;
using Business.RequestHandlers.Product;
using Moq;
using Shared.Models.Results;
using Business.Services.Kafka.Interface;

namespace Business.Test.RequestHandlers.Product;

public class AddSupplyTests : BaseHandlerTest
{
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;

    public AddSupplyTests()
    {
        _mockKafkaProducer = new Mock<IKafkaProducerService>();

        ContainerBuilder.RegisterInstance(_mockKafkaProducer.Object).As<IKafkaProducerService>();

        BuildContainer();
    }

    [Fact]
    public async Task AddSupply_Success_When_Request_Is_Valid_Test()
    {
        var request = new AddSupply.AddSupplyRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };

        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<AddSupply.AddSupplyMessage>()))
            .Returns(Task.CompletedTask);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Adding supply to product request accepted", response.Data);

        _mockKafkaProducer.Verify(x =>
            x.ProduceAsync("product-add-supply", It.Is<AddSupply.AddSupplyMessage>(m =>
                m.ProductId == request.ProductId &&
                m.OrganizationId == request.OrganizationId &&
                m.Quantity == request.Quantity &&
                m.Price == request.Price &&
                m.Date == request.Date &&
                m.OrderId == request.OrderId)),
            Times.Once);
    }

    [Fact]
    public async Task AddSupply_Fail_When_Validation_Fails_Test()
    {
        // Missing ProductId
        var request1 = new AddSupply.AddSupplyRequest
        {
            OrganizationId = 456,
            Quantity = 10,
            Price = 49.99,
            Date = DateTime.UtcNow,
            OrderId = 789
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);
        Assert.Contains("Product Id must not be empty", response1.Message);

        // Invalid Quantity
        var request2 = new AddSupply.AddSupplyRequest
        {
            ProductId = 123,
            OrganizationId = 456,
            Quantity = 0,
            Price = 49.99,
            Date = DateTime.UtcNow,
            OrderId = 789
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);
        Assert.Contains("Quantity must be greater than 0", response2.Message);

        // Negative Price
        var request3 = new AddSupply.AddSupplyRequest
        {
            ProductId = 123,
            OrganizationId = 456,
            Quantity = 10,
            Price = -1,
            Date = DateTime.UtcNow,
            OrderId = 789
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
        Assert.Contains("Price must be greater than or equal to 0", response3.Message);
    }

    [Fact]
    public async Task AddSupply_Fail_When_Kafka_Producer_Fails_Test()
    {
        var request = new AddSupply.AddSupplyRequest
        {
            ProductId = 1,
            OrganizationId = 1,
            Quantity = 1,
            Price = 1.1,
            Date = DateTime.UtcNow,
            OrderId = 1
        };

        var expectedException = new Exception("Kafka connection failed");
        _mockKafkaProducer.Setup(x =>
            x.ProduceAsync(It.IsAny<string>(), It.IsAny<AddSupply.AddSupplyMessage>()))
            .ThrowsAsync(expectedException);

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Error, response.Status);
        Assert.Equal(expectedException.Message, response.Message);
    }
}
