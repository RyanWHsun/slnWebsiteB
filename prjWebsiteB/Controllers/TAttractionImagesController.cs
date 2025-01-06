using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjWebsiteB.Models;

namespace prjWebsiteB.Controllers
{
    public class TAttractionImagesController : Controller
    {
        private readonly dbGroupBContext _context;

        public TAttractionImagesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TAttractionImages
        public async Task<IActionResult> Index()
        {
            var dbGroupBContext = _context.TAttractionImages.Include(t => t.FAttraction);
            return View(await dbGroupBContext.ToListAsync());
        }

        // GET: TAttractionImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionImage = await _context.TAttractionImages
                .Include(t => t.FAttraction)
                .FirstOrDefaultAsync(m => m.FAttractionImageId == id);
            if (tAttractionImage == null)
            {
                return NotFound();
            }

            return View(tAttractionImage);
        }

        // GET: TAttractionImages/Create
        public IActionResult Create()
        {
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId");
            return View();
        }

        // POST: TAttractionImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FAttractionImageId,FAttractionId,FImage")] TAttractionImage tAttractionImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tAttractionImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionImage.FAttractionId);
            return View(tAttractionImage);
        }

        // GET: TAttractionImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionImage = await _context.TAttractionImages.FindAsync(id);
            if (tAttractionImage == null)
            {
                return NotFound();
            }
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionImage.FAttractionId);
            return View(tAttractionImage);
        }

        // POST: TAttractionImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FAttractionImageId,FAttractionId,FImage")] TAttractionImage tAttractionImage)
        {
            if (id != tAttractionImage.FAttractionImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tAttractionImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TAttractionImageExists(tAttractionImage.FAttractionImageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionImage.FAttractionId);
            return View(tAttractionImage);
        }

        // GET: TAttractionImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionImage = await _context.TAttractionImages
                .Include(t => t.FAttraction)
                .FirstOrDefaultAsync(m => m.FAttractionImageId == id);
            if (tAttractionImage == null)
            {
                return NotFound();
            }

            return View(tAttractionImage);
        }

        // POST: TAttractionImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tAttractionImage = await _context.TAttractionImages.FindAsync(id);
            if (tAttractionImage != null)
            {
                _context.TAttractionImages.Remove(tAttractionImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TAttractionImageExists(int id)
        {
            return _context.TAttractionImages.Any(e => e.FAttractionImageId == id);
        }

        // 取得景點圖片
        public async Task<IActionResult> GetPictures(int id) {
            var tAttractionImages = await _context.TAttractionImages
                                                  .Where(img => img.FAttractionId == id)
                                                  .ToListAsync();

            if (tAttractionImages == null || tAttractionImages.Count == 0) {
                var noImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "noImage.jpeg");
                var noImageBytes = await System.IO.File.ReadAllBytesAsync(noImagePath);
                var noImageBase64 = Convert.ToBase64String(noImageBytes);
                return Json(new List<string> { $"data:image/jpeg;base64,{noImageBase64}" });
            }

            var imagesBase64 = tAttractionImages.Select(img => $"data:image/jpeg;base64,{Convert.ToBase64String(img.FImage)}").ToList();
            return Json(imagesBase64);
        }
    }
}
