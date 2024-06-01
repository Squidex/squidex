/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppDto, AppsState, AvatarComponent, ConfirmClickDirective, ControlErrorsComponent, defined, DialogService, FileDropDirective, FormErrorComponent, FormHintComponent, LayoutComponent, ListViewComponent, ProgressBarComponent, SidebarMenuDirective, Subscriptions, TeamsState, TooltipDirective, TourStepDirective, TransferAppForm, TranslatePipe, Types, UpdateAppForm } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    imports: [
        AsyncPipe,
        AvatarComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FileDropDirective,
        FormErrorComponent,
        FormHintComponent,
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
export class MorePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public app!: AppDto;

    public teams: { id: string | null; name: string }[] = [];

    public isEditable = false;
    public isEditableImage = false;
    public isDeletable = false;
    public isTransferable = false;

    public uploading = false;
    public uploadProgress = 10;

    public transferForm = new TransferAppForm();

    public updateForm = new UpdateAppForm();

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly router: Router,
        public readonly teamsState: TeamsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.appsState.selectedApp.pipe(defined())
                .subscribe(app => {
                    this.app = app;

                    this.isDeletable = app.canDelete;
                    this.isEditable = app.canUpdateGeneral;
                    this.isEditableImage = app.canUpdateImage;
                    this.isTransferable = app.canUpdateTeam;

                    this.updateForm.load(app);
                    this.updateForm.setEnabled(this.isEditable);

                    this.transferForm.load(app);
                    this.transferForm.setEnabled(this.isTransferable);
                }));

        this.appsState.reloadApps();
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.updateForm.submit();

        if (value) {
            this.appsState.update(this.app, value)
                .subscribe({
                    next: app => {
                        this.updateForm.submitCompleted({ newValue: app });
                    },
                    error: error => {
                        this.dialogs.notifyError(error);

                        this.updateForm.submitFailed(error);
                    },
                });
        }
    }

    public transfer() {
        if (!this.isTransferable) {
            return;
        }

        const value = this.transferForm.submit();

        if (value) {
            this.appsState.transfer(this.app, value.teamId)
                .subscribe({
                    next: app => {
                        this.transferForm.submitCompleted({ newValue: app });
                    },
                    error: error => {
                        this.dialogs.notifyError(error);

                        this.transferForm.submitFailed(error);
                    },
                });
        }
    }

    public uploadImage(file: ReadonlyArray<File>) {
        if (!this.isEditableImage) {
            return;
        }

        this.uploading = true;
        this.uploadProgress = 0;

        this.appsState.uploadImage(this.app, file[0])
            .subscribe({
                next: value => {
                if (Types.isNumber(value)) {
                        this.uploadProgress = value;
                    }
                },
                error: () => {
                    this.uploading = false;
                },
                complete: () => {
                    this.uploading = false;
                },
            });
    }

    public removeImage() {
        if (!this.isEditableImage) {
            return;
        }

        this.appsState.removeImage(this.app);
    }

    public deleteApp() {
        if (!this.isDeletable) {
            return;
        }

        this.appsState.delete(this.app)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}
