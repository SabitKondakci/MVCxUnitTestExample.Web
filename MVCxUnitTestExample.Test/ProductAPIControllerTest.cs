using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCxUnitTestExample.Web.Controllers;
using MVCxUnitTestExample.Web.Models;
using MVCxUnitTestExample.Web.Repository;
using Xunit;

namespace MVCxUnitTestExample.Test
{
    public class ProductAPIControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsAPIController _apiController;
        private List<Product> products;

        public ProductAPIControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _apiController = new ProductsAPIController(_mockRepo.Object);

            products = new List<Product>()
                {new Product() {Color = "Grey", Id = 3, Name = "OstrichToy", Price = 85.12m, Stock = 133},new Product() {Color = "Blue", Id = 11, Name = "FiberPen", Price = 12m, Stock = 1500}};
        }

        [Fact]
        public async void GetProducts_ActionExecutes_ReturnApiOkResultWithProduct()
        {
            _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(products);
            var resultList = await _apiController.GetProducts();
            var okReturn = Assert.IsType<OkObjectResult>(resultList);

            var retProducts=Assert.IsAssignableFrom<IEnumerable<Product>>(okReturn.Value);

            Assert.Equal(2,retProducts.Count());
        }

        [Fact]
        public async void GetProduct_ProductNull_ReturnNotFoundResult()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var productResult = await _apiController.GetProduct(0);
            var retNotFound = Assert.IsType<NotFoundResult>(productResult);

            Assert.Equal(404,retNotFound.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(11)]
        public async void GetProduct_ValidID_ReturnApiOkResultWithProduct(int id)
        {
            Product product = products.First(x => x.Id == id);
            _mockRepo.Setup(x => x.GetById(id)).ReturnsAsync(product);

            var result = await _apiController.GetProduct(id);
            var retOkResultWithProduct = Assert.IsType<OkObjectResult>(result);
            var retProduct = Assert.IsType<Product>(retOkResultWithProduct.Value);

            Assert.Equal(id,retProduct.Id);
            Assert.Equal(product.Color,retProduct.Color);
            Assert.Equal(product.Price,retProduct.Price);
            Assert.Equal(product.Stock, retProduct.Stock);
            Assert.Equal(product.Name, retProduct.Name);

        }

        [Theory]
        [InlineData(3)]
        public void PutProduct_IdNotEqualProductID_ReturnBadRequestResult(int id)
        {
            Product product = products.First(x => x.Id == id);

            var result = _apiController.PutProduct(0, product);
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(11)]
        public void PutProduct_ActionExecutes_ReturnNoContentResultAndVerifyUpdateMethod(int id)
        {
            Product product = products.First(x => x.Id == id);
            _mockRepo.Setup(x => x.Update(product));

            var result = _apiController.PutProduct(id, product);
            _mockRepo.Verify(x=>x.Update(product),Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnCreatedAtActionAndVerifyCreateMethod()
        {
            Product product = products.First();
            _mockRepo.Setup(x => x.Create(product)).Returns(Task.CompletedTask);

            var result = await _apiController.PostProduct(product);
            var creAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(x=>x.Create(product),Times.Once);
            Assert.Equal("GetProduct",creAtActionResult.ActionName);
        }

        [Fact]
        public async void DeleteProduct_ProductIsNull_ReturnNotFoundResult()
        {
            const int id = 0;
            Product product = null;

            _mockRepo.Setup(x => x.GetById(id)).ReturnsAsync(product);
            var result = await _apiController.DeleteProduct(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(11)]
        public async void DeleteProduct_ActionExecutes_ReturnNoContentResultAndVerifyDeleteMethod(int id)
        {
            Product product = products.First(x => x.Id == id);

            _mockRepo.Setup(x => x.GetById(id)).ReturnsAsync(product);
            _mockRepo.Setup(x => x.Delete(product));

            var result = await _apiController.DeleteProduct(id);
            _mockRepo.Verify(x => x.Delete(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
