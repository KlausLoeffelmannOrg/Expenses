CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Periods" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Periods" PRIMARY KEY AUTOINCREMENT,
    "Year" INTEGER NOT NULL,
    "Kind" INTEGER NOT NULL,
    "Number" INTEGER NOT NULL,
    "StateCode" TEXT NOT NULL,
    "StartDate" TEXT NOT NULL,
    "EndDate" TEXT NOT NULL
);

CREATE TABLE "Drafts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Drafts" PRIMARY KEY AUTOINCREMENT,
    "BookkeepingPeriodId" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "SavedAt" TEXT NOT NULL,
    "Payload" TEXT NOT NULL,
    CONSTRAINT "FK_Drafts_Periods_BookkeepingPeriodId" FOREIGN KEY ("BookkeepingPeriodId") REFERENCES "Periods" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Expenses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Expenses" PRIMARY KEY AUTOINCREMENT,
    "BookkeepingPeriodId" INTEGER NOT NULL,
    "PaidDate" TEXT NOT NULL,
    "Payee" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Tags" TEXT NOT NULL,
    "PaymentReference" TEXT NOT NULL,
    "AmountCents" INTEGER NOT NULL,
    "BusinessUsePercent" INTEGER NOT NULL,
    "ReceiptPath" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "Revision" TEXT NOT NULL,
    CONSTRAINT "FK_Expenses_Periods_BookkeepingPeriodId" FOREIGN KEY ("BookkeepingPeriodId") REFERENCES "Periods" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Reminders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Reminders" PRIMARY KEY AUTOINCREMENT,
    "BookkeepingPeriodId" INTEGER NULL,
    "Text" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "IsDismissed" INTEGER NOT NULL,
    CONSTRAINT "FK_Reminders_Periods_BookkeepingPeriodId" FOREIGN KEY ("BookkeepingPeriodId") REFERENCES "Periods" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Drafts_BookkeepingPeriodId" ON "Drafts" ("BookkeepingPeriodId");

CREATE INDEX "IX_Expenses_BookkeepingPeriodId_PaidDate" ON "Expenses" ("BookkeepingPeriodId", "PaidDate");

CREATE UNIQUE INDEX "IX_Periods_Year_Kind_Number_StateCode" ON "Periods" ("Year", "Kind", "Number", "StateCode");

CREATE INDEX "IX_Reminders_BookkeepingPeriodId" ON "Reminders" ("BookkeepingPeriodId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261106123451_InitialBookkeeping', '10.0.14');

COMMIT;

