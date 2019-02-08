/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    AuthService,
    BackupsService,
    DialogService,
    ResourceOwner,
    RestoreDto,
    RestoreForm
} from '@app/shared';

@Component({
    selector: 'sqx-restore-page',
    styleUrls: ['./restore-page.component.scss'],
    templateUrl: './restore-page.component.html'
})
export class RestorePageComponent extends ResourceOwner implements OnInit {
    public restoreJob: RestoreDto | null;
    public restoreForm = new RestoreForm(this.formBuilder);

    constructor(
        public readonly authState: AuthService,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            timer(0, 2000).pipe(switchMap(() => this.backupsService.getRestore().pipe(onErrorResumeNext())))
                .subscribe(job => {
                    if (job) {
                        this.restoreJob = job;
                    }
                }));
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