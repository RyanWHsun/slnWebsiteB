using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjWebsiteB.Models;
using prjWebsiteB.ViewModels;

namespace prjWebsiteB.Controllers
{
    public class TEventsController : Controller
    {
        private readonly dbGroupBContext _context;

        public TEventsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TEvents
        public async Task<IActionResult> Index(string search, string sortOrder, int? userId)
        {
            // 包含建立者的查詢
            var events = _context.TEvents
                .Include(e => e.FUser)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(search))
            {
                events = events.Where(e =>
                    e.FEventName.Contains(search) ||
                    e.FEventDescription.Contains(search) ||
                    e.FUser.FUserName.Contains(search)); // 加入建立者名字的搜尋
            }

            // 篩選建立者
            if (userId.HasValue)
            {
                events = events.Where(e => e.FUserId == userId.Value);
            }

            // 排序功能
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            events = sortOrder switch
            {
                "name_desc" => events.OrderByDescending(e => e.FEventName),
                "date_asc" => events.OrderBy(e => e.FEventStartDate),
                "date_desc" => events.OrderByDescending(e => e.FEventStartDate),
                _ => events.OrderBy(e => e.FEventName),
            };

            // 傳遞建立者的篩選清單到 View
            ViewData["UserList"] = new SelectList(await _context.TUsers.ToListAsync(), "FUserId", "FUserName");

            return View(await events.ToListAsync());
        }

        // GET: TEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tEvent = await _context.TEvents
                .Include(t => t.TEventImages) // 包含圖片資料
                .FirstOrDefaultAsync(m => m.FEventId == id);

            if (tEvent == null)
            {
                return NotFound();
            }

            return View(tEvent);
        }

        // GET: TEvents/Create
        public IActionResult Create()
        {
            // 提供使用者選單
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserName");
            // 提供活動類型選單
            ViewData["CategoryList"] = new SelectList(_context.TEventCategories, "FEventCategoryId", "FEventCategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TEventCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tEvent = new TEvent
                {
                    FEventName = model.FEventName,
                    FEventDescription = model.FEventDescription,
                    FEventStartDate = model.FEventStartDate,
                    FEventEndDate = model.FEventEndDate,
                    FEventLocation = model.FEventLocation,
                    FEventActivityfee = model.FEventActivityfee,
                    FUserId = model.FUserId,
                    FEventIsActive = model.FEventIsActive,
                    FEventMaxParticipants = model.FEventMaxParticipants,
                    FEventCreatedDate = model.FEventCreatedDate ?? DateTime.Now,
                    FEventUpdatedDate = DateTime.Now
                };

                _context.TEvents.Add(tEvent);
                await _context.SaveChangesAsync();

                // 儲存活動類型對應關係到 TEventCategoryMapping 表
                if (model.FEventCategoryId.HasValue)
                {
                    var eventCategoryMapping = new TEventCategoryMapping
                    {
                        FEventId = tEvent.FEventId, // 新增活動的 ID
                        FEventCategoryId = model.FEventCategoryId.Value // 選擇的類型 ID
                    };

                    _context.TEventCategoryMappings.Add(eventCategoryMapping);
                    await _context.SaveChangesAsync();
                }

                // 儲存圖片到 TEventImage 表
                if (model.UploadedFiles != null && model.UploadedFiles.Any())
                {
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        if (uploadedFile.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await uploadedFile.CopyToAsync(memoryStream);

                                var tEventImage = new TEventImage
                                {
                                    FEventId = tEvent.FEventId, // 新增活動的 ID
                                    FEventImage = memoryStream.ToArray() // 儲存為二進位資料
                                };

                                _context.TEventImages.Add(tEventImage);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // 如果驗證失敗，重新載入下拉選單資料
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserName", model.FUserId);
            ViewData["CategoryList"] = new SelectList(_context.TEventCategories, "FEventCategoryId", "FEventCategoryName", model.FEventCategoryId);
            return View(model);
        }

        // GET: TEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tEvent = await _context.TEvents
                .Include(e => e.FUser) // 包含建立者資訊
                .Include(e => e.TEventImages) // 包含圖片資料
                .FirstOrDefaultAsync(e => e.FEventId == id);

            if (tEvent == null)
            {
                return NotFound();
            }

            // 加入建立者的下拉選單資料
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserName", tEvent.FUserId);

            return View(tEvent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TEvent tEvent, List<IFormFile> uploadedFiles)
        {
            if (id != tEvent.FEventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.TEvents
                        .Include(e => e.TEventImages) // 包含圖片資料
                        .FirstOrDefaultAsync(e => e.FEventId == id);

                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    // 更新活動基本資料
                    existingEvent.FEventName = tEvent.FEventName;
                    existingEvent.FEventDescription = tEvent.FEventDescription;
                    existingEvent.FEventStartDate = tEvent.FEventStartDate;
                    existingEvent.FEventEndDate = tEvent.FEventEndDate;
                    existingEvent.FEventLocation = tEvent.FEventLocation;
                    existingEvent.FEventActivityfee = tEvent.FEventActivityfee;
                    existingEvent.FEventMaxParticipants = tEvent.FEventMaxParticipants;
                    existingEvent.FEventIsActive = tEvent.FEventIsActive;
                    existingEvent.FEventUpdatedDate = DateTime.Now;

                    // 如果有新圖片，儲存到資料庫
                    if (uploadedFiles != null && uploadedFiles.Any())
                    {
                        foreach (var uploadedFile in uploadedFiles)
                        {
                            if (uploadedFile.Length > 0)
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    await uploadedFile.CopyToAsync(memoryStream);

                                    var tEventImage = new TEventImage
                                    {
                                        FEventId = existingEvent.FEventId,
                                        FEventImage = memoryStream.ToArray() // 儲存二進位資料
                                    };

                                    _context.TEventImages.Add(tEventImage);
                                }
                            }
                        }
                    }

                    _context.Update(existingEvent);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TEventExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(tEvent);
        }

        private async Task<bool> TEventExists(int id)
        {
            return await _context.TEvents.AnyAsync(e => e.FEventId == id);
        }

        public async Task<IActionResult> GetEventImage(int id)
        {
            var tEventImage = await _context.TEventImages.FirstOrDefaultAsync(img => img.FEventImageId == id);
            if (tEventImage == null || tEventImage.FEventImage == null)
            {
                // 返回默認佔位圖片
                var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-image.jpg");
                var defaultImage = await System.IO.File.ReadAllBytesAsync(defaultImagePath);
                return File(defaultImage, "image/jpeg");
            }

            // 返回圖片數據
            return File(tEventImage.FEventImage, "image/jpeg");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tEvent = await _context.TEvents
                .Include(t => t.FUser) // 包含建立者資訊
                .Include(t => t.TEventImages) // 包含圖片資料
                .FirstOrDefaultAsync(m => m.FEventId == id);

            if (tEvent == null)
            {
                return NotFound();
            }

            return View(tEvent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tEvent = await _context.TEvents
                .Include(e => e.TEventImages) // 包含圖片資料
                .FirstOrDefaultAsync(e => e.FEventId == id);

            if (tEvent != null)
            {
                // 刪除相關圖片
                if (tEvent.TEventImages != null && tEvent.TEventImages.Any())
                {
                    _context.TEventImages.RemoveRange(tEvent.TEventImages);
                }

                // 刪除活動
                _context.TEvents.Remove(tEvent);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetDetails(int id)
        {
            var tEvent = await _context.TEvents
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(t => t.FEventId == id);

            if (tEvent == null)
            {
                return NotFound();
            }

            return PartialView("_EventDetailsPartial", tEvent);
        }
    }
}