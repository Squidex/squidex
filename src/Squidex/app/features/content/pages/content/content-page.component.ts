/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';

import { ContentVersionSelected } from './../messages';

import {
    AppLanguageDto,
    AppsState,
    CanComponentDeactivate,
    ContentDto,
    ContentsState,
    DialogService,
    EditContentForm,
    ImmutableArray,
    LanguagesState,
    MessageBus,
    SchemaDetailsDto,
    SchemasState,
    Version
} from '@app/shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent implements CanComponentDeactivate, OnDestroy, OnInit {
    private languagesSubscription: Subscription;
    private contentSubscription: Subscription;
    private contentVersionSelectedSubscription: Subscription;
    private selectedSchemaSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentVersion: Version | null;
    public contentForm: EditContentForm;

    public language: AppLanguageDto;
    public languages: ImmutableArray<AppLanguageDto>;

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
    }

    public ngOnDestroy() {
        this.languagesSubscription.unsubscribe();
        this.contentSubscription.unsubscribe();
        this.contentVersionSelectedSubscription.unsubscribe();
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.languagesSubscription =
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages.at(0);
                });

        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema.filter(s => !!s).map(s => s!)
                .subscribe(schema => {
                    this.schema = schema;

                    this.contentForm = new EditContentForm(this.schema, this.languages);
                });

        this.contentSubscription =
            this.contentsState.selectedContent.filter(c => !!c).map(c => c!)
                .subscribe(content => {
                    this.content = content;

                    this.loadContent(content.data);
                });

        this.contentVersionSelectedSubscription =
            this.messageBus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version);
                });
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.form.dirty || !this.content) {
            return Observable.of(true);
        } else {
            return this.dialogs.confirmUnsavedChanges();
        }
    }

    public saveAndPublish() {
        this.saveContent(true);
    }

    public saveAsDraft() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        if (this.content && this.content.status === 'Archived') {
            return;
        }

        const value = this.contentForm.submit();

        if (value) {
            if (this.content) {
                this.contentsState.update(this.content, value)
                    .subscribe(dto => {
                        this.contentForm.submitCompleted();
                    }, error => {
                        this.contentForm.submitFailed(error);
                    });
            } else {
                this.contentsState.create(value, publish)
                    .subscribe(dto => {
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
        this.contentForm.loadData(data, this.content && this.content.status === 'Archived');
    }

    private loadVersion(version: Version) {
        if (this.content) {
            this.contentsState.loadVersion(this.content, version)
                .subscribe(dto => {
                    if (this.content.version.value !== version.toString()) {
                        this.contentVersion = version;
                    } else {
                        this.contentVersion = null;
                    }

                    this.loadContent(dto);
                });
        }
    }

    public showLatest() {
        if (this.contentVersion) {
            this.contentVersion = null;

            this.loadContent(this.content.data);
        }
    }
}