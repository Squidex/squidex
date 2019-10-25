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
    Types,
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
    public isImageEditable: boolean;
    public isDeletable: boolean;

    public uploading = false;
    public uploadProgress = 10;

    public updateForm = new UpdateAppForm(this.formBuilder);

    constructor(
        private readonly appsState: AppsState,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.appsState.selectedApp
                .subscribe(app => {
                    this.app = app;

                    this.isDeletable = app.canDelete;
                    this.isEditable = app.canUpdateGeneral;
                    this.isImageEditable = app.canUpdateImage;

                    this.updateForm.load(app);
                    this.updateForm.setEnabled(this.isEditable);
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

    public uploadImage(file: ReadonlyArray<File>) {
        if (!this.isImageEditable) {
            return;
        }

        this.uploading = true;
        this.uploadProgress = 0;

        this.appsState.uploadImage(this.app, file[0])
            .subscribe(value => {
                if (Types.isNumber(value)) {
                    this.uploadProgress = value;
                }
            }, () => {
                this.uploading = false;
            }, () => {
                this.uploading = false;
            });
    }

    public removeImage() {
        if (!this.isImageEditable) {
            return;
        }

        this.appsState.removeImage(this.app);
    }

    public archiveApp() {
        if (!this.isDeletable) {
            return;
        }

        this.appsState.delete(this.app)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}