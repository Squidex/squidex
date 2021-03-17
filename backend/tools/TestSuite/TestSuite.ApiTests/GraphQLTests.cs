// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace TestSuite.ApiTests
{
    public sealed class GraphQLTests : IClassFixture<ContentFixture>
    {
        public ContentFixture _ { get; }

        public GraphQLTests(ContentFixture fixture)
        {
            _ = fixture;
        }

        public sealed class DynamicEntity : Content<object>
        {
        }

        public sealed class Country
        {
            public CountryData Data { get; set; }
        }

        public sealed class CountryData
        {
            public string Name { get; set; }

            public List<State> States { get; set; }
        }

        public sealed class State
        {
            public StateData Data { get; set; }
        }

        public sealed class StateData
        {
            public string Name { get; set; }

            public List<City> Cities { get; set; }
        }

        public sealed class City
        {
            public CityData Data { get; set; }
        }

        public sealed class CityData
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_create_and_query_with_graphql()
        {
            try
            {
                await CreateSchemasAsync();
                await CreateContentsAsync();
            }
            catch
            {
                // Do nothing
            }

            var countriesClient = _.ClientManager.CreateContentsClient<DynamicEntity, object>("countries");

            var query = new
            {
                query = @"
                {
                    queryCountriesContents {
                        data: flatData {
                            name,
                            states {
                                data: flatData {
                                    name
                                    cities {
                                        data: flatData {
                                            name
                                        }
                                    }
                                }
                            }
                        }
                    }
                }"
            };

            var result1 = await countriesClient.GraphQlAsync<JToken>(query);

            var typed = result1["queryCountriesContents"].ToObject<List<Country>>();

            Assert.Equal("Leipzig", typed[0].Data.States[0].Data.Cities[0].Data.Name);
        }

        private async Task CreateSchemasAsync()
        {
            // STEP 1: Create cities schema.
            var createCitiesRequest = new CreateSchemaDto
            {
                Name = "cities",
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = "name",
                        Properties = new StringFieldPropertiesDto()
                    }
                },
                IsPublished = true
            };

            var cities = await _.Schemas.PostSchemaAsync(_.AppName, createCitiesRequest);


            // STEP 2: Create states schema.
            var createStatesRequest = new CreateSchemaDto
            {
                Name = "states",
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = "name",
                        Properties = new StringFieldPropertiesDto()
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = "cities",
                        Properties = new ReferencesFieldPropertiesDto
                        {
                            SchemaIds = new List<string> { cities.Id }
                        }
                    }
                },
                IsPublished = true
            };

            var states = await _.Schemas.PostSchemaAsync(_.AppName, createStatesRequest);


            // STEP 3: Create countries schema.
            var createCountriesRequest = new CreateSchemaDto
            {
                Name = "countries",
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = "name",
                        Properties = new StringFieldPropertiesDto()
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = "states",
                        Properties = new ReferencesFieldPropertiesDto
                        {
                            SchemaIds = new List<string> { states.Id }
                        }
                    }
                },
                IsPublished = true
            };

            await _.Schemas.PostSchemaAsync(_.AppName, createCountriesRequest);
        }

        private async Task CreateContentsAsync()
        {
            // STEP 1: Create city
            var cityData = new
            {
                name = new
                {
                    iv = "Leipzig"
                }
            };

            var citiesClient = _.ClientManager.CreateContentsClient<DynamicEntity, object>("cities");

            var city = await citiesClient.CreateAsync(cityData, true);


            // STEP 2: Create city
            var stateData = new
            {
                name = new
                {
                    iv = "Saxony"
                },
                cities = new
                {
                    iv = new[] { city.Id }
                }
            };

            var statesClient = _.ClientManager.CreateContentsClient<DynamicEntity, object>("states");

            var state = await statesClient.CreateAsync(stateData, true);


            // STEP 3: Create country
            var countryData = new
            {
                name = new
                {
                    iv = "Germany"
                },
                states = new
                {
                    iv = new[] { state.Id }
                }
            };

            var countriesClient = _.ClientManager.CreateContentsClient<DynamicEntity, object>("countries");

            await countriesClient.CreateAsync(countryData, true);
        }
    }
}
