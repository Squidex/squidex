/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppSettingsDto, AppsState, ConfirmClickDirective, ControlErrorsComponent, EditAppSettingsForm, FormHintComponent, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-settings-page',
    styleUrls: ['./settings-page.component.scss'],
    templateUrl: './settings-page.component.html',
    imports: [
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SettingsPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public isEditable = false;

    public editForm = new EditAppSettingsForm();
    public editingSettings?: AppSettingsDto;

    constructor(
        private readonly appsState: AppsState,
    ) {
    }

    public ngOnInit() {
        this.appsState.loadSettings();

        this.subscriptions.add(
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
        if (!this.isEditable || !this.editingSettings) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.appsState.updateSettings(this.editingSettings, value)
                .subscribe({
                    next: () => {
                        this.editForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.editForm.submitFailed(error);
                    },
                });
        }
    }
}
