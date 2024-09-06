/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Observable, of } from 'rxjs';
import { filter, map, tap } from 'rxjs/operators';
import { AnnotationCreate, AnnotationCreateAfterNavigate, AnnotationsSelect, AnnotationsSelectAfterNavigate, ApiUrlConfig, AppLanguageDto, AppsState, AuthService, AutoSaveKey, AutoSaveService, CanComponentDeactivate, CollaborationService, CommentsState, ConfirmClickDirective, ContentDto, ContentsState, defined, DialogService, DropdownMenuComponent, EditContentForm, LanguageSelectorComponent, LanguagesState, LayoutComponent, LocalStoreService, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, NotifoComponent, PreviousUrl, ResolveAssets, ResolveContents, SchemaDto, SchemasState, Settings, ShortcutDirective, SidebarMenuDirective, Subscriptions, TempService, TitleComponent, ToolbarComponent, ToolbarService, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types, Version, WatchingUsersComponent } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';
import { PreviewButtonComponent } from '../../shared/preview-button.component';
import { ContentEditorComponent } from './editor/content-editor.component';
import { ContentInspectionComponent } from './inspecting/content-inspection.component';
import { ContentReferencesComponent } from './references/content-references.component';

type SaveNavigationMode = 'Close' | 'Add' | 'Edit';

@Component({
    standalone: true,
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    providers: [
        CollaborationService,
        CommentsState,
        ResolveAssets,
        ResolveContents,
        ToolbarService,
    ],
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        ContentEditorComponent,
        ContentExtensionComponent,
        ContentInspectionComponent,
        ContentReferencesComponent,
        DropdownMenuComponent,
        FormsModule,
        LanguageSelectorComponent,
        LayoutComponent,
        ModalDirective,
        ModalPlacementDirective,
        NotifoComponent,
        PreviewButtonComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        ToolbarComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
        WatchingUsersComponent,
    ],
})
export class ContentPageComponent implements CanComponentDeactivate, OnInit {
    private readonly subscriptions = new Subscriptions();
    private readonly mutableContext: Record<string, any>;
    private autoSaveKey!: AutoSaveKey;
    private autoSaveIgnore = false;

    public schema!: SchemaDto;

    public formContext: any;

    public contentTab = this.route.queryParams.pipe(map(x => x['tab'] || 'editor'));
    public contentId = '';
    public content?: ContentDto | null;
    public contentVersion: Version | null = null;
    public contentForm!: EditContentForm;
    public contentFormCompare: EditContentForm | null = null;
    public saveOnlyDropdown = new ModalModel();
    public savePublishDropdown = new ModalModel();

    public dropdown = new ModalModel();

    public language!: AppLanguageDto;
    public languages!: ReadonlyArray<AppLanguageDto>;

    public showIdInput = true;

    public confirmPreview = () => {
        return this.checkPendingChangesBeforePreview();
    };

    constructor(apiUrl: ApiUrlConfig, authService: AuthService, appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly autoSaveService: AutoSaveService,
        private readonly collaboration: CollaborationService,
        private readonly dialogs: DialogService,
        private readonly messageBus: MessageBus,
        private readonly languagesState: LanguagesState,
        private readonly localStore: LocalStoreService,
        private readonly location: Location,
        private readonly previousUrl: PreviousUrl,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
        private readonly tempService: TempService,
    ) {
        const role = appsState.snapshot.selectedApp?.roleName;

        this.mutableContext = {
            apiUrl: apiUrl.buildUrl('api'),
            appId: contentsState.appId,
            appName: contentsState.appName,
            user: { role, ...authService.user?.export() },
        };

        this.showIdInput = localStore.getBoolean(Settings.Local.CONTENT_ID_INPUT);
    }

    public ngOnInit() {
        this.contentsState.loadIfNotLoaded();

        this.subscriptions.add(
            this.messageBus.of(AnnotationCreate)
                .subscribe(async message => {
                    if (message.annotation) {
                        await this.router.navigate(['comments'], { relativeTo: this.route });
                    }

                    setTimeout(() => {
                        this.messageBus.emit(new AnnotationCreateAfterNavigate(message.editorId, message.annotation));
                    });
                }));

        this.subscriptions.add(
            this.messageBus.of(AnnotationsSelect)
                .subscribe(async message => {
                    if (message.annotations.length > 0) {
                        await this.router.navigate(['comments'], { relativeTo: this.route });
                    }

                    setTimeout(() => {
                        this.messageBus.emit(new AnnotationsSelectAfterNavigate(message.annotations));
                    });
                }));

        this.subscriptions.add(
            this.languagesState.isoMasterLanguage
                .subscribe(language => {
                    this.language = language;
                }));

        this.subscriptions.add(
            this.languagesState.isoLanguages
                .subscribe(languages => {
                    this.languages = languages;
                }));

        this.subscriptions.add(
            this.schemasState.selectedSchema.pipe(defined())
                .subscribe(schema => {
                    this.schema = schema;

                    const languageKey = this.localStore.get(this.languageKey());
                    const languageItem = this.languages.find(x => x.iso2Code === languageKey);

                    if (languageItem) {
                        this.language = languageItem;
                    }

                    this.contentForm = new EditContentForm(this.languages, this.schema, this.schemasState.schemaMap, this.formContext);
                }));

        this.subscriptions.add(
            this.contentsState.selectedContent
                .subscribe(content => {
                    const isNewContent = isOtherContent(content, this.content);

                    this.content = content;
                    this.updateContext();
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

                    if (content) {
                        this.collaboration.connect(`apps/${this.schemasState.appName}/collaboration/${content.id}`);
                    } else {
                        this.collaboration.connect(null);
                    }
                }));

        this.subscriptions.add(
            this.contentForm.valueChanges.pipe(filter(_ => this.contentForm.form.enabled))
                .subscribe(value => {
                    if (!Types.equals(value, this.content?.data)) {
                        this.autoSaveService.set(this.autoSaveKey, value);
                    } else {
                        this.autoSaveService.remove(this.autoSaveKey);
                    }
                }));
    }

    private updateContext() {
        this.mutableContext['initialContent'] = this.content;
        this.mutableContext['language'] = this.language;
        this.mutableContext['languages'] = this.languages;
        this.mutableContext['schema'] = this.schema;
        this.formContext = { ...this.mutableContext };
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

    public saveAndPublish(navigationMode: SaveNavigationMode) {
        this.saveContent(true, navigationMode);
    }

    public saveAsDraft(navigationMode: SaveNavigationMode) {
        this.saveContent(false, navigationMode);
    }

    private saveContent(publish: boolean, navigationMode: SaveNavigationMode) {
        const value = this.contentForm.submit();

        if (!value) {
            this.contentForm.submitFailed('i18n:contents.contentNotValid', false);
            return;
        }

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
                        switch (navigationMode) {
                            case 'Add':
                                this.contentForm = new EditContentForm(this.languages, this.schema, this.schemasState.schemaMap, this.formContext);
                                break;

                            case 'Edit':
                                this.contentForm.submitCompleted({ noReset: true });
                                this.contentForm.load(content.data, true);

                                this.router.navigate([content.id, 'history'], { relativeTo: this.route.parent! });
                                break;

                            case 'Close':
                                this.autoSaveIgnore = true;

                                this.router.navigate(['./'], { relativeTo: this.route.parent! });
                                break;
                        }
                    },
                    error: error => {
                        this.contentForm.submitFailed(error);
                    },
                });
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
        if (this.previousUrl.isPath(`/app/${this.contentsState.appName}/content/${this.schema.name}`)) {
            this.location.back();
        } else {
            this.router.navigate([this.schema.name], { relativeTo: this.route.parent!.parent, replaceUrl: true });
        }
    }

    public delete() {
        const content = this.content;

        if (content) {
            this.contentsState.deleteMany([content]).subscribe(() => {
                this.back();
            });
        }
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language = language;
        this.localStore.set(this.languageKey(), language.iso2Code);

        this.updateContext();
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
        if ((this.content && !this.content.canUpdate) || this.autoSaveIgnore) {
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

    public changeShowIdInput(value: boolean) {
        this.localStore.setBoolean(Settings.Local.CONTENT_ID_INPUT, value);

        this.showIdInput = value;
    }

    private languageKey(): any {
        return Settings.Local.CONTENT_LANGUAGE(this.schema.id);
    }
}

function isOtherContent(lhs: ContentDto | undefined | null, rhs: ContentDto | undefined | null) {
    return !lhs || !rhs || lhs.id !== rhs.id;
}
