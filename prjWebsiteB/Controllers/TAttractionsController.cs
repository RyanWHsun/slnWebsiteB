using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using prjWebsiteB.AttractionViewModels;
using prjWebsiteB.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

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

            var data = Json(new {
                tAttraction.FAttractionId,
                tAttraction.FAttractionName,
                tAttraction.FCategoryId,
                tAttraction.FCategory.FAttractionCategoryName,
                tAttraction.FDescription,
                tAttraction.FRegion,
                tAttraction.FAddress,
                tAttraction.FStatus,
                tAttraction.FOpeningTime,
                tAttraction.FClosingTime,
                tAttraction.FPhoneNumber,
                tAttraction.FWebsiteUrl,
                tAttraction.FCreatedDate,
                tAttraction.FUpdatedDate,
                tAttraction.FTrafficInformation
            });
            return data; // 回傳 JSON 格式的資料
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
        public async Task<IActionResult> Create([Bind("FAttractionId,FAttractionName,FDescription,FAddress,FPhoneNumber,FOpeningTime,FClosingTime,FWebsiteUrl,FLongitude,FLatitude,FRegion,FCategoryId,FCreatedDate,FUpdatedDate,FStatus,FTrafficInformation")] Models.TAttraction tAttraction, List<IFormFile>? FImages) {
            if (ModelState.IsValid) {

                // 先存入 tAttraction，並確保主鍵已生成
                tAttraction.FCreatedDate = DateTime.Now;
                tAttraction.FUpdatedDate = DateTime.Now;
                _context.Add(tAttraction);
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
                return RedirectToAction(nameof(Index));
            }
            //ViewData["FCategoryId"] = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryId", tAttraction.FCategoryId);
            return View(GetAttractionViewModels(tAttraction));
        }

        [HttpPost]
        public async Task<IActionResult> Edit() {

            TimeOnly? fOpeningTime, fClosingTime;
            if (TimeSpan.TryParse(Request.Form["fOpeningTime"], out TimeSpan ot)) {
                fOpeningTime = TimeOnly.FromTimeSpan(ot);
            }
            else {
                fOpeningTime = null;
            }
            if (TimeSpan.TryParse(Request.Form["fClosingTime"], out TimeSpan ct)) {
                fClosingTime = TimeOnly.FromTimeSpan(ct);
            }
            else {
                fClosingTime = null;
            }

            int? fCategoryId = int.TryParse(Request.Form["fCategoryId"], out int fCategoryIdResult) ? fCategoryIdResult : null;
            DateTime? fCreatedDate = DateTime.TryParse(Request.Form["fCreatedDate"], out DateTime fCreatedDateResult) ? fCreatedDateResult : null;
            DateTime? fUpdatedDate = DateTime.TryParse(Request.Form["fUpdatedDate"], out DateTime fUpdatedDateResult) ? fUpdatedDateResult : null;

            var tAttraction = new Models.TAttraction {
                FAttractionId = int.Parse(Request.Form["fAttractionId"]),
                FAttractionName = Request.Form["fAttractionName"],
                FDescription = Request.Form["fDescription"],
                FAddress = Request.Form["fAddress"],
                FPhoneNumber = Request.Form["fPhoneNumber"],
                FOpeningTime = fOpeningTime,
                FClosingTime = fClosingTime,
                FWebsiteUrl = Request.Form["fWebsiteUrl"],
                FRegion = Request.Form["fRegion"],
                FCategoryId = fCategoryId,
                FStatus = Request.Form["fStatus"],
                FTrafficInformation = Request.Form["fTrafficInformation"],
                FCreatedDate = fCreatedDate,
                FUpdatedDate = fUpdatedDate
            };

            // 儲存圖片
            var files = Request.Form.Files;
            if (files != null && files.Count > 0) {
                foreach (var image in files) {
                    if (image.Length > 0) {
                        using (var br = new BinaryReader(image.OpenReadStream())) {
                            var tAttractionImage = new TAttractionImage {
                                FAttractionId = tAttraction.FAttractionId,
                                FImage = br.ReadBytes((int)image.Length)
                            };
                            _context.Add(tAttractionImage);
                        }
                    }
                }
            }

            // 更新資料庫
            if (ModelState.IsValid) {
                try {
                    tAttraction.FUpdatedDate = DateTime.Now;
                    _context.Update(tAttraction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!TAttractionExists(tAttraction.FAttractionId)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }

                // 資料一個個對應比較不會出錯
                var updatedData = new {
                    FAttractionId = int.Parse(Request.Form["fAttractionId"]),
                    FAttractionName = Request.Form["fAttractionName"],
                    FCategoryId = fCategoryId,
                    FCategoryName = Request.Form["fCategoryName"],
                    FDescription = Request.Form["fDescription"],
                    FRegion = Request.Form["fRegion"],
                    FAddress = Request.Form["fAddress"],
                    FStatus = Request.Form["fStatus"],
                    FOpeningTime = fOpeningTime,
                    FClosingTime = fClosingTime,
                    FPhoneNumber = Request.Form["fPhoneNumber"],
                    FWebsiteUrl = Request.Form["fWebsiteUrl"],
                    FCreatedDate = fCreatedDate,
                    FUpdatedDate = fUpdatedDate,
                    FTrafficInformation = Request.Form["fTrafficInformation"],  
                };

                return new JsonResult(new {
                    success = true,
                    message = "儲存成功！",
                    updatedData
                }) {
                    StatusCode = 200 // 明確指定 200 狀態碼
                };
            }

            return Json(new { success = false, message = "資料驗證失敗！" });
        }

        // GET: TAttractions/Delete/5
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

        private bool TAttractionExists(int id) {
            return _context.TAttractions.Any(e => e.FAttractionId == id);
        }

        // 將 Models.TAttraction 轉換為 AttractionViewModels.TAttraction
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

        // GET: TAttractions/Edit/5
        //public async Task<IActionResult> Edit(int? id) {
        //    if (id == null) {
        //        return NotFound();
        //    }

        //    var tAttraction = await _context.TAttractions.FindAsync(id);
        //    if (tAttraction == null) {
        //        return NotFound();
        //    }

        //    ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");

        //    return View(GetAttractionViewModels(tAttraction));
        //}

        // POST: TAttractions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit([Bind("FAttractionId,FAttractionName,FDescription,FAddress,FPhoneNumber,FOpeningTime,FClosingTime,FWebsiteUrl,FLongitude,FLatitude,FRegion,FCategoryId,FCreatedDate,FUpdatedDate,FStatus,FTrafficInformation")] Models.TAttraction tAttraction, List<IFormFile>? FImages) {
        //    //if (id != tAttraction.FAttractionId) {
        //    //    return NotFound();
        //    //}

        //    if (ModelState.IsValid) {
        //        try {
        //            tAttraction.FUpdatedDate = DateTime.Now;
        //            _context.Update(tAttraction);
        //            await _context.SaveChangesAsync();

        //            //檢查表單中是否包含名為 "Picture" 的檔案，若有則進行圖片讀取。
        //            if (FImages != null && FImages.Count > 0) {
        //                foreach (var image in FImages) {
        //                    if (image != null && image.Length > 0) {
        //                        //使用 BinaryReader 讀取圖片檔案的內容，並將其轉換為位元組陣列（byte[]）。
        //                        //FImage.OpenReadStream()：開啟圖片檔案的串流來讀取內容。
        //                        using (BinaryReader br = new BinaryReader(image.OpenReadStream())) {
        //                            //br.ReadBytes(...)：將圖片檔案的內容讀取為位元組陣列，並存入 category.Picture 屬性中。
        //                            TAttractionImage tAttractionImage = new TAttractionImage {
        //                                FAttractionId = tAttraction.FAttractionId,
        //                                FImage = br.ReadBytes((int)image.Length)
        //                            };
        //                            _context.Add(tAttractionImage);
        //                        }
        //                    }
        //                }
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //        catch (DbUpdateConcurrencyException) {
        //            if (!TAttractionExists(tAttraction.FAttractionId)) {
        //                return NotFound();
        //            }
        //            else {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");

        //    return View(GetAttractionViewModels(tAttraction));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit() {

        //    TimeOnly? fOpeningTime, fClosingTime;
        //    if (TimeSpan.TryParse(Request.Form["fOpeningTime"], out TimeSpan ot)) {
        //        fOpeningTime = TimeOnly.FromTimeSpan(ot);
        //    }
        //    else {
        //        fOpeningTime = null;
        //    }
        //    if (TimeSpan.TryParse(Request.Form["fClosingTime"], out TimeSpan ct)) {
        //        fClosingTime = TimeOnly.FromTimeSpan(ct);
        //    }
        //    else {
        //        fClosingTime = null;
        //    }

        //    int? fCategoryId = int.TryParse(Request.Form["fCategoryId"], out int fCategoryIdResult)? fCategoryIdResult:null;
        //    DateTime? fCreatedDate = DateTime.TryParse(Request.Form["fCreatedDate"], out DateTime fCreatedDateResult) ? fCreatedDateResult : null;
        //    DateTime? fUpdatedDate = DateTime.TryParse(Request.Form["fUpdatedDate"], out DateTime fUpdatedDateResult) ? fUpdatedDateResult : null;

        //    var tAttraction = new Models.TAttraction {
        //        FAttractionId = int.Parse(Request.Form["fAttractionId"]),
        //        FAttractionName = Request.Form["fAttractionName"],
        //        FDescription = Request.Form["fDescription"],
        //        FAddress = Request.Form["fAddress"],
        //        FPhoneNumber = Request.Form["fPhoneNumber"],
        //        FOpeningTime = fOpeningTime,
        //        FClosingTime = fClosingTime,
        //        FWebsiteUrl = Request.Form["fWebsiteUrl"],
        //        FRegion = Request.Form["fRegion"],
        //        FCategoryId = fCategoryId,
        //        FStatus = Request.Form["fStatus"],
        //        FTrafficInformation = Request.Form["fTrafficInformation"],
        //        FCreatedDate = fCreatedDate,
        //        FUpdatedDate = fUpdatedDate
        //    };

        //    // 處理圖片
        //    var files = Request.Form.Files;
        //    if (files != null && files.Count > 0) {
        //        foreach (var image in files) {
        //            if (image.Length > 0) {
        //                using (var br = new BinaryReader(image.OpenReadStream())) {
        //                    var tAttractionImage = new TAttractionImage {
        //                        FAttractionId = tAttraction.FAttractionId,
        //                        FImage = br.ReadBytes((int)image.Length)
        //                    };
        //                    _context.Add(tAttractionImage);
        //                }
        //            }
        //        }
        //    }

        //    // 更新資料庫
        //    if (ModelState.IsValid) {
        //        try {
        //            tAttraction.FUpdatedDate = DateTime.Now;
        //            _context.Update(tAttraction);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException) {
        //            if (!TAttractionExists(tAttraction.FAttractionId)) {
        //                return NotFound();
        //            }
        //            else {
        //                throw;
        //            }
        //        }
        //        //var returnData = Json(new {
        //        //    success = true,
        //        //    message = "儲存成功！",
        //        //    updatedData = tAttraction
        //        //});

        //        return new JsonResult(new {
        //            success = true,
        //            message = "儲存成功！",
        //            updatedData = tAttraction
        //        }) {
        //            StatusCode = 200 // 明確指定 200 狀態碼
        //        };


        //        //return returnData;
        //        //return Json(new {
        //        //    success = true,
        //        //    message = "儲存成功！",
        //        //    updatedData = tAttraction
        //        //});

        //    }

        //    //ViewBag.Category = new SelectList(_context.TAttractionCategories, "FAttractionCategoryId", "FAttractionCategoryName");
        //    return Json(new { success = false, message = "資料驗證失敗！" });
        //}

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
    }
}
