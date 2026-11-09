/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Pipe, PipeTransform } from '@angular/core';
import { ApiUrlConfig, UserDto, UsersProviderService } from '@app/shared/internal';

@Pipe({
    name: 'sqxScriptName',
    pure: true,
})
export class ScriptNamePipe implements PipeTransform {
    public transform(value: string) {
        if (value === 'queryPre') {
            return 'Prepare Query';
        } else {
            return value.substring(0, 1).toUpperCase() + value.substring(1);
        }
    }
}

@Pipe({
    name: 'sqxUserName',
    pure: false,
})
export class UserNamePipe implements PipeTransform {
    private readonly users = inject(UsersProviderService);

    public transform(userId: string | undefined | null, placeholder = 'Me') {
        if (!userId) {
            return undefined;
        }

        return getDisplayName(this.users.user(userId, placeholder)());
    }
}

@Pipe({
    name: 'sqxUserNameRef',
    pure: false,
})
export class UserNameRefPipe implements PipeTransform {
    private readonly users = inject(UsersProviderService);

    public transform(userId: string | undefined | null, placeholder: string | null = 'Me') {
        if (!userId) {
            return undefined;
        }

        const { type, id } = split(userId);

        if (type === 'subject') {
            return getDisplayName(this.users.user(id, placeholder)());
        } else if (id.endsWith('client')) {
            return id;
        } else {
            return `${id} client`;
        }
    }
}

@Pipe({
    name: 'sqxUserDtoPicture',
    pure: true,
})
export class UserDtoPicture implements PipeTransform {
    private readonly apiUrl = inject(ApiUrlConfig);

    public transform(user: UserDto): string | null {
        return this.apiUrl.buildUrl(`api/users/${user.id}/picture`);
    }
}

@Pipe({
    name: 'sqxUserIdPicture',
    pure: true,
})
export class UserIdPicturePipe implements PipeTransform {
    private readonly apiUrl = inject(ApiUrlConfig);

    public transform(userId: string): string | null {
        return this.apiUrl.buildUrl(`api/users/${userId}/picture`);
    }
}

@Pipe({
    name: 'sqxUserPicture',
    pure: false,
})
export class UserPicturePipe implements PipeTransform {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly users = inject(UsersProviderService);

    public transform(userId: string | undefined | null) {
        if (!userId) {
            return undefined;
        }

        return getPicture(this.users.user(userId)(), this.apiUrl);
    }
}

@Pipe({
    name: 'sqxUserPictureRef',
    pure: false,
})
export class UserPictureRefPipe implements PipeTransform {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly users = inject(UsersProviderService);

    public transform(userId: string | undefined | null) {
        if (!userId) {
            return undefined;
        }

        const { type, id } = split(userId);

        if (type === 'subject') {
            return getPicture(this.users.user(id)(), this.apiUrl);
        } else {
            return './images/squid.svg';
        }
    }
}

function getDisplayName(user: UserDto | undefined) {
    if (!user) {
        return 'Loading...';
    }

    return user.displayName || undefined;
}

function getPicture(user: UserDto | undefined, apiUrl: ApiUrlConfig) {
    if (!user) {
        return '';
    }

    return apiUrl.buildUrl(`api/users/${user.id}/picture`);
}

function split(token: string) {
    const index = token.indexOf(':');

    if (index > 0 && index < token.length - 1) {
        const type = token.substring(0, index);
        const name = token.substring(index + 1);

        return { type, id: name };
    }

    return { type: token, id: token };
}
