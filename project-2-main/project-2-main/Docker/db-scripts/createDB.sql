USE master
GO

DROP DATABASE IF EXISTS SDTrabalho2
CREATE DATABASE SDTrabalho2
GO

USE SDTrabalho2
GO

Create table Users(
Id int identity(1,1) primary key,
Username varchar(200) not null,
Password varchar (250) not null,
Role varchar (200) not null
);



Create table Domicilios (
Num_Admin int identity(1,1) primary key,
Estado varchar(200) not null,
Nome varchar(200) not null,

);

Create table Modalidades (
Id int identity(1,1) primary key,
Megas int not null
)


Create table Modalidades_Domicilios(
Id int identity(1,1) primary key,
DomiciliosNum_Admin int references Domicilios(Num_Admin),
ModalidadesId int references Modalidades(Id)

);

Create table Reserva (
Id int identity(1,1) primary key,
UserId int references Users(Id),
Modalidades_DomiciliosId int references Modalidades_Domicilios(Id),
);
