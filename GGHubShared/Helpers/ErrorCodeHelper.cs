using System.Collections.Generic;
using GGHubShared.Enums;

namespace GGHubShared.Helpers
{
    public static class ErrorCodeHelper
    {
        private static readonly Dictionary<string, Dictionary<ErrorCode, string>> Messages = new()
        {
            ["en"] = new()
            {
                [ErrorCode.InvalidCredentials] = "Invalid credentials",
                [ErrorCode.UserNotFound] = "User not found",
                [ErrorCode.Unauthorized] = "Unauthorized",
                [ErrorCode.ValidationFailed] = "Validation error",
                [ErrorCode.NotFound] = "Not found",
                [ErrorCode.PaymentError] = "Payment error",
                [ErrorCode.InsufficientBalance] = "Insufficient balance",
                [ErrorCode.DuplicateEmail] = "Email is already taken",
                [ErrorCode.DuplicateUsername] = "Username is already taken",
                [ErrorCode.ServerError] = "Server error"
            },
            ["ua"] = new()
            {
                [ErrorCode.InvalidCredentials] = "Невірні дані",
                [ErrorCode.UserNotFound] = "Користувача не знайдено",
                [ErrorCode.Unauthorized] = "Немає доступу",
                [ErrorCode.ValidationFailed] = "Помилка валідації",
                [ErrorCode.NotFound] = "Не знайдено",
                [ErrorCode.PaymentError] = "Помилка оплати",
                [ErrorCode.InsufficientBalance] = "Недостатньо коштів",
                [ErrorCode.DuplicateEmail] = "Email вже використовується",
                [ErrorCode.DuplicateUsername] = "Ім'я користувача вже використовується",
                [ErrorCode.ServerError] = "Помилка сервера"
            },
            ["ru"] = new()
            {
                [ErrorCode.InvalidCredentials] = "Неверные данные",
                [ErrorCode.UserNotFound] = "Пользователь не найден",
                [ErrorCode.Unauthorized] = "Нет доступа",
                [ErrorCode.ValidationFailed] = "Ошибка валидации",
                [ErrorCode.NotFound] = "Не найдено",
                [ErrorCode.PaymentError] = "Ошибка оплаты",
                [ErrorCode.InsufficientBalance] = "Недостаточно средств",
                [ErrorCode.DuplicateEmail] = "Email уже используется",
                [ErrorCode.DuplicateUsername] = "Имя пользователя уже используется",
                [ErrorCode.ServerError] = "Ошибка сервера"
            }
        };

        public static string GetMessage(ErrorCode code, string language = "en")
        {
            if (Messages.TryGetValue(language, out var map) && map.TryGetValue(code, out var message))
                return message;

            if (Messages["en"].TryGetValue(code, out var defaultMessage))
                return defaultMessage;

            return code.ToString();
        }
    }
}
