/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsService,
    getContentValue,
    ImmutableArray,
    MathHelper,
    SchemaDetailsDto,
    SchemasService,
    StatefulControlComponent,
    Types
} from '@app/shared/internal';

export const SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesDropdownComponent), multi: true
};

interface State {
    schema?: SchemaDetailsDto | null;

    contentItems: ImmutableArray<ContentDto>;
    contentNames: ImmutableArray<ContentName>;
}

type ContentName = { name: string, id: string };

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-references-dropdown',
    template: `
        <select class="form-control" [formControl]="selectedId">
            <option [ngValue]="null"></option>
            <option *ngFor="let content of snapshot.contentNames" [ngValue]="content.id">{{content.name}}</option>
        </select>`,
    providers: [SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesDropdownComponent extends StatefulControlComponent<State, string[]> implements OnInit {
    private languageField: AppLanguageDto;

    @Input()
    public schemaId: string;

    @Input()
    public mode: 'Array' | 'Single';

    @Input()
    public set language(value: AppLanguageDto) {
        this.languageField = value;

        this.next(s => ({ ...s, contentNames: this.createContentNames(s.schema, s.contentItems) }));
    }

    public selectedId = new FormControl('');

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly schemasService: SchemasService
    ) {
        super(changeDetector, {
            schema: null,
            contentItems: ImmutableArray.empty(),
            contentNames: ImmutableArray.empty()
        });

        this.own(
            this.selectedId.valueChanges
                .subscribe(value => {
                    if (value) {
                        this.callTouched();
                        this.callChange([value]);
                    } else {
                        this.callTouched();
                        this.callChange([]);
                    }
                }));
    }

    public ngOnInit() {
        if (this.schemaId === MathHelper.EMPTY_GUID) {
            this.selectedId.disable();
            return;
        }

        this.schemasService.getSchema(this.appsState.appName, this.schemaId).pipe(
                switchMap(schema => {
                    if (schema) {
                        return this.contentsService.getContents(this.appsState.appName, this.schemaId, 100, 0);
                    } else {
                        return throwError('Invalid schema');
                    }
                }, (schema, contents) => ({ schema, contents })))
            .subscribe(({ schema, contents }) => {
                const contentItems = ImmutableArray.of(contents.items);
                const contentNames = this.createContentNames(schema, contentItems);

                this.next(s => ({ ...s, schema, contentItems, contentNames }));
            }, () => {
                this.selectedId.disable();
            });
    }

    public writeValue(obj: any) {
        if (Types.isString(obj)) {
            this.selectedId.setValue(obj, NO_EMIT);
        } else if (Types.isArrayOfString(obj)) {
            this.selectedId.setValue(obj[0], NO_EMIT);
        } else {
            this.selectedId.setValue(undefined, NO_EMIT);
        }
    }

    private createContentNames(schema: SchemaDetailsDto | undefined | null, contents: ImmutableArray<ContentDto>): ImmutableArray<ContentName> {
        if (contents.length === 0 || !schema) {
            return ImmutableArray.empty();
        }

        return contents.map(content => {
            const name =
                schema.referenceFields
                    .map(f => getContentValue(content, this.languageField, f, false))
                    .map(v => v.formatted)
                    .join(', ');

            return { name, id: content.id };
        });
    }

    public trackByContent(content: ContentDto) {
        return content.id;
    }
}