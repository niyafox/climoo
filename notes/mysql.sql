DROP TABLE IF EXISTS `mob`;
CREATE TABLE `mob` (
  `id` int NOT NULL AUTO_INCREMENT,
  `objectid` int NOT NULL,
  `parentid` int DEFAULT NULL,
  `pathid` varchar(255) DEFAULT NULL,
  `locationid` int DEFAULT NULL,
  `perms` int NOT NULL,
  `ownerid` int NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `verb`;
CREATE TABLE `verb` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `code` longtext NOT NULL,
  `mobid` int NOT NULL,
  `perms` int NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `attribute`;
CREATE TABLE `attribute` (
  `id` int NOT NULL AUTO_INCREMENT,
  `textcontents` longtext DEFAULT NULL,
  `datacontents` varchar(255) DEFAULT NULL,
  `name` varchar(255) NOT NULL,
  `mimetype` varchar(255) NOT NULL,
  `mobid` int NOT NULL,
  `perms` int NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `mobtable`;
CREATE TABLE `mobtable` (
  `id` int NOT NULL AUTO_INCREMENT,
  `mobid` int NOT NULL,
  `objectid` int NOT NULL,
  `checkpointid` int NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `checkpoint`;
CREATE TABLE `checkpoint` (
  `id` int NOT NULL AUTO_INCREMENT,
  `time` datetime DEFAULT NULL,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `config`;
CREATE TABLE `config` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `intvalue` int DEFAULT NULL,
  `strvalue` varchar(255) DEFAULT NULL,
  `checkpointid` int NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `screen`;
CREATE TABLE `screen` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `text` longtext DEFAULT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `login` varchar(255) NOT NULL,
  `openid` tinyint NOT NULL,
  `password` varchar(255) NOT NULL,
  `objectid` int NOT NULL,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
);

DROP TABLE IF EXISTS `test`;
CREATE TABLE `test` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `str` varchar(255) DEFAULT NULL,
  `longer` longtext DEFAULT NULL,
  `num` int DEFAULT NULL,
  `datacol` varchar(255) DEFAULT NULL,
  `bool` tinyint DEFAULT NULL,
  `time` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
);
