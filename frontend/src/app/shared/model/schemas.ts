/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ArrayFieldPropertiesDto, AssetsFieldPropertiesDto, BooleanFieldPropertiesDto, ComponentFieldPropertiesDto, ComponentsFieldPropertiesDto, DateTimeFieldPropertiesDto, FieldDto, FieldPropertiesDto, FieldRuleAction, GeolocationFieldPropertiesDto, JsonFieldPropertiesDto, NumberFieldPropertiesDto, ReferencesFieldPropertiesDto, RichTextFieldPropertiesDto, StringFieldPropertiesDto, TagsFieldPropertiesDto, UIFieldPropertiesDto, UserInfoFieldPropertiesDto } from './generated';

export type FieldType =
    'Array' |
    'Assets' |
    'Boolean' |
    'Component' |
    'Components' |
    'DateTime' |
    'Json' |
    'Geolocation' |
    'Number' |
    'References' |
    'RichText' |
    'String' |
    'Tags' |
    'UI' |
    'UserInfo';

export const fieldTypes: ReadonlyArray<{ type: FieldType; description: string }> = [
    {
        type: 'String',
        description: 'i18n:schemas.fieldTypes.string.description',
    }, {
        type: 'Assets',
        description: 'i18n:schemas.fieldTypes.assets.description',
    }, {
        type: 'Boolean',
        description: 'i18n:schemas.fieldTypes.boolean.description',
    }, {
        type: 'Component',
        description: 'i18n:schemas.fieldTypes.component.description',
    }, {
        type: 'Components',
        description: 'i18n:schemas.fieldTypes.components.description',
    }, {
        type: 'DateTime',
        description: 'i18n:schemas.fieldTypes.dateTime.description',
    }, {
        type: 'Geolocation',
        description: 'i18n:schemas.fieldTypes.geolocation.description',
    }, {
        type: 'Json',
        description: 'i18n:schemas.fieldTypes.json.description',
    }, {
        type: 'Number',
        description: 'i18n:schemas.fieldTypes.number.description',
    }, {
        type: 'References',
        description: 'i18n:schemas.fieldTypes.references.description',
    }, {
        type: 'RichText',
        description: 'i18n:schemas.fieldTypes.richText.description',
    }, {
        type: 'Tags',
        description: 'i18n:schemas.fieldTypes.tags.description',
    }, {
        type: 'Array',
        description: 'i18n:schemas.fieldTypes.array.description',
    }, {
        type: 'UI',
        description: 'i18n:schemas.fieldTypes.ui.description',
    }, {
        type: 'UserInfo',
        description: 'i18n:schemas.fieldTypes.user.description',
    },
];

export const fieldInvariant = 'iv';

export function createProperties(fieldType: FieldType, values?: any): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'Array':
            properties = new ArrayFieldPropertiesDto(values);
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto(values);
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto(values);
            break;
        case 'Component':
            properties = new ComponentFieldPropertiesDto(values);
            break;
        case 'Components':
            properties = new ComponentsFieldPropertiesDto(values);
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto(values);
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto(values);
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto(values);
            break;
        case 'Number':
            properties = new NumberFieldPropertiesDto(values);
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto(values);
            break;
        case 'RichText':
            properties = new RichTextFieldPropertiesDto(values);
            break;
        case 'String':
            properties = new StringFieldPropertiesDto(values);
            break;
        case 'Tags':
            properties = new TagsFieldPropertiesDto(values);
            break;
        case 'UI':
            properties = new UIFieldPropertiesDto(values);
            break;
        case 'UserInfo':
            properties = new UserInfoFieldPropertiesDto(values);
            break;
        default:
            throw new Error(`Unknown field type ${fieldType}.`);
    }

    return properties;
}

export interface FieldPropertiesVisitor<T> {
    visitArray(properties: ArrayFieldPropertiesDto): T;

    visitAssets(properties: AssetsFieldPropertiesDto): T;

    visitBoolean(properties: BooleanFieldPropertiesDto): T;

    visitComponent(properties: ComponentFieldPropertiesDto): T;

    visitComponents(properties: ComponentsFieldPropertiesDto): T;

    visitDateTime(properties: DateTimeFieldPropertiesDto): T;

    visitGeolocation(properties: GeolocationFieldPropertiesDto): T;

    visitJson(properties: JsonFieldPropertiesDto): T;

    visitNumber(properties: NumberFieldPropertiesDto): T;

    visitReferences(properties: ReferencesFieldPropertiesDto): T;

    visitRichText(properties: RichTextFieldPropertiesDto): T;

    visitString(properties: StringFieldPropertiesDto): T;

    visitTags(properties: TagsFieldPropertiesDto): T;

    visitUI(properties: UIFieldPropertiesDto): T;

    visitUserInfo(properties: UserInfoFieldPropertiesDto): T;
}

export const META_FIELDS = {
    empty: {
        name: '',
        label: '',
        title: '',
    },
    id: {
        name: 'id',
        label: 'i18n:schemas.tableHeaders.id',
        title: 'i18n:schemas.tableHeaders.id_title',
    },
    created: {
        name: 'created',
        label: 'i18n:schemas.tableHeaders.created',
        title: 'i18n:schemas.tableHeaders.created_title',
    },
    createdByAvatar: {
        name: 'createdBy.avatar',
        label: 'i18n:schemas.tableHeaders.createdByShort',
        title: 'i18n:schemas.tableHeaders.createdByShort_title',
    },
    createdByName: {
        name: 'createdBy.name',
        label: 'i18n:schemas.tableHeaders.createdBy',
        title: 'i18n:schemas.tableHeaders.createdBy_title',
    },
    lastModified: {
        name: 'lastModified',
        label: 'i18n:schemas.tableHeaders.lastModified',
        title: 'i18n:schemas.tableHeaders.lastModified_title',
    },
    lastModifiedByAvatar: {
        name: 'lastModifiedBy.avatar',
        label: 'i18n:schemas.tableHeaders.lastModifiedByShort',
        title: 'i18n:schemas.tableHeaders.lastModifiedByShort_title',
    },
    lastModifiedByName: {
        name: 'lastModifiedBy.name',
        label: 'i18n:schemas.tableHeaders.lastModifiedBy',
        title: 'i18n:schemas.tableHeaders.lastModifiedBy_title',
    },
    status: {
        name: 'status',
        label: 'i18n:schemas.tableHeaders.status',
        title: 'i18n:schemas.tableHeaders.status_title',
    },
    statusColor: {
        name: 'status.color',
        label: 'i18n:schemas.tableHeaders.status',
        title: 'i18n:schemas.tableHeaders.status_title',
    },
    statusNext: {
        name: 'status.next',
        label: 'i18n:schemas.tableHeaders.nextStatus',
        title: 'i18n:schemas.tableHeaders.nextStatus_title',
    },
    version: {
        name: 'version',
        label: 'i18n:schemas.tableHeaders.version',
        title: 'i18n:schemas.tableHeaders.version_title',
    },
    translationStatus: {
        name: 'translationStatus',
        label: 'i18n:schemas.tableHeaders.translationStatus',
        title: 'i18n:schemas.tableHeaders.translationStatus_title',
    },
    translationStatusAverage: {
        name: 'translationStatusAverage',
        label: 'i18n:schemas.tableHeaders.translationStatusAverage',
        title: 'i18n:schemas.tableHeaders.translationStatusAverage_title',
    },
};

export const FIELD_RULE_ACTIONS: ReadonlyArray<FieldRuleAction> = [
    'Disable',
    'Hide',
    'Require',
];

export type TableField = Readonly<{
    // The name of the table field.
    name: string;

    // The label for the table header.
    label: string;

    // The title.
    title?: string;

    // The reference to the root field.
    rootField?: FieldDto;
}>;

export function getTableFields(fields: ReadonlyArray<TableField>) {
    const result: string[] = [];

    for (const field of fields) {
        if (field.name?.startsWith('data.')) {
            result.push(field.name);
        }
    }

    result.sort();
    return result;
}

export function tableField(rootField: FieldDto): TableField {
    const label = rootField.displayName;

    return { name: `data.${rootField.name}`, label, rootField };
}

export function tableFields(names: ReadonlyArray<string>, fields: ReadonlyArray<TableField>): TableField[] {
    const result: TableField[] = [];

    for (const name of names) {
        const metaField = Object.values(META_FIELDS).find(x => x.name === name);

        if (metaField) {
            result.push(metaField);
        } else {
            const field = fields.find(x => x.name === name);

            if (field) {
                result.push(field);
            }
        }
    }

    return result;
}