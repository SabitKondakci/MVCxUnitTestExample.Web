using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCxUnitTestExample.Web.Controllers;
using MVCxUnitTestExample.Web.Models;
using MVCxUnitTestExample.Web.Repository;
using Xunit;

namespace MVCxUnitTestExample.Test
{
    public class ProductContollerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepository;
        private readonly ProductsController _controller;
        private List<Product> products;
        public ProductContollerTest()
        {
            _mockRepository = new Mock<IRepository<Product>>(MockBehavior.Loose);
            _controller = new ProductsController(_mockRepository.Object);

            products = new List<Product>()
                {new Product() {Color = "Grey", Id = 3, Name = "OstrichToy", Price = 85.12m, Stock = 133},new Product() {Color = "Blue", Id = 11, Name = "FiberPen", Price = 12m, Stock = 1500}};
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnViewResult()
        {
            var retView = await _controller.Index();
            Assert.IsType<ViewResult>(retView);
        }

        [Fact]
        public async void Index_ActionExecutes_ArgumentProductList()
        {
            _mockRepository.Setup(x => x.GetAll()).ReturnsAsync(products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            Assert.Equal<int>(2,productList.Count());
        }

        [Fact]
        public async void Details_IdIsNull_ReturnNotFoundResult()
        {
            var retView = await _controller.Details(null);
            var notFoundResult = Assert.IsType<NotFoundResult>(retView);
            var code = notFoundResult.StatusCode;
            Assert.Equal(404,code);
        }

        [Fact]
        public async void Details_ProductIsNull_ReturnNotFoundResult()
        {
            Product product = null;
            _mockRepository.Setup(x => x.GetById(-1)).ReturnsAsync(product);
            var productList = await _controller.Details(-1);

            var notFoundResult = Assert.IsType<NotFoundResult>(productList);
            Assert.Equal(404,notFoundResult.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(11)]
        public async void Details_ValidID_ReturnProduct(int productID)
        {
            Product product = products.First(x => x.Id == productID);
            _mockRepository.Setup(x => x.GetById(productID)).ReturnsAsync(product);

            var retList = await _controller.Details(productID);

            var viewResult = Assert.IsType<ViewResult>(retList);

            var resProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(resProduct.Id,productID);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnViewResult()
        {
            var viewResult = _controller.Create();
            Assert.IsType<ViewResult>(viewResult);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_ReturnViewResult()
        {
            _controller.ModelState.AddModelError("Id","Id field is necessary");
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToActionResult()
        {
            _mockRepository.Setup(x=>x.Create(products.First())).Verifiable();
            var result = await _controller.Create(products.First());
            var redToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",redToAction.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product testProduct = null;
            _mockRepository.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>(x => testProduct = x);
            var result = await _controller.Create(products.First());

            _mockRepository.Verify(x=>x.Create(It.IsAny<Product>()),Times.Once);
            Assert.Equal(testProduct.Id,products.First().Id);
        }

        [Fact]
        public async void CreatePOST_InValidModelState_NoneCreateMethodExecute()
        {
            _controller.ModelState.AddModelError("Id", "Id field is necessary");

            Product testProduct = null;
            _mockRepository.Setup(x => x.Create(It.IsAny<Product>())).Callback<Product>(x => testProduct = x);
            var result = await _controller.Create(products.First());

            Assert.IsType<ViewResult>(result);
            Assert.Null(testProduct);

            //or simply _mockRepository.Verify(x=>x.Create(It.IsAny<Product>()),Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnNotFoundResult()
        {
            var retView = await _controller.Edit(null);
            var notFoundResult = Assert.IsType<NotFoundResult>(retView);
            var code = notFoundResult.StatusCode;
            Assert.Equal(404, code);
        }

        [Fact]
        public async void Edit_ProductIsNull_ReturnNotFoundResult()
        {
            Product product = null;
            _mockRepository.Setup(x => x.GetById(-1)).ReturnsAsync(product);
            var productList = await _controller.Edit(-1);

            var notFoundResult = Assert.IsType<NotFoundResult>(productList);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(11)]
        public async void Edit_ValidID_ReturnProduct(int productID)
        {
            Product product = products.First(x => x.Id == productID);
            _mockRepository.Setup(x => x.GetById(productID)).ReturnsAsync(product);

            var retList = await _controller.Edit(productID);

            var viewResult = Assert.IsType<ViewResult>(retList);

            var resProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(resProduct.Id,productID);
        }

        [Theory]
        [InlineData(3)]
        public void EditPOST_IdNotEqualProductID_ReturnNotFoundResult(int productID)
        {
            var retNotFoundResult = _controller.Edit(4, products.First(x => x.Id == productID));
            var result=Assert.IsType<NotFoundResult>(retNotFoundResult);
            Assert.Equal(404,result.StatusCode);
        }

        [Fact]
        public void EditPOST_InValidModelState_ReturnViewResult()
        {
            _controller.ModelState.AddModelError("Id", "Id field is necessary");
            var result = _controller.Edit(3,products.First(x=>x.Id==3));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(3)]
        public  void EditPOST_ValidModelState_ReturnRedirectToActionResult(int productID)
        {
            var result = _controller.Edit(productID,products.First(x=>x.Id==productID));
            var redToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redToAction.ActionName);
        }
        [Theory]
        [InlineData(3)]
        public void EditPOST_ValidModelState_UpdateMethodExecute(int productID)
        {
            Product product = products.First(x => x.Id == productID);
            _mockRepository.Setup(x => x.Update(product));
            _controller.Edit(productID, product);
            _mockRepository.Verify(x=>x.Update(It.IsAny<Product>()),Times.Once);
        }

        [Fact]
        public async void Delete_IdIsNull_ReturnNotFoundResult()
        {
            var retView = await _controller.Delete(null);
            var notFoundResult = Assert.IsType<NotFoundResult>(retView);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async void Delete_ProductIsNull_ReturnNotFoundResult()
        {
            Product product = null;
            _mockRepository.Setup(x => x.GetById(-1)).ReturnsAsync(product);
            var productList = await _controller.Delete(-1);

            var notFoundResult = Assert.IsType<NotFoundResult>(productList);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(11)]
        public async void Delete_ValidID_ReturnProduct(int productID)
        {
            Product product = products.First(x => x.Id == productID);
            _mockRepository.Setup(x => x.GetById(productID)).ReturnsAsync(product);

            var retList = await _controller.Delete(productID);

            var viewResult = Assert.IsType<ViewResult>(retList);

            var resProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(resProduct.Id, productID);
        }

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToActionResult(int id)
        {
            _mockRepository.Setup(x => x.GetById(id)).ReturnsAsync(products.First(x => x.Id == id));
            var result = await _controller.DeleteConfirmed(id);
            var redToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",redToAction.ActionName);

        }

        [Theory]
        [InlineData(3)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int id)
        {
            Product product = products.First(x => x.Id == id);
            _mockRepository.Setup(x => x.Delete(product));
            await _controller.DeleteConfirmed(id);
            _mockRepository.Verify(x => x.Delete(It.IsAny<Product>()), Times.Once);

        }

        [Theory]
        [InlineData(11)]
        public void ProductExists_IdCheck_ReturnTrue(int id)
        {
            Product product = products.First(x => x.Id == id);
            _mockRepository.Setup(x => x.GetById(id)).ReturnsAsync(product);
            var result = _controller.ProductExists(id);
            Assert.True(result);
        }
    }

}
