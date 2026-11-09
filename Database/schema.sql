-- SQLite schema. Applied automatically at startup; existing data is preserved.

CREATE TABLE IF NOT EXISTS `Organizations` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Name` TEXT NOT NULL,
    `CreatedAt` TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `FirstName` TEXT NOT NULL,
    `LastName` TEXT NOT NULL,
    `Email` TEXT COLLATE NOCASE NOT NULL,
    `PasswordHash` TEXT NOT NULL,
    `CreatedAt` TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS `Managers` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `OrganizationId` INTEGER NOT NULL,
    `FirstName` TEXT NOT NULL,
    `LastName` TEXT NOT NULL,
    `Email` TEXT COLLATE NOCASE NOT NULL,
    `PasswordHash` TEXT NOT NULL,
    `CreatedAt` TEXT NOT NULL,
    CONSTRAINT `AK_Managers_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_Managers_Organizations_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `ParkingLots` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `OrganizationId` INTEGER NOT NULL,
    `Name` TEXT NOT NULL,
    `Address` TEXT NOT NULL,
    `CreatedAt` TEXT NOT NULL,
    CONSTRAINT `AK_ParkingLots_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_ParkingLots_Organizations_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `ParkingSpots` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `OrganizationId` INTEGER NOT NULL,
    `ParkingLotId` INTEGER NOT NULL,
    `Number` TEXT NOT NULL,
    `IsReservable` INTEGER NOT NULL,
    CONSTRAINT `AK_ParkingSpots_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_ParkingSpots_ParkingLots_ParkingLotId_OrganizationId` FOREIGN KEY (`ParkingLotId`, `OrganizationId`) REFERENCES `ParkingLots` (`Id`, `OrganizationId`) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS `Reservations` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `ReservationId` TEXT NOT NULL,
    `OrganizationId` INTEGER NOT NULL,
    `ParkingSpotId` INTEGER NOT NULL,
    `UserId` INTEGER NOT NULL,
    `StartTime` TEXT NOT NULL,
    `EndTime` TEXT NOT NULL,
    `Status` TEXT NOT NULL,
    `CreatedAt` TEXT NOT NULL,
    `CancelledByUserId` INTEGER NULL,
    `CancelledByManagerId` INTEGER NULL,
    CONSTRAINT `FK_Reservations_Managers_CancelledByManagerId_OrganizationId` FOREIGN KEY (`CancelledByManagerId`, `OrganizationId`) REFERENCES `Managers` (`Id`, `OrganizationId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_ParkingSpots_ParkingSpotId_OrganizationId` FOREIGN KEY (`ParkingSpotId`, `OrganizationId`) REFERENCES `ParkingSpots` (`Id`, `OrganizationId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_Users_CancelledByUserId` FOREIGN KEY (`CancelledByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Managers_Email` ON `Managers` (`Email`);

CREATE INDEX IF NOT EXISTS `IX_Managers_OrganizationId` ON `Managers` (`OrganizationId`);

CREATE INDEX IF NOT EXISTS `IX_ParkingLots_OrganizationId` ON `ParkingLots` (`OrganizationId`);

CREATE UNIQUE INDEX IF NOT EXISTS `IX_ParkingSpots_ParkingLotId_Number` ON `ParkingSpots` (`ParkingLotId`, `Number`);

CREATE INDEX IF NOT EXISTS `IX_ParkingSpots_ParkingLotId_OrganizationId` ON `ParkingSpots` (`ParkingLotId`, `OrganizationId`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_CancelledByManagerId_OrganizationId` ON `Reservations` (`CancelledByManagerId`, `OrganizationId`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_CancelledByUserId` ON `Reservations` (`CancelledByUserId`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_OrganizationId_Status` ON `Reservations` (`OrganizationId`, `Status`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_ParkingSpotId_OrganizationId` ON `Reservations` (`ParkingSpotId`, `OrganizationId`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_ParkingSpotId_Status_StartTime_EndTime` ON `Reservations` (`ParkingSpotId`, `Status`, `StartTime`, `EndTime`);

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Reservations_ReservationId_Status` ON `Reservations` (`ReservationId`, `Status`);

CREATE INDEX IF NOT EXISTS `IX_Reservations_UserId_Status` ON `Reservations` (`UserId`, `Status`);

CREATE UNIQUE INDEX IF NOT EXISTS `IX_Users_Email` ON `Users` (`Email`);
