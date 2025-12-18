USE [BudgetAppDB];
GO

IF OBJECT_ID('dbo.BudgetLines', 'U') IS NOT NULL DROP TABLE dbo.BudgetLines;
IF OBJECT_ID('dbo.BudgetPeriods', 'U') IS NOT NULL DROP TABLE dbo.BudgetPeriods;
IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;
GO

-------------ACCOUNTS TABLE-------------

CREATE TABLE dbo.Accounts 
(
	AccountID		    INT IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	UserID			    NVARCHAR(450)		NOT NULL,
	AccountName		    NVARCHAR(100)		NOT NULL,
	AccountType		    NVARCHAR(50)		NOT NULL,
	StartingBalance	    DECIMAL(18, 2)		NOT NULL CONSTRAINT DF_Accounts_StartingBalance DEFAULT (0),
	IsClosed		    BIT					NOT NULL CONSTRAINT DF_Accounts_IsClosed DEFAULT (0),
	CreatedAt		    DATETIME2(0)		NOT NULL CONSTRAINT DF_Accounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    ExternalAccountID   NVARCHAR(100)       NULL,
    ExternalProvider    NVARCHAR(100)       NULL
);

ALTER TABLE dbo.Accounts WITH CHECK
ADD CONSTRAINT FK_Accounts_AspNetUsers 
FOREIGN KEY (UserID) REFERENCES dbo.AspNetUsers (Id);

CREATE INDEX IX_Accounts_UserID ON dbo.Accounts (UserID);
GO

-------------CATEGORIES TABLE-------------

CREATE TABLE dbo.Categories
(
	CategoryID		INT IDENTITY(1,1)	NULL PRIMARY KEY,
	UserID			NVARCHAR(450)		NULL,
	CategoryName	NVARCHAR(100)		NOT NULL,
	CategoryType	NVARCHAR(50)		NOT NULL,
	IsArchived		BIT					NOT NULL CONSTRAINT DF_Categories_IsArchived DEFAULT (0),
	DisplayOrder	INT					NOT NULL CONSTRAINT DF_Categories_DisplayOrder DEFAULT (0),
);

ALTER TABLE dbo.Categories WITH CHECK
ADD CONSTRAINT FK_Categories_AspNetUsers
FOREIGN KEY (UserID) REFERENCES dbo.AspNetUsers (Id);

CREATE INDEX IX_Categories_UserID ON dbo.Categories (UserID);
GO

-------------TRANSACTIONS TABLE-------------

CREATE TABLE dbo.Transactions
(
	TransactionID		    INT IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	UserID				    NVARCHAR(450)		NOT NULL,
	AccountID			    INT					NOT NULL,
	CategoryID			    INT					NOT NULL,
	Amount				    DECIMAL(18, 2)		NOT NULL,
	Description			    NVARCHAR(255)		NULL,
	CreatedAt			    DATETIME2(0)		NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    ExternalTransactionId	NVARCHAR(100) NULL,
	ExternalProvider		NVARCHAR(50) NULL
);

ALTER TABLE dbo.Transactions WITH CHECK
ADD CONSTRAINT FK_Transactions_AspNetUsers
FOREIGN KEY (UserID) REFERENCES dbo.AspNetUsers (Id);

ALTER TABLE dbo.Transactions WITH CHECK
ADD CONSTRAINT FK_Transactions_Accounts
FOREIGN KEY (AccountID) REFERENCES dbo.Accounts (AccountID);

ALTER TABLE dbo.Transactions WITH CHECK
ADD CONSTRAINT FK_Transactions_Categories
FOREIGN KEY (CategoryID) REFERENCES dbo.Categories (CategoryID);

CREATE INDEX IX_Transactions_UserID ON dbo.Transactions (UserID);
CREATE INDEX IX_Transactions_AccountID ON dbo.Transactions (AccountID);
CREATE INDEX IX_Transactions_CreatedAt ON dbo.Transactions (CreatedAt);
GO

-------------BUDGET PERIODS TABLE-------------

CREATE TABLE dbo.BudgetPeriods
(
	BudgetPeriodID	INT IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	UserID			NVARCHAR(450)		NOT NULL,
	Year			INT					NOT NULL,
	Month			INT					NOT NULL,
	CreatedAt		DATETIME2(0)		NOT NULL CONSTRAINT DF_BudgetPeriods_CreatedAt DEFAULT (SYSUTCDATETIME())
	CONSTRAINT UQ_BudgetPeriods_UserYearMonth UNIQUE (UserID, Year, Month)
);

ALTER TABLE dbo.BudgetPeriods WITH CHECK
ADD CONSTRAINT FK_BudgetPeriods_AspNetUsers
FOREIGN KEY (UserID) REFERENCES dbo.AspNetUsers (Id);

CREATE INDEX IX_BudgetPeriods_UserID ON dbo.BudgetPeriods (UserID);
GO

-------------BUDGET LINES TABLE-------------

CREATE TABLE dbo.BudgetLines
(
	BudgetLineID		INT IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	BudgetPeriodID		INT					NOT NULL,
	CategoryID			INT					NOT NULL,
	BudgetedAmount		DECIMAL(18, 2)		NOT NULL,
	Notes				NVARCHAR(255)		NULL
);

ALTER TABLE dbo.BudgetLines WITH CHECK
ADD CONSTRAINT FK_BudgetLines_BudgetPeriods
FOREIGN KEY (BudgetPeriodID) REFERENCES dbo.BudgetPeriods (BudgetPeriodID);

ALTER TABLE dbo.BudgetLines WITH CHECK
ADD CONSTRAINT FK_BudgetLines_Categories
FOREIGN KEY (CategoryID) REFERENCES dbo.Categories (CategoryID);

CREATE INDEX IX_BudgetLines_BudgetPeriodID ON dbo.BudgetLines (BudgetPeriodID);
GO

--------------BANK LINK TABLE-------------
CREATE TABLE dbo.BankLinks
(
    BankLinkId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId          NVARCHAR(450)     NOT NULL,
    Provider        NVARCHAR(50)      NOT NULL,
    ItemId          NVARCHAR(100)     NOT NULL,
    AccessToken     NVARCHAR(500)     NOT NULL,
    InstitutionName NVARCHAR(200)     NULL,
    CreatedAt       DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
    IsActive        BIT               NOT NULL DEFAULT (1)
);

ALTER TABLE dbo.BankLinks WITH CHECK
ADD CONSTRAINT FK_BankLinks_AspNetUsers
FOREIGN KEY (UserID) REFERENCES dbo.AspNetUsers (Id);

CREATE INDEX IX_BankLinks_UserID ON dbo.BankLinks (UserID);
GO

-------------END OF TABLE CREATION-------------
----------------------------------------------------------------------------------
-------------PROCEDURES BELOW-------------

-------------GET ACCOUNT TOTALS PROCEDURE-------------

IF OBJECT_ID('dbo.GetAccountTotals', 'P') IS NOT NULL
	DROP PROCEDURE dbo.GetAccountTotals;
GO

CREATE PROCEDURE dbo.GetAccountTotals
(
	@UserID NVARCHAR(450)
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		a.AccountID,
		a.AccountName,
		a.AccountType,
		a.StartingBalance,
		Balance = a.StartingBalance + ISNULL(SUM(t.Amount), 0)
	FROM 
		dbo.Accounts a
	LEFT JOIN dbo.Transactions t 
		ON t.AccountID = a.AccountID
		AND t.UserID = @UserID
	WHERE 
		a.UserID = @UserID
	GROUP BY 
		a.AccountID, a.AccountName, a.AccountType, a.StartingBalance
	ORDER BY a.AccountName;
END;
GO

-------------GET TRANSACTION HISTORY PROCEDURE-------------

IF OBJECT_ID('dbo.GetTransactionHistory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTransactionHistory;
GO

CREATE PROCEDURE dbo.GetTransactionHistory
(
    @UserId NVARCHAR(450),
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.TransactionId,
        t.OccurredOn,
        t.Amount,
        t.Memo,
        a.AccountName		 AS AccountName,
        c.CategoryName       AS CategoryName,
        c.CategoryType
    FROM dbo.Transactions t
    INNER JOIN dbo.Accounts   a ON t.AccountId = a.AccountId
    LEFT  JOIN dbo.Categories c ON t.CategoryId = c.CategoryId
    WHERE
        t.UserId = @UserId
        AND (@FromDate IS NULL OR t.CreatedAt >= @FromDate)
        AND (@ToDate   IS NULL OR t.CreatedAt <= @ToDate)
    ORDER BY t.OccurredOn DESC, t.TransactionId DESC;
END;
GO

-------------Monthly Budget Summary Procedure-------------

IF OBJECT_ID('dbo.GetMonthlyBudgetSummary', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetMonthlyBudgetSummary;
GO

CREATE PROCEDURE dbo.GetMonthlyBudgetSummary
(
    @UserId NVARCHAR(450),
    @Year   INT,
    @Month  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Tx AS
    (
        SELECT
            t.CategoryId,
            SUM(t.Amount) AS ActualAmount
        FROM dbo.Transactions t
        WHERE
            t.UserId = @UserId
            AND YEAR(t.CreatedAt) = @Year
            AND MONTH(t.CreatedAt) = @Month
        GROUP BY t.CategoryId
    )
    SELECT
        bp.BudgetPeriodId,
        bp.Year,
        bp.Month,
        c.CategoryId,
        c.Name          AS CategoryName,
        c.CategoryType,
        bl.BudgetedAmount,
        ISNULL(tx.ActualAmount, 0) AS ActualAmount
    FROM dbo.BudgetPeriods bp
    INNER JOIN dbo.BudgetLines  bl ON bp.BudgetPeriodId = bl.BudgetPeriodId
    INNER JOIN dbo.Categories   c  ON bl.CategoryId = c.CategoryId
    LEFT JOIN Tx tx ON tx.CategoryId = c.CategoryId
    WHERE
        bp.UserId = @UserId
        AND bp.Year = @Year
        AND bp.Month = @Month
    ORDER BY c.CategoryType, c.Name;
END;
GO

-------------END OF PROCEDURES-------------

-------------INSERT INITIAL GLOBAL CATEGORIES-------------
INSERT INTO dbo.Categories (UserId, CategoryName, CategoryType, IsArchived, DisplayOrder)
VALUES
	-- Income
    (NULL, 'Paycheck',           'Income', 0, 10),
    (NULL, 'Side Hustle',        'Income', 0, 20),
    (NULL, 'Other Income',       'Income', 0, 30),

    -- Fixed expenses
    (NULL, 'Rent / Mortgage',    'Expense', 0, 110),
    (NULL, 'Utilities',          'Expense', 0, 120),
    (NULL, 'Internet',           'Expense', 0, 130),
    (NULL, 'Phone',              'Expense', 0, 140),

    -- Variable expenses
    (NULL, 'Groceries',          'Expense', 0, 210),
    (NULL, 'Dining Out',         'Expense', 0, 220),
    (NULL, 'Gas / Transport',    'Expense', 0, 230),
    (NULL, 'Entertainment',      'Expense', 0, 240),
    (NULL, 'Health / Medical',   'Expense', 0, 250),
    (NULL, 'Subscriptions',      'Expense', 0, 260),
    (NULL, 'Miscellaneous',      'Expense', 0, 999);
GO

-------------ALTER TABLE ADDITIONS-------------






