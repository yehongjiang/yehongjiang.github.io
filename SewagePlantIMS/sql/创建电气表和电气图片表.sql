Create table dm_electrical
(
	id int identity(1,1) primary key,
	technology_id int ,
	elec_name nvarchar(2000),
	remarks nvarchar(2000),
	qrcode varchar(2000),
	foreign key(technology_id) references dm_technology(id)
);

Create table dm_elec_pic
(
	id int identity(1,1) primary key,
	pic_name nvarchar(500),
	pic_url varchar(2000),
	add_date datetime,
	elec_id int,
	foreign key(elec_id) references dm_electrical(id)
);