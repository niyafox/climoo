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

create table mob (
	id serial primary key,
	objectid int not null,
	parentid int,
	pathid varchar(255) not null,
	locationid int,
	perms int not null,
	ownerid int not null,
	pulse tinyint not null
);
create table verb (
	id serial primary key,
	name varchar(255) not null,
	code text not null,
	mobid int not null,
	perms int not null
);
create table attribute (
	id serial primary key,
	textcontents text,
	datacontents varchar(255),
	name varchar(255) not null,
	mimetype varchar(255) not null,
	mobid int not null,
	perms int not null
);
create table mobtable (
	id serial primary key,
	mobid int not null,
	objectid int not null,
	checkpointid int not null
);
create table checkpoint (
	id serial primary key,
	time timestamp not null,
	name varchar(255)
);
create table config (
	id serial primary key,
	name varchar(255) not null,
	intvalue int,
	strvalue varchar(255),
	checkpointid int not null
);
create table screen (
	id serial primary key,
	name varchar(255) not null,
	text text
);
create table user (
	id serial primary key,
	login varchar(255) not null,
	openid tinyint not null,
	password varchar(255) not null,
	objectid int not null,
	name varchar(255) not null
);


