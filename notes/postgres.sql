create table mob (id serial primary key,
	objectid int not null,
	parent int,
	pathid varchar(255) not null,
	location int,
	perms int not null,
	owner int not null,
	checkpoint int not null);
create table verb (id serial primary key,
	name varchar(255) not null,
	code text not null,
	object int not null,
	perms int not null);
create table attribute (id serial primary key,
	textcontents text,
	datacontents varchar(255),
	name varchar(255) not null,
	mimetype varchar(255) not null,
	object int not null,
	perms int not null);
create table checkpoint (id serial primary key,
	time timestamp not null,
	name varchar(255));
create table config (id serial primary key,
	name varchar(255) not null,
	intvalue int,
	strvalue varchar(255),
	checkpoint int not null);

