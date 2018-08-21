/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
    AuthService,
    BackupsService,
    DialogService,
    RestoreDto,
    RestoreForm
} from '@app/shared';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html'
})
export class RestorePageComponent implements OnDestroy, OnInit {
    private timerSubscription: Subscription;

    public restoreJob: RestoreDto | null;
    public restoreForm = new RestoreForm(this.formBuilder);

    constructor(
        public readonly authState: AuthService,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.timerSubscription =
            timer(0, 2000).pipe(switchMap(() => this.backupsService.getRestore()))
                .subscribe(dto => {
                    if (dto !== null) {
                        this.restoreJob = dto;
                    }
                });
    }

    public restore() {
        const value = this.restoreForm.submit();

        if (value) {
            this.restoreForm.submitCompleted({});

            this.backupsService.postRestore(value)
                .subscribe(() => {
                    this.dialogs.notifyInfo('Restore started, it can take several minutes to complete.');
                }, error => {
                    this.dialogs.notifyError(error);
                });
        }
    }
}