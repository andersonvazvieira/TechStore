CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Price" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "MinimumStock" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CategoryId" INTEGER NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id")
);

CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261002021029_InitialCreate', '10.0.12');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "StockMovements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StockMovements" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "Type" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "Date" TEXT NOT NULL,
    "Observation" TEXT NOT NULL,
    CONSTRAINT "FK_StockMovements_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_StockMovements_ProductId" ON "StockMovements" ("ProductId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261002033220_AddStockMovements', '10.0.12');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004001158_AddUsers', '10.0.12');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "Users" ADD "Role" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261005165512_AddUserRole', '10.0.12');

COMMIT;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE UNIQUE INDEX "IX_Products_Code" ON "Products" ("Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261005175049_AddUniqueIndexes', '10.0.12');

COMMIT;

BEGIN TRANSACTION;
CREATE TRIGGER TR_Products_PreventNegativeStock_Insert
BEFORE INSERT ON Products
FOR EACH ROW
WHEN NEW.Quantity < 0
BEGIN
    SELECT RAISE(
        ABORT,
        'O estoque nao pode ser negativo.'
    );
END;

CREATE TRIGGER TR_Products_PreventNegativeStock_Update
BEFORE UPDATE OF Quantity ON Products
FOR EACH ROW
WHEN NEW.Quantity < 0
BEGIN
    SELECT RAISE(
        ABORT,
        'O estoque nao pode ser negativo.'
    );
END;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261022005839_AddStockProtectionTriggers', '10.0.12');

COMMIT;

