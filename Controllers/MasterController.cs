using ClosedXML.Excel;
using InventoryManagement.EntityModels;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace InventoryManagement.Controllers
{
    public class MasterController : Controller
    {
        private readonly DbInventoryContext _context;

        public MasterController(DbInventoryContext context)
        {
            _context = context;
        }

        public IActionResult Warehouse(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<WarehouseViewModel> getWarehouseList = new List<WarehouseViewModel>();

            var warehouseList = _context.TblWarehouses.Where(x => x.IsDeleted == false && x.WarehouseId != 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                warehouseList = warehouseList.Where(x => x.Name.Contains(searchTerm));
            }

            int totalRecords = warehouseList.Count();

            var paginatedWarehouse = warehouseList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            foreach (var item in paginatedWarehouse)
            {

                getWarehouseList.Add(new WarehouseViewModel
                {
                    WarehouseId = item.WarehouseId,
                    Name = item.Name
                });
            }

            var viewModel = new WarehouseListViewModel
            {
                Warehouses = getWarehouseList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            return View(viewModel);
        }

        public IActionResult ExportWarehouseToExcel(string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouseList = _context.TblWarehouses
                .Where(x => x.IsDeleted == false && x.WarehouseId != 0)
                .ToList();

            //if (!string.IsNullOrEmpty(searchTerm))
            //{
            //    warehouseList = warehouseList.Where(x => x.Name.Contains(searchTerm));
            //}

            var data = warehouseList
                .Select(x => new
                {
                    x.WarehouseId,
                    x.Name,
                   
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Warehouse List");

                // Set header
                worksheet.Cell(1, 1).Value = "No.";
                worksheet.Cell(1, 2).Value = "Warehouse Name";

                // Apply header formatting
                var headerRange = worksheet.Range("A1:B1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Set column widths
                worksheet.Column(1).Width = 10;  // No.
                worksheet.Column(2).Width = 30;  // Warehouse Name

                // Fill data
                int row = 2;
                int counter = 1;
                foreach (var item in warehouseList)
                {
                    worksheet.Cell(row, 1).Value = counter;
                    worksheet.Cell(row, 2).Value = item.Name;
                    row++;
                    counter++;
                }

                // Return file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "WarehouseList.xlsx");
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddWarehouse(WarehouseViewModel addWarehouse)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = new TblWarehouse
            {
                Name = addWarehouse.Name,
            };

            _context.TblWarehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return RedirectToAction("Warehouse");
        }


        [HttpGet]
        public async Task<IActionResult> EditWarehouse(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = await _context.TblWarehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }

            var viewModel = new WarehouseViewModel
            {
                WarehouseId = warehouse.WarehouseId,
                Name = warehouse.Name
            };

            return PartialView("_EditWarehouseModal", viewModel); 
        }


        [HttpPost]
        public async Task<IActionResult> EditWarehouse(WarehouseViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = await _context.TblWarehouses.FindAsync(model.WarehouseId);
            if (warehouse == null)
                return NotFound();

            warehouse.Name = model.Name;
            warehouse.UpdatedAt = DateTime.Now;

            _context.TblWarehouses.Update(warehouse);
            await _context.SaveChangesAsync();

            return RedirectToAction("Warehouse");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var warehouse = await _context.TblWarehouses.FirstOrDefaultAsync(x => x.WarehouseId == id);

            if (warehouse == null)
            {
                throw new Exception("Warehouse not found");
            }

            warehouse.IsDeleted = true;
            _context.TblWarehouses.Update(warehouse);
            await _context.SaveChangesAsync();

            return RedirectToAction("Warehouse", "Master");
        }


        //public IActionResult Supplier(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        //{
        //    List<SupplierViewModel> getSupplierList = new List<SupplierViewModel>();

        //    var supplierList = _context.TblSuppliers.Where(x => x.IsDeleted == false && x.SupplierId != 0).AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        supplierList = supplierList.Where(x => x.SupplierName.Contains(searchTerm) ||
        //                                               x.Address.Contains(searchTerm) ||
        //                                               x.Email.Contains(searchTerm) ||
        //                                               x.ContactNo.Contains(searchTerm));
        //    }

        //    var paginatedSupplier = supplierList
        //       .Skip((pageNumber - 1) * pageSize)
        //       .Take(pageSize)
        //       .ToList();

        //    foreach (var item in paginatedSupplier)
        //    {
        //        SupplierViewModel supplier = new SupplierViewModel();

        //        supplier.SupplierId = item.SupplierId;
        //        supplier.SupplierName = item.SupplierName;
        //        supplier.ContactNo = item.ContactNo;
        //        supplier.Email = item.Email;
        //        supplier.Address = item.Address;

        //        getSupplierList.Add(supplier);
        //    }

        //    return View(getSupplierList);
        //}

        public IActionResult Supplier(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<SupplierViewModel> getSupplierList = new List<SupplierViewModel>();

            var supplierList = _context.TblSuppliers.Where(x => x.IsDeleted == false && x.SupplierId != 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                supplierList = supplierList.Where(x => x.SupplierName.Contains(searchTerm) ||
                                                       x.Address.Contains(searchTerm) ||
                                                       x.Email.Contains(searchTerm) ||
                                                       x.ContactNo.Contains(searchTerm));
            }

            int totalRecords = supplierList.Count();

            var paginatedSupplier = supplierList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            foreach (var item in paginatedSupplier)
            {
                //SupplierViewModel supplier = new SupplierViewModel();

                //supplier.SupplierId = item.SupplierId;
                //supplier.SupplierName = item.SupplierName;
                //supplier.ContactNo = item.ContactNo;
                //supplier.Email = item.Email;
                //supplier.Address = item.Address;

                //getSupplierList.Add(supplier);

                getSupplierList.Add(new SupplierViewModel
                {
                    SupplierId = item.SupplierId,
                    SupplierName = item.SupplierName,
                    ContactNo = item.ContactNo,
                    Email = item.Email,
                    Address = item.Address,
                });
            }

            var viewModel = new SupplierListViewModel
            {
                Supplies = getSupplierList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult ExportSupplierToExcel(string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var supplierList = _context.TblSuppliers
                .Where(x => x.IsDeleted == false && x.SupplierId != 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                supplierList = supplierList.Where(x =>
                    x.SupplierName.Contains(searchTerm) ||
                    x.Address.Contains(searchTerm) ||
                    x.Email.Contains(searchTerm) ||
                    x.ContactNo.Contains(searchTerm));
            }

            var data = supplierList
                .Select(x => new
                {
                    x.SupplierId,
                    x.SupplierName,
                    x.Email,
                    x.ContactNo,
                    x.Address
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Suppliers");

                // Headers
                worksheet.Cell(1, 1).Value = "Sr.No.";
                worksheet.Cell(1, 2).Value = "Supplier Name";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Contact";
                worksheet.Cell(1, 5).Value = "Address";

                // Header Style
                var headerRange = worksheet.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Column widths
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 40;

                // Data
                int row = 2;
                int srNo = 1;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.SupplierName;
                    worksheet.Cell(row, 3).Value = item.Email;
                    worksheet.Cell(row, 4).Value = item.ContactNo;
                    worksheet.Cell(row, 5).Value = item.Address;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Suppliers.xlsx");
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddSupplier(SupplierViewModel addSupplier)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var supplier = new TblSupplier
            {
                SupplierName = addSupplier.SupplierName,
                Email = addSupplier.Email,
                ContactNo = addSupplier.ContactNo,
                Address = addSupplier.Address,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.TblSuppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction("Supplier");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var supplier = await _context.TblSuppliers.FirstOrDefaultAsync(x => x.SupplierId == id);

            if (supplier == null)
            {
                throw new Exception("Supplier not found");
            }

            supplier.IsDeleted = true;
            _context.TblSuppliers.Update(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction("Supplier", "Master");
        }
        

        [HttpPost]
        public async Task<IActionResult> EditSupplier(SupplierViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var supplier = await _context.TblSuppliers.FindAsync(model.SupplierId);

            if (supplier == null)
                return NotFound();

            supplier.SupplierName = model.SupplierName;
            supplier.Address = model.Address;
            supplier.ContactNo = model.ContactNo;
            supplier.Email = model.Email;
            supplier.UpdatedAt = DateTime.Now;

            _context.TblSuppliers.Update(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction("Supplier");
        }









        //public IActionResult Product(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        //{
        //    List<ProductViewModel> getProductList = new List<ProductViewModel>();

        //    var productList = _context.TblProducts.Where(x => x.IsDeleted == false && x.ProductId != 0).AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        productList = productList.Where(x => x.ProductName.Contains(searchTerm) ||
        //                                        x.ProductName.Contains(searchTerm));
        //    }

        //    var paginatedProduct = productList
        //        .Skip((pageNumber -1 ) * pageSize)
        //        .Take(pageSize)
        //        .ToList();


        //    foreach (var product in paginatedProduct)
        //    {
        //        var aliasNames = _context.TblProductAliases
        //            .Where(x => x.IsDeleted == false && x.FkProductId == product.ProductId)
        //            .Select(x => x.AliasName)
        //            .ToList();

        //        ProductViewModel viewModel = new ProductViewModel
        //        {
        //            ProductId = product.ProductId,
        //            ProductName = product.ProductName,
        //            SkuIdName = product.SkuIdName,
        //            AliasNames = aliasNames,
        //        };

        //        getProductList.Add(viewModel);
        //    }

        //    ViewBag.TotalRecords = pageSize;
        //    ViewBag.CurrentPage = pageNumber;
        //    ViewBag.SearchTerm = searchTerm;
        //    ViewBag.PageSize = pageSize;

        //    return View(getProductList);
        //}



        public IActionResult BarcodePrint()
        {
            var productList = _context.TblProducts.Where(x => x.IsDeleted == false).Select(x => new ProductDto  
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
            }).ToList();

            var viewModel = new ProductViewModel
            {
                ProductNames = productList
            };

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetSkuIdName(int productId)
        {
            var skuIdName = _context.TblProducts
                .Where(x => x.ProductId == productId)
                .Select(x => x.SkuIdName)
                .FirstOrDefault();

            return Json(new { skuIdName });
        }






        public IActionResult Product(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var productList = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductId != 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productList = productList.Where(x => x.ProductName.Contains(searchTerm) ||
                                                     x.SkuIdName.Contains(searchTerm));
            }

            int totalRecords = productList.Count();

            var paginatedProduct = productList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            List<ProductViewModel> getProductList = new List<ProductViewModel>();

            foreach (var product in paginatedProduct)
            {
                var aliasNames = _context.TblProductAliases
                    .Where(x => x.IsDeleted == false && x.FkProductId == product.ProductId)
                    .Select(x => x.AliasName)
                    .ToList();

                getProductList.Add(new ProductViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    SkuIdName = product.SkuIdName,
                    AliasNames = aliasNames
                });
            }

            var viewModel = new ProductListViewModel
            {
                Products = getProductList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            return View(viewModel);
        }


        //Start Code For Export Excel File
        public IActionResult ProductExportToExcel(string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var productList = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductId != 0);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productList = productList.Where(x => x.ProductName.Contains(searchTerm) ||
                                                     x.SkuIdName.Contains(searchTerm));
            }

            var data = productList
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.SkuIdName,
                    AliasNames = _context.TblProductAliases
                                         .Where(a => a.IsDeleted == false && a.FkProductId == p.ProductId)
                                         .Select(a => a.AliasName)
                                         .ToList()
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                worksheet.Cell(1, 1).Value = "No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU Name";
                worksheet.Cell(1, 4).Value = "Alias Names";

                // Apply header formatting
                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                worksheet.Column(1).Width = 10;   // "No."
                worksheet.Column(2).Width = 30;   // "Product Name"
                worksheet.Column(3).Width = 25;   // "SKU Name"
                worksheet.Column(4).Width = 70;   // "Alias Names"

                int row = 2;
                int srNo = 1;

                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.SkuIdName;
                    worksheet.Cell(row, 4).Value = string.Join(", ", item.AliasNames ?? new List<string>());
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ProductList.xlsx");
                }
            }
        }
        //End Code For Export Excel File

        [HttpPost]
        public async Task<ActionResult> AddProduct(ProductViewModel addProduct)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = new TblProduct
            {
                ProductName = addProduct.ProductName,
                SkuIdName = addProduct.SkuIdName,
                LowStockQuantity = addProduct.LowStockQuantity,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
            };

            _context.TblProducts.Add(product);
            await _context.SaveChangesAsync();

            var getId = product.ProductId;

            if(addProduct.AliasNames != null && addProduct.AliasNames.Any())
            {
                foreach( var alias in addProduct.AliasNames)
                {
                    if(!string.IsNullOrWhiteSpace(alias)){

                        var aliasData = new TblProductAlias
                        {
                            FkProductId = getId,
                            AliasName = alias,
                            IsDeleted = false,
                            CreatedAt = DateTime.Now
                        };

                        _context.TblProductAliases.Add(aliasData);
                    }
                }
                
                await _context.SaveChangesAsync();
            }

           

            return RedirectToAction("Product");
        }



        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = _context.TblProducts
                .Where(p => p.ProductId == id && p.IsDeleted == false)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    SkuIdName = p.SkuIdName,
                    LowStockQuantity = p.LowStockQuantity,
                    AliasNames = _context.TblProductAliases
                        .Where(a => a.FkProductId == p.ProductId && a.IsDeleted == false)
                        .Select(a => a.AliasName).ToList()
                }).FirstOrDefault();

            //var viewModel = new ProductListViewModel
            //{
            //    EditProduct = product
            //};

            return View(product);
        }




        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel updatedProduct)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = await _context.TblProducts
                .FirstOrDefaultAsync(p => p.ProductId == updatedProduct.ProductId && p.IsDeleted == false);

            if (product == null)
            {
                return NotFound();
            }

            // Update product details
            product.ProductName = updatedProduct.ProductName;
            product.SkuIdName = updatedProduct.SkuIdName;
            product.LowStockQuantity = updatedProduct.LowStockQuantity;
            product.UpdatedAt = DateTime.Now;

            // Remove existing aliases
            var existingAliases = _context.TblProductAliases
                .Where(a => a.FkProductId == product.ProductId && a.IsDeleted == false);

            _context.TblProductAliases.RemoveRange(existingAliases);

            // Add new aliases
            if (updatedProduct.AliasNames != null && updatedProduct.AliasNames.Any())
            {
                foreach (var alias in updatedProduct.AliasNames)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        _context.TblProductAliases.Add(new TblProductAlias
                        {
                            FkProductId = product.ProductId,
                            AliasName = alias,
                            IsDeleted = false,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Product");
        }

    }
}
