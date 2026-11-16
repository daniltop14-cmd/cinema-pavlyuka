// Описывает форму редактирования содержимого.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён System.ComponentModel.DataAnnotations для типов ниже.
using System.ComponentModel.DataAnnotations;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.ViewModels;
// Описывает форму редактирования содержимого.
public class ContentModel
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Записываем вычисленное значение в public string kind { get; set; }.
    public string Kind { get; set; } = "services";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(300)]
    // Записываем вычисленное значение в public string title { get; set; }.
    public string Title { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(3000)]
    // Записываем вычисленное значение в public string text { get; set; }.
    public string Text { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(300)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public string? Extra { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(0, 1000000)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal? Price { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Order { get; set; }
}
