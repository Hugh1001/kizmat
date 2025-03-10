USE [PaymentsDb]
GO

CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Username] NVARCHAR(250) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

CREATE TABLE [dbo].[UserBalances] (
    [UserId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Balance] DECIMAL(19, 4) NOT NULL,
    [Version] INT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_UserBalances_Users] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
        ON DELETE CASCADE
) ON [PRIMARY];
GO
