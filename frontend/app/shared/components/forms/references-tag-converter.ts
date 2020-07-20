/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ContentDto, getContentValue, LanguageDto, TagConverter, TagValue } from '@app/shared/internal';

export class ReferencesTagsConverter implements TagConverter {
    public suggestions: ReadonlyArray<TagValue> = [];

    constructor(language: LanguageDto, contents: ReadonlyArray<ContentDto>) {
        this.suggestions = this.createTags(language, contents);
    }

    public convertInput(input: string) {
        const result = this.suggestions.find(x => x.name === input);

        return result || null;
    }

    public convertValue(value: any) {
        const result = this.suggestions.find(x => x.id === value);

        return result || null;
    }

    private createTags(language: LanguageDto, contents: ReadonlyArray<ContentDto>): ReadonlyArray<TagValue> {
        if (contents.length === 0) {
            return [];
        }

        const values = contents.map(content => {
            const name =
                content.referenceFields
                    .map(f => getContentValue(content, language, f, false))
                    .map(v => v.formatted || 'No value')
                    .filter(v => !!v)
                    .join(', ');

            return new TagValue(content.id, name, content.id);
        });

        return values;
    }
}