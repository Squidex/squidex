/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, ConfirmClickDirective, ControlErrorsComponent, EditAppSettingsForm, FormHintComponent, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-settings-page',
    styleUrls: ['./settings-page.component.scss'],
    templateUrl: './settings-page.component.html',
    imports: [
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormField,
        FormHintComponent,
        LayoutComponent,
        ListViewComponent,
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
export class SettingsPageComponent {
    protected readonly appsState = inject(AppsState);

    protected readonly isEditable = computed(() => {
        return this.appsState.selectedSettings()?.canUpdate === true;
    });

    protected readonly editForm = new EditAppSettingsForm();

    constructor() {
        this.appsState.loadSettings();

        effect(() => {
            const settings = this.appsState.selectedSettings();
            if (!settings) {
                return;
            }

            this.editForm.load(settings);
            this.editForm.setEnabled(settings.canUpdate);
        });
    }

    protected reload() {
        this.appsState.loadSettings(true);
    }

    protected save() {
        const settings = this.appsState.selectedSettings();
        if (!settings) {
            return;
        }

        if (!this.isEditable()) {
            return;
        }

        this.editForm.submit(value => {
            return this.appsState.updateSettings(settings, value);
        });
    }
}
