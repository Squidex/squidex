/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ApiUrlConfig, AppLanguageDto, AppsState, AuthService, ContentDto, EditContentForm, LanguageDto, ManualContentsState, ResourceOwner, SchemaDetailsDto, SchemaDto, SchemasState, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-creator',
    styleUrls: ['./content-creator.component.scss'],
    templateUrl: './content-creator.component.html',
    providers: [
        ManualContentsState
    ]
})
export class ContentCreatorComponent extends ResourceOwner implements OnInit {
    @Output()
    public select = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input()
    public schemaIds: ReadonlyArray<string>;

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    public schema: SchemaDetailsDto;
    public schemas: ReadonlyArray<SchemaDto> = [];

    public contentFormContext: any;
    public contentForm: EditContentForm;

    constructor(authService: AuthService,
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        public readonly contentsState: ManualContentsState,
        public readonly schemasState: SchemasState,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        super();

        this.contentFormContext = { user: authService.user, apiUrl: apiUrl.buildUrl('api') };
    }

    public ngOnInit() {
        this.schemas = this.schemasState.snapshot.schemas.filter(x => x.canContentsCreate);

        if (this.schemaIds && this.schemaIds.length > 0) {
            this.schemas = this.schemas.filter(x => this.schemaIds.indexOf(x.id) >= 0);
        }

        this.selectSchema(this.schemas[0]);
    }

    public selectSchema(selected: string | SchemaDto) {
        if (Types.is(selected, SchemaDto)) {
            selected = selected.id;
        }

        this.schemasState.loadSchema(selected, true)
            .subscribe(schema => {
                if (schema) {
                    this.schema = schema;

                    this.contentsState.schema = schema;
                    this.contentForm = new EditContentForm(this.languages, this.schema, this.contentFormContext.user);

                    this.changeDetector.markForCheck();
                }
            });
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
            if (!this.canCreate(publish)) {
                return;
            }

            this.contentsState.create(value, publish)
                .subscribe(content => {
                    this.contentForm.submitCompleted({ noReset: true });

                    this.emitSelect(content);
                }, error => {
                    this.contentForm.submitFailed(error);
                });
        } else {
            this.contentForm.submitFailed('i18n:contents.contentNotValid');
        }
    }

    private canCreate(publish: boolean) {
        if (publish) {
            return this.schema.canContentsCreateAndPublish;
        } else {
            return this.schema.canContentsCreateAndPublish;
        }
    }

    public emitComplete() {
        this.select.emit([]);
    }

    public emitSelect(content: ContentDto) {
        this.select.emit([content]);
    }

    public selectLanguage(language: LanguageDto) {
        this.language = language;
    }
}