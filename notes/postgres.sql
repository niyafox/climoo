create table mob (
	id serial primary key,
	objectid int not null,
	parentid int,
	pathid varchar(255) not null,
	locationid int,
	perms int not null,
	ownerid int not null
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


