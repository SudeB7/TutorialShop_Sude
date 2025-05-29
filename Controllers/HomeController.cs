using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TutorialShop.Models;
using TutorialShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TutorialShop.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    // Database connection
    private readonly ApplicationDbContext _context;

    // Manages users and admins
    private readonly UserManager<IdentityUser> _userManager;

    // Dependency injection
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager) {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult About() { return View(); }

    // Upon form submission, reply to the user with feedback.
    [HttpPost]
    public IActionResult Contact(string Name, string Email, string Message) {
        TempData["Message"] = "Thank you for reaching out! We'll get back to you soon.";
        return RedirectToAction("About");
    }
    
    // Action to be taken when a course is bought.
    [Authorize] // User must be logged in
    public async Task<IActionResult> Buy(int courseId) {
        var userId = _userManager.GetUserId(User);

        // Check if already purchased
        bool alreadyBought = await _context.UserCourses
            .AnyAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

        if (alreadyBought) {
            TempData["Message"] = "You already own this course.";
            return RedirectToAction("MyCourses");
        }

        // Add new entry to the database indicating that 
        // the user has bought a course.
        var purchase = new UserCourse {
            UserId = userId,
            CourseId = courseId,
            PurchaseDate = DateTime.UtcNow
        };

        _context.UserCourses.Add(purchase);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Course purchased successfully!";
        // Redirect to course list for the user
        return RedirectToAction("MyCourses");
    }

    // Authorized users may see their bought courses.
    [Authorize]
    public async Task<IActionResult> MyCourses() {
        var userId = _userManager.GetUserId(User);

        var purchasedCourses = await _context.UserCourses
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.Course)
            .ToListAsync();

        return View(purchasedCourses);
    }

    // Only admins may create new courses.
    [Authorize(Roles = "Admin")]
    public IActionResult Create() { return View(); }

    // Only admins may create new courses.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course) {
        if (ModelState.IsValid) {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // Only admins may edit courses.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) return NotFound();

        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        return View(course);
    }

    // Only admins may edit courses.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course) {
        if (id != course.Id) return NotFound();

        if (ModelState.IsValid) {
            try {
                _context.Update(course);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!CourseExists(course.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(course);
    }
    
    // Only admins may delete courses.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (course == null) return NotFound();

        return View(course);
    }

    // Only admins may delete courses.
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
        var course = await _context.Courses.FindAsync(id);
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id) { return _context.Courses.Any(e => e.Id == id); }

    public async Task<IActionResult> Index() {
        var courses = await _context.Courses.ToListAsync();
        var userCourseIds = new List<int>();

        if (User.Identity.IsAuthenticated) {
            var userId = _userManager.GetUserId(User);
            userCourseIds = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync();
        }

        var viewModel = new HomeIndexViewModel {
            Courses = courses,
            UserCourseIds = userCourseIds
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id) {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();
        return View(course);
    }

    public IActionResult Privacy() { return View(); }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
