using MongoDB.Bson.Serialization;
using ProductManagementSystem.Application.Domain.Users.Models;
using ProductManagementSystem.Application.Domain.Subscriptions.Models;
using ProductManagementSystem.Application.Domain.Products.Models;
using ProductManagementSystem.Application.Domain.UserPlans.Domain;
using ProductManagementSystem.Application.Domain.UserPlans.Models;

namespace ProductManagementSystem.Application.Infrastructure.MongoDB;

public static class MongoDbConfiguration
{
    private static bool _configured = false;

    public static void Configure()
    {
        if (_configured) return;


        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(u => u.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Company>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(c => c.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Subscription>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(s => s.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Product>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(p => p.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<UserPlan>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(up => up.Id);
            cm.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<Credential>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });

        _configured = true;
    }
}