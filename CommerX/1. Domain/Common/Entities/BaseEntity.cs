using System;
using System.Collections.Generic;
using System.Text;

namespace CommerX.Domain.Common.Entities
{
    public abstract class BaseEntity
    {
        // ── Identidad ────────────────────────────────────────────────────────
        // protected set: el constructor base lo asigna; las subclases pueden leerlo
        public Guid Id { get; protected set; }

        // ── Auditoría de creación ─────────────────────────────────────────────
        // CreatedBy: Id del usuario que creó el registro (Guid.Empty = no asignado)
        public Guid CreatedBy { get; protected set; }
        // CreatedOn: fecha y hora UTC en que se creó el registro
        public DateTime CreatedOn { get; protected set; }

        // ── Auditoría de modificación ─────────────────────────────────────────
        // ModifiedBy: Id del usuario de la última modificación (Guid.Empty = nunca modificado)
        public Guid ModifiedBy { get; protected set; }
        // ModifiedOn: DateTime? — el "?" indica que puede ser null (nunca modificado)
        public DateTime? ModifiedOn { get; protected set; }
        // ── Constructor protegido con parámetros opcionales ───────────────────
        // "default" como valor por defecto permite que el mismo constructor
        // sirva para crear entidades nuevas Y para materialización con EF Core
        protected BaseEntity(
        Guid id = default,
        Guid createdBy = default,
        DateTime createdOn = default)
        {
            // si llegó default (todos ceros) → generar valor nuevo; si llegó real → conservarlo
            Id = id == default ? Guid.NewGuid() : id;
            CreatedBy = createdBy == default ? Guid.Empty : createdBy;
            CreatedOn = createdOn == default ? DateTime.UtcNow : createdOn;
            // auditoría de modificación: inicia vacía, se completa en Módulo 8
            ModifiedBy = Guid.Empty;
            ModifiedOn = null;
        }
    }
}
