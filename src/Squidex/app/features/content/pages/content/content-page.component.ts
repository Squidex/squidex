/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { debounceTime, filter, onErrorResumeNext, tap } from 'rxjs/operators';

import { ContentVersionSelected } from './../messages';

import {
    ApiUrlConfig,
    AppLanguageDto,
    AuthService,
    AutoSaveKey,
    AutoSaveService,
    CanComponentDeactivate,
    ContentDto,
    ContentsState,
    DialogService,
    EditContentForm,
    fadeAnimation,
    FieldDto,
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
    private isLoadingContent: boolean;
    private autoSaveKey: AutoSaveKey;

    public schema: SchemaDetailsDto;

    public formContext: any;

    public content: ContentDto;
    public contentVersion: Version | null;
    public contentForm: EditContentForm;
    public contentFormCompare: EditContentForm | null = null;

    public dropdown = new ModalModel();

    public language: AppLanguageDto;
    public languages: ReadonlyArray<AppLanguageDto>;

    public get hasContent() {
        return !!this.content;
    }

    public trackByFieldFn: (index: number, field: FieldDto) => any;

    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector: DueTimeSelectorComponent;

    constructor(apiUrl: ApiUrlConfig, authService: AuthService,
        public readonly contentsState: ContentsState,
        private readonly autoSaveService: AutoSaveService,
        private readonly dialogs: DialogService,
        private readonly languagesState: LanguagesState,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState
    ) {
        super();

        this.trackByFieldFn = this.trackByField.bind(this);

        this.formContext = { user: authService.user, apiUrl: apiUrl.buildUrl('api') };
    }

    public ngOnInit() {
        this.contentsState.loadIfNotLoaded();

        this.own(
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages[0];
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;

                    this.contentForm = new EditContentForm(this.languages, this.schema);
                }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    this.autoSaveKey = {
                        schemaId: this.schema.id,
                        schemaVersion: this.schema.version,
                        contentId: content ? content.id : undefined
                    };

                    const autosaved = this.autoSaveService.get(this.autoSaveKey);

                    if (content) {
                        this.content = content;

                        this.loadContent(this.content.dataDraft, true);
                    }

                    if (autosaved && this.isOtherContent(content) && this.contentForm.hasChanges(autosaved)) {
                        this.dialogs.confirm('Unsaved changes', 'You have unsaved changes. Do you want to load them now?')
                            .subscribe(shouldLoad => {
                                if (shouldLoad) {
                                    this.loadContent(autosaved, false);
                                } else {
                                    this.autoSaveService.remove(this.autoSaveKey);
                                }
                            });
                    }
                }));

        this.own(
            this.contentForm.form.valueChanges.pipe(
                    filter(_ => !this.isLoadingContent),
                    filter(_ => this.contentForm.form.enabled),
                    debounceTime(2000)
                ).subscribe(value => {
                    this.autoSaveService.set(this.autoSaveKey, value);
                }));

        this.own(
            this.messageBus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version, message.compare);
                }));
    }

    private isOtherContent(content: ContentDto | null | undefined) {
        return !this.content || !content || content.id !== this.content.id;
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.hasChanged()) {
            return of(true);
        } else {
            return this.dialogs.confirm('Unsaved changes', 'You have unsaved changes, do you want to close the current content view and discard your changes?').pipe(
                tap(confirmed => {
                    if (confirmed) {
                        this.autoSaveService.remove(this.autoSaveKey);
                    }
                })
            );
        }
    }

    public saveAndPublish() {
        this.saveContent(true, false);
    }

    public saveAsDraft() {
        this.saveContent(false, true);
    }

    public save() {
        this.saveContent(false, false);
    }

    private saveContent(publish: boolean, asDraft: boolean) {
        const value = this.contentForm.submit();

        if (value) {
            this.autoSaveService.remove(this.autoSaveKey);

            if (this.content) {
                if (asDraft) {
                    if (this.content && !this.content.canDraftPropose) {
                        return;
                    }

                    this.contentsState.proposeDraft(this.content, value)
                        .subscribe(() => {
                            this.contentForm.submitCompleted({ noReset: true });
                        }, error => {
                            this.contentForm.submitFailed(error);
                        });
                } else {
                    if (this.content && !this.content.canUpdateAny) {
                        return;
                    }

                    this.contentsState.update(this.content, value)
                        .subscribe(() => {
                            this.contentForm.submitCompleted({ noReset: true });
                        }, error => {
                            this.contentForm.submitFailed(error);
                        });
                }
            } else {
                if ((publish && !this.contentsState.snapshot.canCreate) || (!publish && !this.contentsState.snapshot.canCreateAndPublish)) {
                    return;
                }

                this.contentsState.create(value, publish)
                    .subscribe(() => {
                        this.contentForm.submitCompleted({ noReset: true });

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

    private loadContent(data: any, isInitial: boolean) {
        this.isLoadingContent = true;

        this.autoSaveService.remove(this.autoSaveKey);

        try {
            this.contentForm.load(data, isInitial);
            this.contentForm.setEnabled(!this.content || this.content.canUpdateAny);
        } finally {
            this.isLoadingContent = false;
        }
    }

    public discardChanges() {
        this.contentsState.discardDraft(this.content);
    }

    public delete() {
        this.contentsState.deleteMany([this.content]).pipe(onErrorResumeNext())
            .subscribe(() => {
                this.back();
            });
    }

    public publishChanges() {
        this.contentsState.publishDraft(this.content, null);
    }

    public changeStatus(status: string) {
        this.contentsState.changeStatus(this.content, status, null);
    }

    private loadVersion(version: Version | null, compare: boolean) {
        if (!this.content || version === null || version.eq(this.content.version)) {
            this.contentFormCompare = null;
            this.contentVersion = null;
            this.loadContent(this.content.dataDraft, true);
        } else {
            this.contentsState.loadVersion(this.content, version)
                .subscribe(dto => {
                    if (compare) {
                        this.contentFormCompare = new EditContentForm(this.languages, this.schema);

                        this.contentFormCompare.load(dto.payload);
                        this.contentFormCompare.setEnabled(false);

                        this.loadContent(this.content.dataDraft, false);
                    } else {
                        this.contentFormCompare = null;

                        this.loadContent(dto.payload, false);
                    }

                    this.contentVersion = version;
                });
        }
    }

    public showLatest() {
        this.loadVersion(null, false);
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }
}
