/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { Mock } from 'typemoq';
import { AppsState, AuthService, DateTime, FieldDto, FieldPropertiesDto, FieldRuleDto, NestedFieldDto, SchemaDto, SchemaPropertiesDto, SchemaScriptsDto, TeamsState, VersionTag } from '../';

const app = 'my-app';
const creation = DateTime.today().addDays(-2);
const creator = 'me';
const modified = DateTime.now().addDays(-1);
const modifier = 'now-me';
const team = 'my-team';
const version = new VersionTag('1');
const newVersion = new VersionTag('2');

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
    name?: string;
    fields?: FieldDto[];
    fieldsInLists?: string[];
    fieldsInReferences?: string[];
    fieldRules?: FieldRuleDto[];
    properties?: SchemaPropertiesDto;
};

function createSchema({ name, properties, id, fields, fieldsInLists, fieldsInReferences, fieldRules }: SchemaValues = {}) {
    id = id || 1;


    return new SchemaDto({
        id: `${id}`,
        category: `schema-category${id}`,
        created: DateTime.now(),
        createdBy: 'me',
        fieldRules: fieldRules || [],
        fieldsInLists: fieldsInLists || [],
        fieldsInReferences: fieldsInReferences || [],
        isPublished: true,
        isSingleton: false,
        lastModified: DateTime.now(),
        lastModifiedBy: 'me',
        name: name || `schema-name${id}`,
        previewUrls: {},
        properties: properties || new SchemaPropertiesDto(),
        scripts: new SchemaScriptsDto(),
        type: 'Default',
        version: 1,
        fields: fields || [],
        _links: {},
    });
}

type FieldValues = {
    id?: number;
    properties: FieldPropertiesDto;
    isDisabled?: boolean;
    isHidden?: boolean;
    partitioning?: string;
    nested?: NestedFieldDto[];
};

function createField({ properties, id, partitioning, isDisabled, nested }: FieldValues) {
    id = id || 1;

    return new FieldDto({
        fieldId: id,
        isDisabled: isDisabled || false,
        isHidden: false,
        isLocked: false,
        name: `field${id}`,
        partitioning: partitioning || 'language',
        properties,
        nested,
        _links: {},
    });
}

function createNestedField({ properties, id, isDisabled }: FieldValues) {
    id = id || 1;

    return new NestedFieldDto({
        fieldId: id,
        isDisabled: isDisabled || false,
        isHidden: false,
        isLocked: false,
        name: `field${id}`,
        properties,
        _links: {},
    });
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
