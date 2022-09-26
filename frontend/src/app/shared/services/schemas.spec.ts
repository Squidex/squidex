/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { createProperties, META_FIELDS, SchemaPropertiesDto } from '@app/shared/internal';
import { TestValues } from './../state/_test-helpers';

const {
    createField,
    createSchema,
} = TestValues;

describe('SchemaDto', () => {
    const field1 = createField({ properties: createProperties('Array'), id: 1 });
    const field2 = createField({ properties: createProperties('Array'), id: 2 });
    const field3 = createField({ properties: createProperties('Array', { label: 'My Field 3' }), id: 3 });

    it('should return label as display name', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto('Label') });

        expect(schema.displayName).toBe('Label');
    });

    it('should return name as display name if label is undefined', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(undefined) });

        expect(schema.displayName).toBe('schema-name1');
    });

    it('should return name as display name label is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto('') });

        expect(schema.displayName).toBe('schema-name1');
    });

    it('should return configured fields as list fields if fields are declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3], fieldsInLists: ['field1', 'field3', META_FIELDS.status.name] });

        expect(schema.defaultListFields).toEqual([
            { name: field1.name, rootField: field1, label: field1.displayName },
            { name: field3.name, rootField: field3, label: field3.displayName },
            META_FIELDS.status,
        ]);
    });

    it('should return configured fields as references fields if fields are declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3], fieldsInReferences: ['field1', 'field3', META_FIELDS.status.name] });

        expect(schema.defaultReferenceFields).toEqual([
            { name: field1.name, rootField: field1, label: field1.displayName },
            { name: field3.name, rootField: field3, label: field3.displayName },
            META_FIELDS.status,
        ]);
    });

    it('should return first fields as list fields if no field is declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3] });

        expect(schema.defaultListFields).toEqual([
            META_FIELDS.lastModifiedByAvatar,
            { name: field1.name, rootField: field1, label: field1.displayName },
            META_FIELDS.statusColor,
            META_FIELDS.lastModified,
        ]);
    });

    it('should return preset with empty content field as list fields if fields is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto() });

        expect(schema.defaultListFields).toEqual([
            META_FIELDS.lastModifiedByAvatar,
            META_FIELDS.empty,
            META_FIELDS.statusColor,
            META_FIELDS.lastModified,
        ]);
    });

    it('should return first field as reference fields if no field is declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3] });

        expect(schema.defaultReferenceFields).toEqual([
            { name: field1.name, rootField: field1, label: field1.displayName },
        ]);
    });

    it('should return noop field as reference field if list is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto() });

        expect(schema.defaultReferenceFields).toEqual([
            META_FIELDS.empty,
        ]);
    });
});

describe('FieldDto', () => {
    it('should return label as display name', () => {
        const field = createField({ properties: createProperties('Array', { label: 'Label' }) });

        expect(field.displayName).toBe('Label');
    });

    it('should return name as display name if label is null', () => {
        const field = createField({ properties: createProperties('Assets') });

        expect(field.displayName).toBe('field1');
    });

    it('should return name as display name label is empty', () => {
        const field = createField({ properties: createProperties('Assets', { label: '' }) });

        expect(field.displayName).toBe('field1');
    });

    it('should return placeholder as display placeholder', () => {
        const field = createField({ properties: createProperties('Assets', { placeholder: 'Placeholder' }) });

        expect(field.displayPlaceholder).toBe('Placeholder');
    });

    it('should return empty as display placeholder if placeholder is null', () => {
        const field = createField({ properties: createProperties('Assets') });

        expect(field.displayPlaceholder).toBe('');
    });

    it('should return localizable if partitioning is language', () => {
        const field = createField({ properties: createProperties('Assets'), partitioning: 'language' });

        expect(field.isLocalizable).toBeTruthy();
    });

    it('should not return localizable if partitioning is invariant', () => {
        const field = createField({ properties: createProperties('Assets'), partitioning: 'invariant' });

        expect(field.isLocalizable).toBeFalsy();
    });
});
