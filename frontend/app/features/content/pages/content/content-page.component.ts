/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiUrlConfig, AppLanguageDto, AppsState, AuthService, AutoSaveKey, AutoSaveService, CanComponentDeactivate, ContentDto, ContentsState, defined, DialogService, EditContentForm, fadeAnimation, LanguagesState, ModalModel, ResourceOwner, SchemaDto, SchemasState, TempService, Types, Version } from '@app/shared';
import { Observable, of } from 'rxjs';
import { filter, map, tap } from 'rxjs/operators';
import { ContentReferencesComponent } from './references/content-references.component';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    animations: [
        fadeAnimation,
    ],
})
export class ContentPageComponent extends ResourceOwner implements CanComponentDeactivate, OnInit {
    private autoSaveKey: AutoSaveKey;

    @ViewChild(ContentReferencesComponent)
    public references: ContentReferencesComponent;

    public schema: SchemaDto;

    public formContext: any;

    public contentTab = this.route.queryParams.pipe(map(x => x['tab'] || 'editor'));
    public content?: ContentDto | null;
    public contentId = '';
    public contentVersion: Version | null;
    public contentForm: EditContentForm;
    public contentFormCompare: EditContentForm | null = null;

    public dropdown = new ModalModel();

    public language: AppLanguageDto;
    public languages: ReadonlyArray<AppLanguageDto>;

    public confirmPreview = () => {
        return this.checkPendingChangesBeforePreview();
    };

    constructor(apiUrl: ApiUrlConfig, authService: AuthService, appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly autoSaveService: AutoSaveService,
        private readonly dialogs: DialogService,
        private readonly languagesState: LanguagesState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
        private readonly tempService: TempService,
    ) {
        super();

        this.formContext = {
            apiUrl: apiUrl.buildUrl('api'),
            appId: contentsState.appId,
            appName: appsState.appName,
            user: authService.user,
        };
    }

    public ngOnInit() {
        this.contentsState.loadIfNotLoaded();

        this.own(
            this.languagesState.isoMasterLanguage
                .subscribe(language => {
                    this.language = language;
                }));

        this.own(
            this.languagesState.isoLanguages
                .subscribe(languages => {
                    this.languages = languages;
                }));

        this.own(
            this.schemasState.selectedSchema.pipe(defined())
                .subscribe(schema => {
                    this.schema = schema;

                    this.contentForm = new EditContentForm(this.languages, this.schema, this.schemasState.schemaMap, this.formContext);
                }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    const isNewContent = isOtherContent(content, this.content);

                    this.formContext['initialContent'] = content;

                    this.content = content;
                    this.contentForm.setContext(this.formContext);

                    this.autoSaveKey = {
                        schemaId: this.schema.id,
                        schemaVersion: this.schema.version,
                        contentId: content?.id,
                    };

                    const dataAutosaved = this.autoSaveService.fetch(this.autoSaveKey);
                    const dataCloned = this.tempService.fetch();

                    if (dataCloned || content) {
                        this.loadContent(dataCloned || content?.data || {}, true);
                    }

                    if (isNewContent && dataAutosaved && this.contentForm.hasChanges(dataAutosaved)) {
                        this.dialogs.confirm('i18n:contents.unsavedChangesTitle', 'i18n:contents.unsavedChangesText')
                            .subscribe(shouldLoad => {
                                if (shouldLoad) {
                                    this.loadContent(dataAutosaved, false);
                                } else {
                                    this.autoSaveService.remove(this.autoSaveKey);
                                }
                            });
                    }
                }));

        this.own(
            this.contentForm.valueChanges.pipe(filter(_ => this.contentForm.form.enabled))
                .subscribe(value => {
                    if (!Types.equals(value, this.content?.data)) {
                        this.autoSaveService.set(this.autoSaveKey, value);
                    } else {
                        this.autoSaveService.remove(this.autoSaveKey);
                    }
                }));
    }

    public canDeactivate(): Observable<boolean> {
        return this.checkPendingChangesBeforeClose().pipe(
            tap(confirmed => {
                if (confirmed) {
                    this.autoSaveService.remove(this.autoSaveKey);
                }
            }),
        );
    }

    public validate() {
        this.references?.validate();
    }

    public publish() {
        this.references?.publish();
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
                    .subscribe({
                        next: () => {
                            this.contentForm.submitCompleted({ noReset: true });
                        },
                        error: error => {
                            this.contentForm.submitFailed(error);
                        },
                    });
            } else {
                if (!this.canCreate(publish)) {
                    return;
                }

                this.contentsState.create(value, publish, this.contentId)
                    .subscribe({
                        next: content => {
                            this.contentForm.submitCompleted({ noReset: true });

                            this.router.navigate([content.id, 'history'], { relativeTo: this.route.parent! });
                        },
                        error: error => {
                            this.contentForm.submitFailed(error);
                        },
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
            this.contentsState.deleteMany([content]).subscribe(() => {
                this.back();
            });
        }
    }

    public setContentId(id: string) {
        this.contentId = id;
    }

    public checkPendingChangesBeforePreview() {
        return this.checkPendingChanges('i18n:contents.pendingChangesTextToPreview');
    }

    public checkPendingChangesBeforeClose() {
        return this.checkPendingChanges('i18n:contents.pendingChangesTextToClose');
    }

    public checkPendingChangesBeforeChangingStatus() {
        return this.checkPendingChanges('i18n:contents.pendingChangesTextToChange');
    }

    private checkPendingChanges(text: string) {
        if (this.content && !this.content.canUpdate) {
            return of(true);
        }

        return this.contentForm.hasChanged() ?
            this.dialogs.confirm('i18n:contents.pendingChangesTitle', text) :
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
                        this.contentFormCompare = new EditContentForm(this.languages, this.schema, this.schemasState.schemaMap, this.formContext);

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
        this.autoSaveService.remove(this.autoSaveKey);

        this.contentForm.load(data, isInitial);
        this.contentForm.setEnabled(!this.content || this.content.canUpdate);
    }
}

function isOtherContent(lhs: ContentDto | undefined | null, rhs: ContentDto | undefined | null) {
    return !lhs || !rhs || lhs.id !== rhs.id;
}
