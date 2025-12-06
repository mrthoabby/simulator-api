using MongoDB.Driver;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.AppEntities.Shared.Type;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Repository;

public class GlobalParametersRepository : IGlobalParametersRepository
{
    private readonly IMongoCollection<Concept> _globalParametersCollection;
    private readonly ILogger<GlobalParametersRepository> _logger;

    public GlobalParametersRepository(IMongoDatabase database, ILogger<GlobalParametersRepository> logger)
    {
        _globalParametersCollection = database.GetCollection<Concept>("GlobalParameters");
        _logger = logger;

        CreateIndexes();
    }


    public async Task<Concept?> GetAsync(string conceptCode)
    {
        try
        {
            _logger.LogInformation("Getting global parameter: {ConceptCode}", conceptCode);

            var filter = Builders<Concept>.Filter.Eq(d => d.ConceptCode, conceptCode);
            var deduction = await _globalParametersCollection.Find(filter).FirstOrDefaultAsync();

            if (deduction == null)
            {
                _logger.LogWarning("Global parameter not found: {ConceptCode}", conceptCode);
            }
            else
            {
                _logger.LogInformation("Global parameter retrieved successfully: {ConceptCode}", conceptCode);
            }

            return deduction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global parameter: {ConceptCode}", conceptCode);
            throw;
        }
    }

    public async Task<List<Concept>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Getting all global parameters");

            var deductions = await _globalParametersCollection.Find(_ => true).ToListAsync();

            _logger.LogInformation("Retrieved {Count} global parameters", deductions.Count);
            return deductions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all global parameters");
            throw;
        }
    }

    private void CreateIndexes()
    {
        try
        {
            var conceptCodeIndex = Builders<Concept>.IndexKeys.Ascending(d => d.ConceptCode);
            var conceptCodeIndexModel = new CreateIndexModel<Concept>(
                conceptCodeIndex,
                new CreateIndexOptions { Unique = true, Name = "ConceptCode_unique" }
            );

            var typeIndex = Builders<Concept>.IndexKeys.Ascending(d => d.Type);
            var typeIndexModel = new CreateIndexModel<Concept>(
                typeIndex,
                new CreateIndexOptions { Name = "Type_index" }
            );

            var applicationIndex = Builders<Concept>.IndexKeys.Ascending(d => d.Application);
            var applicationIndexModel = new CreateIndexModel<Concept>(
                applicationIndex,
                new CreateIndexOptions { Name = "Application_index" }
            );

            _globalParametersCollection.Indexes.CreateMany(new[]
            {
                conceptCodeIndexModel,
                typeIndexModel,
                applicationIndexModel
            });

            _logger.LogInformation("MongoDB indexes created successfully for GlobalParameters collection");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creating MongoDB indexes for GlobalParameters collection");
        }
    }

    public async Task<Concept> CreateAsync(Concept deduction)
    {
        try
        {
            _logger.LogInformation("Creating global parameter: {ConceptCode}", deduction.ConceptCode);

            await _globalParametersCollection.InsertOneAsync(deduction);

            _logger.LogInformation("Global parameter created successfully: {ConceptCode}", deduction.ConceptCode);
            return deduction;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "Duplicate concept code found when creating global parameter: {ConceptCode}", deduction.ConceptCode);
            throw new ConflictException($"A global parameter with concept code {deduction.ConceptCode} already exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating global parameter: {ConceptCode}", deduction.ConceptCode);
            throw;
        }
    }

    public async Task<Concept?> UpdateAsync(Concept concept)
    {
        try
        {
            _logger.LogInformation("Updating global parameter: {ConceptCode}", concept.ConceptCode);

            var filter = Builders<Concept>.Filter.Eq(d => d.ConceptCode, concept.ConceptCode);
            var result = await _globalParametersCollection.ReplaceOneAsync(filter, concept);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Global parameter not found for update: {ConceptCode}", concept.ConceptCode);
                return null;
            }

            _logger.LogInformation("Global parameter updated successfully: {ConceptCode}", concept.ConceptCode);
            return concept;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating global parameter: {ConceptCode}", concept.ConceptCode);
            throw;
        }
    }

}