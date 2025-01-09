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
    public class TUsersController : Controller
    {
        private readonly dbGroupBContext _context;

        public TUsersController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: TUsers
        public async Task<IActionResult> Index(string sortOrder)
        {
            //ViewBag.FUserRankIdSortParm = String.IsNullOrEmpty(sortOrder) ?
            //    "FUserRankId_desc" : "";
            //ViewBag.FUserRankIdSortParm = String.IsNullOrEmpty(sortOrder) ?

            //return View(_context.TUsers);
            return View(
                _context.TUsers.Select(u => new TUser
            {
                FUserId=u.FUserId,
                FUserRankId = u.FUserRankId,
                FUserName = u.FUserName,
                FUserNickName = u.FUserNickName,
                FUserSex = u.FUserSex,
                FUserBirthday = u.FUserBirthday,
                FUserPhone = u.FUserPhone,
                FUserEmail = u.FUserEmail,
                FUserAddress = u.FUserAddress,
                FUserComeDate = u.FUserComeDate,
                FUserPassword = u.FUserPassword,
                FUserImage = null
            }));
        }

        public JsonResult IndexJson()
        {
            return Json(_context.TUsers);
        }

        public async Task<FileResult> GetPicture(int id)
        {
            TUser? user = await _context.TUsers.FindAsync(id);
            byte[]? content = user?.FUserImage;
            return File(content, "image/jpeg");
        }


        // GET: TUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tUser = await _context.TUsers.Select(u => new TUser
            {
                FUserId = u.FUserId,
                FUserRankId = u.FUserRankId,
                FUserName = u.FUserName,
                FUserNickName = u.FUserNickName,
                FUserSex = u.FUserSex,
                FUserBirthday = u.FUserBirthday,
                FUserPhone = u.FUserPhone,
                FUserEmail = u.FUserEmail,
                FUserAddress = u.FUserAddress,
                FUserComeDate = u.FUserComeDate,
                FUserPassword = u.FUserPassword,
                FUserImage = null
            }).FirstOrDefaultAsync(m => m.FUserId == id);
            if (tUser == null)
            {
                return NotFound();
            }
            return View(tUser);
        }

        // GET: TUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FUserId,FUserRankId,FUserName,FUserImage,FUserNickName,FUserSex,FUserBirthday,FUserPhone,FUserEmail,FUserAddress,FUserComeDate,FUserPassword,FUserNotify,FUserOnLine")] TUser tUser)
        {
            if (ModelState.IsValid)
            {
                if (Request.Form.Files["FUserImage"] != null)
                {
                    using (BinaryReader br = new BinaryReader(Request.Form.Files["FUserImage"].OpenReadStream()))
                    {
                        //將圖片內容讀進去
                        tUser.FUserImage = br.ReadBytes((int)Request.Form.Files["FUserImage"].Length);
                    }
                }
                _context.Add(tUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tUser);
        }

        // GET: TUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tUser = await _context.TUsers.Select(u => new TUser
            {
                FUserId = u.FUserId,
                FUserRankId = u.FUserRankId,
                FUserName = u.FUserName,
                FUserNickName = u.FUserNickName,
                FUserSex = u.FUserSex,
                FUserBirthday = u.FUserBirthday,
                FUserPhone = u.FUserPhone,
                FUserEmail = u.FUserEmail,
                FUserAddress = u.FUserAddress,
                FUserComeDate = u.FUserComeDate,
                FUserPassword = u.FUserPassword,
                FUserImage = null
            }).FirstOrDefaultAsync(m => m.FUserId == id);
            if (tUser == null)
            {
                return NotFound();
            }
            return View(tUser);
        }

        // POST: TUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //限制圖片大小
        [RequestFormLimits(MultipartBodyLengthLimit =8192000)]
        [RequestSizeLimit(8192000)]
        public async Task<IActionResult> Edit(int id, [Bind("FUserId,FUserRankId,FUserName,FUserImage,FUserNickName,FUserSex,FUserBirthday,FUserPhone,FUserEmail,FUserAddress,FUserComeDate,FUserPassword,FUserNotify,FUserOnLine")] TUser tUser)
        {
            if (id != tUser.FUserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //取得原圖
                TUser u = await _context.TUsers.FindAsync(tUser.FUserId);
                if (Request.Form.Files["FUserImage"] != null)
                {
                    using (BinaryReader br = new BinaryReader(Request.Form.Files["FUserImage"].OpenReadStream()))
                    {
                        //將圖片內容讀進去
                        tUser.FUserImage=br.ReadBytes((int)Request.Form.Files["FUserImage"].Length);
                    }
                }
                else {
                    //未上傳圖案維持原值
                    tUser.FUserImage = u.FUserImage;
                }
                //卸離u,只追蹤tUser
                _context.Entry(u).State=EntityState.Detached;

                try
                {
                    _context.Update(tUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TUserExists(tUser.FUserId))
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
            return View(tUser);
        }

        // GET: TUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tUser = await _context.TUsers.Select(u => new TUser
            {
                FUserId = u.FUserId,
                FUserRankId = u.FUserRankId,
                FUserName = u.FUserName,
                FUserNickName = u.FUserNickName,
                FUserSex = u.FUserSex,
                FUserBirthday = u.FUserBirthday,
                FUserPhone = u.FUserPhone,
                FUserEmail = u.FUserEmail,
                FUserAddress = u.FUserAddress,
                FUserComeDate = u.FUserComeDate,
                FUserPassword = u.FUserPassword,
                FUserImage = null
            }).FirstOrDefaultAsync(m => m.FUserId == id);
            if (tUser == null)
            {
                return NotFound();
            }

            return View(tUser);
        }

        // POST: TUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tUser = await _context.TUsers.FindAsync(id);
            if (tUser != null)
            {
                _context.TUsers.Remove(tUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TUserExists(int id)
        {
            return _context.TUsers.Any(e => e.FUserId == id);
        }
    }
}
