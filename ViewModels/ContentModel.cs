// Поля формы редактирования содержимого админки.
using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModels;
public class ContentModel
{
    public int Id { get; set; }
    public string Kind { get; set; } = "services";

    [Required, StringLength(300)]
    public string Title { get; set; } = "";

    [Required, StringLength(3000)]
    public string Text { get; set; } = "";

    [StringLength(300)]
    public string? Extra { get; set; }

    [Range(0, 1000000)]
    public decimal? Price { get; set; }
    public int Order { get; set; }
}
