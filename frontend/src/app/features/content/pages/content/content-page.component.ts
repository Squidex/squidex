/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, Injector, linkedSignal, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { firstValueFrom, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AnnotationCreate, AnnotationCreateAfterNavigate, AnnotationsSelected, AnnotationsSelectedAfterNavigate, ApiUrlConfig, AppLanguageDto, AppsState, AuthService, AutoSaveKey, AutoSaveService, CanComponentDeactivate, CollaborationService, CommentsState, ConfirmClickDirective, ContentDto, ContentsState, DialogService, DropdownMenuComponent, EditContentForm, LanguageSelectorComponent, LanguagesState, LayoutComponent, LocalStoreService, MessageBus, ModalDirective, ModalModel, ModalPlacementDirective, NotifoComponent, PreviousUrl, queryParam, ResolveAssets, ResolveContents, SchemaDto, SchemasState, Settings, ShortcutDirective, SidebarMenuDirective, TempService, TitleComponent, ToolbarComponent, ToolbarService, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, Types, Version, WatchingUsersComponent } from '@app/shared';
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
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly appsState = inject(AppsState);
    private readonly authService = inject(AuthService);
    private readonly autoSaveService = inject(AutoSaveService);
    private readonly collaboration = inject(CollaborationService);
    private readonly dialogs = inject(DialogService);
    private readonly messageBus = inject(MessageBus);
    private readonly languagesState = inject(LanguagesState);
    private readonly localStore = inject(LocalStoreService);
    private readonly location = inject(Location);
    private readonly previousUrl = inject(PreviousUrl);
    private readonly injector = inject(Injector);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly schemasState = inject(SchemasState);
    private readonly tempService = inject(TempService);
    private readonly mutableContext: Record<string, any> = {
        action: 'Create',
        apiUrl: this.apiUrl.buildUrl('api'),
        appId: this.appsState.appId,
        appName: this.appsState.appName,
        user: { role: this.appsState.snapshot.selectedApp?.roleName, ...this.authService.user?.export() },
    };

    private autoSaveKey!: AutoSaveKey;
    private autoSaveIgnore = false;

    protected readonly contentsState = inject(ContentsState);

    protected readonly schema = computed(() => {
        return this.schemasState.selectedSchema()!;
    });

    protected readonly formContext = signal<any>(undefined);
    protected readonly contentTab = queryParam<Tab>('tab', 'editor');
    protected readonly contentId = signal('');
    protected readonly content = signal<ContentDto | null | undefined>(undefined);
    protected readonly contentVersion = signal<Version | null>(null);
    protected readonly contentFormCompare = signal<EditContentForm | null>(null);
    protected readonly saveOnlyDropdown = new ModalModel();
    protected readonly savePublishDropdown = new ModalModel();
    protected readonly dropdown = new ModalModel();

    protected readonly languages = computed(() => {
        return this.languagesState.isoLanguages();
    });

    protected readonly languageStore = computed(() => {
        return this.localStore.value(Settings.Local.CONTENT_LANGUAGE(this.schema().id));
    });

    protected readonly language = computed(() => {
        const languageKey = this.languageStore()();
        const languageMaster = this.languagesState.isoMasterLanguage();

        return this.languages().find(x => x.iso2Code === languageKey) || languageMaster;
    });

    protected readonly contentForm = linkedSignal(() => {
        const schema = this.schema();

        return untracked(() => this.createForm(schema));
    });

    protected readonly showIdInput = this.localStore.boolean(Settings.Local.CONTENT_ID_INPUT);

    protected readonly confirmPreview = () => {
        return this.checkPendingChangesBeforePreview();
    };

    constructor() {
        this.contentsState.loadIfNotLoaded();

        this.messageBus.on(AnnotationCreate, async message => {
            if (message.annotation) {
                await this.router.navigate(['comments'], { relativeTo: this.route });
            }

            setTimeout(() => {
                this.messageBus.emit(new AnnotationCreateAfterNavigate(message.editorId, message.annotation));
            });
        });

        this.messageBus.on(AnnotationsSelected, async message => {
            if (message.commentIds.length > 0) {
                await this.router.navigate(['comments'], { relativeTo: this.route });
            }

            setTimeout(() => {
                this.messageBus.emit(new AnnotationsSelectedAfterNavigate(message.commentIds));
            });
        });

        effect(() => {
            const content = this.contentsState.selectedContent();

            untracked(() => this.updateContent(content));
        });

        effect(() => {
            const contentForm = this.contentForm();
            const contentValue = contentForm.value();

            untracked(() => this.autoSave(contentForm, contentValue));
        });
    }

    private createForm(schema: SchemaDto) {
        return new EditContentForm(this.languages(), schema, this.schemasState.schemaMap, this.formContext(), this.injector);
    }

    private updateContent(content: ContentDto | null | undefined) {
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
    }

    private autoSave(contentForm: EditContentForm, value: any) {
        if (contentForm.form().disabled()) {
            return;
        }

        if (!Types.equals(value, this.content()?.data)) {
            this.autoSaveService.set(this.autoSaveKey, value);
        } else {
            this.autoSaveService.remove(this.autoSaveKey);
        }
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

    protected saveAndPublish(navigationMode: SaveNavigationMode) {
        this.saveContent(true, navigationMode);
    }

    protected saveAsDraft(navigationMode: SaveNavigationMode) {
        this.saveContent(false, navigationMode);
    }

    private async saveContent(publish: boolean, navigationMode: SaveNavigationMode) {
        const contentForm = this.contentForm();
        const content = this.content();

        if (content && !content.canUpdate) {
            return;
        }

        if (!content && !this.canCreate(publish)) {
            return;
        }

        const isSaved = await contentForm.submit(async value => {
            if (content) {
                return await firstValueFrom(this.contentsState.update(content, value));
            }

            const created = await firstValueFrom(this.contentsState.create(value, publish, this.contentId()));

            switch (navigationMode) {
                case 'Add':
                    this.contentForm.set(this.createForm(this.schema()));
                    break;

                case 'Edit':
                    contentForm.load(created.data, true);

                    this.router.navigate([created.id, 'history'], { relativeTo: this.route.parent! });
                    break;

                case 'Close':
                    this.autoSaveIgnore = true;

                    this.router.navigate(['./'], { relativeTo: this.route.parent! });
                    break;
            }

            return created;
        });

        if (!isSaved && !contentForm.error()) {
            contentForm.setError('i18n:contents.contentNotValid');
        }
    }

    private canCreate(publish: boolean) {
        if (publish) {
            return this.contentsState.snapshot.canCreateAndPublish;
        } else {
            return this.contentsState.snapshot.canCreate;
        }
    }

    protected back() {
        const schemaName = this.schema().name;

        if (this.previousUrl.isPath(`/app/${this.contentsState.appName}/content/${schemaName}`)) {
            this.location.back();
        } else {
            this.router.navigate([schemaName], { relativeTo: this.route.parent!.parent, replaceUrl: true });
        }
    }

    protected delete() {
        const content = this.content();

        if (content) {
            this.contentsState.deleteMany([content]).subscribe(() => {
                this.back();
            });
        }
    }

    protected changeLanguage(language: AppLanguageDto) {
        this.languageStore().set(language.iso2Code);

        this.updateContext();
    }

    private checkPendingChangesBeforePreview() {
        return this.checkPendingChanges('i18n:contents.pendingChangesTextToPreview');
    }

    private checkPendingChangesBeforeClose() {
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

    protected loadLatest() {
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
                        const contentFormCompare = this.createForm(this.schema());

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

    protected clone() {
        this.tempService.put(this.content()!.data);

        this.router.navigate(['../new'], { relativeTo: this.route });
    }

    protected changeShowIdInput(value: boolean) {
        this.showIdInput.set(value);
    }
}

function isOtherContent(lhs: ContentDto | undefined | null, rhs: ContentDto | undefined | null) {
    return !lhs || !rhs || lhs.id !== rhs.id;
}
