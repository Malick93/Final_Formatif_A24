using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Services;

namespace WebAPI.Tests;

[TestClass]
public class SeatsControllerTests
{
    [TestMethod]
    public void ReserveSeat()
    {
        var seatMock = new Mock<SeatsService>();
        
    }
}
