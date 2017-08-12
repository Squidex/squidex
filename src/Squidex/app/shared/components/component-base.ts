/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    DialogService,
    ErrorDto,
    Notification
} from './../declarations-base';

export abstract class ComponentBase {
    constructor(
        public readonly dialogs: DialogService
    ) {
    }

    protected notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.dialogs.notify(Notification.error(error.displayMessage));
        } else {
            this.dialogs.notify(Notification.error(error));
        }
    }

    protected notifyInfo(error: string) {
        this.dialogs.notify(Notification.info(error));
    }
}