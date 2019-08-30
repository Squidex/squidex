/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';

import {
    AppDto,
    AppsState,
    ResourceOwner,
    UpdateAppForm
} from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html'
})
export class MorePageComponent extends ResourceOwner implements OnInit {
    public app: AppDto;

    public isEditable: boolean;
    public isDeletable: boolean;

    public updateForm = new UpdateAppForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.appsState.selectedApp
                .subscribe(app => {
                    if (app) {
                        this.app = app;

                        this.isEditable = app.canUpdate;
                        this.isDeletable = app.canDelete;

                        this.updateForm.load(app);
                        this.updateForm.setEnabled(this.isEditable);
                    }
                }));
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.updateForm.submit();

        if (value) {
            this.appsState.update(this.app, value)
                .subscribe(user => {
                    this.updateForm.submitCompleted({ newValue: user });
                }, error => {
                    this.updateForm.submitFailed(error);
                });
        }
    }

    public archiveApp() {
        this.appsState.delete(this.appsState.selectedAppState!)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}

