/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { FieldDto } from '@app/shared';

export interface FieldSection<T> {
    separator?: T;

    fields: ReadonlyArray<T>;
}

@Pipe({
    name: 'sqxGroupFields',
    pure: true
})
export class GroupFieldsPipe<T extends FieldDto> implements PipeTransform {
    public transform(fields: ReadonlyArray<T>) {
        const sections: FieldSection<T>[] = [];

        let currentSeparator: T | undefined = undefined;
        let currentFields: T[] = [];

        for (const field of fields) {
            currentFields.push(field);

            if (!field.properties.isContentField) {
                sections.push({ separator: currentSeparator, fields: currentFields });

                currentFields = [];
                currentSeparator = field;
            }
        }

        if (currentFields.length > 0) {
            sections.push({ separator: currentSeparator, fields: currentFields });
        }

        return sections;
    }
}