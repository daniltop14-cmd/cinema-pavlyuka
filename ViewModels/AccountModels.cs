// Модели форм аккаунта: атрибуты проверки применяются к данным, отправленным браузером.
using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModels;
public class LoginModel
{
    [Required(ErrorMessage = "Введите email"), EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterModel
{
    [Required, StringLength(80), Display(Name = "Имя")]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "Пароли не совпадают"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileModel
{
    [Required, StringLength(80)]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";
}

public class PasswordModel
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
