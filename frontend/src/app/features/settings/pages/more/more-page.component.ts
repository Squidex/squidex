/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, computed, effect, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, AvatarComponent, ConfirmClickDirective, defined, DialogService, FileDropDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, ProgressBarComponent, SidebarMenuDirective, TeamsState, TooltipDirective, TourStepDirective, TransferAppForm, TranslatePipe, Types, UpdateAppForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    imports: [
        AsyncPipe,
        AvatarComponent,
        ConfirmClickDirective,
        FileDropDirective,
        FormErrorComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ProgressBarComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SidebarMenuDirective,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class MorePageComponent {
    public readonly app = toSignal(this.appsState.selectedApp.pipe(defined()));

    public readonly isEditable = computed(() => {
        return this.app()?.canUpdateGeneral === true;
    });

    public readonly isEditableImage = computed(() => {
        return this.app()?.canUpdateImage === true;
    });

    public readonly isDeletable = computed(() => {
        return this.app()?.canDelete === true;
    });

    public readonly isTransferable = computed(() => {
        return this.app()?.canUpdateTeam === true;
    });

    public readonly uploading = signal(false);
    public readonly uploadProgress = signal(10);

    public readonly transferForm = new TransferAppForm();

    public readonly updateForm = new UpdateAppForm();

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly router: Router,
        public readonly teamsState: TeamsState,
    ) {
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

    public save() {
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

    public transfer() {
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

    public uploadImage(file: ReadonlyArray<File>) {
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

    public removeImage() {
        const app = this.app();
        if (!app || !this.isEditableImage()) {
            return;
        }

        this.appsState.removeImage(app);
    }

    public deleteApp() {
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
