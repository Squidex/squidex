/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime, Version } from '@app/framework';
import { createProperties, MetaFields, RootFieldDto, SchemaDto, TableField, TableFields, UIState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

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
        it(`should provide default fields if config is ${test.case}`, () => {
            let fields: ReadonlyArray<TableField>;
            let fieldNames: ReadonlyArray<string>;

            uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
                .returns(() => of(test.fields));

            const tableFields = new TableFields(uiState.object, schema);

            tableFields.listFields.subscribe(result => {
                fields = result;
            });

            tableFields.listFieldNames.subscribe(result => {
                fieldNames = result;
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
        });
    });

    INVALID_CONFIGS.forEach(test => {
        it(`should remove ui state if config is ${test.case}`, () => {
            uiState.setup(x => x.getUser<string[]>('schemas.my-schema.view', []))
                .returns(() => of([]));

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

        const tableFields = new TableFields(uiState.object, schema);

        const config = ['invalid', MetaFields.version];

        tableFields.updateFields(config, true);

        uiState.verify(x => x.set('schemas.my-schema.view', [MetaFields.version], true), Times.once());

        expect().nothing();
    });
});
