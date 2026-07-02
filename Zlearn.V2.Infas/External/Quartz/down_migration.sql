-- This script is for PostgreSQL

-- This initializes the database to pristine for Quartz, by first removing any existing Quartz tables
-- and then recreating them from scratch.
-- Should you only require it to create the tables, set DropDb to 0.

DO $$
  DECLARE DropDb INT := 1; -- Set this to 0 to skip DROP statements, 1 to include them
BEGIN
  IF DropDb = 1 THEN
    SET client_min_messages = WARNING;
    DROP TABLE IF EXISTS qrtz_fired_triggers;
    DROP TABLE IF EXISTS qrtz_paused_trigger_grps;
    DROP TABLE IF EXISTS qrtz_scheduler_state;
    DROP TABLE IF EXISTS qrtz_locks;
    DROP TABLE IF EXISTS qrtz_simprop_triggers;
    DROP TABLE IF EXISTS qrtz_simple_triggers;
    DROP TABLE IF EXISTS qrtz_cron_triggers;
    DROP TABLE IF EXISTS qrtz_blob_triggers;
    DROP TABLE IF EXISTS qrtz_triggers;
    DROP TABLE IF EXISTS qrtz_job_details;
    DROP TABLE IF EXISTS qrtz_calendars;
    SET client_min_messages = NOTICE;
  END IF;
END $$;
