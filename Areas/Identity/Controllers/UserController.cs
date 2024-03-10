// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ControlLight.Areas.Identity.Models.AccountViewModels;
//using ControlLight.Areas.Identity.Models.ManageViewModels;
using ControlLight.Areas.Identity.Models.RoleViewModels;
using ControlLight.Areas.Identity.Models.UserViewModels;
using ControlLight.Data;
using ControlLight.ExtendMethods;
using ControlLight.Models;
using ControlLight.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlLight.Areas.Identity.Controllers
{

    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }



        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage)
        {
            var model = new UserListModel();
            model.currentPage = currentPage;

            var qr = _userManager.Users.OrderBy(u => u.UserName);

            model.totalUsers = await qr.CountAsync();
            model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            if (model.currentPage < 1)
                model.currentPage = 1;
            if (model.currentPage > model.countPages)
                model.currentPage = model.countPages;

            var qr1 = qr.Skip((model.currentPage - 1) * model.ITEMS_PER_PAGE)
                        .Take(model.ITEMS_PER_PAGE)
                        .Select(u => new UserAndRole() {
                            Id = u.Id,
                            UserName = u.UserName,
                            isActive = u.isActive
                        });

            model.users = await qr1.ToListAsync();

            foreach (var user in model.users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.RoleNames = string.Join(",", roles);
            } 
            
            return View(model);
        } 

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            // public SelectList allRoles { get; set; }
            var model = new AddUserRoleModel();
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            //lay thong tin cua user
            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            //lay cac role ma user hien dang co
            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray<string>();

            //lay thong tin cac role cua he thong
            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.allRoles = new SelectList(roleNames);
            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, [Bind("RoleNames")] AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            //lay thong tin user
            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }
           
            //cac role dang co cua user
            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();

            //danh sach nhung role ma user dang co nhung k chon vao nua
            var deleteRoles = OldRoleNames.Where(r => !model.RoleNames.Contains(r));

            //danh sach role moi ma vua chon vao
            var addRoles = model.RoleNames.Where(r => !OldRoleNames.Contains(r));

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            
            ViewBag.allRoles = new SelectList(roleNames);            

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user,deleteRoles);
            if (!resultDelete.Succeeded)
            {
                ModelState.AddModelError(resultDelete);
                return View(model);
            }
            
            var resultAdd = await _userManager.AddToRolesAsync(model.user,addRoles);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError(resultAdd);
                return View(model);
            }

            
            StatusMessage = $"Vừa cập nhật role cho user: {model.user.UserName}";

            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ActiveAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }
            return View(user);
        }

        [HttpPost("{id}"), ActionName("Active")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActiveConfirmAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            if(user.isActive == false)
            {
                user.isActive = true;
            }
            else
            {
                user.isActive = false;
            }

            var result = await _userManager.UpdateAsync(user);

            if( result.Succeeded)
            {
                StatusMessage = $"Cập nhật trạng thái user {user.UserName} thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(user);
        }

        // GET: /User/Delete/
        [HttpGet("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (id == null) return NotFound("Không tìm thấy role");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy User");
            } 
            return View(user);
        }  

        // POST: /User/Delete//1
        [HttpPost("{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmAsync(string id)
        {
            if (id == null) return NotFound("Không tìm thấy User");
            var user = await _userManager.FindByIdAsync(id);
            if  (user == null) return NotFound("Không tìm thấy User");
             
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa: {user.UserName}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(user);
        }        
  }
}
