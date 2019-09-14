/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export class ErrorDto {
    public readonly displayMessage: string;

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: string[] = [],
        public readonly inner?: any
    ) {
        this.displayMessage = formatMessage(message, details);
    }

    public toString() {
        return `ErrorDto(${JSON.stringify(this)})`;
    }
}

export function getDisplayMessage(error?: string | ErrorDto) {
    if (!error) {
        return null;
    } else if (Types.is(error, ErrorDto)) {
        return error.displayMessage;
    } else {
        return error;
    }
}

function formatMessage(message: string, details?: string[]) {
    const appendLast = (row: string, char: string) => {
        const last = row[row.length - 1];

        if (last !== char) {
            return row + char;
        } else {
            return row;
        }
    };

    const removeLast = (row: string, char: string) => {
        const last = row[row.length - 1];

        if (last === char) {
            return row.substr(0, row.length - 1);
        } else {
            return row;
        }
    };

    if (details && details.length > 1) {
        let result = appendLast(message, '.') + '<ul>';

        for (let detail of details) {
            result += `<li>${appendLast(detail, '.')}</li>`;
        }

        result = result + '</ul>';

        return result;
    } else if (details && details.length === 1) {
        return `${appendLast(removeLast(message, '.'), ':')} ${appendLast(details[0], '.')}`;
    } else {
        return appendLast(message, '.');
    }
}