using MongoDB.Bson;
using NotificationSystem.DTO;
using NotificationSystem.Entities;

namespace NotificationSystem.Services;

public interface IPersonsService
{
    Task<ObjectId> Create(PersonCreateDto personCreateDto);
    Task<IList<Person>> GetAll();
    Task<Person?> GetById(ObjectId id);
    Task<Person?> Update(string id, PersonUpdateDto person);
    Task<bool> Delete(string id);
}