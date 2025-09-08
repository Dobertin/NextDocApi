Create database NextDoc;

-- Tabla de roles
CREATE TABLE Roles (
    IdRol INT AUTO_INCREMENT PRIMARY KEY,
    NombreRol VARCHAR(50) NOT NULL,
    Estado BOOLEAN DEFAULT TRUE
);

-- Tabla de áreas
CREATE TABLE Departamentos (
    IdDepartamento INT AUTO_INCREMENT PRIMARY KEY,
    NombreDepartamento VARCHAR(100) NOT NULL,
    Estado BOOLEAN DEFAULT TRUE
);

-- Tipos de documentos (ej. Memorándum, Informe, Solicitud)
CREATE TABLE TiposDocumento (
    IdTipoDocumento INT AUTO_INCREMENT PRIMARY KEY,
    NombreTipo VARCHAR(100) NOT NULL,
    Estado BOOLEAN DEFAULT TRUE
);

-- Tipo de Procesamiento (ej. Recibidos, Remitidos, Certificaciones)
CREATE TABLE Clasificacion (
    IdClasificacion INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Estado BOOLEAN DEFAULT TRUE
);

-- Estados del documento (ej. Registrado, En revisión, Aprobado, Rechazado)
CREATE TABLE EstadosDocumento (
    IdEstado INT AUTO_INCREMENT PRIMARY KEY,
    NombreEstado VARCHAR(50) NOT NULL,
    Estado BOOLEAN DEFAULT TRUE
);

-- Tabla de usuarios
CREATE TABLE Usuarios (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IdRol INT NOT NULL,
    IdDepartamento INT,
    Estado BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (IdRol) REFERENCES Roles(IdRol),
    FOREIGN KEY (IdDepartamento) REFERENCES Departamentos(IdDepartamento)
);

-- Tabla de documentos
CREATE TABLE Documentos (
    IdDocumento INT AUTO_INCREMENT PRIMARY KEY,
    Titulo VARCHAR(200) NOT NULL,
    Descripcion TEXT,
    RutaArchivo VARCHAR(255) NOT NULL,
    FechaCreacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP(),
    IdTipoDocumento INT,
    IdClasificacion INT,
    IdEstado INT,
    IdUsuarioCreador INT,
    IdUsuarioAsignado INT,
    IdDepartamento INT,
    Estado BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (IdTipoDocumento) REFERENCES TiposDocumento(IdTipoDocumento),
    FOREIGN KEY (IdClasificacion) REFERENCES Clasificacion(IdClasificacion),
    FOREIGN KEY (IdEstado) REFERENCES EstadosDocumento(IdEstado),
    FOREIGN KEY (IdUsuarioCreador) REFERENCES Usuarios(IdUsuario),
    FOREIGN KEY (IdUsuarioAsignado) REFERENCES Usuarios(IdUsuario),
    FOREIGN KEY (IdDepartamento) REFERENCES Departamentos(IdDepartamento)
);

-- Historial de cambios del documento
CREATE TABLE HistorialDocumentos (
    IdHistorial INT AUTO_INCREMENT PRIMARY KEY,
    IdDocumento INT NOT NULL,
    IdUsuario INT NOT NULL,
    Accion VARCHAR(100),
    FechaAccion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Comentarios TEXT,
    Estado BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (IdDocumento) REFERENCES Documentos(IdDocumento),
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);

-- Tabla de permisos de acceso por usuario por pantalla
CREATE TABLE PermisosAcceso (
    IdPermiso INT AUTO_INCREMENT PRIMARY KEY,
    TagPantallaID VARCHAR(100),
    IdUsuario INT,
    PuedeVer BOOLEAN DEFAULT TRUE,
    PuedeEditar BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
);


--  Auto-generated SQL script #202506292208
INSERT INTO estadosdocumento (NombreEstado)
	VALUES ('Recibido');
INSERT INTO estadosdocumento (NombreEstado)
	VALUES ('Pendiente');
INSERT INTO estadosdocumento (NombreEstado)
	VALUES ('Procesado');
INSERT INTO estadosdocumento (NombreEstado)
	VALUES ('Archivado');
INSERT INTO estadosdocumento (NombreEstado)
	VALUES ('Eliminado');

--  Auto-generated SQL script #202506292210
INSERT INTO roles (NombreRol)
	VALUES ('Administrador');
INSERT INTO roles (NombreRol)
	VALUES ('Asistente Adm');
INSERT INTO roles (NombreRol)
	VALUES ('Mesa de Partes');

--  Auto-generated SQL script #202506292212
INSERT INTO departamentos (NombreDepartamento)
	VALUES ('DIVADM');
INSERT INTO departamentos (NombreDepartamento)
	VALUES ('UNILOG');
INSERT INTO departamentos (NombreDepartamento)
	VALUES ('UNICONT');
INSERT INTO departamentos (NombreDepartamento)
	VALUES ('DIRPLAINS');

--  Auto-generated SQL script #202506292214
INSERT INTO tiposdocumento (NombreTipo)
	VALUES ('Oficio');
INSERT INTO tiposdocumento (NombreTipo)
	VALUES ('Carta');
INSERT INTO tiposdocumento (NombreTipo)
	VALUES ('Solicitud');

--  Auto-generated SQL script #202506292217
INSERT INTO clasificacion (Nombre)
	VALUES ('Oficio');
INSERT INTO clasificacion (Nombre)
	VALUES ('Certificaciones');
INSERT INTO clasificacion (Nombre)
	VALUES ('Gastos Irrogados');
INSERT INTO clasificacion (Nombre)
	VALUES ('Recibidos');
INSERT INTO clasificacion (Nombre)
	VALUES ('Notas Presupuestales');
INSERT INTO clasificacion (Nombre)
	VALUES ('Hoja de Oficio');
