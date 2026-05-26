using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Tests;

[TestClass]
public class SeatsControllerTests
{
    [TestMethod]
    public void Reserve_Seat_Test()
    {
        //Arrange
        string fakeUserId = "user-123";
        int seatNumber = 4;
        Seat expected = new Seat() { Id = 1, Number = seatNumber, ExamenUserId = fakeUserId };

        Mock<SeatsService> mockService = new Mock<SeatsService>();
        mockService
            .Setup(s => s.ReserveSeat(fakeUserId, seatNumber))
            .Returns(expected);
        Mock<SeatsController> mockController = new Mock<SeatsController>(mockService.Object) { CallBase = true };
        mockController.Setup(c => c.UserId).Returns(fakeUserId);

        //Act
        ActionResult<Seat> result = mockController.Object.ReserveSeat(seatNumber);

        //Assert
        OkObjectResult okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        Seat returnedSeat = okResult.Value as Seat;
        Assert.IsNotNull(returnedSeat);
        Assert.AreEqual(expected.Id, returnedSeat.Id);
        Assert.AreEqual(expected.Number, returnedSeat.Number);
        Assert.AreEqual(expected.ExamenUserId, returnedSeat.ExamenUserId);
    }

    [TestMethod]
    public void Reserve_Seat_Unauthorize()
    {
        //Arrange
        string fakeUserId = "user-123";
        int seatNumber = 4;
        Seat expected = new Seat() { Id = 1, Number = seatNumber, ExamenUserId = fakeUserId };

        Mock<SeatsService> mockService = new Mock<SeatsService>();
        mockService
            .Setup(s => s.ReserveSeat(fakeUserId, seatNumber))
            .Throws<SeatAlreadyTakenException>();
        Mock<SeatsController> mockController = new Mock<SeatsController>(mockService.Object) { CallBase = true };
        mockController.Setup(c => c.UserId).Returns(fakeUserId);

        //Act
        ActionResult<Seat> result = mockController.Object.ReserveSeat(seatNumber);

        //Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public void NotFoundTest()
    {
        string fakeUserId = "user-123";
        int seatNumber = 105;
        Seat expected = new Seat() { Id = 1, Number = seatNumber, ExamenUserId = fakeUserId };

        Mock<SeatsService> mockService = new Mock<SeatsService>();
        mockService
            .Setup(s => s.ReserveSeat(fakeUserId, seatNumber))
            .Throws<SeatOutOfBoundsException>();

        Mock<SeatsController> mockController = new Mock<SeatsController>(mockService.Object) { CallBase = true };
        mockController.Setup(c => c.UserId).Returns(fakeUserId);

        ActionResult<Seat> result = mockController.Object.ReserveSeat(seatNumber);

        NotFoundObjectResult notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual("Could not find " + seatNumber, notFoundResult.Value);
    }

    [TestMethod]
    public void BadRequestTest()
    {
        //Arrange
        string fakeUserId = "user-123";
        int seatNumber = 4;
        Seat expected = new Seat() { Id = 1, Number = seatNumber, ExamenUserId = fakeUserId };

        Mock<SeatsService> mockService = new Mock<SeatsService>();
        mockService
            .Setup(s => s.ReserveSeat(fakeUserId, seatNumber)).Throws<UserAlreadySeatedException>();

        Mock<SeatsController> mockController = new Mock<SeatsController>(mockService.Object) { CallBase = true };
        mockController.Setup(c => c.UserId).Returns(fakeUserId);

        //Act
        ActionResult<Seat> result = mockController.Object.ReserveSeat(seatNumber);

        //Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestResult));
    }
}
