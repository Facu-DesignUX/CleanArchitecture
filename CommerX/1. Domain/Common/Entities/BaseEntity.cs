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

        // ── Auditoría de creación y modificación (Módulo 8) ───────────────────
        // CreatedAt solo tiene getter (EF Core requiere mapeo explícito)
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        // UpdatedAt tiene get y set
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Auditoría histórica (retrocompatibilidad) ─────────────────────────
        public Guid CreatedBy { get; protected set; }
        public DateTime CreatedOn => CreatedAt;
        public Guid ModifiedBy { get; protected set; }
        public DateTime? ModifiedOn => UpdatedAt;
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
            CreatedAt = createdOn == default ? DateTime.UtcNow : createdOn;
            // auditoría de modificación: inicia con la fecha de creación
            ModifiedBy = Guid.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
