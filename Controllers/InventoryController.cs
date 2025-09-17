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


        public JsonResult GetWarehouses()
        {
            var warehouses = _context.TblWarehouses.Where(w => w.IsDeleted == false)
                                     .Select(w => new
                                     {
                                         Id = w.WarehouseId,
                                         Name = w.Name
                                     })
                                     .ToList();

            return Json(warehouses);
        }


        public JsonResult GetRacks(int warehouseId)
        {
            var racks = _context.TblRacks
                .Where(r => r.FkWarehouseId == warehouseId) 
                .Select(r => new
                {
                    id = r.RackId, 
                    name = r.RackNo
                }).ToList();

            return Json(racks);
        }


        //Code For Sagar Add Filters  On 29/08/2025
        //public IActionResult InventoryList(string searchTerm = "", int pageNumber = 1, int pageSize = 10, int warehouseId = 0, string rackId = null, int itemType = 0)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var userRoleId = _context.TblUsers.Where(x => x.UserId == userId).Select(x => x.FkRoleId).FirstOrDefault();

        //    List<StockInViewModel> getStockInList = new List<StockInViewModel>();

        //    var stockInList = _context.TblStockIns.Where(x => x.IsDeleted == false && x.StockInId != 0).AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        stockInList = stockInList.Where(x => x.Barcode.Contains(searchTerm) ||
        //                                             x.ProductQuantity.Contains(searchTerm) ||
        //                                             x.ProductStatus.Contains(searchTerm) ||
        //                                             _context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.Contains(searchTerm)) ||
        //                                             _context.TblProductAliases.Any(a => a.FkProductId == x.FkProductId && a.AliasName.Contains(searchTerm)));
        //    }


        //    if (warehouseId > 0)
        //        stockInList = stockInList.Where(x => x.FkWarehouseId == warehouseId);

        //    // Apply Rack filter
        //    if (rackId != null)
        //        stockInList = stockInList.Where(x => x.RackNo == rackId);

        //    // Apply Item Type filter
        //    if (itemType > 0)
        //        stockInList = stockInList.Where(x => x.Type == itemType.ToString());

        //    var groupedStockInList = stockInList
        //    .GroupBy(x => x.FkProductId)
        //    .Select(g => g.First())
        //    .ToList();

        //    int totalProductsCount = stockInList.Select(x => x.FkProductId).Distinct().Count();
        //    int totalBatchCount = stockInList.Select(x => x.BatchNo).Distinct().Count();
        //    int totalRecords = groupedStockInList.Count();



        //    int totalOutOfStockCount = _context.TblProducts
        //        .Where(x => x.IsDeleted == false && x.AvailableProductQty == "0")
        //        .Count();

        //    ViewBag.ProductCount = totalProductsCount;
        //    ViewBag.BatchCount = totalBatchCount;
        //    //ViewBag.TotalStock = totalStockCount;
        //    ViewBag.TotalOutOfStock = totalOutOfStockCount;

        //    var paginatedstockIn = groupedStockInList
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();


        //    //int totalLowStockCount = 0;

        //    foreach (var item in paginatedstockIn)
        //    {
        //        var aliasNames = _context.TblProductAliases
        //            .Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId)
        //            .Select(x => x.AliasName)
        //            .ToList();

        //        var getProject = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
        //        var getWarehouse = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

        //        var qtyForSingleItem = _context.TblStockIns
        //            .Where(x => x.FkProductId == getProject.ProductId
        //                        && x.IsDeleted == false
        //                        && x.Type == "2")
        //            .Sum(x => Convert.ToInt32(x.AvailableQuantity));

        //        var qtyForBoxItem = _context.TblStockIns
        //            .Where(x => x.FkProductId == getProject.ProductId
        //                       && x.IsDeleted == false
        //                       && x.Type == "1")
        //            .Sum(x => Convert.ToInt32(x.AvailableQuantity));

        //        getStockInList.Add(new StockInViewModel
        //        {
        //            StockInId = item.StockInId,
        //            FkProductId = (int)item.FkProductId,
        //            Barcode = item.Barcode,
        //            Price = (decimal)item.Price,
        //            ProductQuantity = getProject?.AvailableProductQty,
        //            LowStockQty = getProject?.LowStockQuantity,
        //            ProductName = getProject?.ProductName,
        //            SKUName = getProject?.SkuIdName,
        //            WarehouseName = getWarehouse?.Name,
        //            AliasNames = aliasNames,
        //            Type = item.Type,
        //            QtySingleItem = qtyForSingleItem,
        //            QtyBoxItem = qtyForBoxItem

        //        });
        //    }



        //    //Start Code for Get LowStockCount
        //    var products = _context.TblProducts
        //    .Where(p => p.IsDeleted == false)
        //    .ToList();

        //    int totalLowStockCount = products.Count(p =>
        //        int.TryParse(p.AvailableProductQty, out int availableQty) &&
        //        int.TryParse(p.LowStockQuantity, out int lowStockQty) &&
        //        availableQty <= lowStockQty
        //    );

        //    ViewBag.TotalLowStock = totalLowStockCount;
        //    //End Code for Get LowStockCount

        //    var viewModel = new StockInListViewModel
        //    {
        //        UserFkRoleId = (int)userRoleId,
        //        StockIns = getStockInList,
        //        Pagination = new PaginationMetadataViewModel
        //        {
        //            TotalRecords = totalRecords,
        //            CurrentPage = pageNumber,
        //            PageSize = pageSize,
        //            SearchTerm = searchTerm
        //        }
        //    };

        //    return View(viewModel);
        //}


        public IActionResult InventoryList(string searchTerm = "", int pageNumber = 1, int pageSize = 10, int warehouseId = 0, string rackId = null, int itemType = 0, string filterType = "")
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRoleId = _context.TblUsers.Where(x => x.UserId == userId)
                                              .Select(x => x.FkRoleId)
                                              .FirstOrDefault();

            List<StockInViewModel> getStockInList = new List<StockInViewModel>();

            var stockInList = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.StockInId != 0)
                .AsQueryable();

            // ✅ Search filter
            //if (!string.IsNullOrEmpty(searchTerm))
            //{
            //    stockInList = stockInList.Where(x =>
            //        x.Barcode.Contains(searchTerm) ||
            //        x.ProductQuantity.Contains(searchTerm) ||
            //        x.ProductStatus.Contains(searchTerm) ||
            //        _context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.Contains(searchTerm)) ||
            //        _context.TblProductAliases.Any(a => a.FkProductId == x.FkProductId && a.AliasName.Contains(searchTerm))
            //    );
            //}

            //Start This Code For Search Data Very Fast Using Joint
            if (!string.IsNullOrEmpty(searchTerm))
            {
                stockInList =
                    from s in stockInList
                    join p in _context.TblProducts on s.FkProductId equals p.ProductId
                    join a in _context.TblProductAliases on s.FkProductId equals a.FkProductId into aliasGroup
                    from alias in aliasGroup.DefaultIfEmpty()
                    where s.Barcode.Contains(searchTerm)
                       || s.ProductQuantity.Contains(searchTerm)
                       || s.ProductStatus.Contains(searchTerm)
                       || p.ProductName.Contains(searchTerm)
                       || (alias != null && alias.AliasName.Contains(searchTerm))
                    select s;
            }
            //Start This Code For Search Data Very Fast Using Joint

            // ✅ Warehouse filter
            if (warehouseId > 0)
                stockInList = stockInList.Where(x => x.FkWarehouseId == warehouseId);

            // ✅ Rack filter
            if (rackId != null)
                stockInList = stockInList.Where(x => x.RackNo == rackId);

            // ✅ Item Type filter
            if (itemType > 0)
                stockInList = stockInList.Where(x => x.Type == itemType.ToString());

            // ✅ Apply filterType from card clicks
            if (filterType == "LowStock")
            {
                //var lowStockIds = _context.TblProducts
                // .Where(p => p.IsDeleted == false)
                // .ToList() 
                // .Where(p => int.TryParse(p.AvailableProductQty, out int avail) &&
                //             int.TryParse(p.LowStockQuantity, out int lowQty) &&
                //             avail <= lowQty)
                // .Select(p => p.ProductId)
                // .ToList();

                var lowStockIds = _context.TblProducts
                .Where(p => p.IsDeleted == false &&
                            Convert.ToInt32(p.AvailableProductQty) <= Convert.ToInt32(p.LowStockQuantity))
                .Select(p => p.ProductId)
                .ToList();


                stockInList = stockInList.Where(x => lowStockIds.Contains(x.FkProductId.Value));
            }
            else if (filterType == "OutOfStock")
            {
                var outOfStockIds = _context.TblProducts
                    .Where(p => p.IsDeleted == false && p.AvailableProductQty == "0")
                    .Select(p => p.ProductId)
                    .ToList();

                stockInList = stockInList.Where(x => outOfStockIds.Contains(x.FkProductId.Value));
            }
            // If filterType == "All" → no extra condition

            // ✅ Group by product
            var groupedStockInList = stockInList
                .GroupBy(x => x.FkProductId)
                .Select(g => g.First())
                .ToList();

            int totalProductsCount = stockInList.Select(x => x.FkProductId).Distinct().Count();
            int totalBatchCount = stockInList.Select(x => x.BatchNo).Distinct().Count();
            int totalRecords = groupedStockInList.Count();

            int totalOutOfStockCount = _context.TblProducts
                .Count(x => x.IsDeleted == false && x.AvailableProductQty == "0");

            ViewBag.ProductCount = totalProductsCount;
            ViewBag.BatchCount = totalBatchCount;
            ViewBag.TotalOutOfStock = totalOutOfStockCount;

            var paginatedstockIn = groupedStockInList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var item in paginatedstockIn)
            {
                var aliasNames = _context.TblProductAliases
                    .Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId)
                    .Select(x => x.AliasName)
                    .ToList();

                var getProject = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
                var getWarehouse = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                var qtyForSingleItem = _context.TblStockIns
                    .Where(x => x.FkProductId == getProject.ProductId && x.IsDeleted == false && x.Type == "2")
                    .Sum(x => Convert.ToInt32(x.AvailableQuantity));

                var qtyForBoxItem = _context.TblStockIns
                    .Where(x => x.FkProductId == getProject.ProductId && x.IsDeleted == false && x.Type == "1")
                    .Sum(x => Convert.ToInt32(x.AvailableQuantity));

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
                    Type = item.Type,
                    QtySingleItem = qtyForSingleItem,
                    QtyBoxItem = qtyForBoxItem
                });
            }

            // ✅ Low Stock Count
            var products = _context.TblProducts.Where(p => p.IsDeleted == false).ToList();
            int totalLowStockCount = products.Count(p =>
                int.TryParse(p.AvailableProductQty, out int availableQty) &&
                int.TryParse(p.LowStockQuantity, out int lowStockQty) &&
                availableQty <= lowStockQty
            );
            ViewBag.TotalLowStock = totalLowStockCount;

            ViewBag.FilterType = filterType;
            ViewBag.WarehouseId = warehouseId;
            ViewBag.RackId = rackId;
            ViewBag.ItemType = itemType;


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


        //Code For Bhushan Comment On 29/08/2025 For Bhushan

        //public IActionResult InventoryList(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var userRoleId = _context.TblUsers.Where(x => x.UserId == userId).Select(x => x.FkRoleId).FirstOrDefault();

        //    List<StockInViewModel> getStockInList = new List<StockInViewModel>();

        //    var stockInList = _context.TblStockIns.Where(x => x.IsDeleted == false && x.StockInId != 0).AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        stockInList = stockInList.Where(x => x.Barcode.Contains(searchTerm) ||
        //                                             x.ProductQuantity.Contains(searchTerm) ||
        //                                             x.ProductStatus.Contains(searchTerm) ||
        //                                             _context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.Contains(searchTerm)) ||
        //                                             _context.TblProductAliases.Any(a => a.FkProductId == x.FkProductId && a.AliasName.Contains(searchTerm)));
        //    }

        //    var groupedStockInList = stockInList
        //    .GroupBy(x => x.FkProductId)
        //    .Select(g => g.First())
        //    .ToList();

        //    int totalProductsCount = stockInList.Select(x => x.FkProductId).Distinct().Count();
        //    int totalBatchCount = stockInList.Select(x => x.BatchNo).Distinct().Count();
        //    int totalRecords = groupedStockInList.Count();

        //    //int totalStockCount = stockInList.Select(x => x.StockInId).Distinct().Count();
        //    //int totalOutOfStockCount = stockInList.Where(x => Convert.ToInt32(x.AvailableQuantity) == 0).Select(x => x.StockInId).Distinct().Count();

        //    int totalOutOfStockCount = _context.TblProducts
        //        .Where(x => x.IsDeleted == false && x.AvailableProductQty == "0")
        //        .Count();

        //    ViewBag.ProductCount = totalProductsCount;
        //    ViewBag.BatchCount = totalBatchCount;
        //    //ViewBag.TotalStock = totalStockCount;
        //    ViewBag.TotalOutOfStock = totalOutOfStockCount;

        //    var paginatedstockIn = groupedStockInList
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();


        //    //int totalLowStockCount = 0;

        //    foreach (var item in paginatedstockIn)
        //    {
        //        var aliasNames = _context.TblProductAliases
        //            .Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId)
        //            .Select(x => x.AliasName)
        //            .ToList();

        //        var getProject = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
        //        var getWarehouse = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

        //        //Start Old Code for Get LowStockCount i think this code getting wrong count  //int totalLowStockCount = 0;

        //        //if (getProject != null)
        //        //{
        //        //    if (int.TryParse(getProject.AvailableProductQty, out int availableQty) &&
        //        //        int.TryParse(getProject.LowStockQuantity, out int lowStockQty))
        //        //    {
        //        //        if (availableQty <= lowStockQty)
        //        //        {
        //        //            totalLowStockCount++;
        //        //        }
        //        //    }
        //        //}
        //        //ViewBag.TotalLowStock = totalLowStockCount;

        //        //End Old Code for Get LowStockCount i think this code getting wrong count

        //        getStockInList.Add(new StockInViewModel
        //        {
        //            StockInId = item.StockInId,
        //            FkProductId = (int)item.FkProductId,
        //            Barcode = item.Barcode,
        //            Price = (decimal)item.Price,
        //            ProductQuantity = getProject?.AvailableProductQty,
        //            LowStockQty = getProject?.LowStockQuantity,
        //            ProductName = getProject?.ProductName,
        //            SKUName = getProject?.SkuIdName,
        //            WarehouseName = getWarehouse?.Name,
        //            AliasNames = aliasNames,
        //            Type = item.Type

        //        });       
        //    }



        //    //Start Code for Get LowStockCount
        //    var products = _context.TblProducts
        //    .Where(p => p.IsDeleted == false)
        //    .ToList();

        //    int totalLowStockCount = products.Count(p =>
        //        int.TryParse(p.AvailableProductQty, out int availableQty) &&
        //        int.TryParse(p.LowStockQuantity, out int lowStockQty) &&
        //        availableQty <= lowStockQty
        //    );

        //    ViewBag.TotalLowStock = totalLowStockCount;
        //    //End Code for Get LowStockCount

        //    var viewModel = new StockInListViewModel
        //    {
        //        UserFkRoleId = (int)userRoleId,
        //        StockIns = getStockInList,
        //        Pagination = new PaginationMetadataViewModel
        //        {
        //            TotalRecords = totalRecords,
        //            CurrentPage = pageNumber,
        //            PageSize = pageSize,
        //            SearchTerm = searchTerm
        //        }
        //    };

        //    return View(viewModel);
        //}


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

                var skuName = _context.TblSkuBarcodes
                    .Where(x => x.IsDeleted == 0 && x.FkProductId == item.FkProductId)
                    .Select(x => x.Skuname)
                    .ToList();

                var product = _context.TblProducts
                    .FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);

                var warehouse = _context.TblWarehouses
                    .FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

                var qtyForSingleItem = _context.TblStockIns
                   .Where(x => x.FkProductId == product.ProductId && x.IsDeleted == false && x.Type == "2")
                   .Sum(x => Convert.ToInt32(x.AvailableQuantity));

                var qtyForBoxItem = _context.TblStockIns
                    .Where(x => x.FkProductId == product.ProductId && x.IsDeleted == false && x.Type == "1")
                    .Sum(x => Convert.ToInt32(x.AvailableQuantity));

                stockInData.Add(new StockInViewModel
                {
                    StockInId = item.StockInId,
                    FkProductId = (int)item.FkProductId,
                    ProductQuantity = product.AvailableProductQty,
                    ProductName = product?.ProductName,
                    //SKUName = product?.SkuIdName,
                    SkuNames = skuName,
                    WarehouseName = warehouse?.Name,
                    AliasNames = aliasNames,
                    QtySingleItem = qtyForSingleItem,
                    QtyBoxItem = qtyForBoxItem
                });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory");

                worksheet.Cell(1, 1).Value = "No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU";
                worksheet.Cell(1, 4).Value = "Single Item Qty";
                worksheet.Cell(1, 5).Value = "Box Item Qty";
                worksheet.Cell(1, 6).Value = "Total Qty";
                worksheet.Cell(1, 7).Value = "Aliases";

                var header = worksheet.Range("A1:G1");
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.Black;
                header.Style.Fill.BackgroundColor = XLColor.Yellow;

                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 12;
                worksheet.Column(7).Width = 40;

                int row = 2;
                int count = 1;
                foreach (var item in stockInData)
                {
                    worksheet.Cell(row, 1).Value = count;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    //worksheet.Cell(row, 3).Value = item.SKUName;

                    if (item.SkuNames != null && item.SkuNames.Any())
                    {
                        string skuText = string.Join(", ", item.SkuNames);
                        var wrappedText = WrapText(skuText, 15);
                        worksheet.Cell(row, 3).Value = wrappedText;
                    }
                    else
                    {
                        worksheet.Cell(row, 3).Value = "-";
                    }

                    worksheet.Cell(row, 4).Value = item.QtySingleItem;
                    worksheet.Cell(row, 5).Value = item.QtyBoxItem;
                    worksheet.Cell(row, 6).Value = item.ProductQuantity;

                    if (item.AliasNames != null && item.AliasNames.Any())
                    {
                        string aliasText = string.Join(", ", item.AliasNames);
                        var wrappedText = WrapText(aliasText, 25);
                        worksheet.Cell(row, 7).Value = wrappedText;
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = "-";
                    }

                    row++;
                    count++;
                }

                worksheet.Column(7).Style.Alignment.WrapText = true;

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
                                                       x.BatchNo.Contains(searchTerm) ||
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
                StockBatchDetails = StockBatchDetails.Where(x => x.Barcode.Contains(searchTerm) ||
                  _context.TblProducts.Any(s => s.IsDeleted == false && s.ProductId == x.FkProductId && s.ProductName.Contains(searchTerm)) ||
                  _context.TblProductAliases.Any(a => a.IsDeleted == false && a.FkProductId == x.FkProductId && a.AliasName.Contains(searchTerm)) ||
                  _context.TblWarehouses.Any(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false && w.Name.Contains(searchTerm))
                );
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
                    SKUName = item?.Barcode,
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

        //[HttpGet]
        //public JsonResult GetProducts(string term)
        //{
        //    var product = _context.TblProducts
        //        .Where(x => x.IsDeleted == false && x.ProductName.Contains(term))
        //        .Select(x => new
        //        {
        //            id = x.ProductId,
        //            productName = x.ProductName
        //        }).ToList();

        //    return Json(product);
        //}

        

        [HttpGet]
        public JsonResult GetProducts(string term)
        {
            var products = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductName.Contains(term))
                .Select(x => new
                {
                    id = x.ProductId,
                    productName = x.ProductName,
                    skuName = _context.TblSkuBarcodes
                        .Where(s => s.FkProductId == x.ProductId)
                        .Select(s => s.Skuname)
                        .FirstOrDefault(),

                    warehouseId = x.FkWarehouseId,
                    warehouseName = _context.TblWarehouses
                    .Where(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false)
                    .Select(w => w.Name)
                    .FirstOrDefault(),
                        rackId = x.FkRackId,
                        rackNo = _context.TblRacks
                    .Where(r => r.RackId == x.FkRackId && r.IsDeleted == 0)
                    .Select(r => r.RackNo)
                    .FirstOrDefault()

                }).ToList();

            return Json(products);
        }

        [HttpGet]
        public JsonResult GetProductBySku(string sku)
        {
            var result = (from skuEntry in _context.TblSkuBarcodes
                          join product in _context.TblProducts on skuEntry.FkProductId equals product.ProductId
                          where skuEntry.Skuname == sku && product.IsDeleted == false
                          select new
                          {
                              productId = product.ProductId,
                              productName = product.ProductName,
                              skuName = skuEntry.Skuname,

                              warehouseId = product.FkWarehouseId,
                              warehouseName = _context.TblWarehouses
                              .Where(w => w.WarehouseId == product.FkWarehouseId && w.IsDeleted == false)
                              .Select(w => w.Name)
                              .FirstOrDefault(),
                                  rackId = product.FkRackId,
                                  rackNo = _context.TblRacks
                              .Where(r => r.RackId == product.FkRackId && r.IsDeleted == 0)
                              .Select(r => r.RackNo)
                              .FirstOrDefault()

                          }).FirstOrDefault();

            return Json(result);
        }


        //[HttpGet]
        //public JsonResult GetSkus(string term)
        //{
        //    var skus = _context.TblSkuBarcodes
        //        .Where(s => s.Skuname.Contains(term))
        //        .Select(s => new
        //        {
        //            skuName = s.Skuname,
        //            productId = s.FkProductId,
        //            productName = _context.TblProducts
        //                .Where(p => p.ProductId == s.FkProductId)
        //                .Select(p => p.ProductName)
        //                .FirstOrDefault()
        //        })
        //        .ToList();

        //    return Json(skus);
        //}

        //[HttpGet]
        //public JsonResult GetProductSkuByProductId(int productId)
        //{
        //    var sku = _context.TblSkuBarcodes
        //        .Where(s => s.FkProductId == productId)
        //        .Select(s => s.Skuname)
        //        .FirstOrDefault();

        //    return Json(new { skuName = sku });
        //}





        [HttpGet]
        public JsonResult GetRackNosByWarehouse(int warehouseId)
        {
            var rackNos = _context.TblRacks
                .Where(r => r.FkWarehouseId == warehouseId && r.IsDeleted == 0)
                .Select(r => new
                {
                    RackNo = r.RackNo
                }).ToList();

            return Json(rackNos);
        }


        [HttpGet]
        public JsonResult GetRackNo(string term)
        {
            var rackNo = _context.TblRacks
                .Where(x => x.IsDeleted == 0 && x.RackNo.Contains(term))
                .Select(x => new
                {
                    id = x.RackId,
                    rackName = x.RackNo
                }).ToList();
            return Json(rackNo);
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


            var getWarehouse = _context.TblWarehouses.Where(x => x.IsDeleted == false).Select(x => new
            {
                Id = x.WarehouseId,
                warehouseName = x.Name
            }).ToList();

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
                    //Date = item.Date,
                    Date = item.Date.Date.Add(DateTime.Now.TimeOfDay),
                    BatchNo = item.BatchNo,
                    FkSupplierId = item.FkSupplierId,
                    FkWarehouseId = item.FkWarehouseId,
                    FkProductId = item.FkProductId,
                    Type = item.Type,
                    TotalBox = item.TotalBox,
                    AvailableBox = item.TotalBox,
                    PerBoxQty = item.PerBoxQty,
                    ProductQuantity = item.ProductQuantity,
                    AvailableQuantity = item.ProductQuantity,
                    Room = item.Room,
                    RackNo = item.RackNo,
                    Barcode = item.Barcode,
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


                //Start Add New
                var product = await _context.TblProducts
                    .FirstOrDefaultAsync(p => p.ProductId == item.FkProductId);

                if (product != null)
                {
                    // Always update WarehouseId
                    product.FkWarehouseId = item.FkWarehouseId;

                    // Find RackId using RackNo
                    if (!string.IsNullOrEmpty(item.RackNo))
                    {
                        var rack = await _context.TblRacks
                            .FirstOrDefaultAsync(r => r.RackNo == item.RackNo && r.FkWarehouseId == item.FkWarehouseId && r.IsDeleted == 0);

                        if (rack != null)
                        {
                            product.FkRackId = rack.RackId;
                        }
                        else
                        {
                            // RackNo not found → set RackId = 0
                            product.FkRackId = 0;
                        }
                    }
                    else
                    {
                        // RackNo empty → set RackId = 0
                        product.FkRackId = 0;
                    }

                    _context.TblProducts.Update(product);
                }
                //End Add New

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





        //[HttpGet]
        //public JsonResult GetProductByBarcodeInStockOut(string sku)
        //{
        //    var result = (from skuEntry in _context.TblStockIns
        //                  join product in _context.TblProducts on skuEntry.FkProductId equals product.ProductId
        //                  where skuEntry.Barcode == sku && product.IsDeleted == false
        //                  select new
        //                  {
        //                      productId = product.ProductId,
        //                      productName = product.ProductName,
        //                      skuName = skuEntry.Barcode
        //                  }).FirstOrDefault();

        //    return Json(result);
        //}


        [HttpGet]
        public JsonResult GetProductsForStockOut(string term)
        {
            var products = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductName.Contains(term))
                .Select(x => new
                {
                    id = x.ProductId,
                    productName = x.ProductName,
                    skuName = _context.TblStockIns
                        .Where(s => s.FkProductId == x.ProductId)
                        .Select(s => s.Barcode)
                        .FirstOrDefault()
                    //.Distinct()
                    //.ToList()
                }).ToList();

            return Json(products);
        }

        [HttpGet]
        public JsonResult GetProductByBarcodeInStockOut(string sku)
        {
            var stocks = _context.TblStockIns
                //.Where(x => EF.Functions.Collate(x.Barcode, "utf8mb4_bin") == sku && x.IsDeleted == false)  //Code for case-sensitive in barcode 
                .Where(x => x.Barcode == sku && x.IsDeleted == false)
                .ToList();

            if (stocks == null || !stocks.Any())
            {
                return Json(null);
            }

            var firstStock = stocks.First();

            var product = _context.TblProducts
                .FirstOrDefault(p => p.ProductId == firstStock.FkProductId && p.IsDeleted == false);

            if (product == null)
            {
                return Json(null);
            }

            var totalAvailableQuantity = stocks.Sum(s => Convert.ToInt32(s.AvailableQuantity));

            var warehouseQuantities = stocks
                .GroupBy(s => new { s.FkWarehouseId, s.RackNo, s.Type })
                .Select(g => new
                {
                    warehouseId = g.Key.FkWarehouseId,
                    rackNo = g.Key.RackNo,
                    type = g.Key.Type,
                    totalQuantity = g.Sum(s => Convert.ToInt32(s.AvailableQuantity))
                }).ToList();

            var result = new
            {
                productId = product.ProductId,
                productName = product.ProductName,
                skuName = firstStock.Barcode,
                stockInQuantity = totalAvailableQuantity,
                warehouseQuantities = warehouseQuantities,
                type = firstStock.Type,
                perBoxQty = firstStock.PerBoxQty
            };

            return Json(result);
        }

        [HttpGet]
        public JsonResult CheckStockAvailability(string sku, int warehouseId, string rackNo)
        {
            bool exists = _context.TblStockIns.Any(x =>
                x.IsDeleted == false &&
                x.Barcode == sku &&
                x.FkWarehouseId == warehouseId &&
                x.RackNo == rackNo);

            return Json(exists);
        }




        [HttpGet]
        public IActionResult GetProductByBarcode(string barcode)
        {

            var stocks = _context.TblStockIns
                                //.Where(x => EF.Functions.Collate(x.Barcode, "utf8mb4_bin") == barcode).ToList(); //Code for case-sensitive in barcode 
                                .Where(x => x.Barcode == barcode).ToList();

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


        //[HttpGet]
        //public IActionResult GetAvailableQuantity(string barcode, int warehouseId)
        //{
        //    var stocks = _context.TblStockIns
        //        .Where(x => x.IsDeleted == false && x.Barcode == barcode && x.FkWarehouseId == warehouseId)
        //        .ToList();

        //    if (stocks == null || !stocks.Any())
        //    {
        //        return Json(0);
        //    }

        //    var totalAvailableQuantity = stocks.Sum(s => Convert.ToInt32(s.AvailableQuantity));
        //    return Json(totalAvailableQuantity);
        //}


        //New 25/07/2025
        //[HttpGet]
        //public JsonResult GetAvailableQuantity(int warehouseId, string rackNo)
        //{
        //    if (string.IsNullOrWhiteSpace(rackNo))
        //    {
        //        return Json(new { success = false, availableQty = 0 });
        //    }

        //    var totalAvailableQty = _context.TblStockIns
        //        .Where(s => s.FkWarehouseId == warehouseId &&
        //                    s.RackNo == rackNo &&
        //                    s.IsDeleted == false)
        //        .Sum(s => Convert.ToInt32(s.AvailableQuantity));

        //    return Json(new { success = true, availableQty = totalAvailableQty });
        //}














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
                if (string.IsNullOrEmpty(item.Barcode) || item.FkProductId == 0 || string.IsNullOrEmpty(item.Quantity) || item.FkWarehouseId == 0)
                {
                    return BadRequest($"Invalid data for stock item: Barcode, ProductId, Quantity, WarehouseId, or RackNo is missing.");
                }

                int stockOutQty = int.TryParse(item.Quantity, out var outQty) ? outQty : 0;
                if (stockOutQty <= 0)
                {
                    return BadRequest($"Invalid quantity for barcode {item.Barcode}.");
                }

                //var stockInList = await _context.TblStockIns
                //    .Where(x => x.Barcode == item.Barcode &&
                //                x.FkWarehouseId == item.FkWarehouseId &&
                //                x.RackNo == item.RackNo &&
                //                x.IsDeleted == false)
                //    .OrderBy(x => x.StockInId)
                //    .ToListAsync();


                var stockInQuery = _context.TblStockIns
           .Where(x => x.Barcode == item.Barcode &&
                       x.FkWarehouseId == item.FkWarehouseId &&
                       x.IsDeleted == false);

                if (!string.IsNullOrEmpty(item.RackNo))
                {
                    stockInQuery = stockInQuery.Where(x => x.RackNo == item.RackNo);
                }

                var stockInList = await stockInQuery
                    .OrderBy(x => x.StockInId)
                    .ToListAsync();


                //if (!stockInList.Any())
                //{
                //    return BadRequest($"No stock found for barcode {item.Barcode} in warehouse {item.FkWarehouseId} and rack {item.RackNo}.");
                //}

                if (!stockInList.Any())
                {
                    return BadRequest($"No stock found for barcode {item.Barcode} in warehouse {item.FkWarehouseId}" +
                                      (string.IsNullOrEmpty(item.RackNo) ? "" : $" and rack {item.RackNo}."));
                }

                var product = await _context.TblProducts
                    .FirstOrDefaultAsync(x => x.ProductId == item.FkProductId && x.IsDeleted == false);

                if (product == null)
                {
                    return BadRequest($"Product with ID {item.FkProductId} not found.");
                }

                int currentProductQty = int.TryParse(product.AvailableProductQty, out var productQty) ? productQty : 0;
                if (currentProductQty < stockOutQty)
                {
                    return BadRequest($"Insufficient product quantity for barcode {item.Barcode}. Available: {currentProductQty}, Requested: {stockOutQty}.");
                }

                int totalAvailableInRack = stockInList.Sum(s => int.TryParse(s.AvailableQuantity, out var qty) ? qty : 0);
                if (totalAvailableInRack < stockOutQty)
                {
                    return BadRequest($"Insufficient stock in rack {item.RackNo} for barcode {item.Barcode}. Available: {totalAvailableInRack}, Requested: {stockOutQty}.");
                }

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

                    // Start This Code For Update the StockIn table "AvailableBox" on 28/07/2025
                    if (stockIn.FkProductId == item.FkProductId &&
                        stockIn.Barcode == item.Barcode &&
                        stockIn.FkWarehouseId == item.FkWarehouseId &&
                        //stockIn.RackNo == item.RackNo &&
                        (string.IsNullOrEmpty(item.RackNo) || stockIn.RackNo == item.RackNo) &&
                        stockIn.Type == "1" &&
                        item.TotalBox > 0)

                    {
                        decimal currentAvailableBox = stockIn.AvailableBox ?? 0;
                        decimal totalBoxToDeduct = item.TotalBox;
                        stockIn.AvailableBox = currentAvailableBox - totalBoxToDeduct;

                        if (stockIn.AvailableBox < 0)
                        {
                            stockIn.AvailableBox = 0;
                        }

                    }
                    // End This Code For Update the StockIn table "AvailableBox" on 28/07/2025

                    _context.TblStockIns.Update(stockIn);

                    if (remainingQtyToDeduct == 0)
                        break;
                }

                product.AvailableProductQty = (currentProductQty - stockOutQty).ToString();
                _context.TblProducts.Update(product);


                var usedStockIn = stockInList.LastOrDefault(x => int.TryParse(x.AvailableQuantity, out var qty) && qty > 0)
                                  ?? stockInList.Last(); 

                var stockOutEntry = new TblStockOut
                {
                    Barcode = item.Barcode,
                    FkProductId = item.FkProductId,
                    Quantity = item.Quantity,
                    Reason = item.Reason,
                    StockOutDate = DateTime.Now,
                    FkStockInId = usedStockIn.StockInId,
                    TotalBox = item.TotalBox,
                    PerBoxQty = item.PerBoxQty,
                    FkWarehouseId = item.FkWarehouseId,
                    //RackNo = item.RackNo,
                    RackNo = string.IsNullOrEmpty(item.RackNo) ? null : item.RackNo,
                    Type = item.Type
                };

                await _context.TblStockOuts.AddAsync(stockOutEntry);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> StockOut([FromBody] List<StockOutViewModel> addStockOut)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    if (addStockOut == null || !addStockOut.Any())
        //    {
        //        return BadRequest("No stock data provided.");
        //    }

        //    foreach (var item in addStockOut)
        //    {
        //        int stockOutQty = int.TryParse(item.Quantity, out var outQty) ? outQty : 0;

        //        //var stockInList = await _context.TblStockIns
        //        //    .Where(x => x.Barcode == item.Barcode.ToString())
        //        //    .OrderBy(x => x.StockInId) 
        //        //    .ToListAsync();

        //        var stockInList = await _context.TblStockIns
        //           .Where(x => x.Barcode == item.Barcode.ToString() && x.FkWarehouseId == item.FkWarehouseId)
        //           .OrderBy(x => x.StockInId)
        //           .ToListAsync();

        //        var product = await _context.TblProducts
        //            .FirstOrDefaultAsync(x => x.ProductId == item.FkProductId);

        //        int currentProductQty = int.TryParse(product.AvailableProductQty, out var productQty) ? productQty : 0;
        //        int remainingQtyToDeduct = stockOutQty;

        //        foreach (var stockIn in stockInList)
        //        {
        //            int currentQty = int.TryParse(stockIn.AvailableQuantity, out var availableQty) ? availableQty : 0;

        //            if (remainingQtyToDeduct >= currentQty)
        //            {
        //                remainingQtyToDeduct -= currentQty;
        //                stockIn.AvailableQuantity = "0";
        //            }
        //            else
        //            {
        //                stockIn.AvailableQuantity = (currentQty - remainingQtyToDeduct).ToString();
        //                remainingQtyToDeduct = 0;
        //            }

        //            _context.TblStockIns.Update(stockIn);

        //            if (remainingQtyToDeduct == 0)
        //                break;
        //        }


        //        product.AvailableProductQty = (currentProductQty - stockOutQty).ToString();
        //        _context.TblProducts.Update(product);

        //        // Save stock-out transaction (linking to the last used stockIn)
        //        var usedStockIn = stockInList.LastOrDefault(x => int.TryParse(x.AvailableQuantity, out var qty) && qty > 0)
        //                          ?? stockInList.Last(); // Fallback if all became 0

        //        var stockOutEntry = new TblStockOut
        //        {
        //            Barcode = item.Barcode,
        //            FkProductId = item.FkProductId,
        //            Quantity = item.Quantity,
        //            Reason = item.Reason,
        //            StockOutDate = DateTime.Now,
        //            FkStockInId = usedStockIn.StockInId
        //        };

        //        await _context.TblStockOuts.AddAsync(stockOutEntry);
        //    }


        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}



        public IActionResult StocksDetails(int id, int productId, string searchTerm = "", int pageNumber = 1, int pageSize = 10, string stockTypeFilter = "All", string itemTypeFilter = "All", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = _context.TblProducts
                .FirstOrDefault(x => x.IsDeleted == false && x.ProductId == productId);

            var skuName = _context.TblSkuBarcodes
                .Where(x => x.IsDeleted == 0 && x.FkProductId == productId)
                .Select(x => x.Skuname)
                .ToList();

            var skuNamesString = string.Join(", ", skuName);

            var aliasList = _context.TblProductAliases
                .Where(x =>x.IsDeleted == false && x.FkProductId == productId)
                .Select(x => x.AliasName)
                .ToList();

            string aliasNamesString = string.Join(", ", aliasList);


            // Total Stock from all available products
            decimal totalStock = _context.TblProducts
                .Where(x => x.IsDeleted == false && x.ProductId == productId && x.AvailableProductQty != null)
                .Sum(x => Convert.ToInt32(x.AvailableProductQty));

            // Total Box Items for this productId where Type == "1"
            int totalBoxItem = (int)_context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId && x.Type == "1" && x.AvailableBox != null)
                .Sum(x => x.AvailableBox);

            int totalBoxAvailableQty = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId && x.Type == "1" && x.AvailableQuantity != null)
                .Sum(x => Convert.ToInt32(x.AvailableQuantity));

            int totalAvailableQtyForSinle = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId && x.Type == "2" && x.AvailableQuantity != null)
                .Sum(x => Convert.ToInt32(x.AvailableQuantity));

            var model = new StockDetailsViewModel
            {
                CombinedStockList = new List<StockCombinedViewModel>(),
                ProductName = product?.ProductName ?? "",
                SKUName = skuNamesString,
                AliasNames = aliasNamesString,
                StockInId = id,
                FkProjectId = productId,
                ProductQty = product?.AvailableProductQty,
                TotalStock = totalStock,
                TotalBoxItem = totalBoxItem,
                TotalBoxAvailableQty = totalBoxAvailableQty,
                TotalAvailableQtyForSingle = totalAvailableQtyForSinle,
                StockTypeFilter = stockTypeFilter,
                ItemTypeFilter = itemTypeFilter,
                FromDate = fromDate,
                ToDate = toDate

            };

            var stockInQuery = _context.TblStockIns
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            var stockOutQuery = _context.TblStockOuts
                .Where(x => x.IsDeleted == false && x.FkProductId == productId);

            //if (!string.IsNullOrEmpty(searchTerm))
            //{
            //    string lowerSearchTerm = searchTerm.ToLower();


            //    stockInQuery = stockInQuery.Where(x =>
            //        (x.Barcode != null && x.Barcode.ToLower().Contains(lowerSearchTerm)) ||
            //        (x.BatchNo != null && x.BatchNo.ToLower().Contains(lowerSearchTerm)) ||
            //        (x.Type != null && x.Type.ToLower().Contains(lowerSearchTerm)) ||
            //        (x.RackNo != null && x.RackNo.ToLower().Contains(lowerSearchTerm)) ||
            //        (_context.TblWarehouses.Any(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false && w.Name.ToLower().Contains(lowerSearchTerm))) ||
            //        (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm)))
            //    );


            //    bool searchStatusStockIn = "stock in".Contains(lowerSearchTerm);
            //    bool searchStatusStockOut = "stock out".Contains(lowerSearchTerm);

            //    stockOutQuery = stockOutQuery.Where(x =>
            //        (x.Reason != null && x.Reason.ToLower().Contains(lowerSearchTerm)) ||

            //        (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm))) ||
            //        (searchStatusStockOut)
            //    );

            //    if (lowerSearchTerm.Contains("stock in") && !lowerSearchTerm.Contains("stock out"))
            //    {
            //        stockOutQuery = stockOutQuery.Where(x => false);
            //    }

            //    if (lowerSearchTerm.Contains("stock out") && !lowerSearchTerm.Contains("stock in"))
            //    {
            //        stockInQuery = stockInQuery.Where(x => false);
            //    }
            //}

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();

                stockInQuery = stockInQuery.Where(x =>
                    (x.Barcode != null && x.Barcode.ToLower().Contains(lowerSearchTerm)) ||
                    (x.BatchNo != null && x.BatchNo.ToLower().Contains(lowerSearchTerm)) ||
                    (x.Type != null && x.Type.ToLower().Contains(lowerSearchTerm)) ||
                    (x.RackNo != null && x.RackNo.ToLower().Contains(lowerSearchTerm)) ||
                    (_context.TblWarehouses.Any(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false && w.Name.ToLower().Contains(lowerSearchTerm))) ||
                    (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm)))
                );

                stockOutQuery = stockOutQuery.Where(x =>
                    (x.Reason != null && x.Reason.ToLower().Contains(lowerSearchTerm)) ||
                    (x.Type != null && x.Type.ToLower().Contains(lowerSearchTerm)) ||
                    (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm))) ||
                    (_context.TblStockIns.Any(si => si.StockInId == x.FkStockInId && si.BatchNo.ToLower().Contains(lowerSearchTerm))) ||
                    (_context.TblStockIns.Any(si => si.StockInId == x.FkStockInId && si.RackNo.ToLower().Contains(lowerSearchTerm))) ||
                    (_context.TblWarehouses.Any(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false && w.Name.ToLower().Contains(lowerSearchTerm)))
                );
            }

            // Apply Stock Type Filter
            if (stockTypeFilter == "StockIn")
            {
                stockOutQuery = stockOutQuery.Where(x => false);
            }
            else if (stockTypeFilter == "StockOut")
            {
                stockInQuery = stockInQuery.Where(x => false);
            }

            if (!string.IsNullOrEmpty(itemTypeFilter))
            {
                itemTypeFilter = itemTypeFilter.Trim();

                if (itemTypeFilter == "Box Item")
                {
                    stockInQuery = stockInQuery.Where(x => x.Type == "1");
                    stockOutQuery = stockOutQuery.Where(x => x.Type == "1");
                }
                else if (itemTypeFilter == "Single Item")
                {
                    stockInQuery = stockInQuery.Where(x => x.Type == "2");
                    stockOutQuery = stockOutQuery.Where(x => x.Type == "2");
                }
            }


            // Apply Date Filter
            if (fromDate.HasValue && toDate.HasValue)
            {
                DateTime from = fromDate.Value.Date;
                DateTime to = toDate.Value.Date.AddDays(1).AddTicks(-1);

                stockInQuery = stockInQuery.Where(x => x.Date >= from && x.Date <= to);
                stockOutQuery = stockOutQuery.Where(x => x.StockOutDate >= from && x.StockOutDate <= to);
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
                    Type = item.Type,
                    TotalBox = item.TotalBox ?? 0,
                    PerBoxQty = item.PerBoxQty ?? 0

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
                    //Type = Type,
                    Type = item.Type,
                    TotalBox = (decimal)item.TotalBox,
                    PerBoxQty = (decimal)item.PerBoxQty
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



        //    public IActionResult StocksDetails(int id, int productId, string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        //    {
        //        var userId = HttpContext.Session.GetInt32("userId");

        //        if (userId == null || userId == 0)
        //        {
        //            return RedirectToAction("Login", "Auth");
        //        }

        //        var product = _context.TblProducts
        //.            FirstOrDefault(x => x.IsDeleted == false && x.ProductId == productId);

        //        var aliasList = _context.TblProductAliases
        //            .Where(x => x.FkProductId == productId)
        //            .Select(x => x.AliasName)
        //            .ToList();

        //        string aliasNamesString = string.Join(", ", aliasList);


        //        // Total Stock from all available products
        //        decimal totalStock = _context.TblProducts
        //            .Where(x => x.IsDeleted == false && x.ProductId == productId && x.AvailableProductQty != null)
        //            .Sum(x => Convert.ToInt32(x.AvailableProductQty));

        //        // Total Box Items for this productId where Type == "1"
        //        int totalBoxItem = (int)_context.TblStockIns
        //            .Where(x => x.IsDeleted == false && x.FkProductId == productId && x.Type == "1" && x.AvailableBox != null)
        //            .Sum(x => x.AvailableBox);


        //        var model = new StockDetailsViewModel
        //        {
        //            CombinedStockList = new List<StockCombinedViewModel>(),
        //            ProductName = product?.ProductName ?? "",
        //            SKUName = product?.SkuIdName ?? "",
        //            AliasNames = aliasNamesString,
        //            StockInId = id,
        //            FkProjectId = productId,
        //            ProductQty = product?.AvailableProductQty,
        //            TotalStock = totalStock,
        //            TotalBoxItem = totalBoxItem

        //        };

        //        var stockInQuery = _context.TblStockIns
        //            .Where(x => x.IsDeleted == false && x.FkProductId == productId);

        //        var stockOutQuery = _context.TblStockOuts
        //            .Where(x => x.IsDeleted == false && x.FkProductId == productId);

        //        if (!string.IsNullOrEmpty(searchTerm))
        //        {
        //            string lowerSearchTerm = searchTerm.ToLower();


        //            stockInQuery = stockInQuery.Where(x =>
        //                (x.Barcode != null && x.Barcode.ToLower().Contains(lowerSearchTerm)) ||
        //                (x.BatchNo != null && x.BatchNo.ToLower().Contains(lowerSearchTerm)) ||
        //                (x.Type != null && x.Type.ToLower().Contains(lowerSearchTerm)) ||
        //                (x.RackNo != null && x.RackNo.ToLower().Contains(lowerSearchTerm)) ||
        //                (_context.TblWarehouses.Any(w => w.WarehouseId == x.FkWarehouseId && w.IsDeleted == false && w.Name.ToLower().Contains(lowerSearchTerm))) ||
        //                (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm)))
        //            );


        //            bool searchStatusStockIn = "stock in".Contains(lowerSearchTerm);
        //            bool searchStatusStockOut = "stock out".Contains(lowerSearchTerm);

        //            stockOutQuery = stockOutQuery.Where(x =>
        //                (x.Reason != null && x.Reason.ToLower().Contains(lowerSearchTerm)) ||

        //                (_context.TblProducts.Any(p => p.ProductId == x.FkProductId && p.ProductName.ToLower().Contains(lowerSearchTerm))) ||
        //                (searchStatusStockOut)  
        //            );

        //            if (lowerSearchTerm.Contains("stock in") && !lowerSearchTerm.Contains("stock out"))
        //            {
        //                stockOutQuery = stockOutQuery.Where(x => false); 
        //            }

        //            if (lowerSearchTerm.Contains("stock out") && !lowerSearchTerm.Contains("stock in"))
        //            {
        //                stockInQuery = stockInQuery.Where(x => false); 
        //            }
        //        }

        //        var stockInList = stockInQuery.ToList();
        //        var stockOutList = stockOutQuery.ToList();

        //        foreach (var item in stockInList)
        //        {
        //            var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
        //            var getSupplier = _context.TblSuppliers.FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == item.FkSupplierId);
        //            var getLocation = _context.TblWarehouses.FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == item.FkWarehouseId);

        //            model.CombinedStockList.Add(new StockCombinedViewModel
        //            {
        //                Id = item.StockInId,
        //                Date = item.Date ?? DateTime.MinValue,
        //                CreatedAt = item.CreatedAt ?? DateTime.MinValue,
        //                BatchNo = item.BatchNo ?? null,
        //                Quantity = item.ProductQuantity,
        //                ProductName = getProduct?.ProductName,
        //                SupplierName = getSupplier?.SupplierName,
        //                LocationName = getLocation?.Name,
        //                Status = "Stock In",
        //                Reason = "-",
        //                RoomName = item.Room,
        //                RackName = item.RackNo,
        //                Type = item.Type,
        //                TotalBox = (decimal)item.TotalBox,
        //                PerBoxQty = (decimal)item.PerBoxQty

        //            });
        //        }

        //        foreach (var item in stockOutList)
        //        {
        //            var getProduct = _context.TblProducts.FirstOrDefault(x => x.IsDeleted == false && x.ProductId == item.FkProductId);
        //            var getStockInData = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.StockInId == item.FkStockInId);

        //            string supplierName = "";
        //            string locationName = "";
        //            string batchNo = "";
        //            string roomName = "";
        //            string rackName = "";
        //            string Type = "";

        //            if (getStockInData != null)
        //            {

        //                var getSupplier = _context.TblSuppliers
        //                    .FirstOrDefault(x => x.IsDeleted == false && x.SupplierId == getStockInData.FkSupplierId);

        //                var getLocation = _context.TblWarehouses
        //                    .FirstOrDefault(x => x.IsDeleted == false && x.WarehouseId == getStockInData.FkWarehouseId);

        //                supplierName = getSupplier?.SupplierName ?? "";
        //                locationName = getLocation?.Name ?? "";
        //                batchNo = getStockInData.BatchNo ?? "";
        //                roomName = getStockInData.Room ?? "";
        //                rackName = getStockInData.RackNo ?? "";
        //                Type = getStockInData.Type ?? "";
        //            }

        //            model.CombinedStockList.Add(new StockCombinedViewModel
        //            {
        //                Id = item.StockOutId,
        //                Date = item.StockOutDate ?? DateTime.MinValue,
        //                CreatedAt = item.CreatedAt ?? DateTime.MinValue,
        //                BatchNo = batchNo,
        //                Quantity = item.Quantity,
        //                ProductName = getProduct?.ProductName,
        //                //SupplierName = supplierName,
        //                SupplierName = "-",
        //                LocationName = locationName,
        //                Status = "Stock Out",
        //                Reason = item.Reason,
        //                RoomName = roomName,
        //                RackName = rackName,
        //                Type = Type,
        //                TotalBox = (decimal)item.TotalBox,
        //                PerBoxQty = (decimal)item.PerBoxQty
        //            });
        //        }

        //        var sortedList = model.CombinedStockList.OrderByDescending(x => x.CreatedAt).ToList();

        //        int totalRecords = sortedList.Count;
        //        model.CombinedStockList = sortedList
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToList();

        //        model.Pagination = new PaginationMetadataViewModel
        //        {
        //            TotalRecords = totalRecords,
        //            CurrentPage = pageNumber,
        //            PageSize = pageSize,
        //            SearchTerm = searchTerm
        //        };

        //        return View(model);
        //    }







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
                //worksheet.Cell(1, 6).Value = "Supplier Name";
                worksheet.Cell(1, 6).Value = "Quantity";
                worksheet.Cell(1, 7).Value = "Location";

                // Style header
                var header = worksheet.Range("A1:G1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.Yellow;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Set column widths
                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                //worksheet.Column(6).Width = 25;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 30;

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
                    //worksheet.Cell(row, 6).Value = item.SupplierName;
                    worksheet.Cell(row, 6).Value = item.Quantity;
                    //worksheet.Cell(row, 8).Value = $"{item.LocationName} / {item.RoomName} / {item.RackName}";
                    worksheet.Cell(row, 7).Value = $"{item.LocationName}";

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
                saleOrderList = saleOrderList.Where(x => x.OrderProductQty.Contains(searchTerm) ||
                 _context.TblProducts.Any(w => w.ProductId == x.FkProductId && w.IsDeleted == false && w.ProductName.Contains(searchTerm))
                );
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
                var skuName = _context.TblSkuBarcodes.FirstOrDefault(x => x.IsDeleted == 0 && x.SkuId == item.FkSkuId);
                var StockIn = _context.TblStockIns.FirstOrDefault(x => x.IsDeleted == false && x.FkProductId == item.FkProductId);

                var getType = _context.TblStockIns.Where(x => x.IsDeleted == false && x.Barcode == item.ProductName)
                    .Select(x => x.Type).FirstOrDefault();

                // Sum AvailableQuantity from StockIns with same Barcode and Type
                var getAvailableQty = _context.TblStockIns
                    .Where(x => x.IsDeleted == false
                             && x.Barcode == item.ProductName
                             && x.Type == getType)
                    .Sum(x => Convert.ToInt32(x.AvailableQuantity));

                int orderQty = int.TryParse(item.OrderProductQty, out var oq) ? oq : 0;
                //int availableQty = int.TryParse(product?.AvailableProductQty, out var aq) ? aq : 0;
                int availableQty = getAvailableQty;

                string status = availableQty >= orderQty ? "Available" : "Not Available";

                getSaleOrderList.Add(new SaleOrderViewModel
                {
                    OrderId = item.OrderId,
                    OrderNumber = item.OrderNumber,
                    ProductName = product.ProductName,
                    //SKUName = item.ProductName,
                    SKUName = skuName?.Skuname,
                    OrderQuantity = item.OrderProductQty,
                    //AvailableQuantity = product?.AvailableProductQty ?? "0",
                    AvailableQuantity = Convert.ToString(getAvailableQty),
                    OrderDate = (DateTime)item.OrderDate,
                    Status = status,
                    Type = getType
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
                var skuName = _context.TblSkuBarcodes.FirstOrDefault(x => x.IsDeleted == 0 && x.SkuId == item.FkSkuId);
                var aliasName = _context.TblProductAliases.Where(x => x.IsDeleted == false && x.FkProductId == item.FkProductId).Select(x => x.AliasName).ToList();

                int orderQty = int.TryParse(item.OrderProductQty, out var oq) ? oq : 0;
                int availableQty = int.TryParse(product?.AvailableProductQty, out var aq) ? aq : 0;

                string status = availableQty >= orderQty ? "Available" : "Not Available";

                saleOrderDetails.Add(new SaleOrderViewModel
                {
                    OrderId = item.OrderId,
                    ProductName = product?.ProductName,
                    //SKUName = product?.SkuIdName,
                    SKUName = skuName?.Skuname,
                    OrderQuantity = item.OrderProductQty,
                    AvailableQuantity = product?.AvailableProductQty ?? "0",
                    OrderDate = (DateTime)item.OrderDate,
                    Status = status,
                    AliasName = aliasName
                });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SaleOrderDetails");

                // Headers
                worksheet.Cell(1, 1).Value = "Sr.No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU Name";
                worksheet.Cell(1, 4).Value = "Alias Name";
                worksheet.Cell(1, 5).Value = "Order Quantity";
                worksheet.Cell(1, 6).Value = "Available Quantity";
                worksheet.Cell(1, 7).Value = "Order Date";
                worksheet.Cell(1, 8).Value = "Status";
               

                // Header styling
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Column Widths
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 50;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
               

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
                    worksheet.Cell(row, 4).Value = string.Join(", ", item.AliasName);
                    worksheet.Cell(row, 5).Value = item.OrderQuantity;
                    worksheet.Cell(row, 6).Value = item.AvailableQuantity;
                    worksheet.Cell(row, 7).Value = item.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 8).Value = item.Status;
                    
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
                            string skuName = worksheet.Cell(row, 2).GetString().Trim();
                            string productQty = worksheet.Cell(row, 3).GetString().Trim();
                            string orderDateString = worksheet.Cell(row, 4).GetString().Trim();

                            if (string.IsNullOrEmpty(orderNumber) || string.IsNullOrEmpty(skuName) || string.IsNullOrEmpty(productQty) || string.IsNullOrEmpty(orderDateString))
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

                            //var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);
                            var skuBarcode = _context.TblSkuBarcodes.FirstOrDefault(p => p.Skuname == skuName);

                            if (skuBarcode != null)
                            {
                                if (existingSaleOrder != null)
                                {
                                    existingSaleOrder.OrderNumber = orderNumber;
                                    existingSaleOrder.ProductName = skuName;
                                    existingSaleOrder.OrderProductQty = productQty;
                                    existingSaleOrder.OrderDate = orderDate;
                                    existingSaleOrder.UpdatedAt = DateTime.Now;
                                    existingSaleOrder.FkSkuId = skuBarcode.SkuId;
                                    existingSaleOrder.FkProductId = skuBarcode.FkProductId;

                                    saleOrderToUpdate.Add(existingSaleOrder);

                                }
                                else
                                {
                                    var newSaleOrder = new TblSaleOrder
                                    {
                                        OrderNumber = orderNumber,
                                        ProductName = skuName,
                                        OrderProductQty = productQty,
                                        OrderDate = orderDate,
                                        CreatedAt = DateTime.Now,
                                        FkSkuId = skuBarcode.SkuId,
                                        FkProductId = skuBarcode.FkProductId
                                    };

                                    saleOrderToInsert.Add(newSaleOrder);
                                }

                            }
                            else
                            {

                                TempData["ErrorMessage"] = $"SkuName '{skuName}' not found. Please enter a correct sku name.";
                                //return RedirectToAction("SalesOrder");
                                invalidProducts.Add(skuName);
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

        //    List<string> invalidProducts = new List<string>();
        //    List<TblSaleOrder> saleOrderToInsert = new List<TblSaleOrder>();
        //    List<TblSaleOrder> saleOrderToUpdate = new List<TblSaleOrder>();

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

        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);

        //                    if (product != null)
        //                    {
        //                        if (existingSaleOrder != null)
        //                        {
        //                            existingSaleOrder.OrderNumber = orderNumber;
        //                            existingSaleOrder.ProductName = productName;
        //                            existingSaleOrder.OrderProductQty = productQty;
        //                            existingSaleOrder.OrderDate = orderDate;
        //                            existingSaleOrder.UpdatedAt = DateTime.Now;
        //                            existingSaleOrder.FkProductId = product.ProductId;

        //                            saleOrderToUpdate.Add(existingSaleOrder);

        //                            //if (product != null)
        //                            //{
        //                            //    existingSaleOrder.FkProductId = product.ProductId;
        //                            //    saleOrderToUpdate.Add(existingSaleOrder);
        //                            //}
        //                            //else
        //                            //{
        //                            //    TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
        //                            //    return RedirectToAction("SalesOrder");
        //                            //}

        //                        }
        //                        else
        //                        {
        //                            var newSaleOrder = new TblSaleOrder
        //                            {
        //                                OrderNumber = orderNumber,
        //                                ProductName = productName,
        //                                OrderProductQty = productQty,
        //                                OrderDate = orderDate,
        //                                CreatedAt = DateTime.Now,
        //                                FkProductId = product.ProductId
        //                            };

        //                            saleOrderToInsert.Add(newSaleOrder);

        //                            //var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == productName);
        //                            //if (product != null)
        //                            //{
        //                            //    newSaleOrder.FkProductId = product.ProductId;
        //                            //    saleOrderToInsert.Add(newSaleOrder);
        //                            //}
        //                            //else
        //                            //{
        //                            //    TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
        //                            //    return RedirectToAction("SalesOrder");
        //                            //}
        //                        }

        //                    }
        //                    else
        //                    {

        //                        TempData["ErrorMessage"] = $"Product '{productName}' not found. Please enter a correct product name.";
        //                        //return RedirectToAction("SalesOrder");
        //                        invalidProducts.Add(productName);
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







        //[HttpPost]
        //public async Task<ActionResult> AddProduct(ProductViewModel addProduct)
        //{
        //    var userId = HttpContext.Session.GetInt32("userId");

        //    if (userId == null || userId == 0)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }

        //    var product = new TblProduct
        //    {
        //        ProductName = addProduct.ProductName,
        //        //SkuIdName = addProduct.SkuIdName,
        //        LowStockQuantity = addProduct.LowStockQuantity,
        //        FkWarehouseId = addProduct.WarehouseId,
        //        FkRackId = addProduct.RackId,
        //        IsDeleted = false,
        //        CreatedAt = DateTime.Now,
        //    };

        //    _context.TblProducts.Add(product);
        //    await _context.SaveChangesAsync();

        //    var getProductId = product.ProductId;

        //    var skuEntries = new List<TblSkuBarcode>();

        //    if (!string.IsNullOrWhiteSpace(addProduct.SkuForSignleItem))
        //    {
        //        skuEntries.Add(new TblSkuBarcode
        //        {
        //            FkProductId = getProductId,
        //            Skuname = addProduct.SkuForSignleItem,
        //            IsDeleted = 0,
        //            CreatedAt = DateTime.Now,
        //            CreatedBy = userId
        //        });
        //    }

        //    if (!string.IsNullOrWhiteSpace(addProduct.SkuForBox))
        //    {
        //        skuEntries.Add(new TblSkuBarcode
        //        {
        //            FkProductId = getProductId,
        //            Skuname = addProduct.SkuForBox,
        //            IsDeleted = 0,
        //            CreatedAt = DateTime.Now,
        //            CreatedBy = userId
        //        });
        //    }

        //    if (skuEntries.Any())
        //    {
        //        _context.TblSkuBarcodes.AddRange(skuEntries);
        //    }

        //    var getId = product.ProductId;

        //    if (addProduct.AliasNames != null && addProduct.AliasNames.Any())
        //    {
        //        foreach (var alias in addProduct.AliasNames)
        //        {
        //            if (!string.IsNullOrWhiteSpace(alias))
        //            {

        //                var aliasData = new TblProductAlias
        //                {
        //                    FkProductId = getId,
        //                    AliasName = alias,
        //                    IsDeleted = false,
        //                    CreatedAt = DateTime.Now
        //                };

        //                _context.TblProductAliases.Add(aliasData);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //    }



        //    return RedirectToAction("StockIn");
        //}





        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel addProduct)
        {
            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null || userId == 0)
                return Json(new { success = false, message = "Please login first." });

            // ✅ DUPLICATE VALIDATION
            bool productExists = await _context.TblProducts
                .AnyAsync(p => p.ProductName.ToLower() == addProduct.ProductName.ToLower() && p.IsDeleted == false);

            if (productExists)
                return Json(new { success = false, message = "Product Name already exists! Please use a different name." });

            if (!string.IsNullOrWhiteSpace(addProduct.SkuForSignleItem))
            {
                bool singleSkuExists = await _context.TblSkuBarcodes
                    .AnyAsync(s => s.Skuname.ToLower() == addProduct.SkuForSignleItem.ToLower() && s.IsDeleted == 0);

                if (singleSkuExists)
                    return Json(new { success = false, message = "SKU for Single Item already exists!" });
            }

            if (!string.IsNullOrWhiteSpace(addProduct.SkuForBox))
            {
                bool boxSkuExists = await _context.TblSkuBarcodes
                    .AnyAsync(s => s.Skuname.ToLower() == addProduct.SkuForBox.ToLower() && s.IsDeleted == 0);

                if (boxSkuExists)
                    return Json(new { success = false, message = "SKU for Box already exists!" });
            }

            // ✅ Save Product
            var product = new TblProduct
            {
                ProductName = addProduct.ProductName,
                LowStockQuantity = addProduct.LowStockQuantity,
                FkWarehouseId = addProduct.WarehouseId,
                FkRackId = addProduct.RackId,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                AvailableProductQty = "0"
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
                _context.TblSkuBarcodes.AddRange(skuEntries);

            if (addProduct.AliasNames != null && addProduct.AliasNames.Any())
            {
                foreach (var alias in addProduct.AliasNames)
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        _context.TblProductAliases.Add(new TblProductAlias
                        {
                            FkProductId = getProductId,
                            AliasName = alias,
                            IsDeleted = false,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(addProduct.SkuForSignleItem))
            {
                var getRackNo = _context.TblRacks.FirstOrDefault(x => x.IsDeleted == 0 && x.RackId == product.FkRackId).RackNo;
                var stockIn = new TblStockIn
                {

                    BatchNo = addProduct.BatchNo,
                    Date = DateTime.Now,
                    FkSupplierId = 1,
                    FkWarehouseId = addProduct.WarehouseId,
                    FkProductId = getProductId,
                    Type = "2",
                    ProductQuantity = "0",
                    Price = 0,
                    AvailableQuantity = "0",
                    RackNo = getRackNo,
                    Barcode = addProduct.SkuForSignleItem,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now

                };
                _context.TblStockIns.Add(stockIn);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product added successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNewBatchNo()
        {
            string batchNumber;
            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);

                batchNumber = parameters.Get<string>("@newBatchNo");
            }

            return Json(new { batchNo = batchNumber });
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
        //    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
        //        await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
        //        batchNumber = parameters.Get<string>("@newBatchNo");
        //    }

        //    List<TblStockIn> stockInList = new();
        //    List<string> failedRecords = new();

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
        //                    string rackNo = worksheet.Cell(row, 11).GetString().Trim();

        //                    if (string.IsNullOrWhiteSpace(itemName))
        //                        continue;

        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
        //                    if (product == null)
        //                    {
        //                        failedRecords.Add($"Row {row}: Product '{itemName}' not found");
        //                        continue;
        //                    }

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
        //                        ProductQuantity = qty.ToString(),
        //                        AvailableQuantity = qty.ToString(),
        //                        Date = DateTime.Now,
        //                        Type = "2",
        //                        FkWarehouseId = 1,
        //                        FkSupplierId = 1,
        //                        BatchNo = batchNumber,
        //                        RackNo = rackNo
        //                    };

        //                    stockInList.Add(stockIn);
        //                }

        //                if (stockInList.Any())
        //                {
        //                    _context.TblStockIns.AddRange(stockInList);
        //                    await _context.SaveChangesAsync();
        //                }

        //                TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
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
                            try
                            {
                                string itemName = worksheet.Cell(row, 1).GetString().Trim();
                                string itemCode = worksheet.Cell(row, 2).GetString().Trim();
                                string priceText = worksheet.Cell(row, 5).GetString().Trim();
                                string qtyText = worksheet.Cell(row, 9).GetString().Trim();
                                string minumumQty = worksheet.Cell(row, 10).GetString().Trim();
                                string roomName = worksheet.Cell(row, 11).GetString().Trim();
                                string rackNo = worksheet.Cell(row, 12).GetString().Trim();

                                if (string.IsNullOrWhiteSpace(itemName))
                                    continue;

                                if (string.IsNullOrWhiteSpace(itemCode))
                                {
                                    failedRecords.Add($"Row {row}: SKUName is not available.");
                                    continue;
                                }

                                if (!itemCode.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                {
                                    failedRecords.Add($"Row {row}: Invalid SKU '{itemCode}'. Please add correct SKUName e.g. 'A001' (must start with 'A').");
                                    continue;
                                }

                                // ✅ If minQty empty -> set to "0"
                                if (string.IsNullOrWhiteSpace(minumumQty))
                                    minumumQty = "0";

                                var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
                                if (product == null)
                                {
                                    //Strt Code For Duplicate SKU Name not Save
                                    bool skuExistsInOtherProduct = _context.TblSkuBarcodes.Any(s => s.Skuname == itemCode);

                                    if (skuExistsInOtherProduct)
                                    {
                                        // ❌ Duplicate found → skip this record only
                                        failedRecords.Add($"Row {row}: SKU '{itemCode}' already exists in another product.");
                                        continue; // 👉 Move to next record
                                    }
                                    //Strt Code For Duplicate SKU Name not Save

                                    product = new TblProduct
                                    {
                                        ProductName = itemName,
                                        LowStockQuantity = minumumQty,
                                        AvailableProductQty = "0",
                                        CreatedAt = DateTime.Now,
                                        IsDeleted = false
                                    };
                                    _context.TblProducts.Add(product);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    //Strt Code For Duplicate SKU Name not Save
                                    bool skuExistsForOtherProduct = _context.TblSkuBarcodes
                                        .Any(s => s.Skuname == itemCode && s.FkProductId != product.ProductId);

                                    if (skuExistsForOtherProduct)
                                    {
                                        failedRecords.Add($"Row {row}: SKU '{itemCode}' already belongs to another product, cannot save StockIn.");
                                        continue; // skip only this row
                                    }
                                    //Strt Code For Duplicate SKU Name not Save
                                }

                                // ✅ Alias check
                                bool aliasExists =
                                    _context.TblProductAliases.Any(a => a.FkProductId == product.ProductId && a.AliasName == itemName)
                                    || _context.ChangeTracker.Entries<TblProductAlias>()
                                        .Any(e => e.Entity.FkProductId == product.ProductId &&
                                                  e.Entity.AliasName == itemName &&
                                                  e.State == EntityState.Added);

                                if (!aliasExists)
                                {
                                    var alias = new TblProductAlias
                                    {
                                        FkProductId = product.ProductId,
                                        AliasName = itemName
                                    };
                                    _context.TblProductAliases.Add(alias);
                                }

                                // ✅ SKU check
                                //bool skuExists = _context.TblSkuBarcodes
                                //    .Any(s => s.FkProductId == product.ProductId && s.Skuname == itemCode)
                                //    || _context.ChangeTracker.Entries<TblSkuBarcode>()
                                //       .Any(e => e.Entity.FkProductId == product.ProductId && e.Entity.Skuname == itemCode && e.State == EntityState.Added);

                                bool skuExists = _context.TblSkuBarcodes
                                    .Any(s => s.Skuname == itemCode)
                                    || _context.ChangeTracker.Entries<TblSkuBarcode>()
                                       .Any(e => e.Entity.Skuname == itemCode && e.State == EntityState.Added);

                                if (!skuExists)
                                {
                                    var sku = new TblSkuBarcode
                                    {
                                        FkProductId = product.ProductId,
                                        Skuname = itemCode
                                    };
                                    _context.TblSkuBarcodes.Add(sku);
                                }

                                // ✅ Warehouse check
                                TblWarehouse warehouse = null;
                                if (!string.IsNullOrWhiteSpace(roomName))
                                {
                                    warehouse = _context.TblWarehouses
                                        .FirstOrDefault(w => w.Name == roomName && w.IsDeleted == false);

                                    if (warehouse == null)
                                    {
                                        warehouse = new TblWarehouse
                                        {
                                            Name = roomName,
                                            IsDeleted = false,
                                            CreatedAt = DateTime.Now
                                        };
                                        _context.TblWarehouses.Add(warehouse);
                                        await _context.SaveChangesAsync();
                                    }
                                }

                                int warehouseId = warehouse?.WarehouseId ?? 1;

                                // ✅ Rack check
                                TblRack rack = null;
                                if (!string.IsNullOrWhiteSpace(rackNo))
                                {
                                    rack = _context.TblRacks
                                        .FirstOrDefault(r => r.RackNo == rackNo && r.FkWarehouseId == warehouseId && r.IsDeleted == 0);

                                    if (rack == null)
                                    {
                                        rack = new TblRack
                                        {
                                            FkWarehouseId = warehouseId,
                                            RackNo = rackNo,
                                            IsDeleted = 0,
                                            CreatedAt = DateTime.Now
                                        };
                                        _context.TblRacks.Add(rack);
                                        await _context.SaveChangesAsync();
                                    }
                                }

                                if (product != null)
                                {
                                    product.FkWarehouseId = warehouseId;
                                    product.FkRackId = (int)(rack?.RackId); 
                                    _context.TblProducts.Update(product);
                                    await _context.SaveChangesAsync();
                                }

                                // ✅ If empty → default 0
                                decimal price = 0;
                                decimal.TryParse(priceText, out price);

                                int qty = 0;
                                int.TryParse(qtyText, out qty);

                                int currentQty = 0;
                                int.TryParse(product.AvailableProductQty, out currentQty);

                                int updatedQty = currentQty + qty;
                                product.AvailableProductQty = updatedQty.ToString();

                                // 🚨 Skip if RackNo is empty
                                if (string.IsNullOrWhiteSpace(rackNo))
                                {
                                    failedRecords.Add($"Row {row}: RackNo is required but missing.");
                                    continue; 
                                }


                                //if (qty > 0)
                                //{
                                    TblStockIn stockIn = new()
                                    {
                                        FkProductId = product.ProductId,
                                        Barcode = itemCode,
                                        Price = price,
                                        ProductQuantity = qty.ToString(),
                                        AvailableQuantity = qty.ToString(),
                                        Date = DateTime.Now,
                                        Type = "2",
                                        FkWarehouseId = warehouseId,
                                        FkSupplierId = 1,
                                        BatchNo = batchNumber,
                                        RackNo = rackNo
                                    };

                                    stockInList.Add(stockIn);
                                //}

                                
                            }
                            catch (Exception rowEx)
                            {
                                failedRecords.Add($"Row {row}: {rowEx.Message}");
                                continue; 
                            }
                        }

                        if (stockInList.Any())
                        {
                            _context.TblStockIns.AddRange(stockInList);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
                        }

                        if (failedRecords.Any())
                        {
                            TempData["ErrorMessage"] =
                                $"{failedRecords.Count} records failed.<br/>{string.Join("<br/>", failedRecords)}";
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


        //Old Code
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
        //    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
        //        await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
        //        batchNumber = parameters.Get<string>("@newBatchNo");
        //    }

        //    List<TblStockIn> stockInList = new();
        //    List<string> failedRecords = new();

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
        //                    string minumumQty = worksheet.Cell(row, 10).GetString().Trim();
        //                    string roomName = worksheet.Cell(row, 11).GetString().Trim();
        //                    string rackNo = worksheet.Cell(row, 12).GetString().Trim();

        //                    if (string.IsNullOrWhiteSpace(itemName))
        //                        continue;

        //                    if (string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        failedRecords.Add($"Row {row}: SKUName is not available.");
        //                        continue; 
        //                    }


        //                    if (!string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        if (!itemCode.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            failedRecords.Add($"Row {row}: Invalid SKU '{itemCode}'. Please add correct SKUName e.g. 'A001' (must start with 'A').");
        //                            continue; 
        //                        }
        //                    }

        //                    if (string.IsNullOrWhiteSpace(minumumQty))
        //                        minumumQty = "0";

        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
        //                    if (product == null)
        //                    {
        //                        // Create new product
        //                        product = new TblProduct
        //                        {
        //                            ProductName = itemName,
        //                            LowStockQuantity = minumumQty,
        //                            AvailableProductQty = "0", 
        //                            CreatedAt = DateTime.Now,
        //                            IsDeleted = false
        //                        };
        //                        _context.TblProducts.Add(product);
        //                        await _context.SaveChangesAsync(); 
        //                    }



        //                    bool aliasExists =
        //                    _context.TblProductAliases.Any(a => a.FkProductId == product.ProductId && a.AliasName == itemName)
        //                    || _context.ChangeTracker.Entries<TblProductAlias>()
        //                        .Any(e => e.Entity.FkProductId == product.ProductId &&
        //                                  e.Entity.AliasName == itemName &&
        //                                  e.State == EntityState.Added);

        //                    if (!aliasExists)
        //                    {
        //                        var alias = new TblProductAlias
        //                        {
        //                            FkProductId = product.ProductId,
        //                            AliasName = itemName
        //                        };
        //                        _context.TblProductAliases.Add(alias);
        //                    }


        //                    if (!string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        // Save SKU if not already exists
        //                        bool skuExists = _context.TblSkuBarcodes
        //                        .Any(s => s.FkProductId == product.ProductId && s.Skuname == itemCode)
        //                        || _context.ChangeTracker.Entries<TblSkuBarcode>()
        //                           .Any(e => e.Entity.FkProductId == product.ProductId && e.Entity.Skuname == itemCode && e.State == EntityState.Added);

        //                        if (!skuExists)
        //                        {
        //                            var sku = new TblSkuBarcode
        //                            {
        //                                FkProductId = product.ProductId,
        //                                Skuname = itemCode
        //                            };
        //                            _context.TblSkuBarcodes.Add(sku);
        //                        }

        //                    }

        //                    // 🔹 4. Warehouse check/create (Room column)
        //                    TblWarehouse warehouse = null;
        //                    if (!string.IsNullOrWhiteSpace(roomName))
        //                    {
        //                        warehouse = _context.TblWarehouses
        //                            .FirstOrDefault(w => w.Name == roomName && w.IsDeleted == false);

        //                        if (warehouse == null)
        //                        {
        //                            warehouse = new TblWarehouse
        //                            {
        //                                Name = roomName,
        //                                IsDeleted = false,
        //                                CreatedAt = DateTime.Now
        //                            };
        //                            _context.TblWarehouses.Add(warehouse);
        //                            await _context.SaveChangesAsync(); 
        //                        }
        //                    }

        //                    int warehouseId = warehouse?.WarehouseId ?? 1;

        //                    // ✅ Check Rack
        //                    TblRack rack = null;
        //                    if (!string.IsNullOrWhiteSpace(rackNo))
        //                    {
        //                        rack = _context.TblRacks
        //                            .FirstOrDefault(r => r.RackNo == rackNo && r.FkWarehouseId == warehouseId && r.IsDeleted == 0);

        //                        if (rack == null)
        //                        {
        //                            rack = new TblRack
        //                            {
        //                                FkWarehouseId = warehouseId,
        //                                RackNo = rackNo,
        //                                IsDeleted = 0,
        //                                CreatedAt = DateTime.Now
        //                            };
        //                            _context.TblRacks.Add(rack);
        //                            await _context.SaveChangesAsync(); 
        //                        }
        //                    }


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
        //                        ProductQuantity = qty.ToString(),
        //                        AvailableQuantity = qty.ToString(),
        //                        Date = DateTime.Now,
        //                        Type = "2",
        //                        FkWarehouseId = warehouseId,
        //                        FkSupplierId = 1,
        //                        BatchNo = batchNumber,
        //                        RackNo = rackNo
        //                    };

        //                    stockInList.Add(stockIn);
        //                }

        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] =
        //                        $"{failedRecords.Count} records failed. <br/>{string.Join("<br/>", failedRecords)}";

        //                    return RedirectToAction("InventoryList"); 
        //                }

        //                if (stockInList.Any())
        //                {
        //                    _context.TblStockIns.AddRange(stockInList);
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                }
        //                await _context.SaveChangesAsync();

        //                TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
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
                            try
                            {
                                string itemName = worksheet.Cell(row, 3).GetString().Trim();
                                string itemCode = worksheet.Cell(row, 7).GetString().Trim();
                                string recordsCount = worksheet.Cell(row, 4).GetString().Trim();
                                string qtyText = worksheet.Cell(row, 5).GetString().Trim();
                                string roomName = worksheet.Cell(row, 8).GetString().Trim();
                                string rackNo = worksheet.Cell(row, 9).GetString().Trim();
                                string priceText = worksheet.Cell(row, 10).GetString().Trim();

                                if (string.IsNullOrWhiteSpace(itemName))
                                    continue;

                                if (string.IsNullOrWhiteSpace(itemCode))
                                {
                                    failedRecords.Add($"Row {row}: SKUName is not available.");
                                    continue;
                                }

                                if (!itemCode.StartsWith("B", StringComparison.OrdinalIgnoreCase))
                                {
                                    failedRecords.Add($"Row {row}: Invalid SKU '{itemCode}'. Must start with 'B'.");
                                    continue;
                                }

                                // 🔹 Step 1: Check Product
                                var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName && p.IsDeleted == false);
                                if (product == null)
                                {
                                    //Strt Code For Duplicate SKU Name not Save
                                    bool skuExistsInOtherProduct = _context.TblSkuBarcodes.Any(s => s.Skuname == itemCode);

                                    if (skuExistsInOtherProduct)
                                    {
                                        // ❌ Duplicate found → skip this record only
                                        failedRecords.Add($"Row {row}: SKU '{itemCode}' already exists in another product.");
                                        continue; // 👉 Move to next record
                                    }
                                    //End Code For Duplicate SKU Name not Save

                                    product = new TblProduct
                                    {
                                        ProductName = itemName,
                                        IsDeleted = false,
                                        CreatedAt = DateTime.Now,
                                        AvailableProductQty = "0"
                                    };
                                    _context.TblProducts.Add(product);
                                    await _context.SaveChangesAsync();

                                    // Add Alias
                                    TblProductAlias alias = new TblProductAlias
                                    {
                                        FkProductId = product.ProductId,
                                        AliasName = itemName,
                                        IsDeleted = false,
                                        CreatedAt = DateTime.Now
                                    };
                                    _context.TblProductAliases.Add(alias);
                                }
                                else
                                {
                                    //Strt Code For Duplicate SKU Name not Save

                                    bool skuExistsForOtherProduct = _context.TblSkuBarcodes
                                        .Any(s => s.Skuname == itemCode && s.FkProductId != product.ProductId);

                                    if (skuExistsForOtherProduct)
                                    {
                                        failedRecords.Add($"Row {row}: SKU '{itemCode}' already belongs to another product, cannot save StockIn.");
                                        continue; 
                                    }
                                    //End Code For Duplicate SKU Name not Save
                                }

                                // 🔹 Step 2: SKU check
                                //bool skuExists = _context.TblSkuBarcodes
                                //    .Any(s => s.FkProductId == product.ProductId && s.Skuname == itemCode)
                                //    || _context.ChangeTracker.Entries<TblSkuBarcode>()
                                //        .Any(e => e.Entity.FkProductId == product.ProductId && e.Entity.Skuname == itemCode && e.State == EntityState.Added);

                                bool skuExists = _context.TblSkuBarcodes
                                    .Any(s => s.Skuname == itemCode)
                                    || _context.ChangeTracker.Entries<TblSkuBarcode>()
                                       .Any(e => e.Entity.Skuname == itemCode && e.State == EntityState.Added);


                                if (!skuExists)
                                {
                                    var sku = new TblSkuBarcode
                                    {
                                        Skuname = itemCode,
                                        FkProductId = product.ProductId,
                                        IsDeleted = 0,
                                        CreatedAt = DateTime.Now
                                    };
                                    _context.TblSkuBarcodes.Add(sku);
                                }

                                // 🔹 Step 3: Warehouse check/create
                                TblWarehouse warehouse = null;
                                if (!string.IsNullOrWhiteSpace(roomName))
                                {
                                    warehouse = _context.TblWarehouses.FirstOrDefault(w => w.Name == roomName && w.IsDeleted == false);

                                    if (warehouse == null)
                                    {
                                        warehouse = new TblWarehouse
                                        {
                                            Name = roomName,
                                            IsDeleted = false,
                                            CreatedAt = DateTime.Now
                                        };
                                        _context.TblWarehouses.Add(warehouse);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                                int warehouseId = warehouse?.WarehouseId ?? 0; // 👉 0 means invalid, skip TblStockIn later

                                // 🔹 Step 4: Rack check
                                TblRack rack = null;
                                if (!string.IsNullOrWhiteSpace(rackNo))
                                {
                                    rack = _context.TblRacks.FirstOrDefault(r => r.RackNo == rackNo && r.FkWarehouseId == warehouseId && r.IsDeleted == 0);

                                    if (rack == null)
                                    {
                                        rack = new TblRack
                                        {
                                            FkWarehouseId = warehouseId,
                                            RackNo = rackNo,
                                            IsDeleted = 0,
                                            CreatedAt = DateTime.Now
                                        };
                                        _context.TblRacks.Add(rack);
                                    }
                                }

                                if (product != null)
                                {
                                    product.FkWarehouseId = warehouseId;
                                    product.FkRackId = (int)(rack?.RackId);
                                    _context.TblProducts.Update(product);
                                    await _context.SaveChangesAsync();
                                }

                                // 🔹 Step 5: Parse qty
                                int.TryParse(recordsCount, out int boxCount);
                                int.TryParse(qtyText, out int perBoxQty);

                                if (boxCount <= 0 || perBoxQty <= 0 || warehouseId == 0 || string.IsNullOrWhiteSpace(rackNo))
                                {
                                    failedRecords.Add($"Row {row}: Skipped because recordsCount/qty/warehouse/rack missing or invalid.");
                                    continue; // 👉 Skip TblStockIn
                                }

                                int totalQty = boxCount * perBoxQty;
                                int.TryParse(product.AvailableProductQty, out int currentQty);

                                // 🔹 Step 6: Price check
                                decimal.TryParse(priceText, out decimal price);
                                if (price < 0) price = 0; // ensure non-negative

                                // 🔹 Step 7: Add StockIn
                                TblStockIn stockIn = new()
                                {
                                    FkProductId = product.ProductId,
                                    Barcode = itemCode,
                                    Price = price,
                                    ProductQuantity = totalQty.ToString(),
                                    AvailableQuantity = totalQty.ToString(),
                                    Date = DateTime.Now,
                                    Type = "1",
                                    FkWarehouseId = warehouseId,
                                    FkSupplierId = 1,
                                    BatchNo = batchNumber,
                                    RackNo = rackNo,
                                    TotalBox = boxCount,
                                    AvailableBox = boxCount,
                                    PerBoxQty = perBoxQty
                                };

                                stockInList.Add(stockIn);

                                // Update available quantity
                                product.AvailableProductQty = (currentQty + totalQty).ToString();
                            }
                            catch (Exception rowEx)
                            {
                                failedRecords.Add($"Row {row}: {rowEx.Message}");
                                continue;
                            }
                        }

                        // Save Data
                        if (stockInList.Any())
                        {
                            _context.TblStockIns.AddRange(stockInList);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
                        }

                        if (failedRecords.Any())
                        {
                            TempData["ErrorMessage"] =
                                $"{failedRecords.Count} records skipped.<br/>{string.Join("<br/>", failedRecords)}";
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




        //old Code 28/08/2025

        //[HttpPost]
        //public async Task<IActionResult> ImportBoxItems(IFormFile file)
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
        //    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
        //        await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
        //        batchNumber = parameters.Get<string>("@newBatchNo");
        //    }

        //    List<TblStockIn> stockInList = new();
        //    List<string> failedRecords = new();

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
        //                    string itemName = worksheet.Cell(row, 3).GetString().Trim();
        //                    string itemCode = worksheet.Cell(row, 7).GetString().Trim();
        //                    string recordsCount = worksheet.Cell(row, 4).GetString().Trim();
        //                    string qtyText = worksheet.Cell(row, 5).GetString().Trim();
        //                    string roomName = worksheet.Cell(row, 8).GetString().Trim();
        //                    string rackNo = worksheet.Cell(row, 9).GetString().Trim();
        //                    //string lowStockQty = worksheet.Cell(row, 10).GetString().Trim();
        //                    string price = worksheet.Cell(row, 10).GetString().Trim();

        //                    if (string.IsNullOrWhiteSpace(itemName))
        //                        continue;

        //                    if (string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        failedRecords.Add($"Row {row}: SKUName is not available.");
        //                        continue;
        //                    }

        //                    if (!string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        if (!itemCode.StartsWith("B", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            failedRecords.Add($"Row {row}: Invalid SKU '{itemCode}'. Please add correct SKUName e.g. 'B001' (must start with 'A').");
        //                            continue; 
        //                        }
        //                    }

        //                    // 🔹 Step 1: Check Product
        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName && p.IsDeleted == false);
        //                    if (product == null)
        //                    {
        //                        product = new TblProduct
        //                        {
        //                            ProductName = itemName,
        //                            //LowStockQuantity = lowStockQty,
        //                            IsDeleted = false,
        //                            CreatedAt = DateTime.Now,
        //                            AvailableProductQty = "0"
        //                        };
        //                        _context.TblProducts.Add(product);
        //                        await _context.SaveChangesAsync(); 

        //                        // Add Alias
        //                        TblProductAlias alias = new TblProductAlias
        //                        {
        //                            FkProductId = product.ProductId,
        //                            AliasName = itemName,
        //                            IsDeleted = false,
        //                            CreatedAt = DateTime.Now
        //                        };
        //                        _context.TblProductAliases.Add(alias);
        //                    }



        //                    if (!string.IsNullOrWhiteSpace(itemCode))
        //                    {
        //                        // Check both database and in-memory context for duplicate SKU
        //                        bool skuExists = _context.TblSkuBarcodes
        //                            .Any(s => s.FkProductId == product.ProductId && s.Skuname == itemCode)
        //                            || _context.ChangeTracker.Entries<TblSkuBarcode>()
        //                                .Any(e => e.Entity.FkProductId == product.ProductId && e.Entity.Skuname == itemCode && e.State == EntityState.Added);

        //                        if (!skuExists)
        //                        {
        //                            var sku = new TblSkuBarcode
        //                            {
        //                                Skuname = itemCode,
        //                                FkProductId = product.ProductId,
        //                                IsDeleted = 0,
        //                                CreatedAt = DateTime.Now
        //                            };
        //                            _context.TblSkuBarcodes.Add(sku);
        //                        }
        //                    }


        //                    // 🔹 4. Warehouse check/create (Room column)
        //                    TblWarehouse warehouse = null;
        //                    if (!string.IsNullOrWhiteSpace(roomName))
        //                    {
        //                        warehouse = _context.TblWarehouses
        //                            .FirstOrDefault(w => w.Name == roomName && w.IsDeleted == false);

        //                        if (warehouse == null)
        //                        {
        //                            warehouse = new TblWarehouse
        //                            {
        //                                Name = roomName,
        //                                IsDeleted = false,
        //                                CreatedAt = DateTime.Now
        //                            };
        //                            _context.TblWarehouses.Add(warehouse);
        //                            await _context.SaveChangesAsync();
        //                        }
        //                    }

        //                    int warehouseId = warehouse?.WarehouseId ?? 1;


        //                    // 🔹 Step 3: Check Rack
        //                    TblRack rack = null;
        //                    if (!string.IsNullOrWhiteSpace(rackNo))
        //                    {
        //                        rack = _context.TblRacks.FirstOrDefault(r => r.RackNo == rackNo && r.FkWarehouseId == warehouseId && r.IsDeleted == 0);

        //                        if (rack == null)
        //                        {
        //                            rack = new TblRack
        //                            {
        //                                FkWarehouseId = warehouseId,
        //                                RackNo = rackNo,
        //                                IsDeleted = 0,
        //                                CreatedAt = DateTime.Now
        //                            };
        //                            _context.TblRacks.Add(rack);
        //                        }
        //                    }

        //                    // 🔹 Step 4: Parse qty
        //                    if (!int.TryParse(recordsCount, out int boxCount))
        //                    {
        //                        failedRecords.Add($"Row {row}: Invalid recordsCount value '{recordsCount}'");
        //                        continue;
        //                    }

        //                    if (!int.TryParse(qtyText, out int perBoxQty))
        //                    {
        //                        failedRecords.Add($"Row {row}: Invalid qtyText value '{qtyText}'");
        //                        continue;
        //                    }

        //                    int totalQty = boxCount * perBoxQty;

        //                    int currentQty = 0;
        //                    if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
        //                    {
        //                        int.TryParse(product.AvailableProductQty, out currentQty);
        //                    }

        //                    TblStockIn stockIn = new()
        //                    {
        //                        FkProductId = product.ProductId,
        //                        Barcode = itemCode,
        //                        Price = Convert.ToInt32(price),
        //                        ProductQuantity = totalQty.ToString(),
        //                        AvailableQuantity = totalQty.ToString(),
        //                        Date = DateTime.Now,
        //                        Type = "1",
        //                        FkWarehouseId = warehouseId,
        //                        FkSupplierId = 1,
        //                        BatchNo = batchNumber,
        //                        RackNo = rackNo,
        //                        TotalBox = boxCount,
        //                        AvailableBox = boxCount,
        //                        PerBoxQty = perBoxQty
        //                    };

        //                    stockInList.Add(stockIn);

        //                    // Update available quantity
        //                    int updatedQty = currentQty + totalQty;
        //                    product.AvailableProductQty = updatedQty.ToString();
        //                }


        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] =
        //                        $"{failedRecords.Count} records failed. <br/>{string.Join("<br/>", failedRecords)}";

        //                    return RedirectToAction("InventoryList"); 
        //                }

        //                if (stockInList.Any())
        //                {
        //                    _context.TblStockIns.AddRange(stockInList);
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                }

        //                await _context.SaveChangesAsync();

        //                TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
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




















        //[HttpPost]
        //public async Task<IActionResult> ImportBoxItems(IFormFile file)
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
        //    using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@newBatchNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
        //        await connection.ExecuteAsync("GenerateBatchNumber", parameters, commandType: CommandType.StoredProcedure);
        //        batchNumber = parameters.Get<string>("@newBatchNo");
        //    }

        //    List<TblStockIn> stockInList = new();
        //    List<string> failedRecords = new();

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

        //                //for (int row = 2; row <= rowCount; row++)
        //                //{
        //                //    string itemName = worksheet.Cell(row, 3).GetString().Trim();
        //                //    string itemCode = worksheet.Cell(row, 7).GetString().Trim();
        //                //    string recordsCount = worksheet.Cell(row, 4).GetString().Trim();
        //                //    //string priceText = worksheet.Cell(row, 5).GetString().Trim();
        //                //    string qtyText = worksheet.Cell(row, 5).GetString().Trim();
        //                //    string rackNo = worksheet.Cell(row, 8).GetString().Trim();

        //                //    if (string.IsNullOrWhiteSpace(itemName))
        //                //        continue;

        //                //    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
        //                //    if (product == null)
        //                //    {
        //                //        failedRecords.Add($"Row {row}: Product '{itemName}' not found");
        //                //        continue;
        //                //    }

        //                //    //decimal.TryParse(priceText, out decimal price);
        //                //    int.TryParse(qtyText, out int qty);

        //                //    int currentQty = 0;
        //                //    if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
        //                //    {
        //                //        int.TryParse(product.AvailableProductQty, out currentQty);
        //                //    }

        //                //    if (!int.TryParse(recordsCount, out int count))
        //                //    {
        //                //        failedRecords.Add($"Row {row}: Invalid recordsCount value '{recordsCount}'");
        //                //        continue;
        //                //    }


        //                //    for (int i = 0; i < count; i++)
        //                //    {
        //                //        TblStockIn stockIn = new()
        //                //        {
        //                //            FkProductId = product.ProductId,
        //                //            Barcode = itemCode,
        //                //            Price = 0,
        //                //            ProductQuantity = qty.ToString(),
        //                //            AvailableQuantity = qty.ToString(),
        //                //            Date = DateTime.Now,
        //                //            Type = "1",
        //                //            FkWarehouseId = 1,
        //                //            FkSupplierId = 1,
        //                //            BatchNo = batchNumber,
        //                //            RackNo = rackNo,
        //                //            //TotalBox = recordsCount,
        //                //            //PerBoxQty = qtyText
        //                //        };

        //                //        stockInList.Add(stockIn);
        //                //    }

        //                //    // Update available quantity once per item
        //                //    int updatedQty = currentQty + (qty * count);
        //                //    product.AvailableProductQty = updatedQty.ToString();
        //                //}

        //                for (int row = 2; row <= rowCount; row++)
        //                {
        //                    string itemName = worksheet.Cell(row, 3).GetString().Trim();
        //                    string itemCode = worksheet.Cell(row, 7).GetString().Trim();
        //                    string recordsCount = worksheet.Cell(row, 4).GetString().Trim(); // TotalBox
        //                    string qtyText = worksheet.Cell(row, 5).GetString().Trim();      // PerBoxQty
        //                    string rackNo = worksheet.Cell(row, 8).GetString().Trim();

        //                    if (string.IsNullOrWhiteSpace(itemName))
        //                        continue;

        //                    var product = _context.TblProducts.FirstOrDefault(p => p.ProductName == itemName);
        //                    if (product == null)
        //                    {
        //                        failedRecords.Add($"Row {row}: Product '{itemName}' not found");
        //                        continue;
        //                    }

        //                    if (!int.TryParse(recordsCount, out int boxCount))
        //                    {
        //                        failedRecords.Add($"Row {row}: Invalid recordsCount value '{recordsCount}'");
        //                        continue;
        //                    }

        //                    if (!int.TryParse(qtyText, out int perBoxQty))
        //                    {
        //                        failedRecords.Add($"Row {row}: Invalid qtyText value '{qtyText}'");
        //                        continue;
        //                    }

        //                    int totalQty = boxCount * perBoxQty;

        //                    int currentQty = 0;
        //                    if (!string.IsNullOrWhiteSpace(product.AvailableProductQty))
        //                    {
        //                        int.TryParse(product.AvailableProductQty, out currentQty);
        //                    }

        //                    TblStockIn stockIn = new()
        //                    {
        //                        FkProductId = product.ProductId,
        //                        Barcode = itemCode,
        //                        Price = 0,
        //                        ProductQuantity = totalQty.ToString(),
        //                        AvailableQuantity = totalQty.ToString(),
        //                        Date = DateTime.Now,
        //                        Type = "1",
        //                        FkWarehouseId = 1,
        //                        FkSupplierId = 1,
        //                        BatchNo = batchNumber,
        //                        RackNo = rackNo,
        //                        TotalBox = Convert.ToInt32(recordsCount),  
        //                        AvailableBox = Convert.ToInt32(recordsCount),  
        //                        PerBoxQty = Convert.ToInt32(qtyText)
        //                    };

        //                    stockInList.Add(stockIn);

        //                    // Update available quantity
        //                    int updatedQty = currentQty + totalQty;
        //                    product.AvailableProductQty = updatedQty.ToString();
        //                }



        //                if (stockInList.Any())
        //                {
        //                    _context.TblStockIns.AddRange(stockInList);
        //                    await _context.SaveChangesAsync();
        //                }

        //                TempData["SuccessMessage"] = $"{stockInList.Count} records imported successfully.";
        //                if (failedRecords.Any())
        //                {
        //                    TempData["ErrorMessage"] = $"{failedRecords.Count} records failed to import due to incorrect product names.";
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





        //public IActionResult DailyStockReport(DateTime? fromDate, DateTime? toDate, string searchTerm, int pageNumber = 1, int pageSize = 10)
        //{
        //    var today = DateTime.Today;

        //    var from = fromDate ?? today;
        //    //var to = toDate ?? today;
        //    var to = (toDate?.Date ?? today).AddDays(1).AddTicks(-1);

        //    var products = _context.TblProducts.ToList();

        //    // Prepare stock-in data
        //    var stockInQuery = _context.TblStockIns
        //        .Where(x => x.Date >= from && x.Date <= to)
        //        .AsEnumerable()
        //        .GroupBy(x => new { x.FkProductId, Date = x.Date?.Date })
        //        .Select(g => new StockReportViewModel
        //        {
        //            ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
        //            Date = g.Key.Date ?? DateTime.MinValue,
        //            Type = "StockIn",
        //            Quantity = g.Sum(x => Convert.ToInt32(x.ProductQuantity))
        //        });

        //    // Prepare stock-out data
        //    var stockOutQuery = _context.TblStockOuts
        //        .Where(x => x.StockOutDate >= from && x.StockOutDate <= to)
        //        .AsEnumerable()
        //        .GroupBy(x => new { x.FkProductId, Date = x.StockOutDate?.Date })
        //        .Select(g => new StockReportViewModel
        //        {
        //            ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
        //            Date = g.Key.Date ?? DateTime.MinValue,
        //            Type = "StockOut",
        //            Quantity = g.Sum(x => Convert.ToInt32(x.Quantity))
        //        });

        //    // Combine and apply search filtering (after projecting to StockReportViewModel)
        //    var combinedList = stockInQuery.Concat(stockOutQuery).ToList();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        searchTerm = searchTerm.Trim().ToLower();
        //        combinedList = combinedList
        //            .Where(x => x.ProductName.ToLower().Contains(searchTerm) || x.Type.ToLower().Contains(searchTerm))
        //            .ToList();
        //    }

        //    // Order, paginate
        //    var sortedList = combinedList
        //        .OrderByDescending(x => x.Date)
        //        .ThenBy(x => x.ProductName)
        //        .ThenBy(x => x.Type)
        //        .ToList();

        //    int totalRecords = sortedList.Count;

        //    var pagedList = sortedList
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList();

        //    var model = new StockReportPagedViewModel
        //    {
        //        CombinedStockList = pagedList,
        //        Pagination = new PaginationMetadataViewModel
        //        {
        //            TotalRecords = totalRecords,
        //            CurrentPage = pageNumber,
        //            PageSize = pageSize,
        //            SearchTerm = searchTerm
        //        }
        //    };

        //    ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        //    ViewBag.ToDate = to.ToString("yyyy-MM-dd");

        //    return View(model);
        //}


        public IActionResult DailyStockReport(DateTime? fromDate, DateTime? toDate, string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var today = DateTime.Today;

            var from = fromDate ?? today;

            var to = (toDate?.Date ?? today).AddDays(1).AddTicks(-1);

            var products = _context.TblProducts.ToList();

            var stockInQuery = _context.TblStockIns
                .Where(x => x.Date >= from && x.Date <= to)
                .AsEnumerable()
                .GroupBy(x => new { x.FkProductId, x.Barcode, Date = x.Date.Value.Date })
                .Select(g => new StockReportViewModel
                {
                    ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
                    SKUName = g.Key.Barcode,
                    Date = g.Min(x => x.Date ?? DateTime.MinValue),
                    Type = "StockIn",
                    Quantity = g.Sum(x => Convert.ToInt32(x.ProductQuantity))
                });

            var stockOutQuery = _context.TblStockOuts
                .Where(x => x.StockOutDate >= from && x.StockOutDate <= to)
                .AsEnumerable()
                .GroupBy(x => new { x.FkProductId, x.Barcode, Date = x.StockOutDate.Value.Date })
                .Select(g => new StockReportViewModel
                {
                    ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
                    SKUName = g.Key.Barcode,
                    Date = g.Min(x => x.StockOutDate ?? DateTime.MinValue),
                    Type = "StockOut",
                    Quantity = g.Sum(x => Convert.ToInt32(x.Quantity))
                });

            var combinedList = stockInQuery.Concat(stockOutQuery).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                combinedList = combinedList
                    .Where(x => x.ProductName.ToLower().Contains(searchTerm) 
                    || x.Type.ToLower().Contains(searchTerm)
                    || x.Type.ToLower().Contains(searchTerm) 
                    || x.SKUName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            var sortedList = combinedList
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.ProductName)
                .ThenBy(x => x.Type)
                .ToList();

            int totalRecords = sortedList.Count;

            var pagedList = sortedList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new StockReportPagedViewModel
            {
                CombinedStockList = pagedList,
                Pagination = new PaginationMetadataViewModel
                {
                    TotalRecords = totalRecords,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                }
            };

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");

            return View(model);
        }

        public IActionResult DailyStockReportExportExcel(DateTime? fromDate, DateTime? toDate, string searchTerm)
        {
            var today = DateTime.Today;
            var from = fromDate ?? today;
            var to = (toDate?.Date ?? today).AddDays(1).AddTicks(-1);

            var products = _context.TblProducts.ToList();

            var stockInQuery = _context.TblStockIns
                .Where(x => x.Date >= from && x.Date <= to)
                .AsEnumerable()
                .GroupBy(x => new { x.FkProductId, x.Barcode, Date = x.Date.Value.Date })
                .Select(g => new StockReportViewModel
                {
                    ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
                    SKUName = g.Key.Barcode,
                    Date = g.Min(x => x.Date ?? DateTime.MinValue),
                    Type = "StockIn",
                    Quantity = g.Sum(x => Convert.ToInt32(x.ProductQuantity))
                });

            var stockOutQuery = _context.TblStockOuts
                .Where(x => x.StockOutDate >= from && x.StockOutDate <= to)
                .AsEnumerable()
                .GroupBy(x => new { x.FkProductId, x.Barcode, Date = x.StockOutDate.Value.Date })
                .Select(g => new StockReportViewModel
                {
                    ProductName = products.FirstOrDefault(p => p.ProductId == g.Key.FkProductId)?.ProductName ?? "N/A",
                    SKUName = g.Key.Barcode,
                    Date = g.Min(x => x.StockOutDate ?? DateTime.MinValue),
                    Type = "StockOut",
                    Quantity = g.Sum(x => Convert.ToInt32(x.Quantity))
                });

            var combinedList = stockInQuery.Concat(stockOutQuery).ToList();

            // ✅ Apply search filter if needed
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                combinedList = combinedList
                    .Where(x => x.ProductName.ToLower().Contains(searchTerm)
                             || x.Type.ToLower().Contains(searchTerm)
                             || x.SKUName.ToLower().Contains(searchTerm))
                    .ToList();
            }

            // ✅ Sorting like in your main method
            var sortedList = combinedList
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.ProductName)
                .ThenBy(x => x.Type)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DailyReport");
                worksheet.Cell(1, 1).Value = "No.";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "SKU Name";
                worksheet.Cell(1, 4).Value = "Date";
                worksheet.Cell(1, 5).Value = "Type";
                worksheet.Cell(1, 6).Value = "Quantity";

                // ✅ Header formatting
                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                worksheet.Column(1).Width = 10;   // "No."
                worksheet.Column(2).Width = 30;   // "Product Name"
                worksheet.Column(3).Width = 25;   // "SKU Name"
                worksheet.Column(4).Width = 20;   // "Date"
                worksheet.Column(5).Width = 15;   // "Type"
                worksheet.Column(6).Width = 15;   // "Quantity"

                int row = 2;
                int srNo = 1;

                foreach (var item in sortedList)
                {
                    worksheet.Cell(row, 1).Value = srNo++;
                    worksheet.Cell(row, 2).Value = item.ProductName;
                    worksheet.Cell(row, 3).Value = item.SKUName;
                    worksheet.Cell(row, 4).Value = item.Date.ToString("dd/MM/yyyy (hh:mm tt)");
                    worksheet.Cell(row, 5).Value = item.Type;
                    worksheet.Cell(row, 6).Value = item.Quantity;


                    //if (item.Type == "StockIn")
                    //{
                    //    worksheet.Row(row).Style.Font.FontColor = XLColor.Green;
                    //}
                    //else if (item.Type == "StockOut")
                    //{
                    //    worksheet.Row(row).Style.Font.FontColor = XLColor.Red;
                    //}

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "DailyReports.xlsx");
                }
            }
        }




    }
}
