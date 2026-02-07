/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { createProperties, FieldSizes, META_FIELDS, TableSettings, UIState } from '@app/shared/internal';
import { FieldWrappings } from '..';
import { TestValues } from './_test-helpers';

describe('TableSettings', () => {
    let uiState: IMock<UIState>;

    const { createSchema, createField } = TestValues;

    const schema = createSchema({
        name: 'my-schema',
        fields: [
            createField({ id: 1, properties: createProperties('String') }),
        ],
    });

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();
    });

    const INVALID_FIELD = { name: 'invalid', label: 'invalid' };
    const INVALID_CONFIGS = [
        {
            case: 'blank',
            fields: [],
        },
        {
            case: 'broken',
            fields: [{ name: 'invalid', label: 'invalid' }],
        },
    ];

    const EMPTY = { fields: [], sizes: {}, wrappings: {} };

    INVALID_CONFIGS.forEach(test => {
        it(`should provide default fields if config is ${test.case}`, () => {
            let listFields: ReadonlyArray<string>;
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

            uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
                .returns(() => of(config));

            const tableSettings = new TableSettings(uiState.object, schema);

            tableSettings.listFields.subscribe(result => {
                listFields = result.map(x => x.name);
            });

            tableSettings.fieldSizes.subscribe(result => {
                fieldSizes = result;
            });

            tableSettings.fieldWrappings.subscribe(result => {
                fieldWrappings = result;
            });

            expect(listFields!).toEqual([
                META_FIELDS.lastModifiedByAvatar.name,
                `data.${schema.fields[0].name}`,
                META_FIELDS.statusColor.name,
                META_FIELDS.lastModified.name,
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
            uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
                .returns(() => of(({})));

            const tableSettings = new TableSettings(uiState.object, schema);

            tableSettings.updateFields(test.fields, true);

            uiState.verify(x => x.removeAppUser('schemas.my-schema.config'), Times.once());
        });
    });

    it('should eliminate invalid fields from the config', () => {
        let listFields: ReadonlyArray<string>;

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({ fields: ['invalid', META_FIELDS.version.name] })));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.listFields.subscribe(result => {
            listFields = result.map(x => x.name);
        });

        expect(listFields!).toEqual([
            META_FIELDS.version.name,
        ]);
    });

    it('should update config if fields are saved', () => {
        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        const config = [INVALID_FIELD, META_FIELDS.version];

        tableSettings.updateFields(config, true);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', { ...EMPTY, fields: [META_FIELDS.version.name] }), Times.once());
    });

    it('should remove config if fields are saved', () => {
        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.updateFields([], true);

        uiState.verify(x => x.removeAppUser('schemas.my-schema.config'), Times.once());
    });

    it('should update config if fields are only updated', () => {
        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        const config = [INVALID_FIELD, META_FIELDS.version];

        tableSettings.updateFields(config, false);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', It.isAny()), Times.never());
    });

    it('should update config if sizes are saved', () => {
        let fieldSizes: FieldSizes;

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.updateSize(META_FIELDS.version.name, 100, true);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', { ...EMPTY, sizes: { [META_FIELDS.version.name]: 100 } }), Times.once());

        expect(fieldSizes!).toEqual({ [META_FIELDS.version.name]: 100 });
    });

    it('should update config if sizes are only updated', () => {
        let fieldSizes: FieldSizes;

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.updateSize(META_FIELDS.version.name, 100, false);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', It.isAny()), Times.never());

        expect(fieldSizes!).toEqual({ [META_FIELDS.version.name]: 100 });
    });

    it('should update config if wrapping is toggled', () => {
        let fieldWrappings: FieldWrappings;

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.toggleWrapping(META_FIELDS.version.name, true);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', { ...EMPTY, wrappings: { [META_FIELDS.version.name]: true } }), Times.once());

        expect(fieldWrappings!).toEqual({ [META_FIELDS.version.name]: true });
    });

    it('should update config if wrapping is toggled and only updated', () => {
        let fieldWrappings: FieldWrappings;

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(({})));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.toggleWrapping(META_FIELDS.version.name, false);

        uiState.verify(x => x.setAppUser('schemas.my-schema.config', It.isAny()), Times.never());

        expect(fieldWrappings!).toEqual({ [META_FIELDS.version.name]: true });
    });

    it('should provide default fields if reset', () => {
        let listFields: ReadonlyArray<string>;
        let fieldSizes: FieldSizes;
        let fieldWrappings: FieldWrappings;

        const config = {
            fields: [
                META_FIELDS.version.name,
            ],
            sizes: {
                'data.field1': 100,
                'data.field2': 200,
            },
            wrappings: {
                'data.field3': true,
                'data.field4': false,
            },
        };

        uiState.setup(x => x.getAppUser<any>('schemas.my-schema.config', {}))
            .returns(() => of(config));

        const tableSettings = new TableSettings(uiState.object, schema);

        tableSettings.listFields.subscribe(result => {
            listFields = result.map(x => x.name);
        });

        tableSettings.fieldSizes.subscribe(result => {
            fieldSizes = result;
        });

        tableSettings.fieldWrappings.subscribe(result => {
            fieldWrappings = result;
        });

        tableSettings.reset();

        expect(listFields!).toEqual([
            META_FIELDS.lastModifiedByAvatar.name,
            `data.${schema.fields[0].name}`,
            META_FIELDS.statusColor.name,
            META_FIELDS.lastModified.name,
        ]);

        expect(fieldSizes!).toEqual({});
        expect(fieldWrappings!).toEqual({});
    });
});
