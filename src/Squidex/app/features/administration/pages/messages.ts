/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { UserDto } from 'shared';

export class UserCreated {
    constructor(
        public readonly user: UserDto
    ) {
    }
}

export class UserUpdated {
    constructor(
        public readonly user: UserDto
    ) {
    }
}