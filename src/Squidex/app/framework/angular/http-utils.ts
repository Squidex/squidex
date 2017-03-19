/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response } from '@angular/http';
import { Observable } from 'rxjs';

export class EntityCreatedDto {
    constructor(
        public readonly id: any
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

export function catchError(message: string): Observable<any> {
    return this.catch((error: any | Response) => {
        let result = new ErrorDto(500, message);

        if (error instanceof Response) {
            try {
                const body = error.json();

                if (error.status === 412) {
                    result = new ErrorDto(error.status, 'Failed to make the update. Another user has made a change. Please reload.');
                } else if (error.status !== 500) {
                    result = new ErrorDto(error.status, body.message, body.details);
                }
            } catch (e) {
                result = result;
            }
        }

        return Observable.throw(result);
    });
}