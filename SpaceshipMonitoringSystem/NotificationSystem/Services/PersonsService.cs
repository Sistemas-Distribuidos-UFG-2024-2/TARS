using MongoDB.Bson;
using NotificationSystem.DTO;
using NotificationSystem.Entities;
using NotificationSystem.Repositories;

namespace NotificationSystem.Services;

public class PersonsService : IPersonsService
{
    private readonly IPersonsRepository _personsRepository;

    public PersonsService(IPersonsRepository personsRepository)
    {
        _personsRepository = personsRepository;
    }

    public async Task<ObjectId> Create(PersonCreateDto personCreateDto)
    {
        var person = new Person
        {
            Id = ObjectId.GenerateNewId(),
            Name = personCreateDto.Name,
            Email = personCreateDto.Email,
        };
        
        await _personsRepository.Create(person);
        
        return person.Id;
    }

    public async Task<IList<Person>> GetAll()
    {
        return await _personsRepository.GetAll();
    }

    public async Task<Person?> GetById(ObjectId id)
    {
        return await _personsRepository.GetById(id);
    }

    public async Task<Person?> Update(string id, PersonUpdateDto person)
    {
        return await _personsRepository.Update(ObjectId.Parse(id), person.Name, person.Email);
    }

    public async Task<bool> Delete(string id)
    {
        return await _personsRepository.Delete(ObjectId.Parse(id));
    }
}