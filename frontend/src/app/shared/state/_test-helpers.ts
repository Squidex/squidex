/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { Mock } from 'typemoq';
import { AppsState, AuthService, DateTime, FieldPropertiesDto, FieldRule, NestedFieldDto, RootFieldDto, SchemaDto, SchemaPropertiesDto, TeamsState, Version } from './../';

const app = 'my-app';
const creation = DateTime.today().addDays(-2);
const creator = 'me';
const modified = DateTime.now().addDays(-1);
const modifier = 'now-me';
const team = 'my-team';
const version = new Version('1');
const newVersion = new Version('2');

const appsState = Mock.ofType<AppsState>();

appsState.setup(x => x.appName)
    .returns(() => app);

appsState.setup(x => x.selectedApp)
    .returns(() => of(<any>{ name: app }));

const teamsState = Mock.ofType<TeamsState>();

teamsState.setup(x => x.teamId)
    .returns(() => team);

teamsState.setup(x => x.selectedTeam)
    .returns(() => of(<any>{ id: team }));

const authService = Mock.ofType<AuthService>();

authService.setup(x => x.user)
    .returns(() => <any>{ id: modifier, token: modifier });

type SchemaValues = {
    id?: number;
    fields?: ReadonlyArray<RootFieldDto>;
    fieldsInLists?: ReadonlyArray<string>;
    fieldsInReferences?: ReadonlyArray<string>;
    fieldRules?: ReadonlyArray<FieldRule>;
    properties?: SchemaPropertiesDto;
};

function createSchema({ properties, id, fields, fieldsInLists, fieldsInReferences, fieldRules }: SchemaValues = {}) {
    id = id || 1;

    return new SchemaDto({},
        `schema${id}`,
        creation,
        creator,
        modified,
        modifier,
        new Version('1'),
        `schema-name${id}`,
        `schema-category${id}`,
        'Default',
        true,
        properties || new SchemaPropertiesDto(),
        fields,
        fieldsInLists || [],
        fieldsInReferences || [],
        fieldRules || []);
}

type FieldValues = {
    id?: number;
    properties: FieldPropertiesDto;
    isDisabled?: boolean;
    isHidden?: boolean;
    partitioning?: string;
    nested?: ReadonlyArray<NestedFieldDto>;
};

function createField({ properties, id, partitioning, isDisabled, nested }: FieldValues) {
    id = id || 1;

    return new RootFieldDto({}, id, `field${id}`, properties, partitioning || 'language', false, false, isDisabled, nested);
}

function createNestedField({ properties, id, isDisabled }: FieldValues) {
    id = id || 1;

    return new NestedFieldDto({}, id, `nested${id}`, properties, 0, false, false, isDisabled);
}

export const TestValues = {
    app,
    appsState,
    authService,
    createField,
    createNestedField,
    createSchema,
    creation,
    creator,
    modified,
    modifier,
    newVersion,
    team,
    teamsState,
    version,
};
