CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220410181427_initial-migration') THEN
    CREATE TABLE weather_report (
        "Id" uuid NOT NULL,
        "CreatedOn" timestamp with time zone NOT NULL,
        "AverageHighF" numeric NOT NULL,
        "AverageLowF" numeric NOT NULL,
        "RainfallTotalInches" numeric NOT NULL,
        "SnowTotalInches" numeric NOT NULL,
        "ZipCode" text NOT NULL,
        CONSTRAINT "PK_weather_report" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20220410181427_initial-migration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20220410181427_initial-migration', '6.0.3');
    END IF;
END $EF$;
COMMIT;

