using System.Collections.Generic;
using FastTests;
using Raven.NewClient.Client;
using Raven.NewClient.Client.Indexes;
using Raven.NewClient.Client.Indexing;
using Xunit;

namespace SlowTests.Issues
{
    public class RavenDB_3609 : RavenNewTestBase
    {
        private class Profile
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
        }

        //private class HybridShardingResolutionStrategy : DefaultShardResolutionStrategy
        //{
        //    private readonly HashSet<Type> _sharedTypes;
        //    private readonly string _defaultShard;

        //    public HybridShardingResolutionStrategy(IEnumerable<string> shardIds, ShardStrategy shardStrategy,
        //        IEnumerable<Type> sharedTypes, string defaultShard)
        //        : base(shardIds, shardStrategy)
        //    {
        //        _sharedTypes = new HashSet<Type>(sharedTypes);
        //        _defaultShard = defaultShard;
        //    }

        //    public override string GenerateShardIdFor(object entity, object owner)
        //    {
        //        if (!_sharedTypes.Contains(entity.GetType()))
        //            return ShardIds.FirstOrDefault(x => x == _defaultShard);

        //        return base.GenerateShardIdFor(entity, owner);
        //    }
        //}

        private class ProfileIndex : AbstractIndexCreationTask
        {
            public override IndexDefinition CreateIndexDefinition()
            {
                return new IndexDefinition
                {
                    Maps = { @"from profile in docs select new { profile.Id, profile.Name, profile.Location };" }
                };
            }
        }

        [Fact(Skip = "RavenDB-6283")]
        public void Test()
        {
            using (var shard1 = GetDocumentStore())
            {
                using (var shard2 = GetDocumentStore())
                {
                    var shards = new Dictionary<string, IDocumentStore>
                    {
                        {"Shard1", shard1},
                        {"Shard2", shard2},
                    };

                    //var shardStrategy = new ShardStrategy(shards);
                    //shardStrategy.ShardResolutionStrategy = new HybridShardingResolutionStrategy(shards.Keys, shardStrategy, new Type[0], "Shard1");
                    //shardStrategy.ShardingOn<Profile>(x => x.Location);

                    //using (var shardedDocumentStore = new ShardedDocumentStore(shardStrategy))
                    //{
                    //    shardedDocumentStore.Initialize();
                    //    new ProfileIndex().Execute(shardedDocumentStore);

                    //    var facets = new List<Facet>
                    //    {
                    //        new Facet {Name = "Name", Mode = FacetMode.Default}
                    //    };
                    //    var profile = new Profile { Name = "Test", Location = "Shard1" };

                    //    using (var documentSession = shardedDocumentStore.OpenSession())
                    //    {
                    //        documentSession.Store(profile, profile.Id);

                    //        documentSession.SaveChanges();
                    //    }

                    //    using (var documentSession = shardedDocumentStore.OpenAsyncSession())
                    //    {
                    //        var query = documentSession.Query<Profile>("ProfileIndex").Where(x => x.Name == "Test");
                    //        var res = await query.ToFacetsAsync(facets);
                    //        Assert.Equal(1, res.Results.Count);
                    //    }
                    //}
                }
            }
        }
    }
}
