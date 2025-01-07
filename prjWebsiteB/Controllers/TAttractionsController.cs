using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using prjWebsiteB.AttractionViewModels;
using prjWebsiteB.Models;

namespace prjWebsiteB.Controllers {
    public class TAttractionsController : Controller {
        private readonly dbGroupBContext _context;

        public TAttractionsController(dbGroupBContext context) {
            _context = context;
        }

        // GET: TAttractions
        public async Task<IActionResult> Index() {
            var dbGroupBContext = _context.TAttractions.Include(t => t.FCategory);

            ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");

            return View(await dbGroupBContext.ToListAsync());
        }

        // GET: TAttractions/Details/5
        public async Task<IActionResult> Details(int? id) {
           
            if (id == null) {
                return NotFound();
            }

            var tAttraction = await _context.TAttractions
                .Include(t => t.FCategory)
                .FirstOrDefaultAsync(m => m.FAttractionId == id);

            if (tAttraction == null) {
                return NotFound();
            }

            var attractionViewModel = new prjWebsiteB.AttractionViewModels.TAttraction {
                FAttractionId = tAttraction.FAttractionId,
                FAttractionName = tAttraction.FAttractionName,
                FDescription = tAttraction.FDescription,
                FAddress = tAttraction.FAddress,
                FPhoneNumber = tAttraction.FPhoneNumber,
                FOpeningTime = tAttraction.FOpeningTime,
                FClosingTime = tAttraction.FClosingTime,
                FWebsiteUrl = tAttraction.FWebsiteUrl,
                FLongitude = tAttraction.FLongitude,
                FLatitude = tAttraction.FLatitude,
                FRegion = tAttraction.FRegion,
                FCategoryId = tAttraction.FCategoryId,
                //FCategory = tAttraction.FCategory.FAttractionCategoryName,
                FCreatedDate = tAttraction.FCreatedDate,
                FUpdatedDate = tAttraction.FUpdatedDate,
                FStatus = tAttraction.FStatus,
                FTrafficInformation = tAttraction.FTrafficInformation
            };

            ViewBag.categoryName = await _context.TAttractionCategories.Where(c => c.FAttractionCategoryId == tAttraction.FCategoryId)
                .Select(c => c.FAttractionCategoryName)
                .FirstOrDefaultAsync();

            return View(attractionViewModel);
        }

        // GET: TAttractions/Create
        public IActionResult Create() {
            ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");
            return View();
        }

        // POST: TAttractions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Bind] 屬性僅限制 TAttraction 類別中的屬性能夠從表單綁定到 Create 方法中的 tAttraction 參數，但它不會影響其他單獨傳入的參數，例如 IFormFile FImage。
        public async Task<IActionResult> Create([Bind("FAttractionId,FAttractionName,FDescription,FAddress,FPhoneNumber,FOpeningTime,FClosingTime,FWebsiteUrl,FLongitude,FLatitude,FRegion,FCategoryId,FCreatedDate,FUpdatedDate,FStatus,FTrafficInformation")] Models.TAttraction tAttraction, IFormFile? FImage) {
            if (ModelState.IsValid) {

                // 先存入 tAttraction，並確保主鍵已生成
                tAttraction.FCreatedDate = DateTime.Now;
                tAttraction.FUpdatedDate = DateTime.Now;
                _context.Add(tAttraction);
                await _context.SaveChangesAsync();

                //檢查表單中是否包含名為 "Picture" 的檔案，若有則進行圖片讀取。
                if (FImage != null && FImage.Length > 0) {
                    
                    //使用 BinaryReader 讀取圖片檔案的內容，並將其轉換為位元組陣列（byte[]）。
                    //FImage.OpenReadStream()：開啟圖片檔案的串流來讀取內容。
                    using (BinaryReader br = new BinaryReader(FImage.OpenReadStream())) {
                        //br.ReadBytes(...)：將圖片檔案的內容讀取為位元組陣列，並存入 category.Picture 屬性中。
                        TAttractionImage tAttractionImage = new TAttractionImage {
                            FAttractionId = tAttraction.FAttractionId,
                            FImage = br.ReadBytes((int)FImage.Length)
                        };
                        _context.Add(tAttractionImage);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FCategoryId"] = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryId", tAttraction.FCategoryId);
            return View(GetAttractionViewModels(tAttraction));
        }

        // GET: TAttractions/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var tAttraction = await _context.TAttractions.FindAsync(id);
            if (tAttraction == null) {
                return NotFound();
            }

            ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");

            return View(GetAttractionViewModels(tAttraction));
        }

        // POST: TAttractions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("FAttractionId,FAttractionName,FDescription,FAddress,FPhoneNumber,FOpeningTime,FClosingTime,FWebsiteUrl,FLongitude,FLatitude,FRegion,FCategoryId,FCreatedDate,FUpdatedDate,FStatus,FTrafficInformation")] Models.TAttraction tAttraction, List<IFormFile>? FImages) {
            //if (id != tAttraction.FAttractionId) {
            //    return NotFound();
            //}

            if (ModelState.IsValid) {
                try {
                    tAttraction.FUpdatedDate = DateTime.Now;
                    _context.Update(tAttraction);
                    await _context.SaveChangesAsync();

                    //檢查表單中是否包含名為 "Picture" 的檔案，若有則進行圖片讀取。
                    if (FImages != null && FImages.Count > 0) {
                        foreach (var image in FImages) {
                            if (image != null && image.Length > 0) {
                                //使用 BinaryReader 讀取圖片檔案的內容，並將其轉換為位元組陣列（byte[]）。
                                //FImage.OpenReadStream()：開啟圖片檔案的串流來讀取內容。
                                using (BinaryReader br = new BinaryReader(image.OpenReadStream())) {
                                    //br.ReadBytes(...)：將圖片檔案的內容讀取為位元組陣列，並存入 category.Picture 屬性中。
                                    TAttractionImage tAttractionImage = new TAttractionImage {
                                        FAttractionId = tAttraction.FAttractionId,
                                        FImage = br.ReadBytes((int)image.Length)
                                    };
                                    _context.Add(tAttractionImage);
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException) {
                    if (!TAttractionExists(tAttraction.FAttractionId)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");

            return View(GetAttractionViewModels(tAttraction));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }
            var tAttraction = await _context.TAttractions.FindAsync(id);
            if (tAttraction == null) {
                return NotFound();
            }

            // 根據 id(FAttractionId) 找出所有符合的圖片
            var tAttractionImages = await _context.TAttractionImages.Where(img=>img.FAttractionId == id).ToListAsync();
            if (tAttractionImages != null) {
                //使用 RemoveRange 方法一次刪除所有符合條件的圖片記錄。
                _context.TAttractionImages.RemoveRange(tAttractionImages);
            }

            _context.TAttractions.Remove(tAttraction);
            await _context.SaveChangesAsync();

            return Ok(); // 返回 200 狀態碼
        }

        // GET: TAttractions/Delete/5
        //public async Task<IActionResult> Delete(int? id) {
        //    if (id == null) {
        //        return NotFound();
        //    }

        //    var tAttraction = await _context.TAttractions
        //        .Include(t => t.FCategory)
        //        .FirstOrDefaultAsync(m => m.FAttractionId == id);
        //    if (tAttraction == null) {
        //        return NotFound();
        //    }

        //    return View(GetAttractionViewModels(tAttraction));
        //}

        // POST: TAttractions/Delete/5
        //HttpPost：此方法只能處理 POST 請求。
        //ActionName("Delete")：將此方法的動作名稱設定為 Delete，與前端表單的 asp-action="Delete" 對應（雖然方法名稱是 DeleteConfirmed，但外部還是用 Delete 呼叫）。
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id) {
        //    var tAttraction = await _context.TAttractions.FindAsync(id);
        //    if (tAttraction != null) {
        //        _context.TAttractions.Remove(tAttraction);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool TAttractionExists(int id) {
            return _context.TAttractions.Any(e => e.FAttractionId == id);
        }

        public prjWebsiteB.AttractionViewModels.TAttraction GetAttractionViewModels(Models.TAttraction tAttraction) {
            return new prjWebsiteB.AttractionViewModels.TAttraction {
                FAttractionId = tAttraction.FAttractionId,
                FAttractionName = tAttraction.FAttractionName,
                FDescription = tAttraction.FDescription,
                FAddress = tAttraction.FAddress,
                FPhoneNumber = tAttraction.FPhoneNumber,
                FOpeningTime = tAttraction.FOpeningTime,
                FClosingTime = tAttraction.FClosingTime,
                FWebsiteUrl = tAttraction.FWebsiteUrl,
                FLongitude = tAttraction.FLongitude,
                FLatitude = tAttraction.FLatitude,
                FRegion = tAttraction.FRegion,
                FCategoryId = tAttraction.FCategoryId,
                FCreatedDate = tAttraction.FCreatedDate,
                FUpdatedDate = tAttraction.FUpdatedDate,
                FStatus = tAttraction.FStatus,
                FTrafficInformation = tAttraction.FTrafficInformation
            };
        }
    }
}
