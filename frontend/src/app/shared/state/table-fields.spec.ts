/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DateTime, Version } from '@app/framework';
import { createProperties, MetaFields, RootFieldDto, SchemaDto, TableField, TableFields, TableSizes, UIState } from '@app/shared/internal';

describe('TableFields', () => {
    let uiState: IMock<UIState>;

    const schema =
        new SchemaDto({},
            '1',
            DateTime.now(), 'me',
            DateTime.now(), 'me',
            new Version('1'),
            'my-schema',
            'my-category',
            'Default',
            false,
            {},
            [
                new RootFieldDto({}, 1, 'string', createProperties('String'), 'invariant'),
            ]);

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();
    });

    const INVALID_CONFIGS = [
        { case: 'empty', fields: [] },
        { case: 'invalid', fields: ['invalid'] },
    ];

    INVALID_CONFIGS.forEach(test => {
        it(`should provide default fields if config is ${test.case}`, async () => {
            let fields: ReadonlyArray<TableField>;
            let fieldNames: ReadonlyArray<string>;
            let fieldSizes: TableSizes;

            uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
                .returns(() => of(test.fields));

            uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
                .returns(() => of({ 'field': 100 }));

            const tableFields = new TableFields(uiState.object, schema);

            tableFields.listFields.subscribe(result => {
                fields = result;
            });

            tableFields.listFieldNames.subscribe(result => {
                fieldNames = result;
            });

            tableFields.listSizes.subscribe(result => {
                fieldSizes = result;
            });

            expect(fields!).toEqual([
                MetaFields.lastModifiedByAvatar,
                schema.fields[0],
                MetaFields.statusColor,
                MetaFields.lastModified,
            ]);

            expect(fieldNames!).toEqual([
                MetaFields.lastModifiedByAvatar,
                schema.fields[0].name,
                MetaFields.statusColor,
                MetaFields.lastModified,
            ]);

            expect(fieldSizes!).toEqual({ 'field': 100 });
        });
    });

    INVALID_CONFIGS.forEach(test => {
        it(`should remove ui state if config is ${test.case}`, () => {
            uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
                .returns(() => of([]));

            uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
                .returns(() => of({}));

            const tableFields = new TableFields(uiState.object, schema);

            tableFields.updateFields(test.fields, true);

            uiState.verify(x => x.removeUser('schemas.my-schema.view'), Times.once());

            expect().nothing();
        });
    });

    it('should eliminate invalid fields from the config', () => {
        let fields: ReadonlyArray<TableField>;
        let fieldNames: ReadonlyArray<string>;

        const config = ['invalid', MetaFields.version];

        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of(config));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.listFields.subscribe(result => {
            fields = result;
        });

        tableFields.listFieldNames.subscribe(result => {
            fieldNames = result;
        });

        expect(fields!).toEqual([
            MetaFields.version,
        ]);

        expect(fieldNames!).toEqual([
            MetaFields.version,
        ]);
    });

    it('should update config if fields are saved', () => {
        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of([]));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.updateFields(['invalid', MetaFields.version], true);

        uiState.verify(x => x.set('schemas.my-schema.view', [MetaFields.version], true), Times.once());

        expect().nothing();
    });

    it('should remove config if fields are saved', () => {
        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of([]));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.updateFields([], true);

        uiState.verify(x => x.removeUser('schemas.my-schema.view'), Times.once());

        expect().nothing();
    });

    it('should update config if fields are only updated', () => {
        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of([]));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.updateFields(['invalid', MetaFields.version], false);

        uiState.verify(x => x.set('schemas.my-schema.view', [MetaFields.version], true), Times.never());

        expect().nothing();
    });

    it('should update config if sizes are saved', () => {
        let fieldSizes: TableSizes;

        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of([]));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.listSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableFields.updateSize(MetaFields.version, 100, true);

        uiState.verify(x => x.set('schemas.my-schema.sizes', { [MetaFields.version]: 100 }, true), Times.once());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });

    it('should update config if sizes are only updated', () => {
        let fieldSizes: TableSizes;
    
        uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
            .returns(() => of([]));

        uiState.setup(x => x.getUser<TableSizes>('schemas.my-schema.sizes', {}))
            .returns(() => of({}));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.listSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableFields.updateSize(MetaFields.version, 100, false);

        uiState.verify(x => x.set('schemas.my-schema.sizes', It.isAny(), true), Times.never());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });
});
