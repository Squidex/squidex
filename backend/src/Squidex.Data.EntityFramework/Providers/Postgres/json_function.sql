-- =============================================================================
-- TYPE-AGNOSTIC
-- =============================================================================
CREATE OR REPLACE FUNCTION jsonb_empty(val jsonb)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  RETURN val IS NULL
      OR (jsonb_typeof(val) = 'null')
      OR (jsonb_typeof(val) = 'array'  AND jsonb_array_length(val) = 0)
      OR (jsonb_typeof(val) = 'string' AND val #>> '{}' = '');
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_exists(val jsonb)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  RETURN NOT jsonb_empty(val);
END;
$$;


-- =============================================================================
-- TEXT
-- =============================================================================
CREATE OR REPLACE FUNCTION jsonb_text_equals(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' = target
    );
  END IF;
  RETURN val #>> '{}' = target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_notequals(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' = target
    );
  END IF;
  RETURN val #>> '{}' != target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_lessthan(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' < target
    );
  END IF;
  RETURN val #>> '{}' < target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_lessthanorequal(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' <= target
    );
  END IF;
  RETURN val #>> '{}' <= target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_greaterthan(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' > target
    );
  END IF;
  RETURN val #>> '{}' > target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_greaterthanorequal(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e WHERE e #>> '{}' >= target
    );
  END IF;
  RETURN val #>> '{}' >= target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_contains(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE e #>> '{}' LIKE '%' || target || '%'
    );
  END IF;
  RETURN val #>> '{}' LIKE '%' || target || '%';
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_startswith(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE e #>> '{}' LIKE target || '%'
    );
  END IF;
  RETURN val #>> '{}' LIKE target || '%';
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_endswith(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE e #>> '{}' LIKE '%' || target
    );
  END IF;
  RETURN val #>> '{}' LIKE '%' || target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_matchs(val jsonb, target text)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE e #>> '{}' ~ target
    );
  END IF;
  RETURN val #>> '{}' ~ target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_text_in(val jsonb, targets text[])
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE e #>> '{}' = ANY(targets)
    );
  END IF;
  RETURN val #>> '{}' = ANY(targets);
END;
$$;


-- =============================================================================
-- NUMBER
-- =============================================================================
CREATE OR REPLACE FUNCTION jsonb_number_equals(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric = target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric = target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_notequals(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric = target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric != target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_lessthan(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric < target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric < target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_lessthanorequal(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric <= target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric <= target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_greaterthan(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric > target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric > target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_greaterthanorequal(val jsonb, target numeric)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric >= target
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric >= target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_number_in(val jsonb, targets numeric[])
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'number' AND (e #>> '{}')::numeric = ANY(targets)
    );
  END IF;
  IF jsonb_typeof(val) != 'number' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::numeric = ANY(targets);
END;
$$;

-- =============================================================================
-- NULL 
-- =============================================================================
CREATE OR REPLACE FUNCTION jsonb_null_equals(val jsonb)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN 
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'null'
    );
  END IF;
  RETURN val IS NULL OR jsonb_typeof(val) = 'null';
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_null_notequals(val jsonb)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  RETURN NOT jsonb_null_equals(val);
END;
$$;

-- =============================================================================
-- BOOLEAN 
-- =============================================================================
CREATE OR REPLACE FUNCTION jsonb_boolean_equals(val jsonb, target boolean)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'boolean' AND (e #>> '{}')::boolean = target
    );
  END IF;
  IF jsonb_typeof(val) != 'boolean' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::boolean = target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_boolean_notequals(val jsonb, target boolean)
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN NOT EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'boolean' AND (e #>> '{}')::boolean = target
    );
  END IF;
  IF jsonb_typeof(val) != 'boolean' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::boolean != target;
END;
$$;

CREATE OR REPLACE FUNCTION jsonb_boolean_in(val jsonb, targets boolean[])
RETURNS boolean LANGUAGE plpgsql IMMUTABLE AS $$
BEGIN
  IF jsonb_typeof(val) = 'array' THEN
    RETURN EXISTS (
      SELECT 1 FROM jsonb_array_elements(val) AS e
      WHERE jsonb_typeof(e) = 'boolean' AND (e #>> '{}')::boolean = ANY(targets)
    );
  END IF;
  IF jsonb_typeof(val) != 'boolean' THEN RETURN FALSE; END IF;
  RETURN (val #>> '{}')::boolean = ANY(targets);
END;
$$;