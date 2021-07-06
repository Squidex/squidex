/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { ApiUrlConfig, UserDto, UsersProviderService } from '@app/shared/internal';
import { Observable, of, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';

class UserAsyncPipe {
    private lastUserId: string;
    private lastValue: string | undefined = undefined;
    private subscription: Subscription;
    private current: Observable<string | null>;

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

    protected transformInternal(userId: string, transform: (users: UsersProviderService) => Observable<string | null>) {
        if (this.lastUserId !== userId) {
            this.lastUserId = userId;

            if (this.subscription) {
                this.subscription.unsubscribe();
            }

            const pipe = transform(this.users);

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

    public transform(userId: string, placeholder = 'Me') {
        return super.transformInternal(userId, users => users.getUser(userId, placeholder).pipe(map(u => u.displayName)));
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

    public transform(userId: string, placeholder: string | null = 'Me') {
        return super.transformInternal(userId, users => {
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

    public transform(userId: string) {
        return super.transformInternal(userId, users => users.getUser(userId).pipe(map(u => this.apiUrl.buildUrl(`api/users/${u.id}/picture`))));
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

    public transform(userId: string) {
        return super.transformInternal(userId, users => {
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
        const type = token.substr(0, index);
        const name = token.substr(index + 1);

        return { type, id: name };
    }

    return { type: token, id: token };
}
