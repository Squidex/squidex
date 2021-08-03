/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, ReplaySubject, Subject, throwError } from 'rxjs';
import { ErrorDto } from './../utils/error';
import { Types } from './../utils/types';
import { LocalStoreService } from './local-store.service';

export class DialogRequest {
    private readonly resultStream$ = new ReplaySubject<boolean>();

    public get result(): Observable<boolean> {
        return this.resultStream$;
    }

    public get isCompleted() {
        return this.resultStream$.isStopped;
    }

    public get canRemember() {
        return !!this.rememberKey;
    }

    public remember: boolean;

    constructor(
        public readonly title: string,
        public readonly text: string,
        private readonly rememberKey: string | undefined,
        private readonly localStore: LocalStoreService,
    ) {
        if (rememberKey) {
            this.rememberKey = `dialogs.confirm.${rememberKey}`;

            const isConfirmed = this.localStore.getBoolean(this.rememberKey);

            if (isConfirmed) {
                this.resultStream$.next(true);
                this.resultStream$.complete();
            }
        }
    }

    public complete(confirmed: boolean) {
        if (this.rememberKey && this.remember && confirmed) {
            this.localStore.setBoolean(this.rememberKey, true);
        }

        this.resultStream$.next(confirmed);
        this.resultStream$.complete();
    }
}

export class Tooltip {
    constructor(
        public readonly target: any,
        public readonly text: string | null | undefined,
        public readonly position: string,
        public readonly multiple?: boolean,
        public readonly shortcut?: string,
    ) {
    }
}

export class Notification {
    constructor(
        public readonly message: string | ErrorDto,
        public readonly messageType: string,
        public readonly displayTime: number = 10000,
    ) {
    }

    public static error(message: string | ErrorDto): Notification {
        return new Notification(message, 'danger');
    }

    public static info(message: string | ErrorDto): Notification {
        return new Notification(message, 'primary');
    }
}

@Injectable()
export class DialogService {
    private readonly requestStream$ = new Subject<DialogRequest>();
    private readonly notificationsStream$ = new Subject<Notification>();
    private readonly tooltipStream$ = new Subject<Tooltip>();

    public get dialogs(): Observable<DialogRequest> {
        return this.requestStream$;
    }

    public get tooltips(): Observable<Tooltip> {
        return this.tooltipStream$;
    }

    public get notifications(): Observable<Notification> {
        return this.notificationsStream$;
    }

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public notifyError(error: string | ErrorDto) {
        if (Types.is(error, ErrorDto)) {
            this.notify(Notification.error(error));
        } else {
            this.notify(Notification.error(error));
        }

        return throwError(() => error);
    }

    public notifyInfo(text: string) {
        this.notificationsStream$.next(Notification.info(text));
    }

    public notify(notification: Notification) {
        this.notificationsStream$.next(notification);
    }

    public tooltip(tooltip: Tooltip) {
        this.tooltipStream$.next(tooltip);
    }

    public confirm(title: string, text: string, rememberKey?: string): Observable<boolean> {
        const request = new DialogRequest(title, text, rememberKey, this.localStore);

        if (request.isCompleted) {
            return request.result;
        }

        this.requestStream$.next(request);

        return request.result;
    }
}
