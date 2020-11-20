/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiUrlConfig, AppLanguageDto, AppsState, AuthService, AutoSaveKey, AutoSaveService, CanComponentDeactivate, ContentDto, ContentsState, DialogService, EditContentForm, fadeAnimation, FieldForm, FieldSection, LanguagesState, ModalModel, ResourceOwner, RootFieldDto, SchemaDetailsDto, SchemasState, TempService, Version } from '@app/shared';
import { Observable, of } from 'rxjs';
import { filter, tap } from 'rxjs/operators';

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

    public content?: ContentDto | null;
    public contentVersion: Version | null;
    public contentForm: EditContentForm;
    public contentFormCompare: EditContentForm | null = null;

    public dropdown = new ModalModel();

    public language: AppLanguageDto;
    public languages: ReadonlyArray<AppLanguageDto>;

    constructor(apiUrl: ApiUrlConfig, authService: AuthService, appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly autoSaveService: AutoSaveService,
        private readonly dialogs: DialogService,
        private readonly languagesState: LanguagesState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
        private readonly tempService: TempService
    ) {
        super();

        this.formContext = {
            apiUrl: apiUrl.buildUrl('api'),
            appId: appsState.snapshot.selectedApp!.id,
            appName: appsState.snapshot.selectedApp!.name,
            user: authService.user
        };
    }

    public ngOnInit() {
        this.contentsState.loadIfNotLoaded();

        this.own(
            this.languagesState.languagesDtos
                .subscribe(languages => {
                    this.languages = languages;
                    this.language = this.languages.find(x => x.isMaster)!;
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;

                    this.contentForm = new EditContentForm(this.languages, this.schema, this.formContext.user);
                }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    const isNewContent = isOtherContent(content, this.content);

                    this.content = content;

                    this.formContext['initialContent'] = this.content;

                    this.autoSaveKey = {
                        schemaId: this.schema.id,
                        schemaVersion: this.schema.version,
                        contentId: content?.id
                    };

                    const autosaved = this.autoSaveService.get(this.autoSaveKey);

                    if (content) {
                        this.loadContent(content.data, true);
                    }

                    const clone = this.tempService.fetch();

                    if (clone) {
                        this.loadContent(clone, true);
                    } else if (isNewContent && autosaved && this.contentForm.hasChanges(autosaved)) {
                        this.dialogs.confirm('i18n:contents.unsavedChangesTitle', 'i18n:contents.unsavedChangesText')
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
            this.contentForm.valueChanges.pipe(filter(_ => !this.isLoadingContent && this.contentForm.form.enabled))
                .subscribe(value => {
                    this.autoSaveService.set(this.autoSaveKey, value);
                }));
    }

    public canDeactivate(): Observable<boolean> {
        return this.checkPendingChangesBeforeClose().pipe(
            tap(confirmed => {
                if (confirmed) {
                    this.autoSaveService.remove(this.autoSaveKey);
                }
            })
        );
    }

    public saveAndPublish() {
        this.saveContent(true);
    }

    public save() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        const value = this.contentForm.submit();

        if (value) {
            if (this.content) {
                if (!this.content.canUpdate) {
                    return;
                }

                this.contentsState.update(this.content, value)
                    .subscribe(() => {
                        this.contentForm.submitCompleted({ noReset: true });
                    }, error => {
                        this.contentForm.submitFailed(error);
                    });
            } else {
                if (!this.canCreate(publish)) {
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
            this.contentForm.submitFailed('i18n:contents.contentNotValid', false);
        }
    }

    private canCreate(publish: boolean) {
        if (publish) {
            return this.contentsState.snapshot.canCreateAndPublish;
        } else {
            return this.contentsState.snapshot.canCreate;
        }
    }

    public back() {
        this.router.navigate([this.schema.name], { relativeTo: this.route.parent!.parent, replaceUrl: true });
    }

    public delete() {
        const content = this.content;

        if (content) {
            this.contentsState.deleteMany([content]);
        }
    }

    public checkPendingChangesBeforeClose() {
        if (this.content && !this.content.canUpdate) {
            return of(true);
        }

        return this.contentForm.hasChanged() ?
            this.dialogs.confirm('i18n:contents.pendingChangesTitle', 'i18n:contents.pendingChangesTextToClose') :
            of(true);
    }

    public checkPendingChangesBeforeChangingStatus() {
        if (this.content && !this.content.canUpdate) {
            return of(true);
        }

        return this.contentForm.hasChanged() ?
            this.dialogs.confirm('i18n:contents.pendingChangesTitle', 'i18n:contents.pendingChangesTextToChange') :
            of(true);
    }

    public loadLatest() {
        this.loadVersion(null, false);
    }

    public loadVersion(version: Version | null, compare: boolean) {
        const content = this.content;

        if (!content || version === null || version.eq(content.version)) {
            this.contentFormCompare = null;
            this.contentVersion = null;
            this.loadContent(content?.data || {}, true);
        } else {
            this.contentsState.loadVersion(content, version)
                .subscribe(dto => {
                    if (compare) {
                        this.contentFormCompare = new EditContentForm(this.languages, this.schema, this.formContext.user);

                        this.contentFormCompare.load(dto.payload);
                        this.contentFormCompare.setEnabled(false);

                        this.loadContent(content.data, false);
                    } else {
                        this.contentFormCompare = null;

                        this.loadContent(dto.payload, false);
                    }

                    this.contentVersion = version;
                });
        }
    }

    private loadContent(data: any, isInitial: boolean) {
        this.isLoadingContent = true;

        this.autoSaveService.remove(this.autoSaveKey);

        try {
            this.contentForm.load(data, isInitial);
            this.contentForm.setEnabled(!this.content || this.content.canUpdate);
        } finally {
            this.isLoadingContent = false;
        }
    }

    public trackBySection(_index: number, section: FieldSection<RootFieldDto, FieldForm>) {
        return section.separator?.fieldId;
    }
}

function isOtherContent(lhs: ContentDto | null | undefined, rhs: ContentDto | null | undefined) {
    return !lhs || !rhs || lhs.id !== rhs.id;
}