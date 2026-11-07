/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, effect } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, ConfirmClickDirective, ControlErrorsComponent, defined, EditAppSettingsForm, FormHintComponent, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-settings-page',
    styleUrls: ['./settings-page.component.scss'],
    templateUrl: './settings-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class SettingsPageComponent {
    public readonly editForm = new EditAppSettingsForm();
    public readonly editingSettings = toSignal(this.appsState.selectedSettings.pipe(defined()));

    public readonly isEditable = computed(() => {
        return this.editingSettings()?.canUpdate === true;
    });

    constructor(
        private readonly appsState: AppsState,
    ) {
        this.appsState.loadSettings();

        effect(() => {
            const settings = this.editingSettings();
            if (!settings) {
                return;
            }

            this.editForm.load(settings);
            this.editForm.setEnabled(settings.canUpdate);
        });
    }

    public reload() {
        this.appsState.loadSettings(true);
    }

    public save() {
        const settings = this.editingSettings();
        if (!settings) {
            return;
        }

        if (!this.isEditable()) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        this.appsState.updateSettings(settings, value)
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
