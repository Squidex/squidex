/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AuthService, BackupsService, DialogService, RestoreForm, switchSafe } from '@app/shared';
import { timer } from 'rxjs';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html',
})
export class RestorePageComponent {
    public restoreForm = new RestoreForm(this.formBuilder);

    public restoreJob =
        timer(0, 2000).pipe(switchSafe(() => this.backupsService.getRestore()));

    constructor(
        public readonly authState: AuthService,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
    ) {
    }

    public restore() {
        const value = this.restoreForm.submit();

        if (value) {
            this.restoreForm.submitCompleted();

            this.backupsService.postRestore(value)
                .subscribe({
                    next: () => {
                        this.dialogs.notifyInfo('i18n:backups.restoreStarted');
                    },
                    error: error => {
                        this.dialogs.notifyError(error);
                    },
                });
        }
    }
}
