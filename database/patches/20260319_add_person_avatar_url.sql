SET @db = DATABASE();
SET @col_exists = (
  SELECT COUNT(*)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = @db
    AND TABLE_NAME = 'person'
    AND COLUMN_NAME = 'avatar_url'
);

SET @sql = IF(
  @col_exists = 0,
  'ALTER TABLE `person` ADD COLUMN `avatar_url` VARCHAR(512) NULL COMMENT ''头像URL'' AFTER `birthday`',
  'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
