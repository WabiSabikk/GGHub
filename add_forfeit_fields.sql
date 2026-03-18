-- Add forfeit fields to DuelParticipants table
ALTER TABLE [DuelParticipants]
ADD [TechnicalDefeat] bit NOT NULL DEFAULT 0;

ALTER TABLE [DuelParticipants]
ADD [ForfeitReason] nvarchar(500) NULL;

-- Create DuelForfeitLogs table
CREATE TABLE [DuelForfeitLogs] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [DuelId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Reason] int NOT NULL,
    [MeasuredPing] int NULL,
    [PacketLossIn] real NULL,
    [PacketLossOut] real NULL,
    [AutoRefunded] bit NOT NULL DEFAULT 0,
    [AdminReviewed] bit NOT NULL DEFAULT 0,
    [AdminNotes] nvarchar(1000) NULL,
    [ServerLogs] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT 0,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [FK_DuelForfeitLogs_Duels] FOREIGN KEY ([DuelId]) REFERENCES [Duels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DuelForfeitLogs_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

PRINT 'Forfeit fields added successfully!';
