/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DateTime, Version } from '@app/framework';
import { createProperties, FieldSizes, MetaFields, RootFieldDto, SchemaDto, TableField, TableSettings, UIState } from '@app/shared/internal';
import { FieldWrappings } from '..';

describe('TableSettings', () => {
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

    const EMPTY = { fields: [], sizes: {}, wrappings: {} };

    INVALID_CONFIGS.forEach(test => {
        it(`should provide default fields if config is ${test.case}`, () => {
            let listFields: ReadonlyArray<TableField>;
            let listFieldNames: ReadonlyArray<string>;
            let fieldSizes: FieldSizes;
            let fieldWrappings: FieldWrappings;

            const config = {
                fields: test.fields,
                sizes: {
                    field1: 100,
                    field2: 200,
                },
                wrappings: {
                    field3: true,
                    field4: false,
                },
            };

            uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
                .returns(() => of(config));

            const tableSettings = new TableSettings(uiState.object, schema);

            tableSettings.listFields.subscribe(result => {
                listFields = result;
            });

            tableSettings.listFieldNames.subscribe(result => {
                listFieldNames = result;
            });

            tableSettings.fieldSizes.subscribe(result => {
                fieldSizes = result;
            });

            tableSettings.fieldWrappings.subscribe(result => {
                fieldWrappings = result;
            });

            expect(listFields!).toEqual([
                MetaFields.lastModifiedByAvatar,
                schema.fields[0],
                MetaFields.statusColor,
                MetaFields.lastModified,
            ]);

            expect(listFieldNames!).toEqual([
                MetaFields.lastModifiedByAvatar,
                schema.fields[0].name,
                MetaFields.statusColor,
                MetaFields.lastModified,
            ]);

            expect(fieldSizes!).toEqual({ 
                field1: 100,
                field2: 200,
            });
        
            expect(fieldWrappings!).toEqual({ 
                field3: true,
                field4: false,
            });
        });
    });

    INVALID_CONFIGS.forEach(test => {
        it(`should remove ui state if config is ${test.case}`, () => {
            uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
                .returns(() => of(({})));

            const tableSettings = new TableSettings(uiState.object, schema);

            tableSettings.updateFields(test.fields, true);

            uiState.verify(x => x.removeUser('schemas.my-schema.config'), Times.once());

            expect().nothing();
        });
    });

    it('should eliminate invalid fields from the config', () => {
        let listFields: ReadonlyArray<TableField>;
        let listFieldNames: ReadonlyArray<string>;

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({ fields: ['invalid', MetaFields.version] })));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.listFields.subscribe(result => {
            listFields = result;
        });

        tableSettings.listFieldNames.subscribe(result => {
            listFieldNames = result;
        });

        expect(listFields!).toEqual([
            MetaFields.version,
        ]);

        expect(listFieldNames!).toEqual([
            MetaFields.version,
        ]);
    });

    it('should update config if fields are saved', () => {
        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        const config = ['invalid', MetaFields.version];

        tableSettings.updateFields(config, true);

        uiState.verify(x => x.set('schemas.my-schema.config', { ...EMPTY, fields: [MetaFields.version] }, true), Times.once());

        expect().nothing();
    });

    it('should remove config if fields are saved', () => {
        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.updateFields([], true);

        uiState.verify(x => x.removeUser('schemas.my-schema.config'), Times.once());

        expect().nothing();
    });

    it('should update config if fields are only updated', () => {
        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        const config = ['invalid', MetaFields.version];

        tableSettings.updateFields(config, false);

        uiState.verify(x => x.set('schemas.my-schema.config', It.isAny(), true), Times.never());

        expect().nothing();
    });

    it('should update config if sizes are saved', () => {
        let fieldSizes: FieldSizes;

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.updateSize(MetaFields.version, 100, true);

        uiState.verify(x => x.set('schemas.my-schema.config', { ...EMPTY, sizes: { [MetaFields.version]: 100 } }, true), Times.once());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });

    it('should update config if sizes are only updated', () => {
        let fieldSizes: FieldSizes;

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.updateSize(MetaFields.version, 100, false);

        uiState.verify(x => x.set('schemas.my-schema.config', It.isAny(), true), Times.never());

        expect(fieldSizes!).toEqual({ [MetaFields.version]: 100 });
    });

    it('should update config if wrapping is toggled', () => {
        let fieldWrappings: FieldWrappings;

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.toggleWrapping(MetaFields.version, true);

        uiState.verify(x => x.set('schemas.my-schema.config', { ...EMPTY, wrappings: { [MetaFields.version]: true } }, true), Times.once());

        expect(fieldWrappings!).toEqual({ [MetaFields.version]: true });
    });

    it('should update config if wrapping is toggled and only updated', () => {
        let fieldWrappings: FieldWrappings;

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.toggleWrapping(MetaFields.version, false);

        uiState.verify(x => x.set('schemas.my-schema.config', It.isAny(), true), Times.never());

        expect(fieldWrappings!).toEqual({ [MetaFields.version]: true });
    });

    it('should provide default fields if reset', () => {
        let listFields: ReadonlyArray<TableField>;
        let listFieldNames: ReadonlyArray<string>;
        let fieldSizes: FieldSizes;
        let fieldWrappings: FieldWrappings;

        const config = {
            fields: [
                MetaFields.version,
            ],
            sizes: {
                field1: 100,
                field2: 200,
            },
            wrappings: {
                field3: true,
                field4: false,
            },
        };

        uiState.setup(x => x.getUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(config));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.listFields.subscribe(result => {
            listFields = result;
        });

        tableSettings.listFieldNames.subscribe(result => {
            listFieldNames = result;
        });

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.reset();

        expect(listFields!).toEqual([
            MetaFields.lastModifiedByAvatar,
            schema.fields[0],
            MetaFields.statusColor,
            MetaFields.lastModified,
        ]);

        expect(listFieldNames!).toEqual([
            MetaFields.lastModifiedByAvatar,
            schema.fields[0].name,
            MetaFields.statusColor,
            MetaFields.lastModified,
        ]);

        expect(fieldSizes!).toEqual({});
        expect(fieldWrappings!).toEqual({});
    });
});
