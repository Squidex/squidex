/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import { UsersProviderService } from './../declarations-base';

class UserAsyncPipe implements OnDestroy {
    private lastUserId: string;
    private lastValue: string;
    private subscription: Subscription;

    constructor(
        private readonly users: UsersProviderService,
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    protected transformInternal(userId: string, transform: (users: UsersProviderService) => Observable<string>): string {
        if (this.lastUserId !== userId) {
            this.lastUserId = userId;

            if (this.subscription) {
                this.subscription.unsubscribe();
            }

            this.subscription = transform(this.users).subscribe(value => {
                this.lastValue = value;

                this.changeDetector.markForCheck();
            });
        }

        return this.lastValue;
    }
}

@Pipe({
    name: 'userName',
    pure: false
})
export class UserNamePipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string, placeholder = 'Me'): string {
        return super.transformInternal(userId, users => users.getUser(userId, placeholder).map(u => u.displayName));
    }
}

@Pipe({
    name: 'userNameRef',
    pure: false
})
export class UserNameRefPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string, placeholder = 'Me'): string {
        return super.transformInternal(userId, users => {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return users.getUser(parts[1], placeholder).map(u => u.displayName);
            } else {
                if (parts[1].endsWith('client')) {
                    return Observable.of(parts[1]);
                } else {
                    return Observable.of(`${parts[1]}-client`);
                }
            }
        });
    }
}

@Pipe({
    name: 'userEmail',
    pure: false
})
export class UserEmailPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string): string {
        return super.transformInternal(userId, users => users.getUser(userId).map(u => u.email));
    }
}

@Pipe({
    name: 'userEmailRef',
    pure: false
})
export class UserEmailRefPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string): string {
        return super.transformInternal(userId, users => {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return users.getUser(parts[1]).map(u => u.email);
            } else {
                return null;
            }
        });
    }
}

@Pipe({
    name: 'userPicture',
    pure: false
})
export class UserPicturePipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string): string {
        return super.transformInternal(userId, users => users.getUser(userId).map(u => u.pictureUrl));
    }
}

@Pipe({
    name: 'userPictureRef',
    pure: false
})
export class UserPictureRefPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super(users, changeDetector);
    }

    public transform(userId: string): string {
        return super.transformInternal(userId, users => {
             const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return users.getUser(parts[1]).map(u => u.pictureUrl);
            } else {
                return Observable.of('/images/client.png');
            }
        });
    }
}