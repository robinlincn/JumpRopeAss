CREATE TABLE `org` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `org_type` TINYINT NOT NULL COMMENT '机构类型：1培训机构；2中小学',
  `name` VARCHAR(128) NOT NULL COMMENT '机构名称',
  `short_name` VARCHAR(64) NULL COMMENT '机构简称',
  `province` VARCHAR(32) NULL COMMENT '省',
  `city` VARCHAR(32) NULL COMMENT '市',
  `district` VARCHAR(32) NULL COMMENT '区县',
  `address` VARCHAR(255) NULL COMMENT '详细地址',
  `contact_name` VARCHAR(64) NULL COMMENT '联系人姓名',
  `contact_phone` VARCHAR(32) NULL COMMENT '联系人电话',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `deleted_at` DATETIME NULL COMMENT '删除时间（软删）',
  PRIMARY KEY (`id`),
  KEY `idx_org_type` (`org_type`),
  KEY `idx_org_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='机构表（培训机构/中小学）';

CREATE TABLE `school` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name` VARCHAR(128) NOT NULL COMMENT '学校名称',
  `short_name` VARCHAR(64) NULL COMMENT '学校简称',
  `province` VARCHAR(32) NULL COMMENT '省',
  `city` VARCHAR(32) NULL COMMENT '市',
  `district` VARCHAR(32) NULL COMMENT '区县',
  `address` VARCHAR(255) NULL COMMENT '详细地址',
  `contact_name` VARCHAR(64) NULL COMMENT '联系人姓名',
  `contact_phone` VARCHAR(32) NULL COMMENT '联系人电话',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  KEY `idx_school_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='学校主表（中小学）';

CREATE TABLE `person` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `full_name` VARCHAR(64) NOT NULL COMMENT '姓名',
  `gender` TINYINT NULL COMMENT '性别：0未知；1男；2女',
  `id_card_no` VARCHAR(32) NULL COMMENT '身份证号（敏感）',
  `mobile` VARCHAR(32) NULL COMMENT '手机号',
  `birthday` DATE NULL COMMENT '出生日期',
  `avatar_url` VARCHAR(512) NULL COMMENT '头像URL',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1正常；0禁用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `deleted_at` DATETIME NULL COMMENT '删除时间（软删）',
  PRIMARY KEY (`id`),
  KEY `idx_person_name` (`full_name`),
  KEY `idx_person_mobile` (`mobile`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='人员主表';

CREATE TABLE `person_coach` (
  `person_id` BIGINT UNSIGNED NOT NULL COMMENT '人员ID',
  `org_id` BIGINT UNSIGNED NOT NULL COMMENT '所属机构ID（培训机构或中小学）',
  `coach_level` VARCHAR(32) NULL COMMENT '教练等级（可选）',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；0无效',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`person_id`),
  KEY `idx_coach_org` (`org_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='教练员角色表（必须绑定机构）';

CREATE TABLE `person_athlete` (
  `person_id` BIGINT UNSIGNED NOT NULL COMMENT '人员ID',
  `school_id` BIGINT UNSIGNED NOT NULL COMMENT '所属中小学ID（必填，指向school.id）',
  `grade_name` VARCHAR(32) NULL COMMENT '年级（如一年级/七年级等）',
  `class_name` VARCHAR(64) NULL COMMENT '班级（如一班/七年级三班等）',
  `training_org_id` BIGINT UNSIGNED NULL COMMENT '所属培训机构ID（选填）',
  `first_coach_person_id` BIGINT UNSIGNED NULL COMMENT '第一教练员人员ID（终身绑定，仅平台可改）',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；0无效',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`person_id`),
  KEY `idx_athlete_school` (`school_id`),
  KEY `idx_athlete_training_org` (`training_org_id`),
  KEY `idx_athlete_first_coach` (`first_coach_person_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='运动员角色表（中小学必填；培训机构选填；第一教练员终身绑定）';

CREATE TABLE `athlete_affiliation_history` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `athlete_person_id` BIGINT UNSIGNED NOT NULL COMMENT '运动员人员ID',
  `change_type` TINYINT NOT NULL COMMENT '变更类型：1学校；2培训机构',
  `before_org_id` BIGINT UNSIGNED NULL COMMENT '变更前机构ID',
  `after_org_id` BIGINT UNSIGNED NULL COMMENT '变更后机构ID',
  `reason` VARCHAR(255) NULL COMMENT '变更原因',
  `operator_type` TINYINT NOT NULL COMMENT '操作人类型：1后台管理员；2本人；3家长；4教练',
  `operator_user_id` BIGINT UNSIGNED NULL COMMENT '操作人账号ID',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_aff_hist_athlete` (`athlete_person_id`),
  KEY `idx_aff_hist_type_time` (`change_type`, `created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='运动员学校/培训机构变更历史（留痕可追溯）';

CREATE TABLE `athlete_parent_bind` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `athlete_person_id` BIGINT UNSIGNED NOT NULL COMMENT '运动员人员ID',
  `parent_person_id` BIGINT UNSIGNED NOT NULL COMMENT '家长人员ID',
  `relation` VARCHAR(16) NULL COMMENT '关系：父亲/母亲/监护人等',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；0无效',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_athlete_parent` (`athlete_person_id`, `parent_person_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='运动员-家长绑定（支持多个家长）';

CREATE TABLE `person_parent` (
  `person_id` BIGINT UNSIGNED NOT NULL COMMENT '人员ID',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；0无效',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`person_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='家长角色表';

CREATE TABLE `athlete_coach_bind` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `athlete_person_id` BIGINT UNSIGNED NOT NULL COMMENT '运动员人员ID',
  `coach_person_id` BIGINT UNSIGNED NOT NULL COMMENT '教练员人员ID',
  `bind_level` TINYINT NOT NULL COMMENT '绑定级别：2第二教练；3第三教练',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_athlete_bind_level` (`athlete_person_id`, `bind_level`),
  KEY `idx_bind_coach` (`coach_person_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='运动员第二/第三教练员绑定';

CREATE TABLE `user_account` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `user_type` TINYINT NOT NULL COMMENT '账号类型：1小程序；2后台',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1正常；0禁用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_user_type` (`user_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='账号表';

CREATE TABLE `user_wechat` (
  `user_id` BIGINT UNSIGNED NOT NULL COMMENT '账号ID',
  `openid` VARCHAR(64) NOT NULL COMMENT '微信openid',
  `unionid` VARCHAR(64) NULL COMMENT '微信unionid',
  `nickname` VARCHAR(64) NULL COMMENT '微信昵称',
  `avatar_url` VARCHAR(512) NULL COMMENT '微信头像',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uk_openid` (`openid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='小程序微信账号信息';

CREATE TABLE `user_identity_submit` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `user_id` BIGINT UNSIGNED NOT NULL COMMENT '账号ID',
  `real_name` VARCHAR(64) NOT NULL COMMENT '真实姓名',
  `id_card_no` VARCHAR(32) NOT NULL COMMENT '身份证号（敏感）',
  `mobile` VARCHAR(32) NOT NULL COMMENT '手机号',
  `status` TINYINT NOT NULL DEFAULT 0 COMMENT '状态：0待审核；1通过；2驳回',
  `reject_reason` VARCHAR(255) NULL COMMENT '驳回原因',
  `reviewed_by_admin_id` BIGINT UNSIGNED NULL COMMENT '审核后台账号ID',
  `reviewed_at` DATETIME NULL COMMENT '审核时间',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '提交时间',
  PRIMARY KEY (`id`),
  KEY `idx_identity_user_status` (`user_id`, `status`),
  KEY `idx_identity_status_time` (`status`, `created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户认证提交记录（需后台审核）';

CREATE TABLE `news_article` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `title` VARCHAR(200) NOT NULL COMMENT '标题',
  `cover_url` VARCHAR(512) NULL COMMENT '封面图',
  `summary` VARCHAR(500) NULL COMMENT '摘要',
  `content_type` VARCHAR(16) NOT NULL DEFAULT 'text' COMMENT '内容类型：text图文；video视频',
  `content_html` MEDIUMTEXT NULL COMMENT '正文HTML',
  `video_url` VARCHAR(512) NULL COMMENT '视频URL',
  `tags` VARCHAR(128) NULL COMMENT '标签（逗号分隔）',
  `view_count` BIGINT NOT NULL DEFAULT 0 COMMENT '浏览量',
  `publish_at` DATETIME NULL COMMENT '发布时间',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1已发布；0草稿；2下线',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  KEY `idx_news_status_time` (`status`, `publish_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='新闻资讯';

CREATE TABLE `banner` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `position` VARCHAR(32) NOT NULL DEFAULT 'home' COMMENT '位置：home等',
  `title` VARCHAR(100) NULL COMMENT '标题',
  `image_url` VARCHAR(512) NOT NULL COMMENT '图片URL',
  `link_type` VARCHAR(16) NULL COMMENT '跳转类型：none/news/event/url',
  `link_value` VARCHAR(512) NULL COMMENT '跳转值（ID或URL）',
  `sort_no` INT NOT NULL DEFAULT 0 COMMENT '排序号',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_banner_pos_status` (`position`, `status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Banner配置';

CREATE TABLE `about_page` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `code` VARCHAR(32) NOT NULL DEFAULT 'about' COMMENT '页面编码',
  `title` VARCHAR(100) NOT NULL COMMENT '标题',
  `content_html` MEDIUMTEXT NULL COMMENT '内容HTML',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_about_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='关于协会页面内容';

CREATE TABLE `member_unit` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name` VARCHAR(128) NOT NULL COMMENT '单位名称',
  `logo_url` VARCHAR(512) NULL COMMENT 'LOGO',
  `intro` VARCHAR(500) NULL COMMENT '简介',
  `content_html` MEDIUMTEXT NULL COMMENT '详情HTML',
  `contact_name` VARCHAR(64) NULL COMMENT '联系人',
  `contact_phone` VARCHAR(32) NULL COMMENT '联系电话',
  `address` VARCHAR(255) NULL COMMENT '地址',
  `sort_no` INT NOT NULL DEFAULT 0 COMMENT '排序号',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_member_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='会员单位（展示）';

CREATE TABLE `local_association` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name` VARCHAR(128) NOT NULL COMMENT '协会名称',
  `logo_url` VARCHAR(512) NULL COMMENT 'LOGO',
  `intro` VARCHAR(500) NULL COMMENT '简介',
  `content_html` MEDIUMTEXT NULL COMMENT '详情HTML',
  `contact_name` VARCHAR(64) NULL COMMENT '联系人',
  `contact_phone` VARCHAR(32) NULL COMMENT '联系电话',
  `sort_no` INT NOT NULL DEFAULT 0 COMMENT '排序号',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='地方协会（展示）';

CREATE TABLE `event` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `event_type` TINYINT NOT NULL COMMENT '活动类型：1赛事；2评定；3培训',
  `signup_scope` TINYINT NOT NULL DEFAULT 1 COMMENT '报名范围：1开放型；2封闭型（预留）',
  `limit_org_id` BIGINT UNSIGNED NULL COMMENT '封闭型限定机构ID（预留）',
  `title` VARCHAR(128) NOT NULL COMMENT '标题',
  `cover_url` VARCHAR(512) NULL COMMENT '封面图URL',
  `need_audit` TINYINT NOT NULL DEFAULT 1 COMMENT '是否需要审核：1是；0否',
  `need_pay` TINYINT NOT NULL DEFAULT 1 COMMENT '是否需要缴费：1是；0否',
  `signup_start_at` DATETIME NULL COMMENT '报名开始时间',
  `signup_end_at` DATETIME NULL COMMENT '报名结束时间',
  `event_date` DATE NULL COMMENT '活动日期',
  `location` VARCHAR(255) NULL COMMENT '地点',
  `description_html` MEDIUMTEXT NULL COMMENT '详情介绍（HTML）',
  `status` TINYINT NOT NULL DEFAULT 0 COMMENT '状态：0草稿；1报名中；2报名截止；3进行中；4已结束；5下线',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  KEY `idx_event_type_status` (`event_type`, `status`),
  KEY `idx_event_signup_time` (`signup_start_at`, `signup_end_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='活动表（赛事/评定/培训统一建模）';

INSERT INTO `event` (`event_type`, `signup_scope`, `title`, `cover_url`, `need_audit`, `need_pay`, `signup_start_at`, `signup_end_at`, `event_date`, `location`, `description_html`, `status`)
VALUES
(1, 1, '春季校园跳绳公开赛（长沙赛区）', '/banner-1.svg', 1, 1, NOW(), DATE_ADD(NOW(), INTERVAL 10 DAY), DATE_ADD(NOW(), INTERVAL 20 DAY), '长沙市体育馆', '<p>团体与个人项目齐开，支持线上报名与审核缴费。</p>', 1),
(1, 1, '春季团体速度赛 · 省赛', '/banner-2.svg', 1, 1, DATE_ADD(NOW(), INTERVAL 5 DAY), DATE_ADD(NOW(), INTERVAL 15 DAY), DATE_ADD(NOW(), INTERVAL 25 DAY), '湖南省体育中心', '<p>项目丰富、规则透明，电子证书赛后发放。</p>', 0),
(2, 1, '3月等级评定（长沙地区）', '/banner-3.svg', 1, 1, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), DATE_ADD(NOW(), INTERVAL 14 DAY), '长沙市某中学体育馆', '<p>标准化评定流程，后台审核留痕。</p>', 1);

CREATE TABLE `event_group` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `event_id` BIGINT UNSIGNED NOT NULL COMMENT '活动ID',
  `code` VARCHAR(16) NOT NULL COMMENT '组别编码（A1/A2等）',
  `name` VARCHAR(64) NOT NULL COMMENT '组别名称',
  `fee_amount` INT NOT NULL DEFAULT 0 COMMENT '报名费（分）',
  `quota` INT NULL COMMENT '名额（空表示不限）',
  `sort_no` INT NOT NULL DEFAULT 0 COMMENT '排序号',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_event_group_code` (`event_id`, `code`),
  KEY `idx_group_event` (`event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='活动组别/参赛组别';

CREATE TABLE `event_entry` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `event_id` BIGINT UNSIGNED NOT NULL COMMENT '活动ID',
  `group_id` BIGINT UNSIGNED NOT NULL COMMENT '组别ID',
  `athlete_person_id` BIGINT UNSIGNED NOT NULL COMMENT '运动员人员ID',
  `enroll_channel` TINYINT NOT NULL COMMENT '报名方式：1运动员；2家长；3教练（第一教练）',
  `enroll_user_id` BIGINT UNSIGNED NOT NULL COMMENT '报名提交账号ID',
  `status` TINYINT NOT NULL DEFAULT 0 COMMENT '状态：0已提交待审核；1审核驳回；2审核通过待缴费；3支付中；4已缴费；5已确认；6已取消；7支付失败；8退款中；9已退款',
  `audit_remark` VARCHAR(255) NULL COMMENT '审核备注/驳回原因',
  `pay_order_id` BIGINT UNSIGNED NULL COMMENT '支付订单ID',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_entry_event_status` (`event_id`, `status`),
  KEY `idx_entry_athlete` (`athlete_person_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='活动报名表（状态机驱动）';

CREATE TABLE `pay_order` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `biz_type` TINYINT NOT NULL COMMENT '业务类型：1活动报名；2首次发证；3补证',
  `biz_id` BIGINT UNSIGNED NOT NULL COMMENT '业务ID（如entryId/certIssueId）',
  `user_id` BIGINT UNSIGNED NOT NULL COMMENT '下单账号ID',
  `amount` INT NOT NULL COMMENT '订单金额（分）',
  `status` TINYINT NOT NULL DEFAULT 0 COMMENT '状态：0待支付；1已支付；2已关闭；3已退款',
  `wx_out_trade_no` VARCHAR(64) NULL COMMENT '微信商户订单号',
  `paid_at` DATETIME NULL COMMENT '支付完成时间',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_out_trade_no` (`wx_out_trade_no`),
  KEY `idx_pay_biz` (`biz_type`, `biz_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='支付订单表';

CREATE TABLE `pay_wechat_notify` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `pay_order_id` BIGINT UNSIGNED NULL COMMENT '支付订单ID',
  `out_trade_no` VARCHAR(64) NULL COMMENT '商户订单号',
  `transaction_id` VARCHAR(64) NULL COMMENT '微信支付单号',
  `notify_raw` MEDIUMTEXT NOT NULL COMMENT '回调原文（用于审计与排障）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_notify_out_trade_no` (`out_trade_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='微信支付回调记录（原文落库）';

CREATE TABLE `cert_type` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `code` VARCHAR(32) NOT NULL COMMENT '类型编码：coach_cert/athlete_level等',
  `name` VARCHAR(64) NOT NULL COMMENT '证书类型名称',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_cert_type_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='证书类型表';

CREATE TABLE `cert_pricing` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `cert_type_id` BIGINT UNSIGNED NOT NULL COMMENT '证书类型ID',
  `issue_scene` TINYINT NOT NULL COMMENT '发证场景：1首次；2补证',
  `amount` INT NOT NULL COMMENT '价格（分）',
  `effective_from` DATETIME NOT NULL COMMENT '生效时间',
  `effective_to` DATETIME NULL COMMENT '失效时间',
  PRIMARY KEY (`id`),
  KEY `idx_pricing_type_scene_time` (`cert_type_id`, `issue_scene`, `effective_from`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='证书定价（首次/补证分开定价）';

CREATE TABLE `certificate` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `cert_no` VARCHAR(64) NOT NULL COMMENT '证书编号（唯一）',
  `cert_type_id` BIGINT UNSIGNED NOT NULL COMMENT '证书类型ID',
  `holder_person_id` BIGINT UNSIGNED NOT NULL COMMENT '持证人员ID',
  `issue_scene` TINYINT NOT NULL COMMENT '发证场景：1首次；2补证',
  `issue_at` DATETIME NOT NULL COMMENT '发证时间',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；2作废',
  `file_url` VARCHAR(512) NULL COMMENT '电子证书文件URL（PDF/图片）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_cert_no` (`cert_no`),
  KEY `idx_cert_holder` (`holder_person_id`, `cert_type_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='证书表（支持电子版展示）';

CREATE TABLE `admin_user` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `username` VARCHAR(64) NOT NULL COMMENT '账号',
  `password_hash` VARCHAR(255) NOT NULL COMMENT '密码哈希',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_admin_username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='后台用户';

CREATE TABLE `admin_role` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name` VARCHAR(64) NOT NULL COMMENT '角色名称',
  `code` VARCHAR(64) NOT NULL COMMENT '角色编码',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_admin_role_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='后台角色';

CREATE TABLE `admin_user_role` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `admin_user_id` BIGINT UNSIGNED NOT NULL COMMENT '后台用户ID',
  `admin_role_id` BIGINT UNSIGNED NOT NULL COMMENT '后台角色ID',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_admin_user_role` (`admin_user_id`, `admin_role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='后台用户-角色关联';

CREATE TABLE `audit_log` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `actor_type` TINYINT NOT NULL COMMENT '操作人类型：1后台；2小程序',
  `actor_id` BIGINT UNSIGNED NOT NULL COMMENT '操作人ID（后台用户ID或账号ID）',
  `action` VARCHAR(64) NOT NULL COMMENT '动作编码（如identity_approve/first_coach_change）',
  `biz_type` VARCHAR(32) NULL COMMENT '业务类型（如event_entry/user_identity）',
  `biz_id` BIGINT UNSIGNED NULL COMMENT '业务ID',
  `detail_json` MEDIUMTEXT NULL COMMENT '详情JSON（变更前后摘要）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_audit_actor_time` (`actor_type`, `actor_id`, `created_at`),
  KEY `idx_audit_biz` (`biz_type`, `biz_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='操作审计日志';

CREATE TABLE `person_judge` (
  `person_id` BIGINT UNSIGNED NOT NULL COMMENT '人员ID',
  `org_id` BIGINT UNSIGNED NOT NULL COMMENT '所属机构ID（培训机构或中小学）',
  `judge_level` VARCHAR(32) NULL COMMENT '裁判等级（可选）',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1有效；0无效',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`person_id`),
  KEY `idx_judge_org` (`org_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='裁判员角色表（必须绑定机构）';

CREATE TABLE `project_catalog` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `code` VARCHAR(32) NULL COMMENT '项目编码（选填）',
  `name` VARCHAR(128) NOT NULL COMMENT '项目名称',
  `participant_count` INT NOT NULL DEFAULT 1 COMMENT '人数（单人/双人/团队）',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '状态：1启用；0停用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_project_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='项目字典（来源：项目设定.xlsx）';

CREATE TABLE `assessment_record` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '主键',
  `event_id` BIGINT UNSIGNED NULL COMMENT '活动ID（评定/培训）',
  `person_id` BIGINT UNSIGNED NOT NULL COMMENT '人员ID',
  `role_type` TINYINT NOT NULL COMMENT '角色类型：1学员；2教练员；3裁判员',
  `group_name` VARCHAR(64) NULL COMMENT '组别',
  `project_name` VARCHAR(128) NULL COMMENT '项目',
  `level` VARCHAR(32) NULL COMMENT '等级/段位',
  `result_status` VARCHAR(16) NULL COMMENT '考核状态/结果（通过/未通过等）',
  `score` INT NULL COMMENT '分数/成绩',
  `association_name` VARCHAR(128) NULL COMMENT '所属协会',
  `province` VARCHAR(32) NULL COMMENT '省',
  `city` VARCHAR(32) NULL COMMENT '市',
  `district` VARCHAR(32) NULL COMMENT '区县',
  `sign_person` VARCHAR(64) NULL COMMENT '签证人',
  `activity_date` DATE NULL COMMENT '活动日期',
  `issue_date` DATE NULL COMMENT '发证日期',
  `valid_period_text` VARCHAR(64) NULL COMMENT '有效期文本（如2023/9/2-2027/9/1）',
  `coach_name` VARCHAR(64) NULL COMMENT '教练员',
  `location` VARCHAR(255) NULL COMMENT '地点/活动地点',
  `cert_no` VARCHAR(64) NULL COMMENT '证书编号',
  `title` VARCHAR(64) NULL COMMENT '称号（如初级教练员、三级裁判员等）',
  `org_name` VARCHAR(128) NULL COMMENT '所属单位/学校',
  `activity_name` VARCHAR(128) NULL COMMENT '活动名称',
  `issuer_org` VARCHAR(128) NULL COMMENT '发证机构',
  `recommender` VARCHAR(64) NULL COMMENT '推荐人',
  `recommender_phone` VARCHAR(32) NULL COMMENT '推荐人电话',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_assess_person_role_time` (`person_id`, `role_type`, `created_at`),
  KEY `idx_assess_event` (`event_id`),
  KEY `idx_assess_cert_no` (`cert_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='考核记录归档（报名+成绩+发证信息）';

