using TutorialShop.Models;
using System.Collections.Generic;

namespace TutorialShop.Models {
  public class HomeIndexViewModel {
      public List<Course>? Courses { get; set; }
      public List<int>? UserCourseIds { get; set; } 
  }
}