// Описывает и проверяет поля форм аккаунта.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён System.ComponentModel.DataAnnotations для типов ниже.
using System.ComponentModel.DataAnnotations;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.ViewModels;
// Описывает и проверяет поля форм аккаунта.
public class LoginModel
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required(ErrorMessage = "Введите email"), EmailAddress]
    // Записываем вычисленное значение в public string email { get; set; }.
    public string Email { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
    // Записываем вычисленное значение в public string password { get; set; }.
    public string Password { get; set; } = "";
    // Свойство хранит значение модели и доступно другим компонентам.
    public bool RememberMe { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public string? ReturnUrl { get; set; }
}

// Описывает и проверяет поля форм аккаунта.
public class RegisterModel
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(80), Display(Name = "Имя")]
    // Записываем вычисленное значение в public string display name { get; set; }.
    public string DisplayName { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, EmailAddress]
    // Записываем вычисленное значение в public string email { get; set; }.
    public string Email { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, MinLength(8), DataType(DataType.Password)]
    // Записываем вычисленное значение в public string password { get; set; }.
    public string Password { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, Compare(nameof(Password), ErrorMessage = "Пароли не совпадают"), DataType(DataType.Password)]
    // Записываем вычисленное значение в public string confirm password { get; set; }.
    public string ConfirmPassword { get; set; } = "";
}

// Описывает и проверяет поля форм аккаунта.
public class ProfileModel
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(80)]
    // Записываем вычисленное значение в public string display name { get; set; }.
    public string DisplayName { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, EmailAddress]
    // Записываем вычисленное значение в public string email { get; set; }.
    public string Email { get; set; } = "";
}

// Описывает и проверяет поля форм аккаунта.
public class PasswordModel
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, DataType(DataType.Password)]
    // Записываем вычисленное значение в public string current password { get; set; }.
    public string CurrentPassword { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, MinLength(8), DataType(DataType.Password)]
    // Записываем вычисленное значение в public string new password { get; set; }.
    public string NewPassword { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    // Записываем вычисленное значение в public string confirm password { get; set; }.
    public string ConfirmPassword { get; set; } = "";
}
