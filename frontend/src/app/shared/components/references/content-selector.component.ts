/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, forwardRef, Input, numberAttribute, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, of } from 'rxjs';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { LanguageSelectorComponent, ListViewComponent, ModalDialogComponent, PagerComponent, SyncWidthDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { ApiUrlConfig, AppsState, ComponentContentsState, ContentDto, LanguageDto, META_FIELDS, Query, SchemaDto, SchemasService, SchemasState, Subscriptions } from '@app/shared/internal';
import { ContentListCellDirective, ContentListWidthDirective } from '../contents/content-list-cell.directive';
import { ContentListHeaderComponent } from '../contents/content-list-header.component';
import { SearchFormComponent } from '../search/search-form.component';
import { ContentSelectorItemComponent } from './content-selector-item.component';

@Component({
    standalone: true,
    selector: 'sqx-content-selector',
    styleUrls: ['./content-selector.component.scss'],
    templateUrl: './content-selector.component.html',
    providers: [
        ComponentContentsState,
    ],
    imports: [
        AsyncPipe,
        ContentListCellDirective,
        ContentListHeaderComponent,
        ContentListWidthDirective,
        ContentSelectorItemComponent,
        FormsModule,
        LanguageSelectorComponent,
        ListViewComponent,
        ModalDialogComponent,
        PagerComponent,
        SyncWidthDirective,
        TooltipDirective,
        TranslatePipe,
        forwardRef(() => SearchFormComponent),
    ],
})
export class ContentSelectorComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();
    private initialQuery?: string = undefined;

    public readonly metaFields = META_FIELDS;

    @Output()
    public contentSelect = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input({ transform: numberAttribute })
    public maxItems = Number.MAX_VALUE;

    @Input()
    public schemaName?: string | null;

    @Input()
    public schemaIdentifiers?: ReadonlyArray<string>;

    @Input()
    public query?: string;

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public allowDuplicates?: boolean | null;

    @Input()
    public alreadySelectedIds: ReadonlyArray<string> | undefined | null;

    @Input()
    public set alreadySelected(value: ReadonlyArray<ContentDto> | undefined | null) {
        this.alreadySelectedIds = value?.map(x => x.id);
    }

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
    }

    public ngOnInit() {
        this.initialQuery = this.query;

        this.subscriptions.add(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.schemas = this.schemasState.snapshot.schemas.filter(x => x.type === 'Default' && x.canReadContents);

        if (this.schemaIdentifiers && this.schemaIdentifiers.length > 0) {
            this.schemas = this.schemas.filter(x => this.schemaIdentifiers!.includes(x.id) || this.schemaIdentifiers!.includes(x.name));
        }

        this.selectSchema(this.schemas[0]);
    }

    public selectSchema(schema: SchemaDto) {
        this.schema = schema;

        if (schema) {
            this.contentsState.schema = schema;
            this.contentsState.search({ fullText: this.initialQuery || undefined });
            this.initialQuery = undefined;

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
        return !this.allowDuplicates && this.alreadySelectedIds && this.alreadySelectedIds.indexOf(content.id) >= 0;
    }

    public emitClose() {
        this.contentSelect.emit([]);
    }

    public emitSelect() {
        this.contentSelect.emit(Object.values(this.selectedItems));
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
}
