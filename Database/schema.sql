-- Run once in an empty, selected MySQL database. No application data is inserted.

CREATE TABLE `Organizations` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(120) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FirstName` varchar(80) NOT NULL,
    `LastName` varchar(80) NOT NULL,
    `Email` varchar(254) NOT NULL,
    `PasswordHash` varchar(512) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Managers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrganizationId` int NOT NULL,
    `FirstName` varchar(80) NOT NULL,
    `LastName` varchar(80) NOT NULL,
    `Email` varchar(254) NOT NULL,
    `PasswordHash` varchar(512) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `AK_Managers_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_Managers_Organizations_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `ParkingLots` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrganizationId` int NOT NULL,
    `Name` varchar(120) NOT NULL,
    `Address` varchar(240) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `AK_ParkingLots_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_ParkingLots_Organizations_OrganizationId` FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `ParkingSpots` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrganizationId` int NOT NULL,
    `ParkingLotId` int NOT NULL,
    `Number` varchar(20) NOT NULL,
    `IsReservable` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `AK_ParkingSpots_Id_OrganizationId` UNIQUE (`Id`, `OrganizationId`),
    CONSTRAINT `FK_ParkingSpots_ParkingLots_ParkingLotId_OrganizationId` FOREIGN KEY (`ParkingLotId`, `OrganizationId`) REFERENCES `ParkingLots` (`Id`, `OrganizationId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Reservations` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ReservationId` char(36) NOT NULL,
    `OrganizationId` int NOT NULL,
    `ParkingSpotId` int NOT NULL,
    `UserId` int NOT NULL,
    `StartTime` datetime(6) NOT NULL,
    `EndTime` datetime(6) NOT NULL,
    `Status` varchar(16) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CancelledByUserId` int NULL,
    `CancelledByManagerId` int NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reservations_Managers_CancelledByManagerId_OrganizationId` FOREIGN KEY (`CancelledByManagerId`, `OrganizationId`) REFERENCES `Managers` (`Id`, `OrganizationId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_ParkingSpots_ParkingSpotId_OrganizationId` FOREIGN KEY (`ParkingSpotId`, `OrganizationId`) REFERENCES `ParkingSpots` (`Id`, `OrganizationId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_Users_CancelledByUserId` FOREIGN KEY (`CancelledByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE UNIQUE INDEX `IX_Managers_Email` ON `Managers` (`Email`);

CREATE INDEX `IX_Managers_OrganizationId` ON `Managers` (`OrganizationId`);

CREATE INDEX `IX_ParkingLots_OrganizationId` ON `ParkingLots` (`OrganizationId`);

CREATE UNIQUE INDEX `IX_ParkingSpots_ParkingLotId_Number` ON `ParkingSpots` (`ParkingLotId`, `Number`);

CREATE INDEX `IX_ParkingSpots_ParkingLotId_OrganizationId` ON `ParkingSpots` (`ParkingLotId`, `OrganizationId`);

CREATE INDEX `IX_Reservations_CancelledByManagerId_OrganizationId` ON `Reservations` (`CancelledByManagerId`, `OrganizationId`);

CREATE INDEX `IX_Reservations_CancelledByUserId` ON `Reservations` (`CancelledByUserId`);

CREATE INDEX `IX_Reservations_OrganizationId_Status` ON `Reservations` (`OrganizationId`, `Status`);

CREATE INDEX `IX_Reservations_ParkingSpotId_OrganizationId` ON `Reservations` (`ParkingSpotId`, `OrganizationId`);

CREATE INDEX `IX_Reservations_ParkingSpotId_Status_StartTime_EndTime` ON `Reservations` (`ParkingSpotId`, `Status`, `StartTime`, `EndTime`);

CREATE UNIQUE INDEX `IX_Reservations_ReservationId_Status` ON `Reservations` (`ReservationId`, `Status`);

CREATE INDEX `IX_Reservations_UserId_Status` ON `Reservations` (`UserId`, `Status`);

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);
