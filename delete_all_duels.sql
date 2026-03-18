-- Видалення всіх дуелей з урахуванням зв'язків
-- ВАЖЛИВО: Запустити цей скрипт у SQL Server Management Studio або sqlcmd

USE GGHubDB;
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Видалити скарги пов'язані з дуелями
    DELETE FROM Complaints WHERE DuelId IS NOT NULL;
    PRINT 'Видалено записи з Complaints';

    -- 2. Видалити логи forfeit
    DELETE FROM DuelForfeitLogs;
    PRINT 'Видалено записи з DuelForfeitLogs';

    -- 3. Видалити транзакції пов'язані з дуелями
    DELETE FROM Transactions WHERE DuelId IS NOT NULL;
    PRINT 'Видалено записи з Transactions';

    -- 4. Видалити учасників дуелей
    DELETE FROM DuelParticipants;
    PRINT 'Видалено записи з DuelParticipants';

    -- 5. Видалити карти дуелей
    DELETE FROM DuelMaps;
    PRINT 'Видалено записи з DuelMaps';

    -- 6. Видалити ігрові сервери (має CASCADE, але на всякий випадок)
    DELETE FROM GameServers WHERE DuelId IS NOT NULL;
    PRINT 'Видалено записи з GameServers';

    -- 7. Видалити всі дуелі
    DELETE FROM Duels;
    PRINT 'Видалено записи з Duels';

    COMMIT TRANSACTION;
    PRINT 'Всі дуелі успішно видалені';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Помилка при видаленні: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
