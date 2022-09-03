/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { Observable, of, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, UserDto, UsersProviderService } from '@app/shared/internal';

class UserAsyncPipe {
    private lastUserId?: string;
    private lastValue: string | undefined = undefined;
    private subscription?: Subscription;
    private current?: Observable<string | null>;

    constructor(loading: string,
        private readonly users: UsersProviderService,
        private readonly changeDetector: ChangeDetectorRef,
    ) {
        this.lastValue = loading;
    }

    public destroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    protected transformInternal(userId: string | undefined | null, transform: (users: UsersProviderService, userId: string) => Observable<string | null>) {
        if (!userId) {
            return undefined;
        }

        if (this.lastUserId !== userId) {
            this.lastUserId = userId;

            if (this.subscription) {
                this.subscription.unsubscribe();
            }

            const pipe = transform(this.users, userId);

            this.subscription = pipe.subscribe(value => {
                this.lastValue = value || undefined;

                if (this.current === pipe) {
                    this.changeDetector.markForCheck();
                }
            });

            this.current = pipe;
        }

        return this.lastValue;
    }
}

@Pipe({
    name: 'sqxUserName',
    pure: false,
})
export class UserNamePipe extends UserAsyncPipe implements OnDestroy, PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super('Loading...', users, changeDetector);
    }

    public ngOnDestroy() {
        super.destroy();
    }

    public transform(userId: string | undefined | null, placeholder = 'Me') {
        return super.transformInternal(userId, (users, userId) =>
            users.getUser(userId, placeholder).pipe(map(u => u.displayName)));
    }
}

@Pipe({
    name: 'sqxUserNameRef',
    pure: false,
})
export class UserNameRefPipe extends UserAsyncPipe implements OnDestroy, PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super('Loading...', users, changeDetector);
    }

    public ngOnDestroy() {
        super.destroy();
    }

    public transform(userId: string | undefined | null, placeholder: string | null = 'Me') {
        return super.transformInternal(userId, (users, userId) => {
            const { type, id } = split(userId);

            if (type === 'subject') {
                return users.getUser(id, placeholder).pipe(map(u => u.displayName));
            } else if (id.endsWith('client')) {
                return of(id);
            } else {
                return of(`${id} client`);
            }
        });
    }
}

@Pipe({
    name: 'sqxUserDtoPicture',
    pure: false,
})
export class UserDtoPicture implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public transform(user: UserDto): string | null {
        return this.apiUrl.buildUrl(`api/users/${user.id}/picture`);
    }
}

@Pipe({
    name: 'sqxUserIdPicture',
    pure: false,
})
export class UserIdPicturePipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public transform(userId: string): string | null {
        return this.apiUrl.buildUrl(`api/users/${userId}/picture`);
    }
}

@Pipe({
    name: 'sqxUserPicture',
    pure: false,
})
export class UserPicturePipe extends UserAsyncPipe implements OnDestroy, PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig,
    ) {
        super('', users, changeDetector);
    }

    public ngOnDestroy() {
        super.destroy();
    }

    public transform(userId: string | undefined | null) {
        return super.transformInternal(userId, (users, userId) =>
            users.getUser(userId).pipe(map(u => this.apiUrl.buildUrl(`api/users/${u.id}/picture`))));
    }
}

@Pipe({
    name: 'sqxUserPictureRef',
    pure: false,
})
export class UserPictureRefPipe extends UserAsyncPipe implements OnDestroy, PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig,
    ) {
        super('', users, changeDetector);
    }

    public ngOnDestroy() {
        super.destroy();
    }

    public transform(userId: string | undefined | null) {
        return super.transformInternal(userId, (users, userId) => {
            const { type, id } = split(userId);

            if (type === 'subject') {
                return users.getUser(id).pipe(map(u => this.apiUrl.buildUrl(`api/users/${u.id}/picture`)));
            } else {
                return of('./images/client.png');
            }
        });
    }
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
