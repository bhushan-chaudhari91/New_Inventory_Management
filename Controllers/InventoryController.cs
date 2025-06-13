using ClosedXML.Excel;
using Dapper;
using InventoryManagement.EntityModels;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System;
using System.IO;
using OfficeOpenXml;
using System.Globalization;

namespace InventoryManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly DbInventoryContext _context;

        public InventoryController(DbInventoryContext context)
        {
            _context = context;
        }

 
        public IActionResult InventoryList(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRoleId = _context.TblUsers.Where(x => x.UserId == userId).Select(x => x.FkRoleId).FirstOrDefault();

            List<StockInViewModel> getStockInList = new List<StockInViewModel>();

            var stockInList = _context.TblStockIns.Where(x => x.IsDeleted == false && x.StockInId != 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockInList = stockInList.Where(x => x.Barcode.Contains(searchTerm) ||
                                                     x.ProductQuantity.Contains(searchTerm) ||
                                                     x.ProductStatus.Contains(searchTerm));
            }

            var groupedStockInList = stockInList
            .GroupBy(x => x.FkProductId)
            .Select(g => g.First())
            .ToList();

            int totalProductsCount = stockInList.Select(x => x.FkProductId).Distinct().Count();
            int totalBatchCount = stockInList.Select(x => x.BatchNo).Distinct().Count();
            int totalRecords = groupedStockInList.Count();

            //int totalStockCount = stockInList.Select(x => x.StockInId).Distinct().Count();
            //int totalOutOfStockCount = stockInList.Where(x => Convert.ToInt32(x.AvailableQuantity) == 0).Select(x => x.StockInId).Distinct().Count();

            int totalOutOfStockCount = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.AvailableProductQty == "0")
                .Count();

            ViewBag.ProductCount = totalProductsCount;
            ViewBag.BatchCount = totalBatchCount;
            //ViewBag.TotalStock = totalStockCount;
            ViewBag.TotalOutOfStock = totalOutOfStockCount;

            var paginatedstockIn = groupedStockInList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();


            //int totalLowStockCount = 0;

            foreach (var item in paginatedstockIn)
            {
                var aliasNames = _context.TblProductAliases
                    .Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId)
                    .Select(x => x.AliasName)
                    .ToList();

                var getProject = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getWarehouse = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                //Start Old Code for Get LowStockCount i think this code getting wrong count  //int totalLowStockCount = 0;

                //if (getProject != null)
                //{
                //    if (int.TryParse(getProject.AvailableProductQty, out int availableQty) &&
                //        int.TryParse(getProject.LowStockQuantity, out int lowStockQty))
                //    {
                //        if (availableQty <= lowStockQty)
                //        {
                //            totalLowStockCount++;
                //        }
                //    }
                //}
                //ViewBag.TotalLowStock = totalLowStockCount;

                //End Old Code for Get LowStockCount i think this code getting wrong count

                getStockInList.Add(new StockInViewModel
                {
                    StockInId = item.StockInId,
                    FkProductId = (int)item.FkProductId,
                    Barcode = item.Barcode,
                    Price = (decimal)item.Price,
                    ProductQuantity = getProject?.AvailableProductQty,
                    LowStockQty = getProject?.LowStockQuantity,
                    ProductName = getProject?.ProductName,
                    SKUName = getProject?.SkuIdName,
                    WarehouseName = getWarehouse?.Name,
                    AliasNames = aliasNames,
                    Type = item.Type

                });       
            }

           

            //Start Code for Get LowStockCount
            var products = _context.TblProducts
            .Where(p => p.IsDeleted == false)
            .ToList();

            int totalLowStockCount = products.Count(p =>
                int.TryParse(p.AvailableProductQty, out int availableQty) &&
                int.TryParse(p.LowStockQuantity, out int lowStockQty) &&
                availableQty <= lowStockQty
            );

            ViewBag.TotalLowStock = totalLowStockCount;
            //End Code for Get LowStockCount

            var viewModel = new StockInListViewModel
            {
                UserFkRoleId = (int)userRoleId,
                StockIns = getStockInList,
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
        public IActionResult ExportInventoryToExcel(string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var stockInList = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.StockInId != 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockInList = stockInList.Where(x => x.Barcode.Contains(searchTerm) ||
                                                     x.ProductQuantity.Contains(searchTerm) ||
                                                     x.ProductStatus.Contains(searchTerm));
            }

            //var filteredList = stockInList.ToList();

            var filteredList = stockInList
             .GroupBy(x => x.FkProductId)
             .Select(g => g.First())
             .ToList();

            var stockInData = new List<StockInViewModel>();

            foreach (var item in filteredList)
            {
                var aliasNames = _context.TblProductAliases
                    .Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId)
                    .Select(x => x.AliasName)
                    .ToList();

                var product = _context.TblProducts
                    .FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);

                var warehouse = _context.TblWarehouses
                    .FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                stockInData.Add(new StockInViewModel
                {
                    StockInId = item.StockInId,
                    FkProductId = (int)item.FkProductId,
                    ProductQuantity = product.AvailableProductQty,
                    ProductName = product?.ProductName,
                    SKUName = product?.SkuIdName,
                    WarehouseName = warehouse?.Name,
                    AliasNames = aliasNames
                });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory");

                worksheet.Cell(1, 1).Value = "No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU";
                worksheet.Cell(1, 4).Value = "Quantity";
                worksheet.Cell(1, 5).Value = "Aliases";

                var header = worksheet.Range("A1:E1");
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.Black;
                header.Style.Fill.BackgroundColor = XLColor.Yellow;

                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 12;
                worksheet.Column(5).Width = 40;

                int row = 2;
                int count = 1;
                foreach (var item in stockInData)
                {
                    worksheet.Cell(row, 1).Value = count;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.SKUName;
                    worksheet.Cell(row, 4).Value = item.ProductQuantity;

                    if (item.AliasNames != null && item.AliasNames.Any())
                    {
                        string aliasText = string.Join(", ", item.AliasNames);
                        var wrappedText = WrapText(aliasText, 25);
                        worksheet.Cell(row, 5).Value = wrappedText;
                    }
                    else
                    {
                        worksheet.Cell(row, 5).Value = "-";
                    }

                    row++;
                    count++;
                }

                worksheet.Column(5).Style.Alignment.WrapText = true;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "InventoryList.xlsx");
                }
            }
        }

        private string WrapText(string text, int maxLineLength)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if ((currentLine.Length + word.Length + 1) <= maxLineLength)
                {
                    currentLine += (currentLine == "" ? "" : " ") + word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return string.Join(Environment.NewLine, lines);
        }



        public IActionResult BatchesList(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var batchesQuery = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.StockInId != 0).OrderByDescending(x => x.BatchNo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                batchesQuery = batchesQuery.Where(x => x.Barcode.Contains(searchTerm) ||
                                                       x.ProductQuantity.Contains(searchTerm) ||
                                                       x.ProductStatus.Contains(searchTerm));
            }

            int totalProductsCount = batchesQuery.Select(x => x.FkProductId).Distinct().Count();
            int totalBatchCount = batchesQuery.Select(x => x.BatchNo).Distinct().Count();

            //int totalStockCount = batchesQuery.Select(x => x.StockInId).Distinct().Count();
            //int totalOutOfStockCount = batchesQuery.Where(x => Convert.ToInt32(x.AvailableQuantity) == 0).Select(x => x.StockInId).Distinct().Count();

            int totalOutOfStockCount = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.AvailableProductQty == "0")
                .Count();

            ViewBag.ProductCount = totalProductsCount;
            ViewBag.BatchCount = totalBatchCount;
            //ViewBag.TotalStock = totalStockCount;
            ViewBag.TotalOutOfStock = totalOutOfStockCount;

            var batchGroupList = batchesQuery
                .GroupBy(x => new { x.BatchNo, x.Date })
                .Select(group => new
                {
                    BatchId = group.Key.BatchNo,
                    Date = group.Key.Date,
                    ProductsCount = group.Select(g => g.FkProductId).Distinct().Count(),
                    TotalQuantity = group.Sum(g => Convert.ToDecimal(g.ProductQuantity)),
                    WarehouseCount = group.Select(g => g.FkWarehouseId).Distinct().Count(),
                    ProductIds = group.Select(g => g.FkProductId).Distinct()
                });


            //Start Code for Get LowStockCount
            var products = _context.TblProducts
            .Where(p => p.IsDeleted == false)
            .ToList();

            int totalLowStockCount = products.Count(p =>
                int.TryParse(p.AvailableProductQty, out int availableQty) &&
                int.TryParse(p.LowStockQuantity, out int lowStockQty) &&
                availableQty <= lowStockQty
            );

            ViewBag.LowStockCount = totalLowStockCount;
            //End Code for Get LowStockCount

            int totalRecords = batchGroupList.Count();

            var paginatedBatches = batchGroupList
               .OrderByDescending(x => x.BatchId)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            var batchViewModel = paginatedBatches.Select(x => new BatchSummaryViewModel
            {
                BatchNo = x.BatchId,
                Date = x.Date,
                ProductsCount = x.ProductsCount,
                TotalQuantity = x.TotalQuantity,
                WarehouseCount = x.WarehouseCount
            }).ToList();

            var viewModel = new BatchListViewModel
            {
                Batches = batchViewModel,
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
        public IActionResult ExportBatchesToExcel(string searchTerm)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var batchesQuery = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.StockInId != 0);

            // Compulsory filter with searchTerm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                batchesQuery = batchesQuery.Where(x =>
                    x.Barcode.Contains(searchTerm) ||
                    x.ProductQuantity.Contains(searchTerm) ||
                    x.ProductStatus.Contains(searchTerm));
            }

            var batchList = batchesQuery
                .GroupBy(x => new { x.BatchNo, x.Date })
                .Select(group => new
                {
                    BatchId = group.Key.BatchNo,
                    Date = group.Key.Date,
                    ProductsCount = group.Select(g => g.FkProductId).Distinct().Count(),
                    //TotalQuantity = group.Sum(g => Convert.ToDecimal(g.ProductQuantity))
                    TotalQuantity = group.Sum(g => Convert.ToInt32(g.ProductQuantity))
                })
                .ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Batch List");
                worksheet.Cell(1, 1).Value = "Sr. No";
                worksheet.Cell(1, 2).Value = "Batch ID";
                worksheet.Cell(1, 3).Value = "Date";
                worksheet.Cell(1, 4).Value = "Products Count";
                worksheet.Cell(1, 5).Value = "Total Quantity";

                int row = 2;
                int srNo = 1;
                foreach (var batch in batchList)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = batch.BatchId;
                    worksheet.Cell(row, 3).Value = batch.Date?.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 4).Value = batch.ProductsCount;
                    worksheet.Cell(row, 5).Value = batch.TotalQuantity.ToString("0");
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "BatchList.xlsx");
                }
            }
        }




        public IActionResult BatchDetails(string BatchNo, string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {

            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<StockCombinedViewModel> getStockBetch = new List<StockCombinedViewModel>();

            var StockBatchDetails = _context.TblStockIns.
                Where(x => x.IsDeleted == false && x.BatchNo == BatchNo);


            if (!string.IsNullOrEmpty(searchTerm))
            {
                StockBatchDetails = StockBatchDetails.Where(x => x.Barcode.Contains(searchTerm));
            }

            int totalRecords = StockBatchDetails.Count();

            var batchesList = StockBatchDetails
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            foreach (var item in batchesList)
            {
                var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getLocation = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                var aliasNames = _context.TblProductAliases.Where(x => x.IsDeleted == false && x.FkProductId == getProduct.ProductId)
                    .Select(x => x.AliasName).ToList();


                getStockBetch.Add(new StockCombinedViewModel
                {
                    Id = item.StockInId,
                    ProductName = getProduct?.ProductName,
                    SKUName = getProduct?.SkuIdName,
                    Quantity = item.ProductQuantity,
                    //Quantity = getProduct?.AvailableProductQty,
                    LocationName = getLocation?.Name,
                    RoomName = item.Room,
                    RackName = item.RackNo,
                    AliasNames = aliasNames
                });
            }

            int productCount = getStockBetch.Count;
            decimal totalQuantity = getStockBetch.Sum(x => Convert.ToDecimal(x.Quantity));

            var viewModel = new BatchDetailsViewModel
            {


                BatchNo = BatchNo,
                ProductCount = productCount,
                TotalQuantity = totalQuantity,
                StockItems = getStockBetch,

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
        public IActionResult ExportBatchDetailsToExcel(string batchNo, string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var stockBatchDetails = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.BatchNo == batchNo);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockBatchDetails = stockBatchDetails.Where(x => x.Barcode.Contains(searchTerm));
            }

            var batchItems = stockBatchDetails.ToList();
            List<StockCombinedViewModel> exportList = new List<StockCombinedViewModel>();

            foreach (var item in batchItems)
            {
                var product = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var warehouse = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);
                var aliasNames = _context.TblProductAliases
                                        .Where(x => x.IsDeleted == false && x.FkProductId == product.ProductId)
                                        .Select(x => x.AliasName).ToList();

                exportList.Add(new StockCombinedViewModel
                {
                    ProductName = product?.ProductName,
                    SKUName = product?.SkuIdName,
                    Quantity = item.ProductQuantity,
                    AliasNames = aliasNames,
                    LocationName = warehouse?.Name,
                    RoomName = item.Room,
                    RackName = item.RackNo
                });
            }

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Batch Details");
                worksheet.Cell(1, 1).Value = "Sr. No";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU Name";
                worksheet.Cell(1, 4).Value = "Quantity";
                worksheet.Cell(1, 5).Value = "Alias Names";
                worksheet.Cell(1, 6).Value = "Location";

                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                worksheet.Column(1).Width = 10; 
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 40;
                worksheet.Column(6).Width = 40;

                int row = 2;
                int srNo = 1;
                foreach (var item in exportList)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.SKUName;
                    worksheet.Cell(row, 4).Value = item.Quantity;

                    // Format alias names with line breaks
                    worksheet.Cell(row, 5).Value = item.AliasNames != null && item.AliasNames.Any()
                        ? string.Join(Environment.NewLine, BreakText(string.Join(", ", item.AliasNames), 25))
                        : "-";
                    worksheet.Cell(row, 5).Style.Alignment.WrapText = true;

                    //var fullLocation = $"{item.LocationName} / {item.RoomName} / {item.RackName}";
                    var fullLocation = $"{item.LocationName}";
                    worksheet.Cell(row, 6).Value = string.Join(Environment.NewLine, BreakText(fullLocation, 25));
                    worksheet.Cell(row, 6).Style.Alignment.WrapText = true;

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "BatchDetails.xlsx");
                }
            }
        }

        // Helper method to break long text into lines of max length
        private List<string> BreakText(string text, int maxLineLength)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if ((currentLine.Length + word.Length + (string.IsNullOrEmpty(currentLine) ? 0 : 1)) <= maxLineLength)
                {
                    currentLine += (string.IsNullOrEmpty(currentLine) ? "" : " ") + word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }






        //Using this code and that time addd ne product so if avaiable so that time update this product not add new new entry
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> StockIn([FromBody] List<StockInViewModel> addStockIn)
        //{
        //    if (addStockIn == null || !addStockIn.Any())
        //    {
        //        return BadRequest("No stock data provided.");
        //    }

        //    var productQuantity = new Dictionary<int, decimal>();

        //    foreach (var item in addStockIn)
        //    {
        //        if (item.FkSupplierId <= 0 || item.FkWarehouseId <= 0 || item.FkProductId <= 0)
        //        {
        //            return BadRequest("Invalid Supplier, Warehouse, or Product ID.");
        //        }

        //        decimal quantityToAdd = Convert.ToDecimal(item.ProductQuantity);

        //        // Check if a similar entry already exists (based on key fields like BatchNo, ProductId, SupplierId, WarehouseId)
        //        var existingStockIn = await _context.TblStockIns
        //            .FirstOrDefaultAsync(s =>
        //                s.FkProductId == item.FkProductId &&
        //                //s.FkSupplierId == item.FkSupplierId &&
        //                //s.FkWarehouseId == item.FkWarehouseId &&
        //                //s.BatchNo == item.BatchNo &&
        //                s.IsDeleted == false);

        //        if (existingStockIn != null)
        //        {
        //            // Update quantity in existing record
        //            if (decimal.TryParse(existingStockIn.ProductQuantity, out var existingQty))
        //            {
        //                existingStockIn.ProductQuantity = (existingQty + quantityToAdd).ToString();
        //            }
        //            else
        //            {
        //                existingStockIn.ProductQuantity = quantityToAdd.ToString();
        //            }

        //            //existingStockIn.Price = item.Price;
        //            //existingStockIn.Room = item.Room;
        //            //existingStockIn.RackNo = item.RackNo;
        //            //existingStockIn.Barcode = item.barcodeNo;
        //            //existingStockIn.Type = item.Type;
        //            existingStockIn.UpdatedAt = DateTime.Now;

        //            _context.TblStockIns.Update(existingStockIn);
        //        }
        //        else
        //        {
        //            // Add new entry
        //            var stockIn = new TblStockIn
        //            {
        //                Date = item.Date,
        //                BatchNo = item.BatchNo,
        //                FkSupplierId = item.FkSupplierId,
        //                FkWarehouseId = item.FkWarehouseId,
        //                FkProductId = item.FkProductId,
        //                Type = item.Type,
        //                ProductQuantity = item.ProductQuantity,
        //                Room = item.Room,
        //                RackNo = item.RackNo,
        //                Barcode = item.barcodeNo,
        //                Price = item.Price,
        //                IsDeleted = false,
        //                CreatedAt = DateTime.Now
        //            };
        //            _context.TblStockIns.Add(stockIn);
        //        }

        //        // Add to dictionary to update product table later
        //        if (productQuantity.ContainsKey(item.FkProductId))
        //        {
        //            productQuantity[item.FkProductId] += quantityToAdd;
        //        }
        //        else
        //        {
        //            productQuantity[item.FkProductId] = quantityToAdd;
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    // Update available quantity in TblProducts
        //    foreach (var productId in productQuantity.Keys)
        //    {
        //        var product = await _context.TblProducts.FirstOrDefaultAsync(p => p.ProductId == productId);
        //        if (product != null)
        //        {
        //            if (decimal.TryParse(product.AvailableProductQty, out var currentQty))
        //            {
        //                product.AvailableProductQty = (currentQty + productQuantity[productId]).ToString();
        //            }
        //            else
        //            {
        //                product.AvailableProductQty = productQuantity[productId].ToString();
        //            }

        //            _context.TblProducts.Update(product);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}


        [HttpGet]
        public JsonResult GetSuppliers(string term)
        {
            var suppliers = _context.TblSuppliers
                .Where(x => x.IsDeleted == false && x.SupplierName.Contains(term))
                .Select(x => new
                {
                    id = x.SupplierId,
                    supplierName = x.SupplierName
                })
                .ToList();

            return Json(suppliers);
        }

        [HttpGet]
        public JsonResult GetWarehouse(string term)
        {
            var warehose = _context.TblWarehouses
                .Where(x => x.IsDeleted == false && x.Name.Contains(term))
                .Select(x => new
                {
                    id = x.WarehouseId,
                    warehouseName = x.Name
                }).ToList();

            return Json(warehose);
        }

        [HttpGet]
        public JsonResult GetProducts(string term)
        {
            var product = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductName.Contains(term))
                .Select(x => new
                {
                    id = x.ProductId,
                    productName = x.ProductName
                }).ToList();

            return Json(product);
        }


        [HttpGet]
        public async Task<IActionResult> StockIn()
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            //Call Store Produre
            string batchNumber;
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);

                batchNumber = parameters.Get<string>("@newBatchNo");
            }
            //End Store Produre

            //var getSupplier = _context.TblSuppliers.Where(x => x.IsDeleted == false).Select(x => new
            //{
            //    Id = x.SupplierId,
            //    supplierName = x.SupplierName
            //}).ToList();

            var getWarehouse = _context.TblWarehouses.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.WarehouseId,
                warehouseName = x.Name
            }).ToList();




            //var latestProduct = _context.TblProducts
            //.Where(x => x.IsDeleted == false)
            //.OrderByDescending(x => x.CreatedAt)
            //.FirstOrDefault();

            var now = DateTime.Now;
            var timeRange = TimeSpan.FromMinutes(1);

            var matchingProduct = _context.TblProducts
            .Where(x => x.IsDeleted == false && EF.Functions.DateDiffMinute(x.CreatedAt, now) == 0)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

            var getProduct = _context.TblProducts.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.ProductId,
                productName = x.ProductName
            }).ToList();

            var viewModel = new StockInViewModel
            {
                BatchNo = batchNumber,

                //SupplierList = new SelectList(getSupplier, "Id", "supplierName"),
                WarehouseList = new SelectList(getWarehouse, "Id", "warehouseName"),
                ProductList = new SelectList(getProduct, "Id", "productName"),
                FkProductId = matchingProduct?.ProductId ?? 0
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockIn([FromBody] List<StockInViewModel> addStockIn)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (addStockIn == null || !addStockIn.Any())
            {
                return BadRequest("No stock data provided.");
            }

            var productQuantity = new Dictionary<int, decimal>();

            foreach (var item in addStockIn)
            {
                if (item.FkSupplierId <= 0 || item.FkWarehouseId <= 0 || item.FkProductId <= 0)
                {
                    return BadRequest("Invalid Supplier, Warehouse, or Product ID.");
                }

                var stockIn = new TblStockIn
                {
                    Date = item.Date,
                    BatchNo = item.BatchNo,
                    FkSupplierId = item.FkSupplierId,
                    FkWarehouseId = item.FkWarehouseId,
                    FkProductId = item.FkProductId,
                    Type = item.Type,
                    ProductQuantity = item.ProductQuantity,
                    AvailableQuantity = item.ProductQuantity,
                    Room = item.Room,
                    RackNo = item.RackNo,
                    Barcode = item.barcodeNo,
                    Price = item.Price,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,

                };
                _context.TblStockIns.Add(stockIn);

                decimal quantityToAdd = Convert.ToDecimal(item.ProductQuantity);

                if (productQuantity.ContainsKey(item.FkProductId))
                {
                    productQuantity[item.FkProductId] += quantityToAdd;
                }
                else
                {
                    productQuantity[item.FkProductId] = quantityToAdd;
                }
            }
            await _context.SaveChangesAsync();

            foreach (var productId in productQuantity.Keys)
            {
                var stockInEntries = await _context.TblStockIns
                    .Where(s => s.FkProductId == productId && s.IsDeleted == false)
                    .ToListAsync();

                decimal existingQuantity = 0;

                foreach (var entry in stockInEntries)
                {
                    if (decimal.TryParse(entry.AvailableQuantity, out var qty))
                    {
                        existingQuantity += qty;
                    }
                }

                var product = await _context.TblProducts
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product != null)
                {
                    product.AvailableProductQty = Convert.ToString(existingQuantity);
                    _context.TblProducts.Update(product);
                }
                else
                {
                    return NotFound($"Product with ID {productId} not found.");
                }
            }

            await _context.SaveChangesAsync();


            return Ok();
        }

        //this code genarte barcode in productName PR000001 

        //[HttpGet]
        //public JsonResult GetNextBarcode(int productId)
        //{
        //    var product = _context.TblProducts.FirstOrDefault(p => p.ProductId == productId);
        //    if (product == null)
        //    {
        //        return Json(new { barcode = "" });
        //    }

        //    var prefix = product.ProductName.Length >= 2
        //        ? product.ProductName.Substring(0, 2).ToUpper()
        //        : product.ProductName.ToUpper();

        //    var existingBarcodes = _context.TblStockIns
        //        .Where(s => s.FkProductId == productId && s.Barcode.StartsWith(prefix))
        //        .Select(s => s.Barcode)
        //        .ToList();

        //    int max = 0;
        //    foreach (var barcode in existingBarcodes)
        //    {
        //        if (barcode.Length > prefix.Length &&
        //            int.TryParse(barcode.Substring(prefix.Length), out int num))
        //        {
        //            if (num > max) max = num;
        //        }
        //    }

        //    string nextBarcode = prefix + (max + 1).ToString("D6");

        //    return Json(new { barcode = nextBarcode });
        //}




        //This code genarte Barcode in SKU Name
        [HttpGet]
        public JsonResult GetNextBarcode(int productId)
        {
            var product = _context.TblProducts.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return Json(new { barcode = "" });
            }

            var sku = product.SkuIdName?.Trim();
            if (string.IsNullOrEmpty(sku))
            {
                return Json(new { barcode = "" });
            }

            return Json(new { barcode = sku });
        }


        //[HttpGet]
        //public IActionResult GetProductByBarcode(string barcode)
        //{

        //    var stocks = _context.TblStockIns
        //                        .Where(x => EF.Functions.Collate(x.Barcode, "utf8mb4_bin") == barcode).ToList();

        //    if (stocks == null || !stocks.Any())
        //    {
        //        return Json(null);
        //    }

        //    var firstStock = stocks.First();

        //    var product = _context.TblProducts
        //                          .FirstOrDefault(p => p.ProductId == firstStock.FkProductId);

        //    if (product == null)
        //    {
        //        return Json(null);
        //    }

        //    var totalAvailableQuantity = stocks.Sum(s => Convert.ToInt32(s.AvailableQuantity));

        //    var result = new
        //    {
        //        fkSupplierId = product.ProductId,
        //        productName = product.ProductName,
        //        stockInQuantity = totalAvailableQuantity
        //    };

        //    return Json(result);
        //}


        [HttpGet]
        public IActionResult GetProductByBarcode(string barcode)
        {

            var stocks = _context.TblStockIns
                                .Where(x => EF.Functions.Collate(x.Barcode, "utf8mb4_bin") == barcode).ToList();

            if (stocks == null || !stocks.Any())
            {
                return Json(null);
            }

            var firstStock = stocks.First();

            var product = _context.TblProducts
                                  .FirstOrDefault(p => p.ProductId == firstStock.FkProductId);

            if (product == null)
            {
                return Json(null);
            }

            var totalAvailableQuantity = stocks.Sum(s => Convert.ToInt32(s.AvailableQuantity));

            //var warehouseIds = _context.TblStockIns.Where(x => x.IsDeleted == false && x.Barcode == barcode)
            //    .Select(x => x.FkWarehouseId)
            //    .Distinct()
            //    .ToList();

            var warehouseQuantities = stocks
                .GroupBy(s => s.FkWarehouseId)
                .Select(g => new
                {
                    warehouseId = g.Key,
                    totalQuantity = g.Sum(s => Convert.ToInt32(s.AvailableQuantity))
                }).ToList();

            //var warehouses = _context.TblWarehouses.Where(x => x.IsDeleted == false && warehouseIds.Contains(x.WarehouseId))
            //     .Select(x => new
            //     {
            //         id = x.WarehouseId,
            //         name = x.Name
            //     }).ToList();



            var result = new
            {
                fkSupplierId = product.ProductId,
                productName = product.ProductName,
                stockInQuantity = totalAvailableQuantity,
                //warehouseName = warehouses,
                warehouseQuantities = warehouseQuantities
            };

            return Json(result);
        }


        [HttpGet]
        public IActionResult GetAvailableQuantity(string barcode, int warehouseId)
        {
            var stocks = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.Barcode == barcode && x.FkWarehouseId == warehouseId)
                .ToList();

            if (stocks == null || !stocks.Any())
            {
                return Json(0); 
            }

            var totalAvailableQuantity = stocks.Sum(s => Convert.ToInt32(s.AvailableQuantity));
            return Json(totalAvailableQuantity);
        }





        //Old Code Update the StockIn Table Single reocrd Barcode aginst 06/05/2025

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> StockOut([FromBody] List<StockOutViewModel> addStockOut)
        //{
        //    if (addStockOut == null || !addStockOut.Any())
        //    {
        //        return BadRequest("No stock data provided.");
        //    }

        //    foreach (var item in addStockOut)
        //    {
        //        var stockIn = await _context.TblStockIns
        //           .FirstOrDefaultAsync(x => x.Barcode == item.Barcode.ToString());

        //        var product = await _context.TblProducts
        //            .FirstOrDefaultAsync(x => x.ProductId == item.FkProductId);

        //        if (stockIn != null)
        //        {
        //            int currentProductQty = int.TryParse(product.AvailableProductQty, out var qty2) ? qty2 : 0;

        //            int currentQty = int.TryParse(stockIn.ProductQuantity, out var qty) ? qty : 0;
        //            int stockOutQty = int.TryParse(item.Quantity, out var outQty) ? outQty : 0;



        //            int updatedQty = currentQty - stockOutQty;
        //            if (updatedQty < 0) updatedQty = 0;

        //            stockIn.AvailableQuantity = updatedQty.ToString();
        //            _context.TblStockIns.Update(stockIn);

        //            int updateproductQty = currentProductQty - stockOutQty;

        //            product.AvailableProductQty = updateproductQty.ToString();
        //            _context.TblProducts.Update(product);


        //            var StockOut = new TblStockOut
        //            {
        //                Barcode = item.Barcode,
        //                FkProductId = stockIn.FkProductId,
        //                Quantity = item.Quantity,
        //                Reason = item.Reason,
        //                StockOutDate = DateTime.Now,
        //                FkStockInId = stockIn.StockInId
        //            };
        //            await _context.TblStockOuts.AddAsync(StockOut);
        //        }
        //        else
        //        {
        //            return BadRequest($"Stock-in not found for Barcode: {item.Barcode}");
        //        }

        //    }

        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}


        public IActionResult StockOut()
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var getProduct = _context.TblProducts.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.ProductId,
                productName = x.ProductName
            }).ToList();

            var getWarehouse = _context.TblWarehouses.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.WarehouseId,
                warehouseName = x.Name
            }).ToList();

            var viewModel = new StockOutViewModel
            {
                ProductList = new SelectList(getProduct, "Id", "productName"),
                WarehouseList = new SelectList(getWarehouse, "Id", "warehouseName"),
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockOut([FromBody] List<StockOutViewModel> addStockOut)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (addStockOut == null || !addStockOut.Any())
            {
                return BadRequest("No stock data provided.");
            }

            foreach (var item in addStockOut)
            {
                int stockOutQty = int.TryParse(item.Quantity, out var outQty) ? outQty : 0;

                //var stockInList = await _context.TblStockIns
                //    .Where(x => x.Barcode == item.Barcode.ToString())
                //    .OrderBy(x => x.StockInId) 
                //    .ToListAsync();

                var stockInList = await _context.TblStockIns
                   .Where(x => x.Barcode == item.Barcode.ToString() && x.FkWarehouseId == item.FkWarehouseId)
                   .OrderBy(x => x.StockInId)
                   .ToListAsync();

                var product = await _context.TblProducts
                    .FirstOrDefaultAsync(x => x.ProductId == item.FkProductId);

                int currentProductQty = int.TryParse(product.AvailableProductQty, out var productQty) ? productQty : 0;
                int remainingQtyToDeduct = stockOutQty;

                foreach (var stockIn in stockInList)
                {
                    int currentQty = int.TryParse(stockIn.AvailableQuantity, out var availableQty) ? availableQty : 0;

                    if (remainingQtyToDeduct >= currentQty)
                    {
                        remainingQtyToDeduct -= currentQty;
                        stockIn.AvailableQuantity = "0";
                    }
                    else
                    {
                        stockIn.AvailableQuantity = (currentQty - remainingQtyToDeduct).ToString();
                        remainingQtyToDeduct = 0;
                    }

                    _context.TblStockIns.Update(stockIn);

                    if (remainingQtyToDeduct == 0)
                        break;
                }


                product.AvailableProductQty = (currentProductQty - stockOutQty).ToString();
                _context.TblProducts.Update(product);

                // Save stock-out transaction (linking to the last used stockIn)
                var usedStockIn = stockInList.LastOrDefault(x => int.TryParse(x.AvailableQuantity, out var qty) && qty > 0)
                                  ?? stockInList.Last(); // Fallback if all became 0

                var stockOutEntry = new TblStockOut
                {
                    Barcode = item.Barcode,
                    FkProductId = item.FkProductId,
                    Quantity = item.Quantity,
                    Reason = item.Reason,
                    StockOutDate = DateTime.Now,
                    FkStockInId = usedStockIn.StockInId
                };

                await _context.TblStockOuts.AddAsync(stockOutEntry);
            }


            await _context.SaveChangesAsync();
            return Ok();
        }






        public IActionResult StocksDetails(int id, int productId, string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = _context.TblProducts
    .            FirstOrDefault(x => x.IsDeleted == false && x.ProductId == productId);

            var aliasList = _context.TblProductAliases
                .Where(x => x.FkProductId == productId)
                .Select(x => x.AliasName)
                .ToList();

            string aliasNamesString = string.Join(", ", aliasList);


            var model = new StockDetailsViewModel
            {
                CombinedStockList = new List<StockCombinedViewModel>(),
                ProductName = product?.ProductName ?? "",
                SKUName = product?.SkuIdName ?? "",
                AliasNames = aliasNamesString,
                StockInId = id,
                FkProjectId = productId,
                ProductQty = product?.AvailableProductQty

            };

            var stockInQuery = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            var stockOutQuery = _context.TblStockOuts
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockInQuery = stockInQuery.Where(x => x.Barcode.Contains(searchTerm));
                stockOutQuery = stockOutQuery.Where(x => x.Reason.Contains(searchTerm));
            }

            var stockInList = stockInQuery.ToList();
            var stockOutList = stockOutQuery.ToList();

            foreach (var item in stockInList)
            {
                var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getSupplier = _context.TblSuppliers.FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == item.FkSupplierId);
                var getLocation = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                model.CombinedStockList.Add(new StockCombinedViewModel
                {
                    Id = item.StockInId,
                    Date = item.Date ?? DateTime.MinValue,
                    CreatedAt = item.CreatedAt ?? DateTime.MinValue,
                    BatchNo = item.BatchNo ?? null,
                    Quantity = item.ProductQuantity,
                    ProductName = getProduct?.ProductName,
                    SupplierName = getSupplier?.SupplierName,
                    LocationName = getLocation?.Name,
                    Status = "Stock In",
                    Reason = "-",
                    RoomName = item.Room,
                    RackName = item.RackNo,
                    Type = item.Type
                    
                });
            }

            foreach (var item in stockOutList)
            {
                var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getStockInData = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.StockInId == item.FkStockInId);

                string supplierName = "";
                string locationName = "";
                string batchNo = "";
                string roomName = "";
                string rackName = "";
                string Type = "";

                if (getStockInData != null)
                {

                    var getSupplier = _context.TblSuppliers
                        .FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == getStockInData.FkSupplierId);

                    var getLocation = _context.TblWarehouses
                        .FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == getStockInData.FkWarehouseId);

                    supplierName = getSupplier?.SupplierName ?? "";
                    locationName = getLocation?.Name ?? "";
                    batchNo = getStockInData.BatchNo ?? "";
                    roomName = getStockInData.Room ?? "";
                    rackName = getStockInData.RackNo ?? "";
                    Type = getStockInData.Type ?? "";
                }

                model.CombinedStockList.Add(new StockCombinedViewModel
                {
                    Id = item.StockOutId,
                    Date = item.StockOutDate ?? DateTime.MinValue,
                    CreatedAt = item.CreatedAt ?? DateTime.MinValue,
                    BatchNo = batchNo,
                    Quantity = item.Quantity,
                    ProductName = getProduct?.ProductName,
                    //SupplierName = supplierName,
                    SupplierName = "-",
                    LocationName = locationName,
                    Status = "Stock Out",
                    Reason = item.Reason,
                    RoomName = roomName,
                    RackName = rackName,
                    Type = Type
                });
            }

           

            var sortedList = model.CombinedStockList.OrderByDescending(x => x.CreatedAt).ToList();

            int totalRecords = sortedList.Count;
            model.CombinedStockList = sortedList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            model.Pagination = new PaginationMetadataViewModel
            {
                TotalRecords = totalRecords,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            return View(model);
        }



        [HttpPost]
        public IActionResult ExportStockDetailsToExcel(int id, int productId, string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Fetch product data
            var product = _context.TblProducts
                .FirstOrDefault(x => x.IsDeleted == false && x.ProductId == productId);

            // Prepare alias list
            var aliasList = _context.TblProductAliases
                .Where(x => x.FkProductId == productId)
                .Select(x => x.AliasName)
                .ToList();

            string aliasNamesString = string.Join(", ", aliasList);

            // Create model to store data
            var model = new StockDetailsViewModel
            {
                CombinedStockList = new List<StockCombinedViewModel>(),
                ProductName = product?.ProductName ?? "",
                SKUName = product?.SkuIdName ?? "",
                AliasNames = aliasNamesString,
                StockInId = id,
                FkProjectId = productId
            };

            // Query stock in and stock out data
            var stockInQuery = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            var stockOutQuery = _context.TblStockOuts
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            // Apply search term filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockInQuery = stockInQuery.Where(x => x.Barcode.Contains(searchTerm));
                stockOutQuery = stockOutQuery.Where(x => x.Reason.Contains(searchTerm));
            }

            // Fetch the data
            var stockInList = stockInQuery.ToList();
            var stockOutList = stockOutQuery.ToList();

            // Combine stock in and stock out data
            foreach (var item in stockInList)
            {
                var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getSupplier = _context.TblSuppliers.FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == item.FkSupplierId);
                var getLocation = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                model.CombinedStockList.Add(new StockCombinedViewModel
                {
                    Id = item.StockInId,
                    Date = item.Date ?? DateTime.MinValue,
                    TransactionDate = item.CreatedAt ?? DateTime.MinValue,
                    BatchNo = item.BatchNo ?? null,
                    Quantity = item.ProductQuantity,
                    ProductName = getProduct?.ProductName,
                    SupplierName = getSupplier?.SupplierName,
                    LocationName = getLocation?.Name,
                    Status = "Stock In",
                    RoomName = item.Room,
                    RackName = item.RackNo
                });
            }

            foreach (var item in stockOutList)
            {
                var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getStockInData = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.StockInId == item.FkStockInId);

                string supplierName = "";
                string locationName = "";
                string batchNo = "";
                string roomName = "";
                string rackName = "";

                if (getStockInData != null)
                {
                    var getSupplier = _context.TblSuppliers
                        .FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == getStockInData.FkSupplierId);

                    var getLocation = _context.TblWarehouses
                        .FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == getStockInData.FkWarehouseId);

                    supplierName = getSupplier?.SupplierName ?? "";
                    locationName = getLocation?.Name ?? "";
                    batchNo = getStockInData.BatchNo ?? "";
                    roomName = getStockInData.Room ?? "";
                    rackName = getStockInData.RackNo ?? "";
                }

                model.CombinedStockList.Add(new StockCombinedViewModel
                {
                    Id = item.StockOutId,
                    Date = item.StockOutDate ?? DateTime.MinValue,
                    TransactionDate = item.CreatedAt ?? DateTime.MinValue,
                    BatchNo = batchNo,
                    Quantity = item.Quantity,
                    ProductName = getProduct?.ProductName,
                    //SupplierName = supplierName,
                    SupplierName = "-",
                    LocationName = locationName,
                    Status = "Stock Out",
                    RoomName = roomName,
                    RackName = rackName
                });
            }

            var sortedList = model.CombinedStockList.OrderByDescending(x => x.TransactionDate).ToList();

            // Export data to Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock Details");

                // Add headers
                worksheet.Cell(1, 1).Value = "Sr. No";
                worksheet.Cell(1, 2).Value = "Date";
                worksheet.Cell(1, 3).Value = "Product Name";
                worksheet.Cell(1, 4).Value = "Batch ID";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Supplier Name";
                worksheet.Cell(1, 7).Value = "Quantity";
                worksheet.Cell(1, 8).Value = "Location";

                // Style header
                var header = worksheet.Range("A1:H1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.Yellow;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Set column widths
                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 30;

                int row = 2;
                int count = 1;

                // Add data rows
                foreach (var item in sortedList)
                {
                    worksheet.Cell(row, 1).Value = count;
                    worksheet.Cell(row, 2).Value = item.Date.ToShortDateString();
                    worksheet.Cell(row, 3).Value = item.ProductName;
                    worksheet.Cell(row, 4).Value = item.BatchNo;
                    worksheet.Cell(row, 5).Value = item.Status;
                    worksheet.Cell(row, 6).Value = item.SupplierName;
                    worksheet.Cell(row, 7).Value = item.Quantity;
                    //worksheet.Cell(row, 8).Value = $"{item.LocationName} / {item.RoomName} / {item.RackName}";
                    worksheet.Cell(row, 8).Value = $"{item.LocationName}";

                    row++;
                    count++;
                }

                // Export the Excel file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StockDetails.xlsx");
                }
            }
        }


        public IActionResult SalesOrder(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saleMasterQuery = _context.TblSaleOrders
                .Where(x => x.IsDeleted == false && x.OrderId != 0);

            if(!string.IsNullOrEmpty(searchTerm))
            {
                saleMasterQuery = saleMasterQuery.Where(x => x.OrderNumber.Contains(searchTerm));
            }

            // Grouping by OrderNumber
            var groupedOrders = saleMasterQuery
                .GroupBy(x => x.OrderNumber)
                .AsEnumerable()
                .Select(g => new
                {
                    OrderNumber = g.Key,
                    TotalQuantity = g.Sum(x => int.TryParse(x.OrderProductQty, out int qty) ? qty : 0),
                    OrderDate = g.OrderBy(x => x.OrderDate).FirstOrDefault().OrderDate
                });

            int totalRecords = groupedOrders.Count();

            var paginatedOrders = groupedOrders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            List<SaleOrderViewModel> getSaleOrderList = paginatedOrders.Select(order => new SaleOrderViewModel
            {
                OrderNumber = order.OrderNumber,
                OrderQuantity = order.TotalQuantity.ToString(),
                OrderDate = (DateTime)order.OrderDate
            }).ToList();

            var viewModel = new SaleOrderMasterListViewModel
            {
                SaleOrder = getSaleOrderList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            return View(viewModel); // make sure you return the view with the viewModel
        }

        [HttpGet]
        public IActionResult ExportSalesOrderExcel(string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saleMasterQuery = _context.TblSaleOrders
                .Where(x => x.IsDeleted == false && x.OrderId != 0)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                saleMasterQuery = saleMasterQuery.Where(x =>
                    x.ProductName.Contains(searchTerm) ||
                    x.OrderNumber.Contains(searchTerm) ||
                    x.OrderProductQty.Contains(searchTerm));
            }

            // Grouped data to match SalesOrder action
            var groupedOrders = saleMasterQuery
                .GroupBy(x => x.OrderNumber)
                .AsEnumerable()
                .Select(g => new
                {
                    OrderNumber = g.Key,
                    TotalQuantity = g.Sum(x => int.TryParse(x.OrderProductQty, out int qty) ? qty : 0),
                    OrderDate = g.OrderBy(x => x.OrderDate).FirstOrDefault()?.OrderDate
                })
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SaleOrder");

                // Headers
                worksheet.Cell(1, 1).Value = "Sr.No.";
                worksheet.Cell(1, 2).Value = "Order Number";
                worksheet.Cell(1, 3).Value = "Order Date";
                worksheet.Cell(1, 4).Value = "Order Quantity";

                // Header Style
                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Column Widths
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;

                // Data Rows
                int row = 2;
                int srNo = 1;
                foreach (var item in groupedOrders)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.OrderNumber;
                    worksheet.Cell(row, 3).Value = item.OrderDate?.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = item.TotalQuantity;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "SalesOrders.xlsx");
                }
            }
        }


        public IActionResult SalesOrderDetails(string orderNumber, string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saleOrderList = _context.TblSaleOrders
                .Where(x => x.IsDeleted == false && x.OrderId != 0 && x.OrderNumber == orderNumber)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                saleOrderList = saleOrderList.Where(x => x.ProductName.Contains(searchTerm) ||
                                                         x.Status.Contains(searchTerm) ||
                                                         x.OrderProductQty.Contains(searchTerm));
            }


            int totalRecords = saleOrderList.Count();

            var paginatedProduct = saleOrderList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            List<SaleOrderViewModel> getSaleOrderList = new List<SaleOrderViewModel>();

            foreach(var item in paginatedProduct)
            {
                var product = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var StockIn = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.FkProductId == item.FkProductId);

                int orderQty = int.TryParse(item.OrderProductQty, out var oq) ? oq : 0;
                int availableQty = int.TryParse(product?.AvailableProductQty, out var aq) ? aq : 0;

                string status = availableQty >= orderQty ? "Available" : "Not Available";

                getSaleOrderList.Add(new SaleOrderViewModel
                {
                    OrderId = item.OrderId,
                    ProductName = product.ProductName,
                    SKUName = product.SkuIdName,
                    OrderQuantity = item.OrderProductQty,
                    AvailableQuantity = product?.AvailableProductQty ?? "0",
                    OrderDate = (DateTime)item.OrderDate,
                    Status = status,
                });
            }

            var viewModel = new SaleOrderMasterListViewModel
            {
                SaleOrder = getSaleOrderList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                },
                OrderNumber = orderNumber
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult ExportSalesOrderDetailsExcel(string orderNumber, string searchTerm = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saleOrderList = _context.TblSaleOrders
                .Where(x => x.IsDeleted == false && x.OrderId != 0 && x.OrderNumber == orderNumber)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                saleOrderList = saleOrderList.Where(x =>
                    x.ProductName.Contains(searchTerm) ||
                    x.Status.Contains(searchTerm) ||
                    x.OrderProductQty.Contains(searchTerm));
            }

            var data = saleOrderList.ToList();

            List<SaleOrderViewModel> saleOrderDetails = new List<SaleOrderViewModel>();

            foreach (var item in data)
            {
                var product = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var stockIn = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.FkProductId == item.FkProductId);

                int orderQty = int.TryParse(item.OrderProductQty, out var oq) ? oq : 0;
                int availableQty = int.TryParse(product?.AvailableProductQty, out var aq) ? aq : 0;

                string status = availableQty >= orderQty ? "Available" : "Not Available";

                saleOrderDetails.Add(new SaleOrderViewModel
                {
                    OrderId = item.OrderId,
                    ProductName = product?.ProductName,
                    SKUName = product?.SkuIdName,
                    OrderQuantity = item.OrderProductQty,
                    AvailableQuantity = product?.AvailableProductQty ?? "0",
                    OrderDate = (DateTime)item.OrderDate,
                    Status = status
                });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SaleOrderDetails");

                // Headers
                worksheet.Cell(1, 1).Value = "Sr.No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU Name";
                worksheet.Cell(1, 4).Value = "Order Quantity";
                worksheet.Cell(1, 5).Value = "Available Quantity";
                worksheet.Cell(1, 6).Value = "Order Date";
                worksheet.Cell(1, 7).Value = "Status";

                // Header styling
                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Column Widths
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;

                // Column widths
                //worksheet.Columns().AdjustToContents();

                // Data
                int row = 2;
                int srNo = 1;
                foreach (var item in saleOrderDetails)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.SKUName;
                    worksheet.Cell(row, 4).Value = item.OrderQuantity;
                    worksheet.Cell(row, 5).Value = item.AvailableQuantity;
                    worksheet.Cell(row, 6).Value = item.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 7).Value = item.Status;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SaleOrderDetails.xlsx");
                }
            }
        }


        //Start Correct Code Not Delete 14/05/2025

        //[HttpPost]
        //public IActionResult ImportExcel(IFormFile file)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    if (file == null || file.Length == 0)
        //    {
        //        TempData["ErrorMessage"] = "Please select an Excel file.";
        //        return RedirectToAction("Create");
        //    }

        //    try
        //    {
        //        using (var stream = new MemoryStream())
        //        {
        //            file.CopyTo(stream);
        //            stream.Position = 0;

        //            // Use ClosedXML Labibary
        //            using (var workbook = new XLWorkbook(stream))
        //            {
        //                var worksheet = workbook.Worksheets.Worksheet(1); 
        //                var rowCount = worksheet.RowsUsed().Count(); 

        //                List<TblSaleOrder> saleOrderToInsert = new List<TblSaleOrder>();
        //                List<TblSaleOrder> saleOrderToUpdate = new List<TblSaleOrder>();

        //                for (int row = 2; row <= rowCount; row++)
        //                {
        //                    string orderNumber = worksheet.Cell(row, 1).GetString().Trim();
        //                    string productName = worksheet.Cell(row, 2).GetString().Trim();
        //                    string productQty = worksheet.Cell(row, 3).GetString().Trim();
        //                    string orderDateString = worksheet.Cell(row, 4).GetString().Trim();

        //                    if (string.IsNullOrEmpty(orderNumber) || string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productQty) || string.IsNullOrEmpty(orderDateString))
        //                    {
        //                        continue;
        //                    }

        //                    DateTime orderDate;
        //                    bool validDate = false;

        //                    // First try parsing with common formats
        //                    string[] formats = {
        //                        "d/M/yyyy", "dd/MM/yyyy", "yyyy/MM/dd", "MM/dd/yyyy",
        //                        "d/M/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt",
        //                        "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss"
        //                    };

        //                    validDate = DateTime.TryParseExact(orderDateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate);

        //                    // If TryParseExact fails, fallback to TryParse (more flexible)
        //                    if (!validDate)
        //                    {
        //                        validDate = DateTime.TryParse(orderDateString, out orderDate);
        //                    }

        //                    if (!validDate)
        //                    {
        //                        continue; 
        //                    }


        //                    var existingSaleOrder = _context.TblSaleOrders.AsNoTracking().FirstOrDefault(u => u.OrderNumber == orderNumber);

        //                    if (existingSaleOrder != null)
        //                    {
        //                        existingSaleOrder.OrderNumber = orderNumber;
        //                        existingSaleOrder.ProductName = productName;
        //                        existingSaleOrder.OrderProductQty = productQty;
        //                        existingSaleOrder.OrderDate = orderDate;
        //                        existingSaleOrder.UpdatedAt = DateTime.Now;

        //                        var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);
        //                        if (product != null)
        //                        {
        //                            existingSaleOrder.FkProductId = product.ProductId;
        //                            saleOrderToUpdate.Add(existingSaleOrder);
        //                        }
        //                        else
        //                        {
        //                            TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
        //                            return RedirectToAction("SalesOrder"); 
        //                        }

        //                    }
        //                    else
        //                    {
        //                        var newSaleOrder = new TblSaleOrder
        //                        {
        //                            OrderNumber = orderNumber,
        //                            ProductName = productName,
        //                            OrderProductQty = productQty,
        //                            OrderDate = orderDate,
        //                            CreatedAt = DateTime.Now,
        //                        };

        //                        var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);
        //                        if (product != null)
        //                        {
        //                            newSaleOrder.FkProductId = product.ProductId;
        //                            saleOrderToInsert.Add(newSaleOrder);
        //                        }
        //                        else
        //                        {
        //                            TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
        //                            return RedirectToAction("SalesOrder"); 
        //                        }


        //                    }
        //                }

        //                if (saleOrderToInsert.Count > 0)
        //                {
        //                    _context.TblSaleOrders.AddRange(saleOrderToInsert);
        //                    _context.SaveChanges();
        //                }

        //                if (saleOrderToUpdate.Count > 0)
        //                {
        //                    _context.TblSaleOrders.UpdateRange(saleOrderToUpdate);
        //                    _context.SaveChanges();
        //                }

        //                int rowsAffected = saleOrderToInsert.Count + saleOrderToUpdate.Count;
        //                if (rowsAffected > 0)
        //                {
        //                    TempData["SuccessMessage"] = $"Successfully imported {rowsAffected} records!";
        //                }
        //                else
        //                {
        //                    TempData["ErrorMessage"] = "No valid user data found in the file.";
        //                }

        //                return RedirectToAction("SalesOrder");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error importing data: " + ex.Message;
        //        return RedirectToAction("SalesOrder");
        //    }
        //}

        //End Correct Code Not Delete 14/05/2025


        [HttpPost]
        public IActionResult ImportExcel(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an Excel file.";
                return RedirectToAction("Create");
            }

            List<string> invalidProducts = new List<string>();
            List<TblSaleOrder> saleOrderToInsert = new List<TblSaleOrder>();
            List<TblSaleOrder> saleOrderToUpdate = new List<TblSaleOrder>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;

                    // Use ClosedXML Labibary
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.Worksheet(1);
                        var rowCount = worksheet.RowsUsed().Count();



                        for (int row = 2; row <= rowCount; row++)
                        {
                            string orderNumber = worksheet.Cell(row, 1).GetString().Trim();
                            string productName = worksheet.Cell(row, 2).GetString().Trim();
                            string productQty = worksheet.Cell(row, 3).GetString().Trim();
                            string orderDateString = worksheet.Cell(row, 4).GetString().Trim();

                            if (string.IsNullOrEmpty(orderNumber) || string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productQty) || string.IsNullOrEmpty(orderDateString))
                            {
                                continue;
                            }

                            DateTime orderDate;
                            bool validDate = false;

                            // First try parsing with common formats
                            string[] formats = {
                                "d/M/yyyy", "dd/MM/yyyy", "yyyy/MM/dd", "MM/dd/yyyy",
                                "d/M/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss tt",
                                "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss"
                            };

                            validDate = DateTime.TryParseExact(orderDateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate);

                            // If TryParseExact fails, fallback to TryParse (more flexible)
                            if (!validDate)
                            {
                                validDate = DateTime.TryParse(orderDateString, out orderDate);
                            }

                            if (!validDate)
                            {
                                continue;
                            }


                            var existingSaleOrder = _context.TblSaleOrders.AsNoTracking().FirstOrDefault(u => u.OrderNumber == orderNumber);

                            var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);

                            if (product != null)
                            {
                                if (existingSaleOrder != null)
                                {
                                    existingSaleOrder.OrderNumber = orderNumber;
                                    existingSaleOrder.ProductName = productName;
                                    existingSaleOrder.OrderProductQty = productQty;
                                    existingSaleOrder.OrderDate = orderDate;
                                    existingSaleOrder.UpdatedAt = DateTime.Now;
                                    existingSaleOrder.FkProductId = product.ProductId;

                                    saleOrderToUpdate.Add(existingSaleOrder);

                                    //if (product != null)
                                    //{
                                    //    existingSaleOrder.FkProductId = product.ProductId;
                                    //    saleOrderToUpdate.Add(existingSaleOrder);
                                    //}
                                    //else
                                    //{
                                    //    TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
                                    //    return RedirectToAction("SalesOrder");
                                    //}

                                }
                                else
                                {
                                    var newSaleOrder = new TblSaleOrder
                                    {
                                        OrderNumber = orderNumber,
                                        ProductName = productName,
                                        OrderProductQty = productQty,
                                        OrderDate = orderDate,
                                        CreatedAt = DateTime.Now,
                                        FkProductId = product.ProductId
                                    };

                                    saleOrderToInsert.Add(newSaleOrder);

                                    //var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);
                                    //if (product != null)
                                    //{
                                    //    newSaleOrder.FkProductId = product.ProductId;
                                    //    saleOrderToInsert.Add(newSaleOrder);
                                    //}
                                    //else
                                    //{
                                    //    TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
                                    //    return RedirectToAction("SalesOrder");
                                    //}
                                }
                                
                            }
                            else
                            {
                                
                                TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
                                //return RedirectToAction("SalesOrder");
                                invalidProducts.Add(productName);
                            }

                        }

                        if (saleOrderToInsert.Count > 0)
                        {
                            _context.TblSaleOrders.AddRange(saleOrderToInsert);
                            _context.SaveChanges();
                        }

                        if (saleOrderToUpdate.Count > 0)
                        {
                            _context.TblSaleOrders.UpdateRange(saleOrderToUpdate);
                            _context.SaveChanges();
                        }

                        int rowsAffected = saleOrderToInsert.Count + saleOrderToUpdate.Count;
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = $"Successfully imported {rowsAffected} records!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No valid user data found in the file.";
                        }

                        return RedirectToAction("SalesOrder");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error importing data: " + ex.Message;
                return RedirectToAction("SalesOrder");
            }
        }

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

            if (addProduct.AliasNames != null && addProduct.AliasNames.Any())
            {
                foreach (var alias in addProduct.AliasNames)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {

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



            return RedirectToAction("StockIn");
        }




        //[HttpPost]
        //public async Task<IActionResult> StockInDataExcelImport(IFormFile file)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    if (file == null || file.Length == 0)
        //    {
        //        TempData["ErrorMessage"] = "Please select an Excel file.";
        //        return RedirectToAction("Create");
        //    }

        //    string batchNumber = "";

        //    // 1. Get Batch Number from stored procedure
        //    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
        //        await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
        //        batchNumber = parameters.Get<string>("@newBatchNo");
        //    }

        //    List<TblStockIn> stockInList = new();

        //    try
        //    {
        //        using (var stream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(stream);
        //            stream.Position = 0;

        //            using (var workbook = new XLWorkbook(stream))
        //            {
        //                var worksheet = workbook.Worksheets.Worksheet(1);
        //                var rowCount = worksheet.RowsUsed().Count();

        //                for (int row = 2; row <= rowCount; row++)
        //                {
        //                    string itemName = worksheet.Cell(row, 1).GetString().Trim();       
        //                    string itemCode = worksheet.Cell(row, 2).GetString().Trim();      
        //                    string priceText = worksheet.Cell(row, 5).GetString().Trim();      
        //                    string qtyText = worksheet.Cell(row, 9).GetString().Trim();        

        //                    if (string.IsNullOrWhiteSpace(itemName)) continue;

        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
        //                    if (product == null) continue;

        //                    decimal.TryParse(priceText, out decimal price);
        //                    int.TryParse(qtyText, out int qty);


        //                    int currentQty = 0;
        //                    if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
        //                    {
        //                        int.TryParse(product.AvailableProductQty, out currentQty);
        //                    }

        //                    int updatedQty = currentQty + qty;
        //                    product.AvailableProductQty = updatedQty.ToString();



        //                    TblStockIn stockIn = new()
        //                    {
        //                        FkProductId = product.ProductId,
        //                        Barcode = itemCode,
        //                        Price = price,
        //                        ProductQuantity = Convert.ToString(qty),
        //                        AvailableQuantity = Convert.ToString(qty),
        //                        Date = DateTime.Now,
        //                        Type = "2",
        //                        FkWarehouseId = 1,
        //                        FkSupplierId = 1,
        //                        BatchNo = batchNumber
        //                    };

        //                    stockInList.Add(stockIn);
        //                }

        //                if (stockInList.Any())
        //                {
        //                    _context.TblStockIns.AddRange(stockInList);
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully!";
        //                }
        //                else
        //                {
        //                    TempData["ErrorMessage"] = "No valid records found.";
        //                }
        //            }
        //        }

        //        return RedirectToAction("InventoryList");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error importing data: " + ex.Message;
        //        return RedirectToAction("InventoryList");
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> StockInDataExcelImport(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an Excel file.";
                return RedirectToAction("Create");
            }

            string batchNumber = "";
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
                batchNumber = parameters.Get<string>("@newBatchNo");
            }

            List<TblStockIn> stockInList = new();
            List<string> failedRecords = new();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.Worksheet(1);
                        var rowCount = worksheet.RowsUsed().Count();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string itemName = worksheet.Cell(row, 1).GetString().Trim();
                            string itemCode = worksheet.Cell(row, 2).GetString().Trim();
                            string priceText = worksheet.Cell(row, 5).GetString().Trim();
                            string qtyText = worksheet.Cell(row, 9).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(itemName))
                                continue;

                            var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
                            if (product == null)
                            {
                                failedRecords.Add($"Row {row}: Product '{itemName}' not found");
                                continue;
                            }

                            decimal.TryParse(priceText, out decimal price);
                            int.TryParse(qtyText, out int qty);

                            int currentQty = 0;
                            if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
                            {
                                int.TryParse(product.AvailableProductQty, out currentQty);
                            }

                            int updatedQty = currentQty + qty;
                            product.AvailableProductQty = updatedQty.ToString();

                            TblStockIn stockIn = new()
                            {
                                FkProductId = product.ProductId,
                                Barcode = itemCode,
                                Price = price,
                                ProductQuantity = qty.ToString(),
                                AvailableQuantity = qty.ToString(),
                                Date = DateTime.Now,
                                Type = "2",
                                FkWarehouseId = 1,
                                FkSupplierId = 1,
                                BatchNo = batchNumber
                            };

                            stockInList.Add(stockIn);
                        }

                        if (stockInList.Any())
                        {
                            _context.TblStockIns.AddRange(stockInList);
                            await _context.SaveChangesAsync();
                        }

                        TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
                        if (failedRecords.Any())
                        {
                            TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
                        }

                    }
                }

                return RedirectToAction("InventoryList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error importing data: " + ex.Message;
                return RedirectToAction("InventoryList");
            }
        }



        [HttpPost]
        public async Task<IActionResult> ImportBoxItems(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an Excel file.";
                return RedirectToAction("Create");
            }

            string batchNumber = "";
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
                batchNumber = parameters.Get<string>("@newBatchNo");
            }

            List<TblStockIn> stockInList = new();
            List<string> failedRecords = new();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.Worksheet(1);
                        var rowCount = worksheet.RowsUsed().Count();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string itemName = worksheet.Cell(row, 3).GetString().Trim();
                            string itemCode = worksheet.Cell(row, 2).GetString().Trim();
                            string priceText = worksheet.Cell(row, 5).GetString().Trim();
                            string qtyText = worksheet.Cell(row, 9).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(itemName))
                                continue;

                            var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
                            if (product == null)
                            {
                                failedRecords.Add($"Row {row}: Product '{itemName}' not found");
                                continue;
                            }

                            decimal.TryParse(priceText, out decimal price);
                            int.TryParse(qtyText, out int qty);

                            int currentQty = 0;
                            if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
                            {
                                int.TryParse(product.AvailableProductQty, out currentQty);
                            }

                            int updatedQty = currentQty + qty;
                            product.AvailableProductQty = updatedQty.ToString();

                            TblStockIn stockIn = new()
                            {
                                FkProductId = product.ProductId,
                                Barcode = itemCode,
                                Price = price,
                                ProductQuantity = qty.ToString(),
                                AvailableQuantity = qty.ToString(),
                                Date = DateTime.Now,
                                Type = "2",
                                FkWarehouseId = 1,
                                FkSupplierId = 1,
                                BatchNo = batchNumber
                            };

                            stockInList.Add(stockIn);
                        }

                        if (stockInList.Any())
                        {
                            _context.TblStockIns.AddRange(stockInList);
                            await _context.SaveChangesAsync();
                        }

                        TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
                        if (failedRecords.Any())
                        {
                            TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
                        }

                    }
                }

                return RedirectToAction("InventoryList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error importing data: " + ex.Message;
                return RedirectToAction("InventoryList");
            }
        }
    }
}
