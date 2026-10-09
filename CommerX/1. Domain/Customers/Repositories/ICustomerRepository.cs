// CommerX.Domain/Customers/Repositories/ICustomerRepository.cs

//¿Por qué esta interfaz va en Domain y no en Application?
/*
El contrato del repositorio va en Domain porque trabaja con el cliente
que es una entidad del dominio. Si selo ponemos en aplication, el Domain terminaría 
dependiendo de las capas de afuera, y eso rompe la regla de oro de esta arquitectura: 
las dependencias siempre tienen que apuntar hacia adentro
 */
using CommerX.Domain.Customers.Entities;

namespace CommerX.Domain.Customers.Repositories;

// contrato de persistencia Infrastructure lo implementa con EF Core
public interface ICustomerRepository
{
    // verifica si ya existe un cliente con ese número de documento
    // retorna bool — el UseCase solo necesita saber si existe, no obtener la entidad
    Task<bool> ExistsByDocumentAsync(string document);

    // verifica si ya existe un cliente registrado con ese email
    Task<bool> ExistsByEmailAsync(string email);

    // busca un cliente por su número de documento
    // devuelve null si no existe
    Task<Customer?> FindByDocumentAsync(string document);

    // busca un cliente por su Id
    Task<Customer?> FindByIdAsync(Guid id);

    // busca un cliente por su email
    Task<Customer?> FindByEmailAsync(string email);

    // persiste el nuevo cliente en el almacenamiento
    Task AddAsync(Customer customer);

    // actualiza un cliente existente
    Task UpdateAsync(Customer customer);
}