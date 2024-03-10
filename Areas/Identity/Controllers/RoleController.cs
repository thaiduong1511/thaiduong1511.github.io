// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ControlLight.Areas.Identity.Models.RoleViewModels;
using ControlLight.Data;
using ControlLight.ExtendMethods;
using ControlLight.Models;
using ControlLight.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlLight.Areas.Identity.Controllers
{

    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/Role/[action]")]
    public class RoleController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        public IdentityRole role { get; set; }

        [BindProperty]
        public EditRoleModel InputModel {get; set;}

        public RoleController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /Role/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
           var r = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
           var roles = new List<RoleModel>();
           foreach (var _r in r)
           {
               var claims = await _roleManager.GetClaimsAsync(_r);
               var claimsString = claims.Select(c => c.Type  + "=" + c.Value);

               var rm = new RoleModel()
               {
                   Name = _r.Name,
                   Id = _r.Id,
               };
               roles.Add(rm);
           }

            return View(roles);
        } 

        // GET: /Role/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        
        // POST: /Role/Create
        [HttpPost, ActionName(nameof(Create))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateRoleModel model)
        {
            if  (!ModelState.IsValid)
            {
                return View();
            }

            var newRole = new IdentityRole(model.Name);
            var result = await _roleManager.CreateAsync(newRole);
   
            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa tạo role mới: {model.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View();
        }     

        // GET: /Role/Delete/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> DeleteAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 
            return View(role);
        }
        
        // POST: /Role/Edit/1
        [HttpPost("{roleid}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if  (role == null) return NotFound("Không tìm thấy role");
             
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa: {role.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(role);
        }     

        // GET: /Role/Edit/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> EditAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
               return NotFound("Khong tim thay role");
            }
            InputModel = new EditRoleModel()
            {
                Name = role.Name
            };
            return View(InputModel);
            
        }
        
        // POST: /Role/Edit/1
        [HttpPost("{roleid}"), ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 
            //model.role = role;
            if (!ModelState.IsValid)
            {
                return View();
            }
    
            role.Name = InputModel.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa đổi tên: {InputModel.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(InputModel);
        }

        // GET: /Role/AddRoleClaim/roleid
          

        // POST: /Role/AddRoleClaim/roleid
       
        // GET: /Role/EditRoleClaim/claimid
        
        // GET: /Role/EditRoleClaim/claimid
       
        // POST: /Role/EditRoleClaim/claimid
       


    }
}
