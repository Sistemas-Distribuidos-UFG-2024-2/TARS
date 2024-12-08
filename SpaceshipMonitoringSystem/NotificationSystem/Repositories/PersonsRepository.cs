using MongoDB.Bson;
using MongoDB.Driver;
using NotificationSystem.Database;
using NotificationSystem.Entities;

namespace NotificationSystem.Repositories;

public class PersonsRepository : IPersonsRepository
{
    private readonly IRepository<Person> _repository;

    public PersonsRepository(IMongoDbContext dbContext)
    {
        _repository = dbContext.GetRepository<Person>();
    }

    public async Task Create(Person person)
    {
        await _repository.InsertOne(person);
    }

    public async Task<IList<Person>> GetAll()
    {
        var filter = Builders<Person>.Filter.Empty;
        return await _repository.Find(filter);
    }

    public async Task<Person?> GetById(ObjectId id)
    {
        var filter = Builders<Person>.Filter.Eq(p => p.Id, id);
        return await _repository.FindOne(filter);
    }

    public async Task<Person?> Update(ObjectId id, string? name, string? email)
    {
        var filter = Builders<Person>.Filter.Eq(p => p.Id, id);
        
        var updateDefinitionBuilder = Builders<Person>.Update;
        var updates = new List<UpdateDefinition<Person>> ();
        
        if (name is not null)
        {
            updates.Add(updateDefinitionBuilder.Set(p => p.Name, name));
        }

        if (email is not null)
        {
            updates.Add(updateDefinitionBuilder.Set(p => p.Email, email));
        }

        var options = new FindOneAndUpdateOptions<Person>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _repository.FindOneAndUpdate(filter, updateDefinitionBuilder.Combine(updates), options);
    }

    public async Task<bool> Delete(ObjectId id)
    {
        var filter = Builders<Person>.Filter.Eq(p => p.Id, id);
        return await _repository.DeleteOne(filter);
    }
}