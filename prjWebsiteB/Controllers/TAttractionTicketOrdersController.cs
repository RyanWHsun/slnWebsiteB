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
    public class TAttractionTicketOrdersController : Controller
    {
        private readonly dbGroupBContext _context;

        public TAttractionTicketOrdersController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TAttractionTicketOrders
        public async Task<IActionResult> Index()
        {
            var dbGroupBContext = _context.TAttractionTicketOrders.Include(t => t.FAttractionTicket).Include(t => t.FBuyer);
            return View(await dbGroupBContext.ToListAsync());
        }

        // GET: TAttractionTicketOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionTicketOrder = await _context.TAttractionTicketOrders
                .Include(t => t.FAttractionTicket)
                .Include(t => t.FBuyer)
                .FirstOrDefaultAsync(m => m.FAttractionTicketOrderId == id);
            if (tAttractionTicketOrder == null)
            {
                return NotFound();
            }

            return View(tAttractionTicketOrder);
        }

        // GET: TAttractionTicketOrders/Create
        public IActionResult Create()
        {
            ViewData["FAttractionTicketId"] = new SelectList(_context.TAttractionTickets, "FAttractionTicketId", "FAttractionTicketId");
            ViewData["FBuyerId"] = new SelectList(_context.TUsers, "FUserId", "FUserId");
            return View();
        }

        // POST: TAttractionTicketOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FAttractionTicketOrderId,FBuyerId,FAttractionTicketId,FOrderQty,FUnitPrice,FCreatedDate")] TAttractionTicketOrder tAttractionTicketOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tAttractionTicketOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FAttractionTicketId"] = new SelectList(_context.TAttractionTickets, "FAttractionTicketId", "FAttractionTicketId", tAttractionTicketOrder.FAttractionTicketId);
            ViewData["FBuyerId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionTicketOrder.FBuyerId);
            return View(tAttractionTicketOrder);
        }

        // GET: TAttractionTicketOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionTicketOrder = await _context.TAttractionTicketOrders.FindAsync(id);
            if (tAttractionTicketOrder == null)
            {
                return NotFound();
            }
            ViewData["FAttractionTicketId"] = new SelectList(_context.TAttractionTickets, "FAttractionTicketId", "FAttractionTicketId", tAttractionTicketOrder.FAttractionTicketId);
            ViewData["FBuyerId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionTicketOrder.FBuyerId);
            return View(tAttractionTicketOrder);
        }

        // POST: TAttractionTicketOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FAttractionTicketOrderId,FBuyerId,FAttractionTicketId,FOrderQty,FUnitPrice,FCreatedDate")] TAttractionTicketOrder tAttractionTicketOrder)
        {
            if (id != tAttractionTicketOrder.FAttractionTicketOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tAttractionTicketOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TAttractionTicketOrderExists(tAttractionTicketOrder.FAttractionTicketOrderId))
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
            ViewData["FAttractionTicketId"] = new SelectList(_context.TAttractionTickets, "FAttractionTicketId", "FAttractionTicketId", tAttractionTicketOrder.FAttractionTicketId);
            ViewData["FBuyerId"] = new SelectList(_context.TUsers, "FUserId", "FUserId", tAttractionTicketOrder.FBuyerId);
            return View(tAttractionTicketOrder);
        }

        // GET: TAttractionTicketOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tAttractionTicketOrder = await _context.TAttractionTicketOrders
                .Include(t => t.FAttractionTicket)
                .Include(t => t.FBuyer)
                .FirstOrDefaultAsync(m => m.FAttractionTicketOrderId == id);
            if (tAttractionTicketOrder == null)
            {
                return NotFound();
            }

            return View(tAttractionTicketOrder);
        }

        // POST: TAttractionTicketOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tAttractionTicketOrder = await _context.TAttractionTicketOrders.FindAsync(id);
            if (tAttractionTicketOrder != null)
            {
                _context.TAttractionTicketOrders.Remove(tAttractionTicketOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TAttractionTicketOrderExists(int id)
        {
            return _context.TAttractionTicketOrders.Any(e => e.FAttractionTicketOrderId == id);
        }
    }
}
