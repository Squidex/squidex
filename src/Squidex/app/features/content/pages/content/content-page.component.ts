/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import { ContentVersionSelected } from './../messages';

import {
    AppLanguageDto,
    AppsState,
    CanComponentDeactivate,
    ContentDto,
    ContentsState,
    DialogService,
    EditContentForm,
    fadeAnimation,
    ImmutableArray,
    LanguagesState,
    MessageBus,
    ModalModel,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    Version
} from '@app/shared';

import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentPageComponent extends ResourceOwner implements CanComponentDeactivate, OnInit {
    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentVersion: Version | null;
    public contentForm: EditContentForm;
    public contentFormCompare: EditContentForm | null = null;

    public dropdown = new ModalModel();

    public language: AppLanguageDto;
    public languages: ImmutableArray<AppLanguageDto>;

    @ViewChild('dueTimeSelector')
    public dueTimeSelector: DueTimeSelectorComponent;

    constructor(
        public readonly appsState: AppsState,
        private readonly contentsState: ContentsState,
        private readonly dialogs: DialogService,
        private readonly languagesState: LanguagesState,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages.at(0);
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    if (schema) {
                        this.schema = schema!;

                        this.contentForm = new EditContentForm(this.schema, this.languages);
                    }
                }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    if (content) {
                        this.content = content;

                        this.loadContent(this.content.dataDraft);
                    }
                }));

        this.own(
            this.messageBus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version, message.compare);
                }));
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.form.dirty || !this.content) {
            return of(true);
        } else {
            return this.dialogs.confirm('Unsaved changes', 'You have unsaved changes, do you want to close the current content view and discard your changes?');
        }
    }

    public saveAndPublish() {
        this.saveContent(true, false);
    }

    public saveAsProposal() {
        this.saveContent(false, true);
    }

    public save() {
        this.saveContent(false, false);
    }

    private saveContent(publish: boolean, asProposal: boolean) {
        if (this.content && this.content.status === 'Archived') {
            return;
        }

        const value = this.contentForm.submit();

        if (value) {
            if (this.content) {
                if (asProposal) {
                    this.contentsState.proposeUpdate(this.content, value)
                        .subscribe(() => {
                            this.contentForm.submitCompleted();
                        }, error => {
                            this.contentForm.submitFailed(error);
                        });
                } else {
                    this.contentsState.update(this.content, value)
                        .subscribe(() => {
                            this.contentForm.submitCompleted();
                        }, error => {
                            this.contentForm.submitFailed(error);
                        });
                }
            } else {
                this.contentsState.create(value, publish)
                    .subscribe(() => {
                        this.back();
                    }, error => {
                        this.contentForm.submitFailed(error);
                    });
            }
        } else {
            this.dialogs.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    public back() {
        this.router.navigate([this.schema.name], { relativeTo: this.route.parent!.parent, replaceUrl: true });
    }

    private loadContent(data: any) {
        this.contentForm.loadContent(data, this.content && this.content.status === 'Archived');
    }

    public discardChanges() {
        this.contentsState.discardChanges(this.content).pipe(onErrorResumeNext()).subscribe();
    }

    public publish() {
        this.changeContentItems('Publish', 'Published');
    }

    public unpublish() {
        this.changeContentItems('Unpublish', 'Draft');
    }

    public archive() {
        this.changeContentItems('Archive', 'Archived');
    }

    public restore() {
        this.changeContentItems('Restore', 'Draft');
    }

    public delete() {
        this.contentsState.deleteMany([this.content]).pipe(onErrorResumeNext())
            .subscribe(() => {
                this.back();
            });
    }

    public publishChanges() {
        this.dueTimeSelector.selectDueTime('Publish').pipe(
                switchMap(d => this.contentsState.publishChanges(this.content, d)), onErrorResumeNext())
            .subscribe();
    }

    private changeContentItems(action: string, status: string) {
        this.dueTimeSelector.selectDueTime(action).pipe(
                switchMap(d => this.contentsState.changeStatus(this.content, action, status, d)), onErrorResumeNext())
            .subscribe();
    }

    private loadVersion(version: Version | null, compare: boolean) {
        if (!this.content || version === null || version.eq(this.content.version)) {
            this.contentFormCompare = null;
            this.contentVersion = null;
            this.contentForm.load(this.content.dataDraft);
        } else {
            this.contentsState.loadVersion(this.content, version)
                .subscribe(dto => {
                    if (compare) {
                        if (this.contentFormCompare === null) {
                            this.contentFormCompare = new EditContentForm(this.schema, this.languages);
                            this.contentFormCompare.form.disable();
                        }

                        this.contentFormCompare.load(dto.payload);
                        this.contentForm.load(this.content.dataDraft);
                    } else {
                        if (this.contentFormCompare) {
                            this.contentFormCompare = null;
                        }

                        this.contentForm.load(dto.payload);
                    }

                    this.contentVersion = version;
                });
        }
    }

    public showLatest() {
        this.loadVersion(null, false);
    }
}