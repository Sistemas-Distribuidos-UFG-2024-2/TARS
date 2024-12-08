using MongoDB.Bson;
using NotificationSystem.Entities;

namespace NotificationSystem.Repositories;

public interface IPersonsRepository
{
    Task Create(Person person);
    Task<IList<Person>> GetAll();
    Task<Person?> GetById(ObjectId id);
    Task<Person?> Update(ObjectId id, string? name, string? email);
    Task<bool> Delete(ObjectId id);
}