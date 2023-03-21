// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class GraphQLFixture : ContentFixture
{
    public sealed class DynamicEntity : Content<object>
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await CreateSchemasAsync();
        await CreateContentsAsync();
    }

    private async Task CreateSchemasAsync()
    {
        async Task<string> CreateSchemaAsync(CreateSchemaDto request)
        {
            try
            {
                var response = await Client.Schemas.PostSchemaAsync(request);

                return response.Id;
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }

                var schema = await Client.Schemas.GetSchemaAsync(request.Name);

                return schema.Id;
            }
        }

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

        var citiesId = await CreateSchemaAsync(createCitiesRequest);


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
                        SchemaIds = new List<string> { citiesId }
                    }
                }
            },
            IsPublished = true
        };

        var statesId = await CreateSchemaAsync(createStatesRequest);


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
                        SchemaIds = new List<string> { statesId }
                    }
                }
            },
            IsPublished = true
        };

        await CreateSchemaAsync(createCountriesRequest);
    }

    private async Task CreateContentsAsync()
    {
        var countriesClient = Client.Contents<DynamicEntity, object>("countries");
        var countriesResponse = await countriesClient.GetAsync();

        if (countriesResponse.Total > 0)
        {
            return;
        }

        async Task<string> CreateCityAsync(string name)
        {
            var citySAData = new
            {
                name = new
                {
                    iv = name
                }
            };

            var citiesClient = Client.Contents<DynamicEntity, object>("cities");
            var cityResponse = await citiesClient.CreateAsync(citySAData, ContentCreateOptions.AsPublish);

            return cityResponse.Id;
        }

        async Task<string> CreateStateAsync(string name, string cityId)
        {
            var citySAData = new
            {
                name = new
                {
                    iv = name
                },
                cities = new
                {
                    iv = new[] { cityId }
                }
            };

            var statesClient = Client.Contents<DynamicEntity, object>("states");
            var stateResponse = await statesClient.CreateAsync(citySAData, ContentCreateOptions.AsPublish);

            return stateResponse.Id;
        }

        // STEP 1: Create state 1
        var sachsenCapital = await CreateCityAsync("Leipzig");
        var sachstenState = await CreateStateAsync("Sachsen", sachsenCapital);


        // STEP 1: Create state 2
        var badenWCapital = await CreateCityAsync("Stuttgart");
        var badenWState = await CreateStateAsync("Baden Württemberg", badenWCapital);


        // STEP 3: Create country
        var countryData = new
        {
            name = new
            {
                iv = "Germany"
            },
            states = new
            {
                iv = new[] { sachstenState, badenWState }
            }
        };

        await countriesClient.CreateAsync(countryData, ContentCreateOptions.AsPublish);
    }
}
