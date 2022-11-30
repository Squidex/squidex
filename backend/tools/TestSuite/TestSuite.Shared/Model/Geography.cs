// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.Model;

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
