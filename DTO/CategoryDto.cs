namespace TestToken.DTO
{
    public class CategoryDto
    {
        public string Name {  get; set; }
        public string? Image { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
