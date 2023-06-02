using Microsoft.AspNetCore.Mvc;
using APIWebNWind.Data;
using APIWebNWind.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace APIWebNWind.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly NorthwindContext contexto;

        public ProductController(NorthwindContext context)
        {
            contexto = context;
        }

        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return contexto.Products.OrderBy(p => p.ProductName);

        }

        /*
         Obtener el nombre del producto, categoría (nombre), existencia de todos los productos que estan activos con el uso de JOIN
       
         */

        [HttpGet]
        [Route("GetProductsNodiscontinued")]
        public IEnumerable<object> GetProductsNodiscontinued()
        {
            var result =
                from c in contexto.Categories
                join p in contexto.Products on c.CategoryId equals p.CategoryId
                where p.Discontinued == false
                select new
                {
                    Nombre=p.ProductName,
                    Categoria=c.CategoryName,
                    Existencias=p.UnitsInStock
                };
            return result;
        }

        [HttpGet]
        [Route("GetProductsNodiscontinued2")]
        public IEnumerable<object> GetProductsNodiscontinued2()
        {
            var result = contexto.Products
                .Where(p=>p.Discontinued==false)
                .Join(contexto.Categories,
                     (p) => p.CategoryId,
                     (c) => c.CategoryId,
                     (p, c) =>
                         new
                         {
                             Nombre = p.ProductName,
                             Categoria = c.CategoryName,
                             Existencias = p.UnitsInStock
                         }
                     );

            return result;


            //var result = contexto.Categories.
            //    Join(contexto.Products,
            //         (c) => c.CategoryId,
            //         (p) => p.CategoryId,
            //         (c, p) => 
            //             new
            //             {
            //                 Nombre = p.ProductName,
            //                 Categoria = c.CategoryName,
            //                 Existencias = p.UnitsInStock,
            //                 Activo= !p.Discontinued
            //             }
            //         )
            //    .Where(p=> p.Activo );

            //return result;
        }


        [HttpGet]
        [Route("GetNameAndPrice")]
        public IEnumerable<object> GetNameAndPrice()
        {
            IEnumerable<object> lista =
                from producto in contexto.Products
                select new
                {
                    Name = producto.ProductName,
                    Price = producto.UnitPrice,
                    Category=producto.Category.CategoryName
                };

            return lista;
        }

        [HttpGet]
        [Route("GetNameAndPrice2")]
        public IEnumerable<Product> GetNameAndPrice2()
        {
            Product p= new Product()
            {
                ProductName = "A",
                UnitPrice = 1
            };

            Product p1 = new Product();
            p1.ProductName = "A";
            p1.UnitPrice = 1;
            


            IEnumerable<Product> listaP =
                from producto in contexto.Products
                select new Product()
                {
                    ProductName = producto.ProductName,
                    UnitPrice = producto.UnitPrice
                };
            return listaP;
        }

        [HttpGet]
        [Route("GetInventarioCategoria")]
        public IEnumerable<object> GetInventarioCategoria()
        {
            IEnumerable<object> lista =
                contexto.Products.
                Join(contexto.Categories,
                (p)=>p.CategoryId,
                (c) => c.CategoryId,
                (p, c)=>
                    new
                    {
                        Categoria = c.CategoryName,
                        Existencia = p.UnitsInStock
                    }
                ).GroupBy(pc=>pc.Categoria)
                .Select(grupo=>
                    new { 
                        Categoria=grupo.Key,
                        Inventario=grupo.Sum(g=>g.Existencia)
                    }
                );

            return lista;
        }

        [HttpPost]
        [Route("GetSales")]
        public IEnumerable<object> GetSalesByMonth([FromBody] SalesRequest request)
        {
            DateTime startDate = request.StartDate;
            DateTime endDate = request.EndDate;
            var result =
                from o in contexto.Orders
                join od in contexto.Orderdetails on o.OrderId equals od.OrderId
                where o.OrderDate >= startDate && o.OrderDate <= endDate
                group new { od.UnitPrice, od.Quantity } by new { Year = o.OrderDate.Value.Year, Month = o.OrderDate.Value.Month } into g
                orderby g.Key.Year, g.Key.Month
                select new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sales = g.Sum(x => x.UnitPrice * x.Quantity)
                };

            return result;
        }
        [HttpGet]
        [Route("GetTopFiveProducts/{year}")]
        public IEnumerable<object> GetTopFiveProducts(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);

            var result = new List<object>();

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var quarterlyResult = contexto.Products
                    .Join(contexto.Orderdetails,
                        p => p.ProductId,
                        od => od.ProductId,
                        (p, od) => new { Product = p, OrderDetail = od })
                    .Join(contexto.Orders,
                        join1 => join1.OrderDetail.OrderId,
                        o => o.OrderId,
                        (join1, o) => new { join1.Product, join1.OrderDetail, Order = o })
                    .Where(join2 => join2.Order.OrderDate.Value >= startDate &&
                                     join2.Order.OrderDate.Value <= endDate &&
                                     ((join2.Order.OrderDate.Value.Month - 1) / 3 + 1) == quarter)
                    .GroupBy(join2 => new { join2.Product.ProductName, Quarter = quarter })
                    .Select(group => new
                    {
                        Nombre = group.Key.ProductName,
                        Trimestre = group.Key.Quarter,
                        UnidadesVendidas = group.Sum(g => g.OrderDetail.Quantity)
                    })
                    .OrderByDescending(item => item.UnidadesVendidas)
                    .Take(5);

                result.AddRange(quarterlyResult);
            }

            return result;
        }
        public class SalesRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        [HttpPost]
        [Route("GetProduct")]
        public IEnumerable<object> GetProduct([FromBody] ProductRequest request)
        {
            DateTime startDate = request.StartDate;
            DateTime endDate = request.EndDate;
            string nombreProducto = request.Product;

            var result = contexto.Orders
                .Join(contexto.Orderdetails, o => o.OrderId, od => od.OrderId, (o, od) => new { Order = o, OrderDetail = od })
                .Join(contexto.Products, od => od.OrderDetail.ProductId, p => p.ProductId, (od, p) => new { OrderDetail = od, Product = p })
                .Where(x => x.Product.ProductName == nombreProducto && x.OrderDetail.Order.OrderDate >= startDate && x.OrderDetail.Order.OrderDate <= endDate)
                .GroupBy(x => new { Month = x.OrderDetail.Order.OrderDate.Value.Month, Year = x.OrderDetail.Order.OrderDate.Value.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Amount = g.Sum(x => x.OrderDetail.OrderDetail.Quantity)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month);

            return result;
        }



        public class ProductRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Product { get; set; } 
        }

        [HttpGet]
        [Route("GetNameProducts")]
        public IEnumerable<string> GetNameProducts()
        {
            var result = from product in contexto.Products
                         select product.ProductName;

            return result.ToArray();
        }

    }
}
