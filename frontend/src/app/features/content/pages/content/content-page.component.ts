/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, Location } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, signal, untracked } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Observable, of } from 'rxjs';
import { filter, map, tap } from 'rxjs/operators';
import { AnnotationCreate, AnnotationCreateAfterNavigate, AnnotationsSelected, AnnotationsSelectedAfterNavigate, ApiUrlConfig, AppLanguageDto, AppsState, AuthService, AutoSaveKey, AutoSaveService, CanComponentDeactivate, CollaborationService, CommentsState, ConfirmClickDirective, ContentDto, ContentsState, defined, DialogService, DropdownMenuComponent, EditContentForm, LanguageSelectorComponent, LanguagesState, LayoutComponent, LocalStoreService, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, NotifoComponent, PreviousUrl, ResolveAssets, ResolveContents, SchemaDto, SchemasState, Settings, ShortcutDirective, SidebarMenuDirective, TempService, TitleComponent, ToolbarComponent, ToolbarService, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types, Version, WatchingUsersComponent } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';
import { PreviewButtonComponent } from '../../shared/preview-button.component';
import { ContentEditorComponent } from './editor/content-editor.component';
import { ContentInspectionComponent } from './inspecting/content-inspection.component';
import { ContentReferencesComponent } from './references/content-references.component';

type SaveNavigationMode = 'Close' | 'Add' | 'Edit';

type Tab = 'editor' | 'references' | 'referencing' | 'inspect' | 'extension';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class ContentPageComponent implements CanComponentDeactivate {
    private readonly mutableContext: Record<string, any>;
    private autoSaveKey!: AutoSaveKey;
    private autoSaveIgnore = false;

    public readonly schema = signal<SchemaDto>(null!);

    public readonly formContext = signal<any>(undefined);

    public readonly contentTab = toSignal(this.route.queryParams.pipe(map(x => x['tab'] as Tab || 'editor')), { initialValue: 'editor' });
    public readonly contentId = signal('');
    public readonly content = signal<ContentDto | null | undefined>(undefined);
    public readonly contentVersion = signal<Version | null>(null);
    public readonly contentForm = signal<EditContentForm>(null!);
    public readonly contentFormCompare = signal<EditContentForm | null>(null);
    public readonly saveOnlyDropdown = new ModalModel();
    public readonly savePublishDropdown = new ModalModel();

    public readonly dropdown = new ModalModel();

    public readonly language = signal<AppLanguageDto>(null!);
    public readonly languages = signal<ReadonlyArray<AppLanguageDto>>([]);

    public readonly showIdInput = signal(this.localStore.getBoolean(Settings.Local.CONTENT_ID_INPUT));

    public readonly confirmPreview = () => {
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
            action: 'Create',
            apiUrl: apiUrl.buildUrl('api'),
            appId: contentsState.appId,
            appName: contentsState.appName,
            user: { role, ...authService.user?.export() },
        };

        this.contentsState.loadIfNotLoaded();

        effect(onCleanup => {
            const subscriptions = untracked(() => this.subscribe());

            onCleanup(() => {
                for (const subscription of subscriptions) {
                    subscription.unsubscribe();
                }
            });
        });
    }

    private subscribe() {
        return [
            this.messageBus.of(AnnotationCreate)
                .subscribe(async message => {
                    if (message.annotation) {
                        await this.router.navigate(['comments'], { relativeTo: this.route });
                    }

                    setTimeout(() => {
                        this.messageBus.emit(new AnnotationCreateAfterNavigate(message.editorId, message.annotation));
                    });
                }),
            this.messageBus.of(AnnotationsSelected)
                .subscribe(async message => {
                    if (message.commentIds.length > 0) {
                        await this.router.navigate(['comments'], { relativeTo: this.route });
                    }

                    setTimeout(() => {
                        this.messageBus.emit(new AnnotationsSelectedAfterNavigate(message.commentIds));
                    });
                }),
            this.languagesState.isoMasterLanguage
                .subscribe(language => {
                    this.language.set(language);
                }),
            this.languagesState.isoLanguages
                .subscribe(languages => {
                    this.languages.set(languages);
                }),
            this.schemasState.selectedSchema.pipe(defined())
                .subscribe(schema => {
                    this.schema.set(schema);

                    const languageKey = this.localStore.get(this.languageKey());
                    const languageItem = this.languages().find(x => x.iso2Code === languageKey);

                    if (languageItem) {
                        this.language.set(languageItem);
                    }

                    this.contentForm.set(new EditContentForm(this.languages(), schema, this.schemasState.schemaMap, this.formContext()));
                }),
            this.contentsState.selectedContent
                .subscribe(content => {
                    const isNewContent = isOtherContent(content, this.content());

                    this.content.set(content);
                    this.updateContext();
                    this.contentForm().setContext(this.formContext());

                    const schema = this.schema();

                    this.autoSaveKey = {
                        schemaId: schema.id,
                        schemaVersion: schema.version,
                        contentId: content?.id,
                    };

                    const dataAutosaved = this.autoSaveService.fetch(this.autoSaveKey);
                    const dataCloned = this.tempService.fetch();

                    if (dataCloned || content) {
                        this.loadContent(dataCloned || content?.data || {}, true);
                    }

                    if (isNewContent && dataAutosaved && this.contentForm().hasChanges(dataAutosaved)) {
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
                }),
            this.subscribeToAutoSave(),
        ];
    }

    private subscribeToAutoSave() {
        const contentForm = this.contentForm();

        return contentForm.valueChanges.pipe(filter(_ => contentForm.form.enabled))
            .subscribe(value => {
                if (!Types.equals(value, this.content()?.data)) {
                    this.autoSaveService.set(this.autoSaveKey, value);
                } else {
                    this.autoSaveService.remove(this.autoSaveKey);
                }
            });
    }

    private updateContext() {
        const content = this.content();

        this.mutableContext['action'] = content ? 'Create' : 'Update';
        this.mutableContext['initialContent'] = content;
        this.mutableContext['language'] = this.language();
        this.mutableContext['languages'] = this.languages();
        this.mutableContext['schema'] = this.schema();
        this.formContext.set({ ...this.mutableContext });
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
        const contentForm = this.contentForm();

        const value = contentForm.submit();
        if (!value) {
            contentForm.submitFailed('i18n:contents.contentNotValid', false);
            return;
        }

        const content = this.content();
        if (content) {
            if (!content.canUpdate) {
                return;
            }

            this.contentsState.update(content, value)
                .subscribe({
                    next: () => {
                        contentForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        contentForm.submitFailed(error);
                    },
                });
        } else {
            if (!this.canCreate(publish)) {
                return;
            }

            this.contentsState.create(value, publish, this.contentId())
                .subscribe({
                    next: content => {
                        switch (navigationMode) {
                            case 'Add':
                                this.contentForm.set(new EditContentForm(this.languages(), this.schema(), this.schemasState.schemaMap, this.formContext()));
                                break;

                            case 'Edit':
                                contentForm.submitCompleted({ noReset: true });
                                contentForm.load(content.data, true);

                                this.router.navigate([content.id, 'history'], { relativeTo: this.route.parent! });
                                break;

                            case 'Close':
                                this.autoSaveIgnore = true;

                                this.router.navigate(['./'], { relativeTo: this.route.parent! });
                                break;
                        }
                    },
                    error: error => {
                        contentForm.submitFailed(error);
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
        const schemaName = this.schema().name;

        if (this.previousUrl.isPath(`/app/${this.contentsState.appName}/content/${schemaName}`)) {
            this.location.back();
        } else {
            this.router.navigate([schemaName], { relativeTo: this.route.parent!.parent, replaceUrl: true });
        }
    }

    public delete() {
        const content = this.content();

        if (content) {
            this.contentsState.deleteMany([content]).subscribe(() => {
                this.back();
            });
        }
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language.set(language);
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
        if (this.autoSaveIgnore) {
            return of(true);
        }

        const content = this.content();
        if (content && !content.canUpdate) {
            return of(true);
        }

        return this.contentForm().hasChanged() ?
            this.dialogs.confirm('i18n:contents.pendingChangesTitle', text) :
            of(true);
    }

    public loadLatest() {
        this.loadVersion(null, false);
    }

    public loadVersion(version: number | null, compare: boolean) {
        const content = this.content();

        if (!content || version === null || version === content.version) {
            this.contentFormCompare.set(null);
            this.contentVersion.set(null);
            this.loadContent(content?.data || {}, true);
        } else {
            this.contentsState.loadVersion(content, version)
                .subscribe(dto => {
                    if (compare) {
                        const contentFormCompare = new EditContentForm(this.languages(), this.schema(), this.schemasState.schemaMap, this.formContext());

                        contentFormCompare.load(dto.payload);
                        contentFormCompare.setEnabled(false);

                        this.contentFormCompare.set(contentFormCompare);
                        this.loadContent(content.data, false);
                    } else {
                        this.contentFormCompare.set(null);

                        this.loadContent(dto.payload, false);
                    }

                    this.contentVersion.set(version);
                });
        }
    }

    private loadContent(data: any, isInitial: boolean) {
        this.autoSaveService.remove(this.autoSaveKey);

        const content = this.content();

        this.contentForm().load(data, isInitial);
        this.contentForm().setEnabled(!content || content.canUpdate);
    }

    public clone() {
        this.tempService.put(this.content()!.data);

        this.router.navigate(['../new'], { relativeTo: this.route });
    }

    public changeShowIdInput(value: boolean) {
        this.localStore.setBoolean(Settings.Local.CONTENT_ID_INPUT, value);

        this.showIdInput.set(value);
    }

    private languageKey(): any {
        return Settings.Local.CONTENT_LANGUAGE(this.schema().id);
    }
}

function isOtherContent(lhs: ContentDto | undefined | null, rhs: ContentDto | undefined | null) {
    return !lhs || !rhs || lhs.id !== rhs.id;
}
