using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
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
            var dbGroupBContext = _context.TPosts.Include(t => t.TPostImages);
            return View(dbGroupBContext);
        }
        public async Task<IActionResult> Posts()
        {
            int loginId = 1; //待串登入資料
            var dbGroupBContext = _context.TPosts.Include(t => t.TPostImages).Where(t => t.FUserId == loginId);
            return View(dbGroupBContext);
        }
        public IActionResult Create()
        {
            ViewBag.FIsPublic = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "公開" },
                new SelectListItem { Value = "false", Text = "私人" }
            };
            return PartialView("_CreatePartial", new prjWebsiteB.Models.TPost());
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("FTitle", "FContent", "FIsPublic")] TPost post)
        {
            int loginId = 1; //待串登入資料
            if (Request.Form.Files["FImage"] != null)
            {
                using (BinaryReader reader = new BinaryReader(Request.Form.Files["FImage"].OpenReadStream()))
                {
                    byte[] imageData = reader.ReadBytes((int)Request.Form.Files["FImage"].Length);
                    TPostImage postImage = new TPostImage
                    {
                        FImage = imageData
                    };
                    post.TPostImages.Add(postImage);
                }
            }
            post.FUserId = loginId;
            post.FCreatedAt = DateTime.Now;
            _context.Add(post);
            await _context.SaveChangesAsync();
            var dbGroupBContext = _context.TPosts.Include(t => t.TPostImages).Where(t => t.FUserId == loginId);
            return PartialView("_UserPostsPartial", dbGroupBContext);
        }

        // GET: TPosts/Search/searchString
        [Route("/TPosts/Search/{searchString?}")]
        public async Task<IActionResult> Search(string searchString)
        {
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.TPostImages);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_PostListPartial", dbGroupBContext);
        }
        // GET: TPosts/SearchByUser
        [Route("/TPosts/SearchByUser/{searchString?}")]
        public async Task<IActionResult> SearchByUser(string searchString)
        {
            int loginId = 1; //待串登入資料
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.TPostImages).Where(e=>e.FUserId==loginId);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_UserPostsPartial", dbGroupBContext);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool isPublic, string searchString)
        {
            ModelState.Remove("searchString");
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
            tPost.FUpdatedAt = DateTime.Now;
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
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.TPostImages);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_PostListPartial", dbGroupBContext);
        }

        // GET: TPosts/Details/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.FIsPublic = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "公開" },
                new SelectListItem { Value = "false", Text = "私人" }
            };
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_EditPartial", tPost);
        }
        public async Task<IActionResult> EditByUser(int? id)
        {
            ViewBag.FIsPublic = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "公開" },
                new SelectListItem { Value = "false", Text = "私人" }
            };
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_EditByUserPartial", tPost);
        }
        [HttpPost]
        public async Task<IActionResult> EditByUser(TPost post)
        {
            var tImage = _context.TPostImages.FirstOrDefault(i => i.FPostId == post.FPostId);
            if (Request.Form.Files["FImage"] != null)
            {
                using (BinaryReader reader = new BinaryReader(Request.Form.Files["FImage"].OpenReadStream()))
                {
                    byte[] imageData = reader.ReadBytes((int)Request.Form.Files["FImage"].Length);
                    tImage.FImage = imageData;
                    _context.Update(tImage);
                    await _context.SaveChangesAsync();

                }
            }
            if (post.FPostId == null)
            {
                return NotFound();
            }

            var tPost = _context.TPosts.FirstOrDefault(e=>e.FPostId==post.FPostId);

            if (tPost == null)
            {
                return NotFound();
            }
            
            tPost.FTitle = post.FTitle;
            tPost.FContent = post.FContent;
            tPost.FIsPublic = post.FIsPublic;
            tPost.FUpdatedAt = DateTime.Now;
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
            
            int loginId = 1; //待串登入資料
            var dbGroupBContext = _context.TPosts.Include(t => t.TPostImages).Where(t => t.FUserId == loginId);
            return PartialView("_UserPostsPartial", dbGroupBContext);
        }

        // GET: TPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_DeletePartial", tPost);
        }
        public async Task<IActionResult> DeleteByUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteByUserPartial", tPost);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string searchString, string page)
        {
            var tPost = await _context.TPosts.FindAsync(id);
            if (tPost != null)
            {
                _context.TPosts.Remove(tPost);
            }
            await _context.SaveChangesAsync();

            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.TPostImages);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_PostListPartial", dbGroupBContext);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteByUser(int id, string searchString, string page)
        {
            var tPost = await _context.TPosts.FindAsync(id);
            if (tPost != null)
            {
                _context.TPosts.Remove(tPost);
            }
            await _context.SaveChangesAsync();
            int loginId = 1; //待串登入資料
            IQueryable<TPost> dbGroupBContext = _context.TPosts.Include(t => t.TPostImages).Where(e=>e.FUserId == loginId);
            if (!string.IsNullOrEmpty(searchString))
            {
                dbGroupBContext = dbGroupBContext.Where(e => e.FTitle.Contains(searchString) || e.FContent.Contains(searchString));
            }
            return PartialView("_UserPostsPartial", dbGroupBContext);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", tPost);
        }
        public async Task<IActionResult> DetailsByUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tPost = await _context.TPosts
                .Include(t => t.TPostImages)
                .FirstOrDefaultAsync(m => m.FPostId == id);
            if (tPost == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsByUserPartial", tPost);
        }

        private bool TPostExists(int id)
        {
            return _context.TPosts.Any(e => e.FPostId == id);
        }
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<FileResult> GetPicture(int id)
        {
            var tPost = _context.TPosts.Include(t => t.TPostImages).FirstOrDefault(p => p.FPostId == id);
            var image = tPost?.TPostImages.FirstOrDefault().FImage;
            return File(image, "image/jpeg");
        }
    }
}
