/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DateTime, Version } from '@app/framework';
import { createProperties, MetaFields, RootFieldDto, SchemaDto, TableField, TableFields, TableSettings, TableSizes, UIState } from '@app/shared/internal';

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
        { case: 'blank', fields: [] },
        { case: 'broken', fields: ['invalid'] },
    ];

    INVALID_CONFIGS.forEach(test => {
        it(`should provide default fields if config is ${test.case}`, () => {
            let fields: ReadonlyArray<TableField>;
            let fieldNames: ReadonlyArray<string>;
            let fieldSizes: TableSizes;

            uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
                .returns(() => of(({ fields: test.fields, sizes: { field: 100 } })));

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
            uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
                .returns(() => of(({})));

            const tableFields = new TableFields(uiState.object, schema);

            tableFields.updateFields(test.fields, true);

            uiState.verify(x => x.removeUser('schemas.my-schema.config'), Times.once());

            expect().nothing();
        });
    });

    it('should eliminate invalid fields from the config', () => {
        let fields: ReadonlyArray<TableField>;
        let fieldNames: ReadonlyArray<string>;

        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({ fields: ['invalid', MetaFields.version] })));

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
        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableFields = new TableFields(uiState.object, schema);

        const config = ['invalid', MetaFields.version];

        tableFields.updateFields(config, true);

        uiState.verify(x => x.set('schemas.my-schema.config', { fields: [MetaFields.version], sizes: {} }, true), Times.once());

        expect().nothing();
    });

    it('should remove config if fields are saved', () => {
        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.updateFields([], true);

        uiState.verify(x => x.removeUser('schemas.my-schema.config'), Times.once());

        expect().nothing();
    });

    it('should update config if fields are only updated', () => {
        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableFields = new TableFields(uiState.object, schema);

        const config = ['invalid', MetaFields.version];

        tableFields.updateFields(config, false);

        uiState.verify(x => x.set('schemas.my-schema.config', It.isAny(), true), Times.never());

        expect().nothing();
    });

    it('should update config if sizes are saved', () => {
        let fieldSizes: TableSizes;

        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.listSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableFields.updateSize(MetaFields.version, 100, true);

        uiState.verify(x => x.set('schemas.my-schema.config', { fields: [], sizes: { [MetaFields.version]: 100 } }, true), Times.once());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });

    it('should update config if sizes are only updated', () => {
        let fieldSizes: TableSizes;

        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableFields = new TableFields(uiState.object, schema);

        tableFields.listSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableFields.updateSize(MetaFields.version, 100, false);

        uiState.verify(x => x.set('schemas.my-schema.config', It.isAny(), true), Times.never());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });

    it('should provide default fields if reset', () => {
        let fields: ReadonlyArray<TableField>;
        let fieldNames: ReadonlyArray<string>;
        let fieldSizes: TableSizes;

        uiState.setup(x => x.getUser<TableSettings>('schemas.my-schema.config', {}))
            .returns(() => of(({ fields: [MetaFields.version], sizes: { field: 100 } })));

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

        tableFields.reset();

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

        expect(fieldSizes!).toEqual({});
    });
});
