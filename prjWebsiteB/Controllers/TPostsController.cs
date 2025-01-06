﻿using System;
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
        [HttpPost]
        public async Task<IActionResult> Update(int id, bool isPublic)
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

            tPost.FIsPublic = isPublic;

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
            }
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.FCategory);
            
            return PartialView("_PostListPartial", dbGroupBContext);
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
