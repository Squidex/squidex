/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-pipe-impure

import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { Observable, of, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    ApiUrlConfig,
    AssetDto,
    formatHistoryMessage,
    HistoryEventDto,
    MathHelper,
    UserDto,
    UsersProviderService
} from '@app/shared/internal';

@Pipe({
    name: 'sqxHistoryMessage',
    pure: false
})
export class HistoryMessagePipe implements OnDestroy, PipeTransform {
    private subscription: Subscription;
    private lastMessage: string;
    private lastValue: string | null = null;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly users: UsersProviderService
    ) {
    }

    public ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    public transform(event: HistoryEventDto): string | null {
        if (!event) {
            return this.lastValue;
        }

        if (this.lastMessage !== event.message) {
            this.lastMessage = event.message;

            if (this.subscription) {
                this.subscription.unsubscribe();
            }

            this.subscription =  formatHistoryMessage(event.message, this.users).subscribe(value => {
                this.lastValue = value;

                this.changeDetector.markForCheck();
            });
        }

        return this.lastValue;
    }
}

class UserAsyncPipe implements OnDestroy {
    private lastUserId: string;
    private lastValue: string | undefined = undefined;
    private subscription: Subscription;

    constructor(loading: string,
        private readonly users: UsersProviderService,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        this.lastValue = loading;
    }

    public ngOnDestroy() {
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

            this.subscription = transform(this.users).subscribe(value => {
                this.lastValue = value || undefined;

                this.changeDetector.markForCheck();
            });
        }

        return this.lastValue;
    }
}

@Pipe({
    name: 'sqxUserName',
    pure: false
})
export class UserNamePipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super('Loading...', users, changeDetector);
    }

    public transform(userId: string, placeholder = 'Me') {
        return super.transformInternal(userId, users => users.getUser(userId, placeholder).pipe(map(u => u.displayName)));
    }
}

@Pipe({
    name: 'sqxUserNameRef',
    pure: false
})
export class UserNameRefPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef) {
        super('Loading...', users, changeDetector);
    }

    public transform(userId: string, placeholder: string | null = 'Me') {
        return super.transformInternal(userId, users => {
            const { type, id } = split(userId);

            if (type === 'subject') {
                return users.getUser(id, placeholder).pipe(map(u => u.displayName));
            } else {
                if (id.endsWith('client')) {
                    return of(id);
                } else {
                    return of(`${id} client`);
                }
            }
        });
    }
}

@Pipe({
    name: 'sqxUserDtoPicture',
    pure: false
})
export class UserDtoPicture implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(user: UserDto): string | null {
        return this.apiUrl.buildUrl(`api/users/${user.id}/picture`);
    }
}

@Pipe({
    name: 'sqxUserIdPicture',
    pure: false
})
export class UserIdPicturePipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(userId: string): string | null {
        return this.apiUrl.buildUrl(`api/users/${userId}/picture`);
    }
}

@Pipe({
    name: 'sqxUserPicture',
    pure: false
})
export class UserPicturePipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super('', users, changeDetector);
    }

    public transform(userId: string) {
        return super.transformInternal(userId, users => users.getUser(userId).pipe(map(u => this.apiUrl.buildUrl(`api/users/${u.id}/picture`))));
    }
}

@Pipe({
    name: 'sqxUserPictureRef',
    pure: false
})
export class UserPictureRefPipe extends UserAsyncPipe implements PipeTransform {
    constructor(users: UsersProviderService, changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super('', users, changeDetector);
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

@Pipe({
    name: 'sqxAssetUrl',
    pure: true
})
export class AssetUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(asset: AssetDto): string {
        return `${asset.fullUrl(this.apiUrl)}&sq=${MathHelper.guid()}`;
    }
}

@Pipe({
    name: 'sqxAssetPreviewUrl',
    pure: true
})
export class AssetPreviewUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public transform(asset: AssetDto): string {
        return asset.fullUrl(this.apiUrl);
    }
}

@Pipe({
    name: 'sqxFileIcon',
    pure: true
})
export class FileIconPipe implements PipeTransform {
    public transform(asset: { mimeType: string, fileType: string }): string {
        const knownTypes = [
            'doc',
            'docx',
            'pdf',
            'ppt',
            'pptx',
            'video',
            'xls',
            'xlsx'
        ];

        let mimeIcon: string;

        const mimeParts = asset.mimeType.split('/');

        if (mimeParts.length === 2 && mimeParts[0].toLowerCase() === 'video') {
            mimeIcon = 'video';
        } else {
            mimeIcon = knownTypes.indexOf(asset.fileType) >= 0 ? asset.fileType : 'generic';
        }

        return `./images/asset_${mimeIcon}.png`;
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