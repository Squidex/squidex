/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

const ALLOWED_TEXTS = ['Option 1', 'Option 2'];
const ALLOWED_NUMBERS = [1, 2];

function field(name: string, fieldType: string, editor?: string, properties?: object) {
    return {
        name,
        partitioning: 'invariant',
        properties: { fieldType, editor, label: editor ? `${fieldType} (${editor})` : fieldType, ...properties },
    };
}

// A schema with a field for every editor, to verify the styling of all of them.
export const EDITORS_SCHEMA = {
    isPublished: true,
    type: 'Default',
    fields: [
        field('string-input', 'String', 'Input'),
        field('string-color', 'String', 'Color'),
        field('string-dropdown', 'String', 'Dropdown', { allowedValues: ALLOWED_TEXTS }),
        field('string-html', 'String', 'Html'),
        field('string-markdown', 'String', 'Markdown'),
        field('string-radio', 'String', 'Radio', { allowedValues: ALLOWED_TEXTS }),
        field('string-rich-text', 'String', 'RichText'),
        field('string-slug', 'String', 'Slug'),
        field('string-stock-photo', 'String', 'StockPhoto'),
        field('string-text-area', 'String', 'TextArea'),
        field('number-input', 'Number', 'Input'),
        field('number-dropdown', 'Number', 'Dropdown', { allowedValues: ALLOWED_NUMBERS }),
        field('number-radio', 'Number', 'Radio', { allowedValues: ALLOWED_NUMBERS }),
        field('number-stars', 'Number', 'Stars'),
        field('boolean-checkbox', 'Boolean', 'Checkbox'),
        field('boolean-toggle', 'Boolean', 'Toggle'),
        field('date-time-date', 'DateTime', 'Date'),
        field('date-time-date-time', 'DateTime', 'DateTime'),
        field('references-list', 'References', 'List'),
        field('references-dropdown', 'References', 'Dropdown'),
        field('references-tags', 'References', 'Tags'),
        field('references-checkboxes', 'References', 'Checkboxes'),
        field('tags-tags', 'Tags', 'Tags'),
        field('tags-checkboxes', 'Tags', 'Checkboxes', { allowedValues: ALLOWED_TEXTS }),
        field('tags-dropdown', 'Tags', 'Dropdown', { allowedValues: ALLOWED_TEXTS }),
        field('assets', 'Assets'),
        field('geolocation', 'Geolocation'),
        field('json', 'Json'),
        field('rich-text', 'RichText'),
        field('user-info', 'UserInfo'),
        field('ui', 'UI'),
        {
            ...field('array', 'Array'),
            nested: [
                field('nested-string', 'String', 'Input'),
                field('nested-number', 'Number', 'Input'),
            ],
        },
    ],
};
