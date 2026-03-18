namespace GGHubBot.Services
{
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations;

        public LocalizationService()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["ua"] = new()
                {
                    ["welcome"] = "🎮 Ласкаво просимо до CS2 Duels!\n\nОберіть мову:",
                    ["language_selected"] = "✅ Мову обрано",
                    ["main_menu"] = "🏠 Головне меню",
                    ["duels"] = "⚔️ Дуелі",
                    ["tournaments"] = "🏆 Турніри",
                    ["profile"] = "👤 Профіль",
                    ["wallet"] = "💰 Гаманець",
                    ["settings"] = "⚙️ Налаштування",
                    ["help"] = "🆘 Допомога",

                    ["create_duel"] = "🆕 Створити дуель",
                    ["find_duel"] = "🔍 Знайти дуель",
                    ["my_duels"] = "Мої дуелі",
                    ["join_by_link"] = "🔗 Приєднатися за посиланням",
                    ["enter_invite_link"] = "🔗 Надішліть посилання на дуель:",
                    ["enter_invite_code"] = "🎫 Надішліть код запрошення:",
                    ["duel_joined"] = "✅ Ви приєдналися до дуелі! Очікуємо внесок депозиту за дуель",

                    ["create_tournament"] = "🆕 Створити турнір",
                    ["available_tournaments"] = "🔍 Доступні турніри",
                    ["my_tournaments"] = "👥 Мої турніри",
                    ["join_by_code"] = "🎫 Приєднатися за кодом",

                    ["balance"] = "💵 Баланс",
                    ["deposit"] = "➕ Поповнити",
                    ["withdraw"] = "➖ Вивести",
                    ["transaction_history"] = "📜 Історія транзакцій",

                    ["auth_required"] = "🔐 Для продовження потрібна авторизація через Steam",
                    ["login_steam"] = "🔐 Увійти через Steam",
                    ["back"] = "⬅️ Назад",
                    ["cancel"] = "❌ Скасувати",
                    ["confirm"] = "✅ Підтвердити",
                    ["next"] = "➡️ Далі",
                    ["previous"] = "⬅️ Попередній крок",

                    ["step"] = "Крок",
                    ["of"] = "з",

                    ["duel_format"] = "Оберіть формат дуеля:",
                    ["entry_fee"] = "Введіть entry fee (від €5):",
                    ["select_maps"] = "Оберіть карти:",
                    ["round_format"] = "Оберіть формат раундів:",
                    ["confirm_duel"] = "Підтвердіть створення дуеля:",
                    ["prime_only"] = "Тільки Prime?",
                    ["warmup_time"] = "Тривалість розминки:",
                    ["max_rounds"] = "Кількість раундів:",
                    ["pay"] = "Заплатити",
                    ["pay_balance"] = "Оплата з балансу",
                    ["pay_cryptomus"] = "Оплата Cryptomus",
                    ["get_server"] = "Отримати сервер",
                    ["join_steam"] = "Запустити в Steam",
                    ["server"] = "Сервер",
                    ["password"] = "Пароль",
                    ["entry_fee_paid"] = "✅ Внесок прийнято! 🎉",
                    ["payment_status"] = "Статус платежу: {0}",
                    ["both_deposits_paid"] = "✅ Депозит прийнято від обох сторін! Підтвердьте готовність коли будете готові розпочати",
                    ["waiting_opponent"] = "⏳ Очікуємо оплату від супротивника. Ви отримаєте сповіщення після його оплати",
                    ["waiting_opponent_ready"] = "⏳ Очікується готовність від супротивника",
                    ["payment_expired"] = "⌛ Час платежу вийшов. Спробуйте знову",
                    ["check_payment"] = "Перевірити",
                    ["press_ready"] = "Натисніть 'Готов', коли будете готові",
                    ["ready"] = "Готово",
                    ["forfeit_match"] = "❌ Покинути матч",

                    ["tournament_title"] = "Введіть назву турніру:",
                    ["tournament_description"] = "Введіть опис турніру (або пропустіть):",
                    ["players_per_team"] = "Кількість гравців у команді:",
                    ["max_teams"] = "Максимальна кількість команд:",
                    ["tournament_entry_fee"] = "Entry fee для команди:",
                    ["server_start"] = "Очікуйте запуск серверу (приблизно 20 секунд)\nВи отримаєте сповіщення з даниими підключення до серверу",

                    ["error"] = "❌ Помилка",
                    ["success"] = "✅ Успішно",
                    ["loading"] = "⏳ Завантаження...",
                    ["no_data"] = "📭 Немає даних",

                    ["duel_created"] = "✅ Дуель створено успішно!",
                    ["opponent_joined"] = "✅ Супротивник приєднався до дуелі! Очікуємо внесок депозиту за дуель",
                    ["tournament_created"] = "✅ Турнір створено успішно!",
                    ["duel_invite_rules"] = "Приєднайтесь за посиланням протягом 15 хв та підтвердьте готовність. Матч почнеться після підтвердження усіх гравців.",
                    ["invalid_amount"] = "❌ Невірна сума",
                    ["insufficient_balance"] = "❌ Недостатньо коштів"
                },

                ["en"] = new()
                {
                    ["welcome"] = "🎮 Welcome to CS2 Duels!\n\nSelect language:",
                    ["language_selected"] = "✅ Language selected",
                    ["main_menu"] = "🏠 Main menu",
                    ["duels"] = "⚔️ Duels",
                    ["tournaments"] = "🏆 Tournaments",
                    ["profile"] = "👤 Profile",
                    ["wallet"] = "💰 Wallet",
                    ["settings"] = "⚙️ Settings",
                    ["help"] = "🆘 Help",

                    ["create_duel"] = "🆕 Create duel",
                    ["find_duel"] = "🔍 Find duel",
                    ["my_duels"] = "My duels",
                    ["join_by_link"] = "🔗 Join by link",
                    ["enter_invite_link"] = "🔗 Send duel invite link:",
                    ["enter_invite_code"] = "🎫 Send invite code:",
                    ["duel_joined"] = "✅ Joined the duel! Awaiting deposit for the duel",

                    ["create_tournament"] = "🆕 Create tournament",
                    ["available_tournaments"] = "🔍 Available tournaments",
                    ["my_tournaments"] = "👥 My tournaments",
                    ["join_by_code"] = "🎫 Join by code",

                    ["balance"] = "💵 Balance",
                    ["deposit"] = "➕ Deposit",
                    ["withdraw"] = "➖ Withdraw",
                    ["transaction_history"] = "📜 Transaction history",

                    ["auth_required"] = "🔐 Steam authorization required to continue",
                    ["login_steam"] = "🔐 Login via Steam",
                    ["back"] = "⬅️ Back",
                    ["cancel"] = "❌ Cancel",
                    ["confirm"] = "✅ Confirm",
                    ["next"] = "➡️ Next",
                    ["previous"] = "⬅️ Previous step",

                    ["step"] = "Step",
                    ["of"] = "of",

                    ["duel_format"] = "Select duel format:",
                    ["entry_fee"] = "Enter entry fee (from €5):",
                    ["select_maps"] = "Select maps:",
                    ["round_format"] = "Select round format:",
                    ["confirm_duel"] = "Confirm duel creation:",
                    ["prime_only"] = "Prime only?",
                    ["warmup_time"] = "Warmup duration:",
                    ["max_rounds"] = "Max rounds:",
                    ["pay"] = "Pay",
                    ["pay_balance"] = "Pay from balance",
                    ["pay_cryptomus"] = "Pay with Cryptomus",
                    ["get_server"] = "Get server",
                    ["join_steam"] = "Join via Steam",
                    ["server"] = "Server",
                    ["password"] = "Password",
                    ["entry_fee_paid"] = "✅ Entry fee accepted! 🎉",
                    ["payment_status"] = "Payment status: {0}",
                    ["both_deposits_paid"] = "✅ Deposit accepted from both sides! Confirm readiness in 'My duels' to receive the server",
                    ["waiting_opponent"] = "⏳ Waiting for opponent payment. We'll notify you once it's completed",
                    ["waiting_opponent_ready"] = "⏳ Waiting for opponent readiness",
                    ["payment_expired"] = "⌛ Payment time expired. Try again",
                    ["check_payment"] = "Check",
                    ["press_ready"] = "Press 'Ready' when prepared",
                    ["ready"] = "Ready",
                    ["forfeit_match"] = "❌ Forfeit match",

                    ["tournament_title"] = "Enter tournament title:",
                    ["tournament_description"] = "Enter tournament description (or skip):",
                    ["players_per_team"] = "Players per team:",
                    ["max_teams"] = "Maximum teams:",
                    ["tournament_entry_fee"] = "Entry fee per team:",
                    ["server_start"] = "Please wait for server startup (approximately 20 seconds)\nYou will receive notification with server connection details",

                    ["error"] = "❌ Error",
                    ["success"] = "✅ Success",
                    ["loading"] = "⏳ Loading...",
                    ["no_data"] = "📭 No data",

                    ["duel_created"] = "✅ Duel created successfully!",
                    ["opponent_joined"] = "✅ Your opponent has joined the duel! Awaiting deposit for the duel",
                    ["tournament_created"] = "✅ Tournament created successfully!",
                    ["duel_invite_rules"] = "Join via the link within 15 minutes and confirm readiness. The match starts once all players are ready.",
                    ["invalid_amount"] = "❌ Invalid amount",
                    ["insufficient_balance"] = "❌ Insufficient balance"
                },

                ["ru"] = new()
                {
                    ["welcome"] = "🎮 Добро пожаловать в CS2 Duels!\n\nВыберите язык:",
                    ["language_selected"] = "✅ Язык выбран",
                    ["main_menu"] = "🏠 Главное меню",
                    ["duels"] = "⚔️ Дуэли",
                    ["tournaments"] = "🏆 Турниры",
                    ["profile"] = "👤 Профиль",
                    ["wallet"] = "💰 Кошелёк",
                    ["settings"] = "⚙️ Настройки",
                    ["help"] = "🆘 Помощь",

                    ["create_duel"] = "🆕 Создать дуэль",
                    ["find_duel"] = "🔍 Найти дуэль",
                    ["my_duels"] = "Мои дуэли",
                    ["join_by_link"] = "🔗 Присоединиться по ссылке",
                    ["enter_invite_link"] = "🔗 Отправьте ссылку на дуэль:",
                    ["enter_invite_code"] = "🎫 Отправьте код приглашения:",
                    ["duel_joined"] = "✅ Вы присоединились к дуэли! Ожидаем внесение депозита за дуэль",

                    ["create_tournament"] = "🆕 Создать турнир",
                    ["available_tournaments"] = "🔍 Доступные турниры",
                    ["my_tournaments"] = "👥 Мои турниры",
                    ["join_by_code"] = "🎫 Присоединиться по коду",

                    ["balance"] = "💵 Баланс",
                    ["deposit"] = "➕ Пополнить",
                    ["withdraw"] = "➖ Вывести",
                    ["transaction_history"] = "📜 История транзакций",

                    ["auth_required"] = "🔐 Для продолжения требуется авторизация через Steam",
                    ["login_steam"] = "🔐 Войти через Steam",
                    ["back"] = "⬅️ Назад",
                    ["cancel"] = "❌ Отменить",
                    ["confirm"] = "✅ Подтвердить",
                    ["next"] = "➡️ Далее",
                    ["previous"] = "⬅️ Предыдущий шаг",

                    ["step"] = "Шаг",
                    ["of"] = "из",

                    ["duel_format"] = "Выберите формат дуэли:",
                    ["entry_fee"] = "Введите entry fee (от €5):",
                    ["select_maps"] = "Выберите карты:",
                    ["round_format"] = "Выберите формат раундов:",
                    ["confirm_duel"] = "Подтвердите создание дуэли:",
                    ["prime_only"] = "Только Prime?",
                    ["warmup_time"] = "Длительность разминки:",
                    ["max_rounds"] = "Количество раундов:",
                    ["pay"] = "Заплатить",
                    ["pay_balance"] = "Оплата с баланса",
                    ["pay_cryptomus"] = "Оплата Cryptomus",
                    ["get_server"] = "Получить сервер",
                    ["join_steam"] = "Запустить в Steam",
                    ["server"] = "Сервер",
                    ["password"] = "Пароль",
                    ["entry_fee_paid"] = "✅ Взнос принят! 🎉",
                    ["payment_status"] = "Статус платежа: {0}",
                    ["both_deposits_paid"] = "✅ Депозит принят от обеих сторон! Подтвердите готовность в меню 'Мои дуэли', чтобы получить сервер",
                    ["waiting_opponent"] = "⏳ Ожидаем оплату от соперника. Вы получите уведомление после оплаты",
                    ["waiting_opponent_ready"] = "⏳ Ожидается готовность от соперника",
                    ["payment_expired"] = "⌛ Время оплаты истекло. Попробуйте снова",
                    ["check_payment"] = "Проверить",
                    ["press_ready"] = "Нажмите 'Готов', когда будете готовы",
                    ["ready"] = "Готов",
                    ["forfeit_match"] = "❌ Покинуть матч",

                    ["tournament_title"] = "Введите название турнира:",
                    ["tournament_description"] = "Введите описание турнира (или пропустите):",
                    ["players_per_team"] = "Игроков в команде:",
                    ["max_teams"] = "Максимум команд:",
                    ["tournament_entry_fee"] = "Entry fee за команду:",
                    ["server_start"] = "Ожидайте запуск сервера (приблизительно 20 секунд)\nВы получите уведомление с данными подключения к серверу",

                    ["error"] = "❌ Ошибка",
                    ["success"] = "✅ Успешно",
                    ["loading"] = "⏳ Загрузка...",
                    ["no_data"] = "📭 Нет данных",

                    ["duel_created"] = "✅ Дуэль создана успешно!",
                    ["opponent_joined"] = "✅ Соперник присоединился к дуэли! Ожидаем внесение депозита за дуэль",
                    ["tournament_created"] = "✅ Турнир создан успешно!",
                    ["duel_invite_rules"] = "Перейдите по ссылке в течение 15 минут и подтвердите готовность. Матч начнётся после подтверждения всех игроков.",
                    ["invalid_amount"] = "❌ Неверная сумма",
                    ["insufficient_balance"] = "❌ Недостаточно средств"
                }
            };
        }

        public string GetText(string key, string language = "ua")
        {
            if (_translations.ContainsKey(language) && _translations[language].ContainsKey(key))
            {
                return _translations[language][key];
            }

            if (_translations["ua"].ContainsKey(key))
            {
                return _translations["ua"][key];
            }

            return key;
        }

        public string[] GetAvailableLanguages() => ["ua", "en", "ru"];

        public string GetLanguageName(string language) => language switch
        {
            "ua" => "🇺🇦 Українська",
            "en" => "🇺🇸 English",
            "ru" => "🇷🇺 Русский",
            _ => language
        };
    }
}