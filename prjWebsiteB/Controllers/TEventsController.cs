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
                .Include(t => t.FUser)
                .Include(t => t.TEventCategoryMappings)
                .ThenInclude(m => m.FEventCategory)
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
        public async Task<IActionResult> Create(TEventCreateViewModel model, IFormFile uploadedFile)
        {
            if (ModelState.IsValid)
            {
                // 建立 TEvent 實例
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

                // 儲存活動至資料庫
                _context.TEvents.Add(tEvent);
                await _context.SaveChangesAsync();

                // 如果有上傳圖片，將圖片儲存到 TEventImage 表格
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await uploadedFile.CopyToAsync(memoryStream);
                        var tEventImage = new TEventImage
                        {
                            FEventId = tEvent.FEventId,
                            FEventImage = memoryStream.ToArray()
                        };
                        _context.TEventImages.Add(tEventImage);
                        await _context.SaveChangesAsync();
                    }
                }

                // 如果有選擇活動類型，將其儲存到 TEventCategoryMapping 表格
                if (model.FEventCategoryId.HasValue)
                {
                    var eventCategoryMapping = new TEventCategoryMapping
                    {
                        FEventId = tEvent.FEventId, // 關聯活動 ID
                        FEventCategoryId = model.FEventCategoryId.Value // 選擇的活動類型 ID
                    };
                    _context.TEventCategoryMappings.Add(eventCategoryMapping);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // 如果驗證失敗，重新加載選單資料
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
                .Include(e => e.FUser)
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
        public async Task<IActionResult> Edit(int id, TEvent tEvent, IFormFile uploadedFile)
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
                        .Include(e => e.TEventCategoryMappings)
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

                    // 更新圖片（只有當有新圖片上傳時）
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await uploadedFile.CopyToAsync(memoryStream);

                            var existingImage = await _context.TEventImages.FirstOrDefaultAsync(img => img.FEventId == id);
                            if (existingImage != null)
                            {
                                // 更新圖片
                                existingImage.FEventImage = memoryStream.ToArray();
                                _context.TEventImages.Update(existingImage);
                            }
                            else
                            {
                                // 新增圖片
                                var newImage = new TEventImage
                                {
                                    FEventId = id,
                                    FEventImage = memoryStream.ToArray()
                                };
                                _context.TEventImages.Add(newImage);
                            }
                        }
                    }
                    else
                    {
                        // 如果沒有新的圖片，直接略過圖片更新邏輯
                        // 不需要對 existingImage 做任何操作
                    }

                    // 更新活動類型映射
                    var existingMapping = existingEvent.TEventCategoryMappings.FirstOrDefault();
                    if (existingMapping != null)
                    {
                        existingMapping.FEventCategoryId = tEvent.TEventCategoryMappings.FirstOrDefault()?.FEventCategoryId;
                    }
                    else if (tEvent.TEventCategoryMappings.FirstOrDefault()?.FEventCategoryId != null)
                    {
                        var newMapping = new TEventCategoryMapping
                        {
                            FEventId = id,
                            FEventCategoryId = tEvent.TEventCategoryMappings.FirstOrDefault().FEventCategoryId.Value
                        };
                        _context.TEventCategoryMappings.Add(newMapping);
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

            // 如果驗證失敗，重新加載下拉選單並返回視圖
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserName", tEvent.FUserId);
            ViewData["CategoryList"] = new SelectList(_context.TEventCategories, "FEventCategoryId", "FEventCategoryName", tEvent.TEventCategoryMappings.FirstOrDefault()?.FEventCategoryId);

            return View(tEvent);
        }

        private async Task<bool> TEventExists(int id)
        {
            return await _context.TEvents.AnyAsync(e => e.FEventId == id);
        }

        public async Task<IActionResult> GetEventImage(int id)
        {
            var eventImage = await _context.TEventImages.FirstOrDefaultAsync(img => img.FEventId == id);
            if (eventImage == null || eventImage.FEventImage == null)
            {
                var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-image.jpg");
                if (!System.IO.File.Exists(defaultImagePath))
                {
                    return NotFound();
                }

                var defaultImage = await System.IO.File.ReadAllBytesAsync(defaultImagePath);
                return File(defaultImage, "image/jpeg");
            }

            return File(eventImage.FEventImage, "image/jpeg");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tEvent = await _context.TEvents
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FEventId == id);
            if (tEvent == null)
            {
                return NotFound();
            }

            return View(tEvent);
        }

        // POST: TEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tEvent = await _context.TEvents
                .Include(e => e.TEventCategoryMappings)
                .Include(e => e.TEventImages)
                .FirstOrDefaultAsync(e => e.FEventId == id);

            if (tEvent != null)
            {
                // 刪除關聯的 TEventCategoryMapping
                _context.TEventCategoryMappings.RemoveRange(tEvent.TEventCategoryMappings);

                // 刪除關聯的 TEventImage
                _context.TEventImages.RemoveRange(tEvent.TEventImages);

                // 刪除活動
                _context.TEvents.Remove(tEvent);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}