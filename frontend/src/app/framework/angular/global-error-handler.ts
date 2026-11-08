/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ErrorHandler, inject, NgZone, Service } from '@angular/core';
import { DialogService } from '../internal';

@Service({ autoProvided: false })
export class GlobalErrorHandler implements ErrorHandler {
    private readonly dialogs = inject(DialogService);
    private readonly zone = inject(NgZone);

    public handleError(error: any): void {
        const chunkFailedMessage = /Loading chunk [\d]+ failed/;

        if (chunkFailedMessage.test(error.message)) {
            this.zone.run(() => {
                this.dialogs.confirm('i18n:common.errors.chunkLoadingTitle', 'i18n:common.errors.chunkLoadingText')
                    .subscribe(() => {
                        location.reload();
                    });
            });
        }
    }
}