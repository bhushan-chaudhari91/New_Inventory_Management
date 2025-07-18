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

            var existingSupplier = await _context.TblSuppliers
        .FirstOrDefaultAsync(s => s.SupplierName == addSupplier.SupplierName && s.IsDeleted == false);

            if (existingSupplier != null)
            {
                TempData["ErrorMessage"] = "Supplier already exists.";
                return RedirectToAction("Supplier");
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
                //SkuIdName = addProduct.SkuIdName,
                LowStockQuantity = addProduct.LowStockQuantity,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
            };

            _context.TblProducts.Add(product);
            await _context.SaveChangesAsync();

            var getProductId = product.ProductId;

            var skuEntries = new List<TblSkuBarcode>();

            if (!string.IsNullOrWhiteSpace(addProduct.SkuForSignleItem))
            {
                skuEntries.Add(new TblSkuBarcode
                {
                    FkProductId = getProductId,
                    Skuname = addProduct.SkuForSignleItem,
                    IsDeleted = 0,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                });
            }

            if (!string.IsNullOrWhiteSpace(addProduct.SkuForBox))
            {
                skuEntries.Add(new TblSkuBarcode
                {
                    FkProductId = getProductId,
                    Skuname = addProduct.SkuForBox,
                    IsDeleted = 0,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                });
            }

            if (skuEntries.Any())
            {
                _context.TblSkuBarcodes.AddRange(skuEntries);
            }

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



        //[HttpGet]
        //public IActionResult EditProduct(int id)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var product = _context.TblProducts
        //        .Where(p => p.ProductId == id && p.IsDeleted == false)
        //        .Select(p => new ProductViewModel
        //        {
        //            ProductId = p.ProductId,
        //            ProductName = p.ProductName,
        //            SkuIdName = p.SkuIdName,
        //            LowStockQuantity = p.LowStockQuantity,
        //            AliasNames = _context.TblProductAliases
        //                .Where(a => a.FkProductId == p.ProductId && a.IsDeleted == false)
        //                .Select(a => a.AliasName).ToList()
        //        }).FirstOrDefault();



        //    return View(product);
        //}


        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Load product main info
            var productEntity = _context.TblProducts
                .FirstOrDefault(p => p.ProductId == id && p.IsDeleted == false);

            if (productEntity == null)
            {
                return NotFound();
            }

            // Load SKU Barcodes for this product
            var skuBarcodes = _context.TblSkuBarcodes
                .Where(s => s.FkProductId == id && s.IsDeleted == 0)
                .Select(s => s.Skuname)
                .ToList();

            // Distinguish by prefix
            var skuForSingleItem = skuBarcodes.FirstOrDefault(s => s.StartsWith("A"));
            var skuForBox = skuBarcodes.FirstOrDefault(s => s.StartsWith("B"));

            // Load Aliases
            var aliasNames = _context.TblProductAliases
                .Where(a => a.FkProductId == id && a.IsDeleted == false)
                .Select(a => a.AliasName)
                .ToList();

            var viewModel = new ProductViewModel
            {
                ProductId = productEntity.ProductId,
                ProductName = productEntity.ProductName,
                LowStockQuantity = productEntity.LowStockQuantity,
                SkuForSignleItem = skuForSingleItem,
                SkuForBox = skuForBox,
                AliasNames = aliasNames
            };

            return View(viewModel);
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
            //product.SkuIdName = updatedProduct.SkuIdName;
            product.LowStockQuantity = updatedProduct.LowStockQuantity;
            product.UpdatedAt = DateTime.Now;



            // Start Code For SKU Name Update on 15/07/2025 by Bhushan
            var existingSkus = _context.TblSkuBarcodes
                .Where(s => s.FkProductId == product.ProductId && s.IsDeleted == 0);

            _context.TblSkuBarcodes.RemoveRange(existingSkus);

            // Validate and add new SKUs
            var newSkuEntries = new List<TblSkuBarcode>();

            if (!string.IsNullOrWhiteSpace(updatedProduct.SkuForSignleItem))
            {
                if (!updatedProduct.SkuForSignleItem.StartsWith("A"))
                {
                    ModelState.AddModelError("SkuForSignleItem", "SKU for Single Item must start with 'A'.");
                }
                else
                {
                    newSkuEntries.Add(new TblSkuBarcode
                    {
                        FkProductId = product.ProductId,
                        Skuname = updatedProduct.SkuForSignleItem,
                        IsDeleted = 0,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(updatedProduct.SkuForBox))
            {
                if (!updatedProduct.SkuForBox.StartsWith("B"))
                {
                    ModelState.AddModelError("SkuForBox", "SKU for Box must start with 'B'.");
                }
                else
                {
                    newSkuEntries.Add(new TblSkuBarcode
                    {
                        FkProductId = product.ProductId,
                        Skuname = updatedProduct.SkuForBox,
                        IsDeleted = 0,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    });
                }
            }

            // Save new SKUs
            if (newSkuEntries.Any())
            {
                _context.TblSkuBarcodes.AddRange(newSkuEntries);
            }
            // End Code For SKU Name Update on 15/07/2025 by Bhushan



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

        public IActionResult RackNo(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }


            List<RackNoVeiwModel> getRackNoList = new List<RackNoVeiwModel>();

            var rackNoList = _context.TblRacks.Where(x => x.IsDeleted == 0 && x.RackId != 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                rackNoList = rackNoList.Where(x => x.RackNo.Contains(searchTerm));
            }

            int totalRecords = rackNoList.Count();

            var paginatedWarehouse = rackNoList
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            foreach (var item in paginatedWarehouse)
            {
                var warehouseName = _context.TblWarehouses.FirstOrDefault(x => x.WarehouseId == item.FkWarehouseId).Name;

                getRackNoList.Add(new RackNoVeiwModel
                {
                    RackId = item.RackId,
                    RackNo = item.RackNo,
                   WarehouseName = warehouseName
                });
            }

            var getWarehouse = _context.TblWarehouses.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.WarehouseId,
                warehouseName = x.Name
            }).ToList();

            var viewModel = new RackNoListViewModel
            {
                WarehouseList = new SelectList(getWarehouse, "Id", "warehouseName"),
                rackNo = getRackNoList,
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
        public async Task<IActionResult> AddRackNo(RackNoVeiwModel addRackno)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var rack = new TblRack
            {
                RackNo = addRackno.RackNo,
                FkWarehouseId = addRackno.FKWarehouseId,
                CreatedAt = DateTime.Now,
                CreatedBy = userId
            };

            _context.TblRacks.Add(rack);
            await _context.SaveChangesAsync();

            return RedirectToAction("RackNo");
        }

        //[HttpGet]
        //public async Task<IActionResult> EditRackNo(int id)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var rack = await _context.TblRacks.FindAsync(id);
        //    if (rack == null)
        //    {
        //        return NotFound();
        //    }


        //    var viewModel = new RackNoVeiwModel
        //    {
        //        RackId = rack.RackId,
        //        FKWarehouseId = rack.FkWarehouseId ?? 0,
        //        RackNo = rack.RackNo
        //    };

        //    return PartialView("_EditWarehouseModal", viewModel);
        //}




        [HttpPost]
        public async Task<IActionResult> EditRackNo(RackNoVeiwModel model)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var rackdata = await _context.TblRacks.FindAsync(model.RackId);
            if (rackdata == null)
                return NotFound();

            rackdata.RackNo = model.RackNo;
            rackdata.FkWarehouseId = model.FKWarehouseId;
            rackdata.UpdatedAt = DateTime.Now;
            rackdata.UpdatedBy = userId;

            _context.TblRacks.Update(rackdata);
            await _context.SaveChangesAsync();

            return RedirectToAction("RackNo");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteRackNo(int id)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var rackData = await _context.TblRacks.FirstOrDefaultAsync(x => x.RackId == id);

            if (rackData == null)
            {
                throw new Exception("RackNo not found");
            }

            rackData.IsDeleted = 1;
            _context.TblRacks.Update(rackData);
            await _context.SaveChangesAsync();

            return RedirectToAction("RackNo", "Master");
        }


    }
}
