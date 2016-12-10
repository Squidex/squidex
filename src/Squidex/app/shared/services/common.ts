/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2Http from '@angular/http';

import { Observable } from 'rxjs';

export class EntityCreatedDto {
    constructor(
        public readonly id: string
    ) {
    }
}

export class ErrorDto {
    public get displayMessage(): string {
        let result = this.message;

        if (this.details && this.details.length > 0) {
            const detailMessage = this.details[0];

            const lastChar = result[result.length - 1];

            if (lastChar !== '.' && lastChar !== ',') {
                result += '.';
            }

            result += ' ';
            result += detailMessage;
        }

        const lastChar = result[result.length - 1];

        if (lastChar !== '.') {
            result += '.';
        }

        return result;
    }

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: string[] = []
    ) {
    }
}

export function handleError(message: string, error: Ng2Http.Response | any) {
    let result = new ErrorDto(500, message);

    if (error instanceof Ng2Http.Response && error.status !== 500) {
        const body = error.json();

        result = new ErrorDto(error.status, body.message, body.details);
    }

    return Observable.throw(result);
}