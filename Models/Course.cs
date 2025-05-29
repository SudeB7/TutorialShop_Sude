namespace TutorialShop.Models {
    public class Course {   
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Difficulty { get; set; } 
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public string? ImageUrl { get; set; }
    }
}
