﻿using System.Linq;
using FastTests;
using Raven.Client.Documents;
using Xunit;

namespace SlowTests.Issues
{
    public class RavenDB_9693 : RavenTestBase
    {
        [Fact]
        public void LinqOrderByDistanceShouldGenerateQueryProperly()
        {
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    var query = session.Query<Item>()
                        .Where(x => x.Name == "John")
                        .Spatial(factory => factory.Point(x => x.Latitude, x => x.Longitude), factory => factory.WithinRadius(10, 10, 10))
                        .OrderByDistanceDescending("someField", 10, 10);

                    var iq = RavenTestHelper.GetIndexQuery(query);

                    Assert.Equal("FROM Items WHERE Name = $p0 AND spatial.within(spatial.point(Latitude, Longitude), spatial.circle($p1, $p2, $p3)) ORDER BY spatial.distance(someField, spatial.point($p4, $p5)) DESC", iq.Query);
                }
            }
        }

        private class Item
        {
            public string Name { get; set; }

            public double Latitude { get; set; }

            public double Longitude { get; set; }
        }
    }
}