/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalizerService } from './../services/localizer.service';

export class ErrorDto {
    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: ReadonlyArray<string> = [],
        public readonly inner?: any
    ) {
    }

    public translate(localizer: LocalizerService) {
        let result = appendLast(localizer.getOrKey(this.message), '.');

        if (this.details && this.details.length > 0) {
            result += '\n\n';

            for (const detail of this.details) {
                const translated = localizer.getOrKey(detail);

                result += ` * ${appendLast(translated, '.')}\n`;
            }
        }

        return result;
    }

    public toString() {
        return `ErrorDto(${JSON.stringify(this)})`;
    }
}

function appendLast(row: string, char: string) {
    const last = row[row.length - 1];

    if (last !== char) {
        return row + char;
    } else {
        return row;
    }
}