/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class UserCreated {
    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly pictureUrl: string
    ) {
    }
}

export class UserUpdated {
    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string
    ) {
    }
}