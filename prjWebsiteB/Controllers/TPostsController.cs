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
    public class TPostsController : Controller
    {
        private readonly dbGroupBContext _context;

        public TPostsController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TPosts/Index
        public async Task<IActionResult> Index()
        {
            var dbGroupBContext = _context.TPosts.Include(t => t.FCategory);
            return View(dbGroupBContext);
        }

        // GET: TPosts/Search/searchString
        [Route("/TPosts/Search/{searchString?}")]
        public async Task<IActionResult> Search(string searchString)
        {
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.FCategory);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_PostListPartial", dbGroupBContext);
        }

        // GET: TPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.FCategory)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return View(tPost);
        }

        // GET: TPosts/Create
        public IActionResult Create()
        {
            ViewData["FCategoryId"] = new SelectList(_context.TPostCategories, "FCategoryId", "FCategoryId");
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId");
            return View();
        }

        // POST: TPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FPostId,FUserId,FTitle,FContent,FCreatedAt,FUpdatedAt,FIsPublic,FCategoryId")] TPost tPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FCategoryId"] = new SelectList(_context.TPostCategories, "FCategoryId", "FCategoryId", tPost.FCategoryId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tPost.FUserId);
            return View(tPost);
        }

        // GET: TPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts.FindAsync(id);
            if (tPost == null)
            {
                return NotFound();
            }
            ViewData["FCategoryId"] = new SelectList(_context.TPostCategories, "FCategoryId", "FName", tPost.FCategoryId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tPost.FUserId);
            return View(tPost);
        }

        // POST: TPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FPostId,FUserId,FTitle,FContent,FCreatedAt,FUpdatedAt,FIsPublic,FCategoryId")] TPost tPost)
        {
            if (id != tPost.FPostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TPostExists(tPost.FPostId))
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
            ViewData["FCategoryId"] = new SelectList(_context.TPostCategories, "FCategoryId", "FCategoryId", tPost.FCategoryId);
            ViewData["FUserId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tPost.FUserId);
            return View(tPost);
        }

        // GET: TPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.FCategory)
                .Include(t => t.FUser)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return View(tPost);
        }

        // POST: TPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tPost = await _context.TPosts.FindAsync(id);
            if (tPost != null)
            {
                _context.TPosts.Remove(tPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TPostExists(int id)
        {
            return _context.TPosts.Any(e => e.FPostId == id);
        }
    }
}
