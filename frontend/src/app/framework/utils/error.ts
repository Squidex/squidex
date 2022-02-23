/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalizerService } from './../services/localizer.service';
import { StringHelper } from './string-helper';
import { Types } from './types';

export class ErrorDetailsDto {
    public readonly message: string;
    public readonly properties: ReadonlyArray<string> = [];

    constructor(
        public readonly originalMessage: string,
    ) {
        const propertySeparator = originalMessage.indexOf(': ');

        if (propertySeparator > 0 && propertySeparator < originalMessage.length - 1) {
            this.properties =
                originalMessage
                    .substring(0, propertySeparator)
                    .split(', ')
                    .map(x => x.trim()).filter(x => x.length > 0);

            this.message = originalMessage.substring(propertySeparator + 2);
        } else {
            this.message = originalMessage;
        }
    }
}

export class ErrorDto {
    public readonly details: ReadonlyArray<ErrorDetailsDto> = [];

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly errorCode?: string | null,
        details?: ReadonlyArray<string> | ReadonlyArray<ErrorDetailsDto>,
        public readonly inner?: any,
    ) {
        if (Types.isArrayOfString(details)) {
            this.details = details.map(x => new ErrorDetailsDto(x));
        } else if (Types.isArray(details)) {
            this.details = details;
        }
    }

    public translate(localizer: LocalizerService) {
        let result = StringHelper.appendLast(localizer.getOrKey(this.message), '.');

        if (this.details && this.details.length > 0) {
            result += '\n\n';

            for (const detail of this.details) {
                const translated = localizer.getOrKey(detail.originalMessage);

                result += ` * ${StringHelper.appendLast(translated, '.')}\n`;
            }
        }

        return result;
    }

    public toString() {
        return `ErrorDto(${JSON.stringify(this)})`;
    }
}
