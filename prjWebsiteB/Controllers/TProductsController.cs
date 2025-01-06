using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjWebsiteB.Models;
using prjWebsiteB.ViewModels;

namespace prjWebsiteB.Controllers
{
    public class TProductsController : Controller
    {
        private readonly dbGroupBContext _context;

        public TProductsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TProducts
        public async Task<IActionResult> Index(string searchString)
        {
            var productsQuery = _context.TProducts.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.FProductName.Contains(searchString));
            }

            var products = await productsQuery.Select(p => new ProductViewModel
            {
                FProductId = p.FProductId,
                FUserId = p.FUserId,
                FProductName = p.FProductName,
                FProductDescription = p.FProductDescription,
                FProductPrice = p.FProductPrice,
                FIsOnSale = p.FIsOnSales,
                FProductDateAdd = p.FProductDateAdd,
                FProductUpdated = p.FProductUpdated,
                FStock = p.FStock,
                FCategoryName = p.FProductCategory.FCategoryName,
                FImage = null,
            }).ToListAsync();

            return View(products);
        }

        // GET: TProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tProduct = await _context.TProducts
                        .Include(t => t.FProductCategory)
                        .Include(t => t.FUser)
                        .FirstOrDefaultAsync(m => m.FProductId == id);

            if (tProduct == null)
            {
                return NotFound();
            }

            //填進VM
            var product = new ProductViewModel
            {
                FProductId = tProduct.FProductId,
                FProductCategoryId = tProduct.FProductCategoryId,
                FUserId = tProduct.FUserId,
                FProductName = tProduct.FProductName,
                FProductDescription = tProduct.FProductDescription,
                FProductPrice = tProduct.FProductPrice,
                FIsOnSale = tProduct.FIsOnSales,
                FProductDateAdd = tProduct.FProductDateAdd,
                FProductUpdated = tProduct.FProductUpdated,
                FStock = tProduct.FStock,
                FCategoryName = tProduct.FProductCategory.FCategoryName,
                FImage = null
            };
            return View(product);
        }

        // GET: TProducts/Create
        public IActionResult Create()
        {
            // 建立 SelectList 並加入一個空白選項
            var productCategories = _context.TProductCategories.ToList();
            productCategories.Insert(0, new TProductCategory { FProductCategoryId = 0, FCategoryName = "" }); // 添加空白選項
            ViewData["FProductCategoryId"] = new SelectList(productCategories, "FProductCategoryId", "FCategoryName");

            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId");
            return View(new ProductViewModel());
        }

        // POST: TProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel newItem, IFormFile imageUpload1, IFormFile imageUpload2, IFormFile imageUpload3)
        {
            // 定義允許的圖片類型
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            // 移除文件欄位的模型狀態錯誤
            ModelState.Remove("imageUpload1");
            ModelState.Remove("imageUpload2");
            ModelState.Remove("imageUpload3");
            if (ModelState.IsValid)
            {
                var product = new TProduct
                {
                    FUserId = newItem.FUserId,
                    FProductCategoryId = newItem.FProductCategoryId,
                    FProductName = newItem.FProductName,
                    FProductDescription = newItem.FProductDescription,
                    FProductPrice = newItem.FProductPrice,
                    FIsOnSales = newItem.FIsOnSale,
                    FProductDateAdd = DateTime.Now,
                    FProductUpdated = null,
                    FStock = newItem.FStock
                };

                // 先保存產品，獲取生成的 ProductId
                _context.Add(product);
                await _context.SaveChangesAsync(); 

                // 處理圖片上傳
                if (imageUpload1 != null && imageUpload1.Length > 0)
                {
                    // 讀取並保存第一張圖片
                    var productImage1 = new TProductImage
                    {
                        FProductId = product.FProductId,
                        FImage = await ReadUploadImage(imageUpload1)
                    };
                    _context.Add(productImage1);
                }
                if (imageUpload2 != null && imageUpload2.Length > 0)
                {
                    // 讀取並保存第二張圖片
                    var productImage2 = new TProductImage
                    {
                        FProductId = product.FProductId,
                        FImage = await ReadUploadImage(imageUpload2)
                    };
                    _context.Add(productImage2);
                }
                if (imageUpload3 != null && imageUpload3.Length > 0)
                {
                    // 讀取並保存第三張圖片
                    var productImage3 = new TProductImage
                    {
                        FProductId = product.FProductId,
                        FImage = await ReadUploadImage(imageUpload3)
                    };
                    _context.Add(productImage3);
                }

                // 保存所有變更
                await _context.SaveChangesAsync();

                // 保存成功後重定向到產品列表頁面
                return RedirectToAction(nameof(Index));
            }
            var productCategories = _context.TProductCategories.ToList();
            productCategories.Insert(0, new TProductCategory { FProductCategoryId = 0, FCategoryName = "" }); // 添加空白選項
            ViewData["FProductCategoryId"] = new SelectList(productCategories, "FProductCategoryId", "FCategoryName");
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", newItem.FUserId);
            return View(newItem);
        }


        // GET: TProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tProduct = await _context.TProducts
                .Include(t => t.FProductCategory)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FProductId == id);

            if (tProduct == null)
            {
                return NotFound();
            }
            // 創建並填充 ProductViewModel
            var productViewModel = new ProductViewModel
            {
                FProductId = tProduct.FProductId,
                FUserId = tProduct.FUserId,
                FProductCategoryId = tProduct.FProductCategoryId,
                FProductName = tProduct.FProductName,
                FProductDescription = tProduct.FProductDescription,
                FProductPrice = tProduct.FProductPrice,
                FIsOnSale = tProduct.FIsOnSales,
                FProductDateAdd = tProduct.FProductDateAdd,
                FProductUpdated = tProduct.FProductUpdated,
                FStock = tProduct.FStock,
                FCategoryName = tProduct.FProductCategory.FCategoryName,
                FImage = null,  //tProduct.TProductImages.FirstOrDefault()?.FImage
            };
            ViewData["FProductCategoryId"] = new SelectList(_context.TProductCategories, "FProductCategoryId", "FCategoryName", tProduct.FProductCategoryId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tProduct.FUserId);
            return View(productViewModel);
        }

        // POST: TProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FProductId,FUserId,FProductCategoryId,FProductName,FProductDescription,FProductPrice,FIsOnSale,FStock")] ProductViewModel editItem, IFormFile imageUpload1, IFormFile imageUpload2, IFormFile imageUpload3)
        {
            if (id != editItem.FProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 從資料庫中查詢產品，包含圖片
                    var product = await _context.TProducts
                        .Include(t => t.FProductCategory)
                        .Include(t => t.FUser)
                        .Include(t => t.TProductImages)
                        .FirstOrDefaultAsync(m => m.FProductId == id);

                    // 檢查產品是否存在
                    if (product == null)
                    {
                        return NotFound();
                    }
                    // 更新產品屬性
                    product.FProductId = editItem.FProductId;
                    product.FProductCategoryId = editItem.FProductCategoryId.Value; // 更新產品類別 ID
                    product.FProductName = editItem.FProductName; // 更新產品名稱
                    product.FProductDescription = editItem.FProductDescription; // 更新產品描述
                    product.FProductPrice = editItem.FProductPrice; // 更新產品價格
                    product.FIsOnSales = editItem.FIsOnSale; // 更新上架狀態
                    product.FProductUpdated = DateTime.Now; // 更新產品更新日期
                    product.FStock = editItem.FStock; // 更新庫存數量

                    // 處理圖片上傳
                    var image1 = product.TProductImages.FirstOrDefault(img => img.FProductImageId == editItem.FProductImageId);
                    if (image1 != null)
                    {
                        image1.FImage = await ReadUploadImage(imageUpload1);
                    }
                    else
                    {
                        product.TProductImages.Add(new TProductImage { FProductId=editItem.FProductId,FImage = await ReadUploadImage(imageUpload1) });
                    }
                              



                // 更新產品數據
                _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(editItem.FProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["FProductCategoryId"] = new SelectList(_context.TProductCategories, "FProductCategoryId", "FCategoryName", editItem.FProductCategoryId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", editItem.FUserId);
            return View(editItem);
        }

        // 將圖片轉換為二進位
        private async Task<byte[]> ReadUploadImage(IFormFile image)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }



        private bool ProductExists(int id)
        {
            return _context.TProducts.Any(e => e.FProductId == id);
        }


        // GET: TProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tProduct = await _context.TProducts
                .Include(t => t.FProductCategory)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FProductId == id);
            if (tProduct == null)
            {
                return NotFound();
            }

            return View(tProduct);
        }

        // POST: TProducts/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.TProducts.FindAsync(id);
            if (product != null)
            {
                //var productImages = _context.ProductImages.Where(img => img.ProductId == id).ToList();
                //_context.ProductImages.RemoveRange(productImages);
                _context.TProducts.Remove(product);
            }
            else
            {
                return NotFound();
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TProductExists(int id)
        {
            return _context.TProducts.Any(e => e.FProductId == id);
        }

        //GET: TProducts/GetPictures/FProductId
        public async Task<IActionResult> GetPictures(int id)
        {
            // 根據產品 ID 查詢所有圖片
            var images = await _context.TProductImages
                                      .Where(img => img.FProductId == id)
                                      .Select(img => img.FImage)
                                      .ToListAsync();
   
            var imagesResult = images.Select(image => Convert.ToBase64String(image)).ToList();

             return Json(imagesResult);
        }

        //GET: TProducts/GetPicture/FProductId
        public async Task<FileResult> GetPicture(int id) //取單張
        {
            // 根據產品 ID 查詢所有圖片
            var image = await _context.TProductImages
                                      .Where(img => img.FProductId == id)
                                      .Select(img => img.FImage)
                                      .FirstOrDefaultAsync();
            if (image == null || image.Length == 0)
            {
                // 如果圖片為空，返回預設圖片
                var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/No_Pic_Tao.jpg");
                var defaultImage = System.IO.File.ReadAllBytes(defaultImagePath);
                return File(defaultImage, "image/jpeg");
            }

            return File(image, "image/jpeg");
        }
    }
}


