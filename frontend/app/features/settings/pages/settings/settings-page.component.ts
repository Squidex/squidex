/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AppSettingsDto, AppsState, EditAppSettingsForm, ResourceOwner } from '@app/shared';

@Component({
    selector: 'sqx-settings-page',
    styleUrls: ['./settings-page.component.scss'],
    templateUrl: './settings-page.component.html',
})
export class SettingsPageComponent extends ResourceOwner implements OnInit {
    public isEditable = false;

    public editForm = new EditAppSettingsForm(this.formBuilder);
    public editingSettings: AppSettingsDto;

    constructor(
        private readonly appsState: AppsState,
        private readonly formBuilder: FormBuilder,
    ) {
        super();
    }

    public ngOnInit() {
        this.appsState.loadSettings();

        this.own(
            this.appsState.selectedSettings
                .subscribe(settings => {
                    if (settings) {
                        this.isEditable = settings.canUpdate;

                        this.editForm.load(settings);
                        this.editForm.setEnabled(settings.canUpdate);

                        this.editingSettings = settings;
                    }
                }));
    }

    public reload() {
        this.appsState.loadSettings(true);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.appsState.updateSettings(this.editingSettings, value)
                .subscribe(() => {
                    this.editForm.submitCompleted({ noReset: true });
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}
