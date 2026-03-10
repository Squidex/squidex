-- =============================================================================
-- TYPE-AGNOSTIC
-- =============================================================================
DROP FUNCTION IF EXISTS json_empty;;
CREATE FUNCTION json_empty(col JSON, path VARCHAR(500))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF val IS NULL THEN RETURN 1; END IF;
  IF JSON_TYPE(val) = 'NULL'   THEN RETURN 1; END IF;
  IF JSON_TYPE(val) = 'ARRAY'  AND JSON_LENGTH(val) = 0 THEN RETURN 1; END IF;
  IF JSON_TYPE(val) = 'STRING' AND JSON_UNQUOTE(val) = '' THEN RETURN 1; END IF;
  RETURN 0;
END;;

DROP FUNCTION IF EXISTS json_exists;;
CREATE FUNCTION json_exists(col JSON, path VARCHAR(500))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  RETURN NOT json_empty(col, path);
END;;


-- =============================================================================
-- NULL
-- =============================================================================
DROP FUNCTION IF EXISTS json_null_equals;;
CREATE FUNCTION json_null_equals(col JSON, path VARCHAR(500))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE COALESCE(JSON_TYPE(jt.elem), 'NULL') = 'NULL'
    );
  END IF;
  RETURN COALESCE(JSON_TYPE(val), 'NULL') = 'NULL';
END;;

DROP FUNCTION IF EXISTS json_null_notequals;;
CREATE FUNCTION json_null_notequals(col JSON, path VARCHAR(500))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  RETURN NOT json_null_equals(col, path);
END;;


-- =============================================================================
-- TEXT
-- =============================================================================
DROP FUNCTION IF EXISTS json_text_equals;;
CREATE FUNCTION json_text_equals(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) = target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) = target;
END;;

DROP FUNCTION IF EXISTS json_text_notequals;;
CREATE FUNCTION json_text_notequals(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) = target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 1; END IF;
  RETURN JSON_UNQUOTE(val) != target;
END;;

DROP FUNCTION IF EXISTS json_text_lessthan;;
CREATE FUNCTION json_text_lessthan(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) < target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) < target;
END;;

DROP FUNCTION IF EXISTS json_text_lessthanorequal;;
CREATE FUNCTION json_text_lessthanorequal(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) <= target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) <= target;
END;;

DROP FUNCTION IF EXISTS json_text_greaterthan;;
CREATE FUNCTION json_text_greaterthan(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) > target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) > target;
END;;

DROP FUNCTION IF EXISTS json_text_greaterthanorequal;;
CREATE FUNCTION json_text_greaterthanorequal(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) >= target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) >= target;
END;;

DROP FUNCTION IF EXISTS json_text_contains;;
CREATE FUNCTION json_text_contains(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) LIKE CONCAT('%', target, '%')
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) LIKE CONCAT('%', target, '%');
END;;

DROP FUNCTION IF EXISTS json_text_startswith;;
CREATE FUNCTION json_text_startswith(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) LIKE CONCAT(target, '%')
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) LIKE CONCAT(target, '%');
END;;

DROP FUNCTION IF EXISTS json_text_endswith;;
CREATE FUNCTION json_text_endswith(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) LIKE CONCAT('%', target)
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) LIKE CONCAT('%', target);
END;;

DROP FUNCTION IF EXISTS json_text_matchs;;
CREATE FUNCTION json_text_matchs(col JSON, path VARCHAR(500), target VARCHAR(2000))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_UNQUOTE(jt.elem) REGEXP target
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_UNQUOTE(val) REGEXP target;
END;;

DROP FUNCTION IF EXISTS json_text_in;;
CREATE FUNCTION json_text_in(col JSON, path VARCHAR(500), targets JSON)
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'STRING' AND JSON_CONTAINS(targets, jt.elem)
    );
  END IF;
  IF JSON_TYPE(val) != 'STRING' THEN RETURN 0; END IF;
  RETURN JSON_CONTAINS(targets, val);
END;;


-- =============================================================================
-- NUMBER
-- =============================================================================
DROP FUNCTION IF EXISTS json_number_equals;;
CREATE FUNCTION json_number_equals(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END = target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) = target;
END;;

DROP FUNCTION IF EXISTS json_number_notequals;;
CREATE FUNCTION json_number_notequals(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END = target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 1; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) != target;
END;;

DROP FUNCTION IF EXISTS json_number_lessthan;;
CREATE FUNCTION json_number_lessthan(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END < target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) < target;
END;;

DROP FUNCTION IF EXISTS json_number_lessthanorequal;;
CREATE FUNCTION json_number_lessthanorequal(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END <= target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) <= target;
END;;

DROP FUNCTION IF EXISTS json_number_greaterthan;;
CREATE FUNCTION json_number_greaterthan(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END > target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) > target;
END;;

DROP FUNCTION IF EXISTS json_number_greaterthanorequal;;
CREATE FUNCTION json_number_greaterthanorequal(col JSON, path VARCHAR(500), target DECIMAL(65, 10))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
                 THEN CAST(JSON_UNQUOTE(jt.elem) AS DECIMAL(65, 10))
            END >= target
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN CAST(JSON_UNQUOTE(val) AS DECIMAL(65, 10)) >= target;
END;;

DROP FUNCTION IF EXISTS json_number_in;;
CREATE FUNCTION json_number_in(col JSON, path VARCHAR(500), targets JSON)
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) IN ('INTEGER', 'DOUBLE', 'DECIMAL')
        AND JSON_CONTAINS(targets, jt.elem)
    );
  END IF;
  IF JSON_TYPE(val) NOT IN ('INTEGER', 'DOUBLE', 'DECIMAL') THEN RETURN 0; END IF;
  RETURN JSON_CONTAINS(targets, val);
END;;


-- =============================================================================
-- BOOLEAN
-- =============================================================================
DROP FUNCTION IF EXISTS json_boolean_equals;;
CREATE FUNCTION json_boolean_equals(col JSON, path VARCHAR(500), target TINYINT(1))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  DECLARE target_json JSON;
  SET val = JSON_EXTRACT(col, path);
  SET target_json = IF(target, CAST('true' AS JSON), CAST('false' AS JSON));
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) = 'BOOLEAN' THEN jt.elem END = target_json
    );
  END IF;
  IF JSON_TYPE(val) != 'BOOLEAN' THEN RETURN 0; END IF;
  RETURN val = target_json;
END;;

DROP FUNCTION IF EXISTS json_boolean_notequals;;
CREATE FUNCTION json_boolean_notequals(col JSON, path VARCHAR(500), target TINYINT(1))
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  DECLARE target_json JSON;
  SET val = JSON_EXTRACT(col, path);
  SET target_json = IF(target, CAST('true' AS JSON), CAST('false' AS JSON));
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE CASE WHEN JSON_TYPE(jt.elem) = 'BOOLEAN' THEN jt.elem END = target_json
    );
  END IF;
  IF JSON_TYPE(val) != 'BOOLEAN' THEN RETURN 1; END IF;
  RETURN val != target_json;
END;;

DROP FUNCTION IF EXISTS json_boolean_in;;
CREATE FUNCTION json_boolean_in(col JSON, path VARCHAR(500), targets JSON)
RETURNS TINYINT(1) DETERMINISTIC
BEGIN
  DECLARE val JSON;
  SET val = JSON_EXTRACT(col, path);
  IF JSON_TYPE(val) = 'ARRAY' THEN
    RETURN EXISTS (
      SELECT 1 FROM JSON_TABLE(val, '$[*]' COLUMNS (elem JSON PATH '$')) AS jt
      WHERE JSON_TYPE(jt.elem) = 'BOOLEAN'
        AND EXISTS (
          SELECT 1 FROM JSON_TABLE(targets, '$[*]' COLUMNS (t JSON PATH '$')) AS tt
          WHERE IF(jt.elem = CAST('true' AS JSON), 1, 0) = CAST(JSON_UNQUOTE(tt.t) AS UNSIGNED)
        )
    );
  END IF;
  IF JSON_TYPE(val) != 'BOOLEAN' THEN RETURN 0; END IF;
  RETURN EXISTS (
    SELECT 1 FROM JSON_TABLE(targets, '$[*]' COLUMNS (t JSON PATH '$')) AS tt
    WHERE IF(val = CAST('true' AS JSON), 1, 0) = CAST(JSON_UNQUOTE(tt.t) AS UNSIGNED)
  );
END;;