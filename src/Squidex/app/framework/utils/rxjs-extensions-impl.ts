/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';

import { DialogService } from './../services/dialog.service';

export function nextBy<T>(updater: (value: T) => T): void {
    return this.next(updater(this.value));
}

export function notify(dialogs: DialogService) {
    return this.catch((error: any) => {
        dialogs.notifyError(error);

        return Observable.throw(error);
    });
}