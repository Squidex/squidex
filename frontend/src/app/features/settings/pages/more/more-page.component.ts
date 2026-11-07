/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, AvatarComponent, ConfirmClickDirective, DialogService, FileDropDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, ProgressBarComponent, SidebarMenuDirective, SignalSelectDirective, TeamsState, TooltipDirective, TourStepDirective, TransferAppForm, TranslatePipe, Types, UpdateAppForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    imports: [
        AvatarComponent,
        ConfirmClickDirective,
        FileDropDirective,
        FormErrorComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ProgressBarComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SidebarMenuDirective,
        SignalSelectDirective,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class MorePageComponent {
    private readonly appsState = inject(AppsState);
    private readonly dialogs = inject(DialogService);

    protected readonly router = inject(Router);
    protected readonly teamsState = inject(TeamsState);

    protected readonly app = this.appsState.selectedApp;

    protected readonly isEditable = computed(() => {
        return this.app()?.canUpdateGeneral === true;
    });

    protected readonly isEditableImage = computed(() => {
        return this.app()?.canUpdateImage === true;
    });

    protected readonly isDeletable = computed(() => {
        return this.app()?.canDelete === true;
    });

    protected readonly isTransferable = computed(() => {
        return this.app()?.canUpdateTeam === true;
    });

    protected readonly uploading = signal(false);
    protected readonly uploadProgress = signal(10);

    protected readonly transferForm = new TransferAppForm();

    protected readonly updateForm = new UpdateAppForm();

    constructor() {
        effect(() => {
            const app = this.app();
            if (!app) {
                return;
            }

            this.updateForm.load(app);
            this.updateForm.setEnabled(this.isEditable());

            this.transferForm.load(app);
            this.transferForm.setEnabled(this.isTransferable());
        });

        this.appsState.reloadApps();
    }

    protected save() {
        const app = this.app();
        if (!app || !this.isEditable()) {
            return;
        }

        const value = this.updateForm.submit();
        if (!value) {
            return;
        }

        this.appsState.update(app, value)
            .subscribe({
                next: updated => {
                    this.updateForm.submitCompleted({ newValue: updated });
                },
                error: error => {
                    this.dialogs.notifyError(error);

                    this.updateForm.submitFailed(error);
                },
            });
    }

    protected transfer() {
        const app = this.app();
        if (!app || !this.isTransferable()) {
            return;
        }

        const value = this.transferForm.submit();
        if (!value) {
            return;
        }

        this.appsState.transfer(app, value)
            .subscribe({
                next: updated => {
                    this.transferForm.submitCompleted({ newValue: updated });
                },
                error: error => {
                    this.dialogs.notifyError(error);

                    this.transferForm.submitFailed(error);
                },
            });
    }

    protected uploadImage(file: ReadonlyArray<File>) {
        const app = this.app();
        if (!app || !this.isEditableImage()) {
            return;
        }

        this.uploading.set(true);
        this.uploadProgress.set(0);

        this.appsState.uploadImage(app, file[0])
            .subscribe({
                next: value => {
                    if (Types.isNumber(value)) {
                        this.uploadProgress.set(value);
                    }
                },
                error: () => {
                    this.uploading.set(false);
                },
                complete: () => {
                    this.uploading.set(false);
                },
            });
    }

    protected removeImage() {
        const app = this.app();
        if (!app || !this.isEditableImage()) {
            return;
        }

        this.appsState.removeImage(app);
    }

    protected deleteApp() {
        const app = this.app();
        if (!app || !this.isDeletable()) {
            return;
        }

        this.appsState.delete(app)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}
