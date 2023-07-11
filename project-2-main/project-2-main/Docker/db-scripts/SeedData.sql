-- Inserção de dados na tabela Users
INSERT INTO Users (Username, Password, Role)
VALUES
  ('user1', 'password1', 'ADMIN'),
  ('user2', 'password2', 'OPERATOR'),
  ('user3', 'password3', 'OPERATOR');

-- Inserção de dados na tabela Domicilios
INSERT INTO Domicilios (Estado, Nome)
VALUES                                                    -- NUM ADMIN (ID)
  ('RESERVADO', 'Castro Daire'),			 -- 1
  ('RESERVADO', 'Souto de Alva'),			 -- 2
  ('RESERVADO', 'Soutelo'),					 -- 3

  ('TERMINADO', 'Rebordosa'),				 -- 4
  ('TERMINADO', 'Marco de Canaveses'),		 -- 5
  ('TERMINADO', 'Vila Mea'),				 -- 6

  ('TERMINADO', 'Lisboa'),					 -- 7
  ('TERMINADO', 'Porto'),                      -- 8
  ('TERMINADO', 'Gaia'),						 -- 9
  
  ('TERMINADO', 'Braga'),					 -- 10
  ('TERMINADO', 'Paços de Ferreira'),        -- 11
  ('TERMINADO', 'Ponte de Lima');			 -- 12


-- Inserção de dados na tabela Modalidades
INSERT INTO Modalidades (Megas)
VALUES
  (50),   -- 1
  (100),  -- 2
  (200),  -- 3
  (300),  -- 4
  (400),  -- 5
  (500);  -- 6

-- Inserção de dados na tabela Modalidades_Domicilios
INSERT INTO Modalidades_Domicilios (DomiciliosNum_Admin, ModalidadesId)
VALUES
  (1, 1),   -- CASTRO DAIRE
  (1, 2),
  (1, 3),
  (1, 4),

  (2, 3),   -- SOUTO DE ALVA
  (2, 6),

  (3, 1),   -- SOUTELO
  (3, 2),
  (3, 3),
  (3, 4),
  (3, 5),
  (3, 6),

  (4, 1),   -- REBORDOSA
  (4, 2),
  (4, 3),
  (4, 4),

  (5, 1),   -- MARCO DE CANAVESES
  (5, 3),
  (5, 5),
  (5, 2),

  (6, 1),   -- VILA MEA
  (6, 2),
  (6, 3),

  (7, 1),   -- LISBOA
  (7, 2),   
  (7, 3),
  (7, 4),

  (8, 1),   -- PORTO
  (8, 2),
  (8, 3),

  (9, 1),   -- GAIA
  (9, 2),
  (9, 3),
  (9, 4),

  (10, 1),   -- BRAGA
  (10, 2),
  (10, 3),
  (10, 4),
  (10, 5),

  (11, 1),   -- PAÇOS DE FERREIRA
  (11, 2),
  (11, 3),

  (12, 1),   -- PONTE DE LIMA
  (12, 2);


-- Inserção de dados na tabela Reserva
INSERT INTO Reserva (UserId, Modalidades_DomiciliosId)
VALUES
  (2, 1),
  (2, 5),
  (3, 12);