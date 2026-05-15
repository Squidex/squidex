-- =============================================================================
-- TYPE-AGNOSTIC
-- =============================================================================
CREATE OR ALTER FUNCTION dbo.json_empty(@col NVARCHAR(MAX), @path NVARCHAR(500))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  DECLARE @val NVARCHAR(MAX) = JSON_VALUE(@col, @path);
  IF @query IS NOT NULL AND LEFT(LTRIM(@query), 1) = '['
    RETURN CASE WHEN (SELECT COUNT(*) FROM OPENJSON(@query)) = 0 THEN 1 ELSE 0 END;
  IF @query IS NOT NULL AND LEFT(LTRIM(@query), 1) = '{'
    RETURN 0;
  IF @val IS NULL RETURN 1;
  IF @val = '' RETURN 1;
  RETURN 0;
END;;

CREATE OR ALTER FUNCTION dbo.json_exists(@col NVARCHAR(MAX), @path NVARCHAR(500))
RETURNS BIT
AS
BEGIN
  RETURN 1 - dbo.json_empty(@col, @path);
END;;


-- =============================================================================
-- NULL
-- =============================================================================
CREATE OR ALTER FUNCTION dbo.json_null_equals(@col NVARCHAR(MAX), @path NVARCHAR(500))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 0
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) IS NULL THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_null_notequals(@col NVARCHAR(MAX), @path NVARCHAR(500))
RETURNS BIT
AS
BEGIN
  RETURN 1 - dbo.json_null_equals(@col, @path);
END;;


-- =============================================================================
-- TEXT
-- =============================================================================
CREATE OR ALTER FUNCTION dbo.json_text_equals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) = @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_notequals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN NOT EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) != @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_lessthan(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] < @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) < @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_lessthanorequal(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] <= @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) <= @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_greaterthan(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] > @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) > @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_greaterthanorequal(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] >= @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) >= @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_contains(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] LIKE '%' + @target + '%'
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) LIKE '%' + @target + '%' THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_startswith(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] LIKE @target + '%'
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) LIKE @target + '%' THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_endswith(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] LIKE '%' + @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) LIKE '%' + @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_text_matchs(@col NVARCHAR(MAX), @path NVARCHAR(500), @target NVARCHAR(2000))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j WHERE j.[type] = 1 AND j.[value] LIKE @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN JSON_VALUE(@col, @path) LIKE @target THEN 1 ELSE 0 END;
END;;


-- =============================================================================
-- NUMBER
-- =============================================================================
CREATE OR ALTER FUNCTION dbo.json_number_equals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) = @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_number_notequals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN NOT EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) != @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_number_lessthan(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) < @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) < @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_number_lessthanorequal(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) <= @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) <= @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_number_greaterthan(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) > @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) > @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_number_greaterthanorequal(@col NVARCHAR(MAX), @path NVARCHAR(500), @target DECIMAL(38, 10))
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 2
        AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) >= @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN TRY_CAST(JSON_VALUE(@col, @path) AS DECIMAL(38, 10)) >= @target THEN 1 ELSE 0 END;
END;;


-- =============================================================================
-- BOOLEAN
-- =============================================================================
CREATE OR ALTER FUNCTION dbo.json_boolean_equals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target BIT)
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 3
        AND IIF(j.[value] = 'true', 1, IIF(j.[value] = 'false', 0, NULL)) = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN IIF(JSON_VALUE(@col, @path) = 'true', 1, IIF(JSON_VALUE(@col, @path) = 'false', 0, NULL)) = @target THEN 1 ELSE 0 END;
END;;

CREATE OR ALTER FUNCTION dbo.json_boolean_notequals(@col NVARCHAR(MAX), @path NVARCHAR(500), @target BIT)
RETURNS BIT
AS
BEGIN
  DECLARE @query NVARCHAR(MAX) = JSON_QUERY(@col, @path);
  IF @query IS NOT NULL
    RETURN CASE WHEN NOT EXISTS (
      SELECT 1 FROM OPENJSON(@query) AS j
      WHERE j.[type] = 3
        AND IIF(j.[value] = 'true', 1, IIF(j.[value] = 'false', 0, NULL)) = @target
    ) THEN 1 ELSE 0 END;
  RETURN CASE WHEN IIF(JSON_VALUE(@col, @path) = 'true', 1, IIF(JSON_VALUE(@col, @path) = 'false', 0, NULL)) != @target THEN 1 ELSE 0 END;
END;;