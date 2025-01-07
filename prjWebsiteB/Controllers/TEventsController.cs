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
                    FEventUpdatedDate = DateTime.Now,
                    FEventUrl = "images/default-image.jpg"
                };

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }

                    tEvent.FEventUrl = Path.Combine("images", uniqueFileName).Replace("\\", "/");
                }

                _context.TEvents.Add(tEvent);
                await _context.SaveChangesAsync();

                if (model.FEventCategoryId.HasValue)
                {
                    var eventCategoryMapping = new TEventCategoryMapping
                    {
                        FEventId = tEvent.FEventId,
                        FEventCategoryId = model.FEventCategoryId.Value
                    };
                    _context.TEventCategoryMappings.Add(eventCategoryMapping);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

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
                    var existingEvent = await _context.TEvents.FirstOrDefaultAsync(e => e.FEventId == id);
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

                    // 如果有新圖片，儲存並更新路徑
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }

                        existingEvent.FEventUrl = Path.Combine("images", uniqueFileName).Replace("\\", "/");
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
            var tEvent = await _context.TEvents.FirstOrDefaultAsync(e => e.FEventId == id);
            if (tEvent == null || string.IsNullOrEmpty(tEvent.FEventUrl))
            {
                return NotFound();
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tEvent.FEventUrl);
            if (!System.IO.File.Exists(imagePath))
            {
                imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-image.jpg");
            }

            var image = await System.IO.File.ReadAllBytesAsync(imagePath);
            return File(image, "image/jpeg");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tEvent = await _context.TEvents
                .Include(t => t.FUser) // 包含建立者資訊
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
                .Include(e => e.TEventCategoryMappings) // 包含活動類型映射
                .FirstOrDefaultAsync(e => e.FEventId == id); // 根據活動 ID 查找

            if (tEvent != null)
            {
                // 刪除圖片檔案（如果不是預設圖片）
                if (!string.IsNullOrEmpty(tEvent.FEventUrl) && tEvent.FEventUrl != "images/default-image.jpg")
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tEvent.FEventUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath); // 刪除圖片檔案
                    }
                }

                // 刪除關聯的 TEventCategoryMapping
                _context.TEventCategoryMappings.RemoveRange(tEvent.TEventCategoryMappings);

                // 刪除活動
                _context.TEvents.Remove(tEvent);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}