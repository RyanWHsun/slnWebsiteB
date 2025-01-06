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
    public class TAttractionCommentsController : Controller
    {
        private readonly dbGroupBContext _context;

        public TAttractionCommentsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TAttractionComments
        public async Task<IActionResult> Index()
        {
            var dbGroupBContext = _context.TAttractionComments.Include(t => t.FAttraction).Include(t => t.FUser);
            return View(await dbGroupBContext.ToListAsync());
        }

        // GET: TAttractionComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionComment = await _context.TAttractionComments
                .Include(t => t.FAttraction)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FCommentId == id);
            if (tAttractionComment == null)
            {
                return NotFound();
            }

            return View(tAttractionComment);
        }

        // GET: TAttractionComments/Create
        public IActionResult Create()
        {
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId");
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId");
            return View();
        }

        // POST: TAttractionComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FCommentId,FAttractionId,FUserId,FRating,FComment,FCreatedDate")] TAttractionComment tAttractionComment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tAttractionComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionComment.FAttractionId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionComment.FUserId);
            return View(tAttractionComment);
        }

        // GET: TAttractionComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionComment = await _context.TAttractionComments.FindAsync(id);
            if (tAttractionComment == null)
            {
                return NotFound();
            }
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionComment.FAttractionId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionComment.FUserId);
            return View(tAttractionComment);
        }

        // POST: TAttractionComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FCommentId,FAttractionId,FUserId,FRating,FComment,FCreatedDate")] TAttractionComment tAttractionComment)
        {
            if (id != tAttractionComment.FCommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tAttractionComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TAttractionCommentExists(tAttractionComment.FCommentId))
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
            ViewData["FAttractionId"] = new SelectList(_context.TAttractions, "FAttractionId", "FAttractionId", tAttractionComment.FAttractionId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionComment.FUserId);
            return View(tAttractionComment);
        }

        // GET: TAttractionComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionComment = await _context.TAttractionComments
                .Include(t => t.FAttraction)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FCommentId == id);
            if (tAttractionComment == null)
            {
                return NotFound();
            }

            return View(tAttractionComment);
        }

        // POST: TAttractionComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tAttractionComment = await _context.TAttractionComments.FindAsync(id);
            if (tAttractionComment != null)
            {
                _context.TAttractionComments.Remove(tAttractionComment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TAttractionCommentExists(int id)
        {
            return _context.TAttractionComments.Any(e => e.FCommentId == id);
        }
    }
}
