/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

DROP TABLE IF EXISTS `mob`;
CREATE TABLE `mob` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `objectid` int NOT NULL,
  `parentid` int DEFAULT NULL,
  `pathid` varchar(255) DEFAULT NULL,
  `locationid` int DEFAULT NULL,
  `perms` int NOT NULL,
  `ownerid` int NOT NULL,
  `pulse` tinyint NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `verb`;
CREATE TABLE `verb` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `code` longtext NOT NULL,
  `mobid` bigint unsigned NOT NULL,
  `perms` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `attribute`;
CREATE TABLE `attribute` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `textcontents` longtext DEFAULT NULL,
  `datacontents` longblob DEFAULT NULL,
  `name` varchar(255) NOT NULL,
  `mimetype` varchar(255) NOT NULL,
  `mobid` bigint unsigned NOT NULL,
  `perms` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `mobtable`;
CREATE TABLE `mobtable` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `mobid` bigint unsigned NOT NULL,
  `objectid` int NOT NULL,
  `checkpointid` bigint unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `checkpoint`;
CREATE TABLE `checkpoint` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `time` datetime DEFAULT NULL,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `config`;
CREATE TABLE `config` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `intvalue` int DEFAULT NULL,
  `strvalue` varchar(255) DEFAULT NULL,
  `checkpointid` bigint unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `screen`;
CREATE TABLE `screen` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `text` longtext DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `login` varchar(255) NOT NULL,
  `openid` tinyint NOT NULL,
  `password` varchar(255) NOT NULL,
  `objectid` int NOT NULL,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;

DROP TABLE IF EXISTS `test`;
CREATE TABLE `test` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `str` varchar(255) DEFAULT NULL,
  `longer` longtext DEFAULT NULL,
  `num` int DEFAULT NULL,
  `datacol` longblob DEFAULT NULL,
  `bool` tinyint DEFAULT NULL,
  `time` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB;
