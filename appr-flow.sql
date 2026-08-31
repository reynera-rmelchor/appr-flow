USE [rmelchor_test]
GO

-- =============================================================================
-- SISTEMA DE FLUJOS DE APROBACIÓN - MS SQL SERVER (T-SQL)
-- Mapeos Numéricos Actualizados:
--   usuarios.rol: 1 = ADMIN, 2 = USUARIO
--   flujos.estado: 1 = PENDIENTE, 2 = APROBADO, 3 = RECHAZADO
--   flujo_pasos.estado: 1 = PENDIENTE, 2 = APROBADO, 3 = RECHAZADO, 4 = BLOQUEADO
-- =============================================================================

-- 1. TABLA: usuarios
CREATE TABLE dbo.usuarios (
    id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    email NVARCHAR(150) NOT NULL,
    rol TINYINT NOT NULL CONSTRAINT DF_usuarios_rol DEFAULT 2, -- 1: ADMIN, 2: USUARIO
    activo BIT NOT NULL CONSTRAINT DF_usuarios_activo DEFAULT 1,
    creado_en DATETIME2(7) NOT NULL CONSTRAINT DF_usuarios_creado_en DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT PK_usuarios PRIMARY KEY CLUSTERED (id),
    CONSTRAINT UQ_usuarios_email UNIQUE (email),
    CONSTRAINT CK_usuarios_rol CHECK (rol IN (1, 2))
);
GO

-- 2. TABLA: plantillas
CREATE TABLE dbo.plantillas (
    id INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    activo BIT NOT NULL CONSTRAINT DF_plantillas_activo DEFAULT 1,
    creado_en DATETIME2(7) NOT NULL CONSTRAINT DF_plantillas_creado_en DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT PK_plantillas PRIMARY KEY CLUSTERED (id)
);
GO

-- 3. TABLA: plantilla_pasos
CREATE TABLE dbo.plantilla_pasos (
    id INT IDENTITY(1,1) NOT NULL,
    plantilla_id INT NOT NULL,
    orden INT NOT NULL,
    usuario_aprobador_id INT NOT NULL,
    creado_en DATETIME2(7) NOT NULL CONSTRAINT DF_plantilla_pasos_creado_en DEFAULT SYSUTCDATETIME(),
    
    CONSTRAINT PK_plantilla_pasos PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_plantilla_pasos_plantilla FOREIGN KEY (plantilla_id) 
        REFERENCES dbo.plantillas(id) ON DELETE CASCADE,
    CONSTRAINT FK_plantilla_pasos_usuario FOREIGN KEY (usuario_aprobador_id) 
        REFERENCES dbo.usuarios(id),
    CONSTRAINT UQ_plantilla_pasos_orden UNIQUE (plantilla_id, orden),
    CONSTRAINT CK_plantilla_pasos_orden CHECK (orden > 0)
);
GO

-- 4. TABLA: flujos
CREATE TABLE dbo.flujos (
    id INT IDENTITY(1,1) NOT NULL,
    plantilla_id INT NOT NULL,
    usuario_creador_id INT NOT NULL,
    titulo NVARCHAR(200) NOT NULL,
    descripcion NVARCHAR(MAX) NULL,
    estado TINYINT NOT NULL CONSTRAINT DF_flujos_estado DEFAULT 1, -- 1: PENDIENTE, 2: APROBADO, 3: RECHAZADO
    creado_en DATETIME2(7) NOT NULL CONSTRAINT DF_flujos_creado_en DEFAULT SYSUTCDATETIME(),
    actualizado_en DATETIME2(7) NULL,
    
    CONSTRAINT PK_flujos PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_flujos_plantilla FOREIGN KEY (plantilla_id) 
        REFERENCES dbo.plantillas(id),
    CONSTRAINT FK_flujos_usuario_creador FOREIGN KEY (usuario_creador_id) 
        REFERENCES dbo.usuarios(id),
    CONSTRAINT CK_flujos_estado CHECK (estado IN (1, 2, 3))
);
GO

-- 5. TABLA: flujo_pasos
CREATE TABLE dbo.flujo_pasos (
    id INT IDENTITY(1,1) NOT NULL,
    flujo_id INT NOT NULL,
    orden INT NOT NULL,
    usuario_asignado_id INT NOT NULL,
    usuario_decision_id INT NULL,
    -- 1: PENDIENTE, 2: APROBADO, 3: RECHAZADO, 4: BLOQUEADO
    estado TINYINT NOT NULL CONSTRAINT DF_flujo_pasos_estado DEFAULT 1, 
    observacion NVARCHAR(MAX) NULL,
    fecha_decision DATETIME2(7) NULL,
    
    CONSTRAINT PK_flujo_pasos PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_flujo_pasos_flujo FOREIGN KEY (flujo_id) 
        REFERENCES dbo.flujos(id) ON DELETE CASCADE,
    CONSTRAINT FK_flujo_pasos_usuario_asignado FOREIGN KEY (usuario_asignado_id) 
        REFERENCES dbo.usuarios(id),
    CONSTRAINT FK_flujo_pasos_usuario_decision FOREIGN KEY (usuario_decision_id) 
        REFERENCES dbo.usuarios(id),
    CONSTRAINT UQ_flujo_pasos_orden UNIQUE (flujo_id, orden),
    CONSTRAINT CK_flujo_pasos_orden CHECK (orden > 0),
    CONSTRAINT CK_flujo_pasos_estado CHECK (estado IN (1, 2, 3, 4))
);
GO

-- =============================================================================
-- ÍNDICES
-- =============================================================================

CREATE NONCLUSTERED INDEX IX_flujo_pasos_usuario_estado 
    ON dbo.flujo_pasos (usuario_asignado_id, estado)
    INCLUDE (flujo_id, orden);

CREATE NONCLUSTERED INDEX IX_flujo_pasos_flujo_orden 
    ON dbo.flujo_pasos (flujo_id, orden);

CREATE NONCLUSTERED INDEX IX_flujos_creador_estado 
    ON dbo.flujos (usuario_creador_id, estado);
GO

-- =============================================================================
-- PROCEDIMIENTOS ALMACENADOS
-- =============================================================================

-- SP: Crear flujo inicializando estados numéricos
CREATE OR ALTER PROCEDURE dbo.sp_CrearFlujoDesdePlantilla
    @PlantillaId INT,
    @UsuarioCreadorId INT,
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(MAX) = NULL,
    @NuevoFlujoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Insertar flujo con estado 1 (PENDIENTE)
        INSERT INTO dbo.flujos (plantilla_id, usuario_creador_id, titulo, descripcion, estado)
        VALUES (@PlantillaId, @UsuarioCreadorId, @Titulo, @Descripcion, 1);

        SET @NuevoFlujoId = SCOPE_IDENTITY();

        -- Copiar pasos: el primero arranca en 1 (PENDIENTE), los demás en 4 (BLOQUEADO)
        INSERT INTO dbo.flujo_pasos (flujo_id, orden, usuario_asignado_id, estado)
        SELECT 
            @NuevoFlujoId,
            orden,
            usuario_aprobador_id,
            CASE WHEN orden > 1 THEN 4 ELSE 1 END -- 4 (BLOQUEADO)
        FROM dbo.plantilla_pasos
        WHERE plantilla_id = @PlantillaId
        ORDER BY orden ASC;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Procesar decisión con valores numéricos
CREATE OR ALTER PROCEDURE dbo.sp_ProcesarDecisionPaso
    @FlujoPasoId INT,
    @UsuarioId INT,
    @Decision TINYINT,          -- 2: APROBADO, 3: RECHAZADO
    @Observacion NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @FlujoId INT, @OrdenActual INT, @UsuarioAsignadoId INT, @EstadoPaso TINYINT;
        DECLARE @RolUsuario TINYINT;

        -- Validar valor del parámetro @Decision
        IF @Decision NOT IN (2, 4)
        BEGIN
            RAISERROR('La decisión enviada es inválida. Debe ser 2 (APROBADO) o 3 (RECHAZADO).', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Obtener datos del paso actual
        SELECT 
            @FlujoId = fp.flujo_id,
            @OrdenActual = fp.orden,
            @UsuarioAsignadoId = fp.usuario_asignado_id,
            @EstadoPaso = fp.estado
        FROM dbo.flujo_pasos fp
        WHERE fp.id = @FlujoPasoId;

        -- Validar que exista y esté en 1 (PENDIENTE)
        IF @FlujoPasoId IS NULL OR @EstadoPaso <> 1
        BEGIN
            RAISERROR('El paso no existe o no se encuentra en estado PENDIENTE.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validar permisos (usuario asignado o rol 1: ADMIN)
        SELECT @RolUsuario = rol FROM dbo.usuarios WHERE id = @UsuarioId;
        IF @UsuarioId <> @UsuarioAsignadoId AND @RolUsuario <> 1
        BEGIN
            RAISERROR('El usuario no cuenta con permisos para autorizar/rechazar este paso.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Registrar decisión en el paso actual
        UPDATE dbo.flujo_pasos
        SET estado = @Decision,
            usuario_decision_id = @UsuarioId,
            observacion = @Observacion,
            fecha_decision = SYSUTCDATETIME()
        WHERE id = @FlujoPasoId;

        -- Evaluar avance o rechazo total
        IF @Decision = 3 -- RECHAZADO
        BEGIN
            UPDATE dbo.flujos 
            SET estado = 3, -- El estado de la tabla 'flujos' se establece a 3 (RECHAZADO)
                actualizado_en = SYSUTCDATETIME() 
            WHERE id = @FlujoId;
        END
        ELSE IF @Decision = 2 -- APROBADO
        BEGIN
            DECLARE @SiguientePasoId INT;
            SELECT TOP 1 @SiguientePasoId = id 
            FROM dbo.flujo_pasos 
            WHERE flujo_id = @FlujoId AND orden = (@OrdenActual + 1);

            IF @SiguientePasoId IS NOT NULL
            BEGIN
                -- Cambiar siguiente paso a 1 (PENDIENTE)
                UPDATE dbo.flujo_pasos SET estado = 1 WHERE id = @SiguientePasoId;
            END
            ELSE
            BEGIN
                -- Completado: flujo pasa a 2 (APROBADO)
                UPDATE dbo.flujos 
                SET estado = 2, 
                actualizado_en = SYSUTCDATETIME() 
                WHERE id = @FlujoId;
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO