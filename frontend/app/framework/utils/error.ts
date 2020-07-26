/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalizerService } from '../services/localizer.service';

export class ErrorDto {
    public readonly displayMessage: string;
    public readonly localizer: LocalizerService;

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: ReadonlyArray<string> = [],
        public readonly inner?: any
    ) {
        this.displayMessage = formatMessage(message, details);
        this.localizer = LocalizerService.getInstance();
    }

    public toString() {
        return `ErrorDto(${JSON.stringify(this)})`;
    }
}

function formatMessage(message: string, details?: ReadonlyArray<string>) {
    message = this.localizer.get(message);
    let result = appendLast(message, '.');

    if (details && details.length > 0) {
        result += '\n\n';

        for (const detail of details) {
            result += ` * ${appendLast(detail, '.')}\n`;
        }
    }

    return result;
}

function appendLast(row: string, char: string) {
    const last = row[row.length - 1];

    if (last !== char) {
        return row + char;
    } else {
        return row;
    }
}