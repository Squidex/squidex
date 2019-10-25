/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class ErrorDto {
    public readonly displayMessage: string;

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: ReadonlyArray<string> = [],
        public readonly inner?: any
    ) {
        this.displayMessage = formatMessage(message, details);
    }

    public toString() {
        return `ErrorDto(${JSON.stringify(this)})`;
    }
}

function formatMessage(message: string, details?: ReadonlyArray<string>) {
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