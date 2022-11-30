/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { ApiUrlConfig, AppsState, ComponentContentsState, ContentDto, LanguageDto, META_FIELDS, Query, ResourceOwner, SchemaDto, SchemasService, SchemasState } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-selector[language][languages]',
    styleUrls: ['./content-selector.component.scss'],
    templateUrl: './content-selector.component.html',
    providers: [
        ComponentContentsState,
    ],
})
export class ContentSelectorComponent extends ResourceOwner implements OnInit {
    public readonly metaFields = META_FIELDS;

    @Output()
    public select = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input()
    public maxItems = Number.MAX_VALUE;

    @Input()
    public schemaName?: string | null;

    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input()
    public schemaNames?: ReadonlyArray<string>;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public allowDuplicates?: boolean | null;

    @Input()
    public alreadySelected: ReadonlyArray<ContentDto> | undefined | null;

    public schema!: SchemaDto;
    public schemas: ReadonlyArray<SchemaDto> = [];

    public selectedItems: { [id: string]: ContentDto } = {};
    public selectionCount = 0;
    public selectedAll = false;

    public querySource = new BehaviorSubject<SchemaDto | null>(null);
    public queryModel =
        this.querySource.pipe(map(x => x?.name), distinctUntilChanged(),
            switchMap(x => {
                if (x) {
                    return this.schemasService.getFilters(this.appsState.appName, x);
                } else {
                    return of(null);
                }
            }));

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        public readonly contentsState: ComponentContentsState,
        public readonly schemasState: SchemasState,
        public readonly schemasService: SchemasService,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.schemas = this.schemasState.snapshot.schemas.filter(x => x.type === 'Default' && x.canReadContents);

        if (this.schemaIds && this.schemaIds.length > 0) {
            this.schemas = this.schemas.filter(x => this.schemaIds!.includes(x.id));
        }

        if (this.schemaNames && this.schemaNames.length > 0) {
            this.schemas = this.schemas.filter(x => this.schemaNames!.includes(x.name));
        }

        this.selectSchema(this.schemas[0]);
    }

    public selectSchema(schema: SchemaDto) {
        this.schema = schema;

        if (schema) {
            this.contentsState.schema = schema;
            this.contentsState.load();

            this.updateModel();
        }
    }

    public reload() {
        this.contentsState.load(true);
    }

    public reloadTotal() {
        this.contentsState.load(true, false);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public isItemSelected(content: ContentDto) {
        return !!this.selectedItems[content.id];
    }

    public isItemAlreadySelected(content: ContentDto) {
        return !this.allowDuplicates && this.alreadySelected && !!this.alreadySelected.find(x => x.id === content.id);
    }

    public emitComplete() {
        this.select.emit([]);
    }

    public emitSelect() {
        this.select.emit(Object.values(this.selectedItems));
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (const content of this.contentsState.snapshot.contents) {
                if (!this.isItemAlreadySelected(content)) {
                    this.selectedItems[content.id] = content;
                }
            }
        }

        this.updateSelectionSummary();
    }

    public selectContent(content: ContentDto) {
        if (this.selectedItems[content.id]) {
            delete this.selectedItems[content.id];
        } else {
            this.selectedItems[content.id] = content;
        }

        this.updateSelectionSummary();
    }

    private updateSelectionSummary() {
        this.selectionCount = Object.keys(this.selectedItems).length;
        this.selectedAll = this.selectionCount === this.contentsState.snapshot.contents.length;
    }

    private updateModel() {
        this.querySource.next(this.schema);
    }

    public trackByContent(_index: number, content: ContentDto): string {
        return content.id;
    }
}
