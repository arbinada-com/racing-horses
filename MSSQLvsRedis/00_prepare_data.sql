USE master;
DROP DATABASE IF EXISTS kv_db;
GO

-- Create a test database
CREATE DATABASE kv_db
ON PRIMARY
	(NAME = N'kv_db_data', FILENAME = N'/tmp/kv_db_data.mdf', SIZE = 512MB),
FILEGROUP fg_kv_db_memory_optimized CONTAINS MEMORY_OPTIMIZED_DATA
	(NAME = N'kv_db_memory_optimized', FILENAME = N'/tmp/kv_db_memory_optimized.sdf')
LOG ON
	(NAME = N'kv_db_log', FILENAME = N'/tmp/kv_db_data_log.ldf', SIZE = 512MB)
GO
ALTER DATABASE kv_db SET RECOVERY SIMPLE;
ALTER DATABASE kv_db SET DELAYED_DURABILITY = FORCED;
GO

USE kv_db

CREATE TABLE kv (
   k uniqueidentifier NOT NULL,
   v nvarchar(100)    NOT NULL,
   CONSTRAINT PK_kv PRIMARY KEY NONCLUSTERED HASH (k)
      WITH (BUCKET_COUNT = 1000000)
)
WITH (
    MEMORY_OPTIMIZED = ON,
    DURABILITY = SCHEMA_AND_DATA  -- persistent data
);
GO

DECLARE @i           int;
DECLARE @max_count   int = 1000000;
DECLARE @batch_count int = 10000;

DELETE FROM kv;

SET @i = 0;
WHILE @i < @max_count BEGIN
    INSERT INTO kv (k, v)
    VALUES(
        newid(),
        -- generates a random string of 100 characters
        LEFT(REPLACE(REPLACE((SELECT CRYPT_GEN_RANDOM(100) FOR XML PATH(''), BINARY BASE64), '+', ''), '/', ''), 100)
    );
    SET @i = @i + 1;
    IF @i % @batch_count = 0 BEGIN
        PRINT @i;
    END
END
PRINT 'Finished populating key-value data';

SELECT COUNT(1) FROM kv;
-- SELECT TOP 100 * FROM kv
