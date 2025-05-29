using System;
using Microsoft.AspNetCore.Identity;

namespace TutorialShop.Models {
  public class UserCourse {
      public string? UserId { get; set; }
      public IdentityUser? User { get; set; }

      public int CourseId { get; set; }
      public Course? Course { get; set; }

      public DateTime PurchaseDate { get; set; }
  }
}